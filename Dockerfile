# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY BankingKata.sln .
COPY BankingKata.Domain/BankingKata.Domain.csproj BankingKata.Domain/
COPY BankingKata.Application/BankingKata.Application.csproj BankingKata.Application/
COPY BankingKata.Infrastructure/BankingKata.Infrastructure.csproj BankingKata.Infrastructure/
COPY BankingKata.Api/BankingKata.Api.csproj BankingKata.Api/

RUN dotnet restore BankingKata.Api/BankingKata.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/BankingKata.Api
RUN dotnet build BankingKata.Api.csproj -c Release -o /app/build
RUN dotnet publish BankingKata.Api.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

# Copy published app
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/api/health || exit 1

EXPOSE 5000

ENTRYPOINT ["dotnet", "BankingKata.Api.dll"]
