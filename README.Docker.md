# TazaOrda Docker Deployment

## Prerequisites
- Docker
- Docker Compose

## Quick Start

1. Build and run all services:
```bash
docker-compose up -d
```

2. Stop all services:
```bash
docker-compose down
```

3. View logs:
```bash
docker-compose logs -f api
```

## Services

- **API**: ASP.NET Core Web API (Port 8080)
- **PostgreSQL**: Database (Port 5432)
- **MinIO**: Object storage (Port 9000, Console 9001)

## Access Points

- API: http://localhost:8080
- MinIO Console: http://localhost:9001 (admin/password)
- PostgreSQL: localhost:5432

## Environment Variables

Update the following in `docker-compose.yml` for production:
- `Jwt__SecretKey`: Change to a secure random string
- `POSTGRES_PASSWORD`: Change to a secure password
- `MINIO_ROOT_PASSWORD`: Change to a secure password

## Database Migrations

Run migrations after first start:
```bash
docker-compose exec api dotnet ef database update
```

## Rebuild After Code Changes

```bash
docker-compose up -d --build api
```
