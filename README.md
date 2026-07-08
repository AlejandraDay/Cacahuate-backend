# Cacahuate - Backend

Este repositorio contiene el backend de la aplicación Cacahuate, una API en ASP.NET Core con PostgreSQL.

## Requisitos de entorno

- Windows 10/11
- Visual Studio 2022 o Visual Studio 2022 Community/Professional/Enterprise
- .NET 9 SDK
- PostgreSQL 16 (recomendado)
- Git
- Opcional: pgAdmin o SQL Shell (`psql`)

## Versiones recomendadas

- Visual Studio: 2022 (17.8 o superior)
- SDK .NET: 9.0.x
- PostgreSQL: 16.x

> El proyecto está configurado para `net9.0`.

## Abrir el proyecto en Visual Studio

1. Abre Visual Studio.
2. Selecciona `Open a project or solution`.
3. Navega a la carpeta del repositorio y abre `Cacahuate.sln`.
4. Espera a que Visual Studio restaure los paquetes NuGet.

## Configurar PostgreSQL

### 1. Instalar PostgreSQL

Puedes instalar PostgreSQL con el instalador oficial o usando Winget:

```powershell
winget install PostgreSQL.PostgreSQL.16
```

### 2. Crear la base de datos

Abre `psql` o `pgAdmin` y ejecuta:

```sql
CREATE DATABASE cacahuate_db;
```

Si quieres usar la contraseña por defecto del proyecto, configura el usuario `postgres` así:

```sql
ALTER USER postgres WITH PASSWORD 'postgres';
```

### 3. Verificar la cadena de conexión

El proyecto usa esta cadena de conexión en `Cacahuate.API/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=cacahuate_db;Username=postgres;Password=postgres"
}
```

Si cambiaste usuario, contraseña, nombre de base de datos o puerto, actualiza esta línea.

## Restaurar dependencias

Desde Visual Studio normalmente se restauran automáticamente. Si necesitas hacerlo manualmente, abre una terminal en la raíz y ejecuta:

```powershell
dotnet restore Cacahuate.sln
```

## Instalar / actualizar Entity Framework CLI

Si no tienes `dotnet-ef`, instálalo:

```powershell
dotnet tool install --global dotnet-ef
```

Si ya lo tienes, actualízalo para que coincida con el SDK:

```powershell
dotnet tool update --global dotnet-ef
```

## Aplicar migraciones y crear la base de datos

Desde la raíz del repositorio ejecuta:

```powershell
dotnet ef database update --project Cacahuate.DataAccess --startup-project Cacahuate.API
```

Si estás ubicado dentro de `Cacahuate.DataAccess`, usa este comando:

```powershell
dotnet ef database update --project .\Cacahuate.DataAccess.csproj --startup-project ..\Cacahuate.API\Cacahuate.API.csproj
```

> Esto es importante porque `AppDbContext` se configura en `Cacahuate.API`.

## Ejecutar la API desde Visual Studio

1. Selecciona `Cacahuate.API` como proyecto de inicio.
2. Asegúrate de que el perfil de ejecución use `https` o `http` según prefieras.
3. Presiona `F5` o `Ctrl+F5`.

## Ejecutar la API desde terminal

```powershell
dotnet run --project .\Cacahuate.API\Cacahuate.API.csproj
```

La API quedará disponible en:

- HTTP: `http://localhost:5004`
- HTTPS: `https://localhost:7191`
- Swagger: `http://localhost:5004/swagger` o `https://localhost:7191/swagger`

## Estructura relevante del backend

- `Cacahuate.API`: proyecto web ASP.NET Core.
- `Cacahuate.DataAccess`: proyecto de Entity Framework Core, contexto y migraciones.
- `Cacahuate.Services`: lógica de negocio e implementación de servicios.
- `Cacahuate.Shared`: DTOs, enums y tipos compartidos.

## Problemas comunes

### `relation "FormTemplates" does not exist`

Significa que no se aplicaron las migraciones. Ejecuta:

```powershell
dotnet ef database update --project Cacahuate.DataAccess --startup-project Cacahuate.API
```

### `Unable to resolve service for type 'Microsoft.EntityFrameworkCore.DbContextOptions`1[Cacahuate.DataAccess.Context.AppDbContext]'`

Ejecuta el comando de migraciones con el `--startup-project` correcto. El proyecto de inicio debe ser `Cacahuate.API`.

### `dotnet ef` no encontrado

Reinicia la terminal después de instalar o actualizar `dotnet-ef`, y verifica que la carpeta de herramientas globales esté en tu `PATH`.

## Últimos consejos

- Usa Visual Studio 2022 para mejor compatibilidad con .NET 9.
- Si usas otra versión de PostgreSQL, ajusta la cadena de conexión.
- Si el puerto 5432 está ocupado, cambia el puerto en PostgreSQL y en `appsettings.json`.
- Si trabajas en otro ambiente, revisa que `appsettings.Development.json` no sobrescriba la conexión.
