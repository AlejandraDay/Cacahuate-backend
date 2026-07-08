# Cacahuate

API de Cacahuate desarrollada con ASP.NET Core y PostgreSQL.

## Requisitos

- Windows 10/11
- .NET SDK 9.x
- PostgreSQL 16 (recomendado)
- Git
- Opcional: pgAdmin o SQL Shell (psql)

## 1. Instalar PostgreSQL en Windows

### Opción recomendada con Winget

Abre PowerShell y ejecuta:

```powershell
winget install PostgreSQL.PostgreSQL.16
```

Durante la instalación, define una contraseña para el usuario `postgres`.
Si prefieres, puedes usar la contraseña `postgres` para que coincida con la configuración por defecto del proyecto.

### Verificar la instalación

Puedes abrir `pgAdmin` o el comando `psql` desde SQL Shell.

## 2. Crear la base de datos

Abre `psql` o `pgAdmin` y ejecuta lo siguiente:

```sql
CREATE DATABASE cacahuate_db;
```

Si quieres asegurar que el usuario `postgres` tenga la contraseña `postgres`, ejecuta:

```sql
ALTER USER postgres WITH PASSWORD 'postgres';
```

> Si usas otra contraseña, tendrás que actualizar la cadena de conexión en el proyecto.

## 3. Credenciales que usa este proyecto

El proyecto está configurado por defecto para usar estas credenciales:

- Host: `localhost`
- Puerto: `5432`
- Base de datos: `cacahuate_db`
- Usuario: `postgres`
- Contraseña: `postgres`

La cadena de conexión está en:

```json
Cacahuate.API/appsettings.json
```

Si cambiaste la contraseña de PostgreSQL, actualiza esta parte:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=cacahuate_db;Username=postgres;Password=postgres"
}
```

## 4. Restaurar dependencias

Desde la raíz del proyecto ejecuta:

```powershell
dotnet restore Cacahuate.sln
```

## 5. Instalar la herramienta de Entity Framework (si no está instalada)

```powershell
dotnet tool install --global dotnet-ef
```

## 6. Crear o actualizar la base de datos

```powershell
dotnet ef database update --project Cacahuate.DataAccess --startup-project Cacahuate.API
```

Si aparece un error de que `dotnet-ef` no se encuentra, reinicia la terminal o verifica que el PATH incluya la ruta de instalación de dotnet tools.

## 7. Ejecutar la API

```powershell
dotnet run --project .\Cacahuate.API\Cacahuate.API.csproj
```

La API quedará disponible en:

- HTTP: `http://localhost:5004`
- HTTPS: `https://localhost:7191`
- Swagger: `http://localhost:5004/swagger` o `https://localhost:7191/swagger`

## 8. Notas importantes

- Al iniciar la API, se ejecuta un seeding inicial de plantillas de formularios.
- Si la base de datos no existe, debes crearla antes de correr la aplicación.
- Si el puerto 5432 está ocupado o PostgreSQL está instalado en otro puerto, ajusta la cadena de conexión.

## 9. Solución rápida si algo falla

### Error de conexión a PostgreSQL

Verifica que:

- PostgreSQL esté corriendo como servicio.
- La contraseña en la cadena de conexión coincida con la de PostgreSQL.
- La base de datos `cacahuate_db` exista.

### Error de `dotnet ef`

Ejecuta:

```powershell
dotnet tool update --global dotnet-ef
```
