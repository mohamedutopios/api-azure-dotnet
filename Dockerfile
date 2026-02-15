# ============================================
# STAGE 1 : Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restaurer les dépendances (cache layer)
COPY RestApi/RestApi.csproj RestApi/
RUN dotnet restore RestApi/RestApi.csproj

# Copier et publier
COPY . .
WORKDIR /src/RestApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# STAGE 2 : Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Installer curl pour le healthcheck
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

# Utilisateur non-root
RUN adduser --disabled-password --no-create-home appuser

COPY --from=build /app/publish .

USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=5 \
    CMD curl -f http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "RestApi.dll"]
