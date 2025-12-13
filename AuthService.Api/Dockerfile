# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY AuthService.Api/AuthService.Api.csproj AuthService.Api/
COPY AuthService.Application/AuthService.Application.csproj AuthService.Application/
COPY AuthService.Domain/AuthService.Domain.csproj AuthService.Domain/
COPY AuthService.Infrastructure/AuthService.Infrastructure.csproj AuthService.Infrastructure/
COPY Directory.Packages.props ./

RUN dotnet restore "AuthService.Api/AuthService.Api.csproj"

# Copy source code and build
COPY . .
WORKDIR /src/AuthService.Api
RUN dotnet build "AuthService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AuthService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy email templates if they exist
COPY --from=publish /app/publish/EmailTemplates ./EmailTemplates

# Create directory for HTTPS certificates
RUN mkdir -p /https && chown -R appuser:appgroup /https

# Create directory for data protection keys
RUN mkdir -p /app/.aspnet/DataProtection-Keys && chown -R appuser:appgroup /app/.aspnet

# Set ownership of app directory
RUN chown -R appuser:appgroup /app

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Switch to non-root user
USER appuser

# Set entry point
ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
