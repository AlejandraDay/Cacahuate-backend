# Despliegue (plan gratuito)

| Parte | Servicio | Repo |
|---|---|---|
| Base de datos | Neon (PostgreSQL) | — |
| Backend .NET 9 | Render (Docker) | `Cacahuate-backend` |
| Frontend React | Cloudflare Pages | `Cacahuate-frontend` |

Orden: **Neon → Render → Cloudflare → volver a Render para CORS**.

## 1. Base de datos — Neon

1. https://neon.tech → crear proyecto (región cercana, p. ej. AWS us-east).
2. Copiar la **connection string** (formato `postgresql://usuario:clave@host/neondb?sslmode=require`).
   Usar la conexión directa (sin `-pooler` en el host) para que las migraciones funcionen bien.

No hace falta crear tablas: la API aplica las migraciones al arrancar.

## 2. Backend — Render

1. https://render.com → **New → Blueprint** → conectar el repo `Cacahuate-backend`.
   Render lee `render.yaml` y crea el servicio `cacahuate-api` (Docker, plan free).
2. Completar las variables que pide:
   - `ConnectionStrings__DefaultConnection` → la connection string de Neon.
   - `Cors__AllowedOrigins` → dejar vacío por ahora (se completa en el paso 4).
   - `Jwt__Key` se genera solo. `Seed__DemoData=true` carga datos demo.
3. Esperar el deploy y comprobar `https://<servicio>.onrender.com/health` → `{"status":"ok"}`.
   Swagger: `https://<servicio>.onrender.com/swagger`.

El plan free se duerme tras ~15 min sin tráfico; la primera petición tarda 30–60 s.

## 3. Frontend — Cloudflare Pages

1. https://dash.cloudflare.com → **Workers & Pages → Create → Pages → Connect to Git** → repo `Cacahuate-frontend`.
2. Configuración de build:
   - Framework preset: **React (Vite)**
   - Build command: `npm run build`
   - Output directory: `dist`
   - Variable de entorno: `VITE_API_URL` = `https://<servicio>.onrender.com/api`
3. Deploy → queda en `https://<proyecto>.pages.dev`.

`VITE_API_URL` se incluye en el build: si cambia, hay que volver a desplegar.

## 4. CORS

En Render → `cacahuate-api` → Environment → `Cors__AllowedOrigins` = `https://<proyecto>.pages.dev`
(varios orígenes separados por coma, sin `/` final). Guardar: Render redespliega solo.

## Usuarios demo

Con `Seed__DemoData=true`: `admin1@gmail.com`, `therapist1@gmail.com`, `parent1@gmail.com`
(y therapist2–4, parent2–5), contraseña `Control123`.
**El repo es público, así que esas credenciales también lo son**: para algo más que una prueba,
cambiar las contraseñas o poner `Seed__DemoData=false` en una base nueva.

## Configuración local

Los secretos ya no están en `appsettings.json`. Para desarrollo van en
`Cacahuate.API/appsettings.Development.json` (ignorado por git):

```json
{
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;Port=5432;Database=cacahuate_db;Username=postgres;Password=..." },
  "Jwt": { "Key": "una-clave-de-al-menos-32-caracteres..." },
  "Email": { "Password": "" }
}
```
