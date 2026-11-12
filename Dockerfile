# ── Build ────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first so the dependency layer is cached between builds
COPY Cacahuate.sln ./
COPY Cacahuate.API/Cacahuate.API.csproj Cacahuate.API/
COPY Cacahuate.Services/Cacahuate.Services.csproj Cacahuate.Services/
COPY Cacahuate.DataAccess/Cacahuate.DataAccess.csproj Cacahuate.DataAccess/
COPY Cacahuate.Shared/Cacahuate.Shared.csproj Cacahuate.Shared/
RUN dotnet restore Cacahuate.API/Cacahuate.API.csproj

COPY . .
RUN dotnet publish Cacahuate.API/Cacahuate.API.csproj -c Release -o /app/publish --no-restore

# ── Runtime ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=8080
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "Cacahuate.API.dll"]
