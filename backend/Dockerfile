# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching)
COPY RealSync.slnx ./
COPY src/RealSync.Api/RealSync.Api.csproj src/RealSync.Api/
COPY src/RealSync.Core/RealSync.Core.csproj src/RealSync.Core/
COPY src/RealSync.Data/RealSync.Data.csproj src/RealSync.Data/
COPY src/RealSync.Services/RealSync.Services.csproj src/RealSync.Services/
COPY src/RealSync.Shared/RealSync.Shared.csproj src/RealSync.Shared/

# Restore
RUN dotnet restore RealSync.slnx

# Copy all source code
COPY . .

# Build
RUN dotnet publish src/RealSync.Api/RealSync.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RealSync.Api.dll"]
