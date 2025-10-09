# Project Setup on Linux Server

1. Clone the project into `./opt/bookhub/`
   ```bash
   git clone https://github.com/psssart/Book-Hub
   ```

2. Create secret files next to the `docker-compose` file
   * `.env` file:
   ```
   POSTGRES_PASSWORD=strong_password
   SMTP_PASSWORD=secret_smtp_password
   JWT_KEY=very_long_jwt_key
   ```

3. First deployment
   ```bash
   # from the project root
   docker compose -f docker-compose.prod.yml build
   docker compose -f docker-compose.prod.yml up -d sql
   # run migrations
   docker compose -f docker-compose.prod.yml run --rm migrator
   # start the application
   docker compose -f docker-compose.prod.yml up -d app
   docker compose -f docker-compose.prod.yml ps
   ```

4. Updating without pain
   ```bash
   git pull
   docker compose -f docker-compose.prod.yml build app
   docker compose -f docker-compose.prod.yml up -d app
   ```

5. Full reset
   ```bash
   docker compose -f docker-compose.prod.yml down -v   # removes containers AND sql-data volume
   ```
