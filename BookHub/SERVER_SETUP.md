# Project Setup on Linux Server

1. Clone the project into `./opt/bookhub/`
   ```bash
   git clone https://github.com/psssart/Book-Hub
   ```

2. Create secrets (`.env` file) like `.env.example` next to the `docker-compose` file

3. EF-bundle
   * Build
   ```bash
   docker run --rm -t \
   -v "$PWD":/src -w /src \
   mcr.microsoft.com/dotnet/sdk:8.0 \
   bash -lc '
   set -euo pipefail
   export PATH="$PATH:/root/.dotnet/tools"
   dotnet tool install --global dotnet-ef --version 8.*
   dotnet restore
   dotnet build WebApp/WebApp.csproj -c Release
   dotnet ef migrations bundle \
   --project App.DAL.EF/App.DAL.EF.csproj \
   --startup-project WebApp/WebApp.csproj \
   --configuration Release \
   --force \
   -o ./artifacts/efbundle
   chmod +x ./artifacts/efbundle
   '
   ```
   * Delete
   ```bash
   rm -f artifacts/efbundle
   ```

4. First deployment
   ```bash
   # from the project root
   docker compose -f docker-compose.prod.yml build # --pull --no-cache ap
   docker compose -f docker-compose.prod.yml up -d sql
   # run migrations
   docker compose -f docker-compose.prod.yml run --rm migrator
   # start the application
   docker compose -f docker-compose.prod.yml up -d app
   docker compose -f docker-compose.prod.yml ps
   ```

5. Updating without pain
   ```bash
   git pull
   docker compose -f docker-compose.prod.yml build app
   docker compose -f docker-compose.prod.yml up -d app
   ```

6. Check Logs
   * Containers
   >docker compose -f docker-compose.prod.yml ps
   * App
   >docker compose -f docker-compose.prod.yml logs -f app
   * Migrator
   >docker compose -f docker-compose.prod.yml logs -f migrator

7. Reset
   * Remove containers
   ```bash
   docker compose -f docker-compose.prod.yml down
   ```
   * Remove containers AND sql-data volume
   ```bash
   docker compose -f docker-compose.prod.yml down -v
   ```
