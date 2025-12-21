# MusicStreamingService

Backend for a music streaming platform: users and artists, albums and songs, playlists and likes, playback telemetry, subscriptions/billing, audit logs, and playlist import automation backed by MinIO (S3-compatible storage).

## What’s inside
- Tech: .NET 9 (ASP.NET Core, EF Core + Npgsql), PostgreSQL, MinIO, Docker Compose.
- Domains: users/regions, artists/albums/songs, playlists (likes and songs), streaming events, subscriptions/payments, playlist import tasks and staging, auditing.
- Automation: background `PlaylistImportWorker`, audit logging via `IAuditable` + `AuditLogSaveChangesInterceptor`.

## Run with Docker (recommended)
1) Start stack: `docker compose up --build`  
   - Postgres (5432), MinIO (9000/9001), app container.  
2) Apply migrations (host): `dotnet ef database update --project MusicStreamingService.Data`  
   - Or rely on the app migrator if enabled.

## Local dev (hosted services)
1) Install: .NET 9 SDK, PostgreSQL, Python 3, MinIO/S3-compatible storage.  
2) Configure: connection string + storage creds in `appsettings.Development.json` or env vars (defaults match `env/postgres.env` and `env/minio.env`).  
3) Run: `dotnet run --project MusicStreamingService`

## Configuration
- Database: `DesignDbConnectionString` in `appsettings.json` (and any `ConnectionStrings` entries) should point to Postgres. Docker defaults: `Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres;`.
- Auth: toggle with `AuthEnabled` (`true` in `appsettings.json`, `false` in `appsettings.Development.json`).
- Storage (MinIO/S3): configure via env vars or appsettings. Docker uses `env/minio.env` by default.
- Migrations: `dotnet ef database update --project MusicStreamingService.Data` from host, or let the app migrator handle it.

### Sample environment
```bash
# Postgres (env/postgres.env)
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DATABASE=postgres

# MinIO (env/minio.env)
MINIO_ROOT_USER=user
MINIO_ROOT_PASSWORD=subvev-jyzcyR-bymcy0

# App overrides (env or appsettings.*)
ASPNETCORE_URLS = http://0.0.0.0:5077
ASPNETCORE_ENVIRONMENT = Development
PsqlConfiguration__Host = postgres
PsqlConfiguration__Port = 5432
PsqlConfiguration__Username = postgres
PsqlConfiguration__Password = <secret-key>
PsqlConfiguration__Database = postgres
PasswordServiceConfig__SaltSize = 16
PasswordServiceConfig__IterationsCount = 600000
PasswordServiceConfig__NumBytesRequested = 32
JwtConfiguration__SecretKey = <secret-key>
JwtConfiguration__Issuer = music-streaming-service
JwtConfiguration__Audience = music-streaming-service
JwtConfiguration__AccessTokenExpiration = 04:00:00
JwtConfiguration__RefreshTokenExpiration = 30.00:00:00
MinioConfiguration__Endpoint = minio:9000
MinioConfiguration__AccessKey = user
MinioConfiguration__SecretKey = <secret-key>
MinioConfiguration__ExpireTimeInSeconds = 3600
MinioConfiguration__UseSsl = false
```

## Data & tooling
- Mock relational seed: `python3 scripts/generate_mock_data.py --users 240 --seed 42` → `scripts/mock_data.sql` (albums/songs/playlists/favorites/streams). Scale with `--users`/`--seed`.
- Playlist import sample: `scripts/playlist_import_sample.json` (3,000 entries; includes “Nonexistent Track ####” rows) shaped for `ImportFileContent` used by `PlaylistImportWorker`.

## Playlist import at a glance
1) Upload JSON to object storage and create a `PlaylistImportTask` pointing to it.  
2) Worker reads the file, creates `PlaylistImportStagingEntries`, runs matching, then inserts matched songs into playlists.

## Notes
- Audit logging is enabled for all entities implementing `IAuditable`; `AuditLogEntity` is excluded from self-logging.
