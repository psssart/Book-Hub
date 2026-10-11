# Project Setup on Linux Server

Production runs from prebuilt images, the project is **not cloned** on the server:
* `app` — pulled from GHCR (`ghcr.io/psssart/book-hub-app:<tag>`), built and pushed by CI
* `sql` — Postgres 16 + PostGIS, built on the server from `db.Dockerfile`
* `migrator` — optional, the app applies EF migrations itself on startup

CI (`.github/workflows/build-and-deploy.yml`) deploys every push to `main` into
`APP_DIR=/home/deploy/apps/BookHub/BookHub`, so the server uses the same directory
(or change `APP_DIR` in the workflow).

Request flow: `Browser → Cloudflare (DNS / proxy) → nginx :443 → 127.0.0.1:8080 (app) → sql (docker network)`

Final layout of the app directory:
```
/home/deploy/apps/BookHub/BookHub/
├── docker-compose.prod.yml   # copied from the repo, do not rename (CI uses this name)
├── db.Dockerfile             # copied from the repo (CI also syncs it)
├── .env                      # secrets, chmod 600
├── app_data_protection/      # ASP.NET data protection keys, owned by uid 64198
└── artifacts/                # efbundle + checksum, uploaded by CI
```

## Automatic setup (`server-setup.sh`)
The script does all steps below (packages, deploy user, files, `.env`, certificate, nginx, systemd, checks).
It is idempotent — safe to run again on an already configured server.

Before running: add the DNS record (step 5) and create a GitHub PAT with `read:packages`
(the GHCR package is private).
```bash
# from your machine
scp BookHub/server-setup.sh root@<server>:/root/
# on the server
echo '<PAT>' > /root/ghcr.token && chmod 600 /root/ghcr.token
sudo /root/server-setup.sh --email you@example.com --ghcr-token-file /root/ghcr.token \
  --ssh-pubkey /root/github-actions.pub   # optional: key for CI deploy
```
* `docker-compose.prod.yml`, `db.Dockerfile` and `.env.example` are taken next to the script or downloaded from GitHub.
* `.env`: an existing one is kept; otherwise `--env-file` is copied or `.env.example` is used.
  Example secrets are replaced with generated ones (`POSTGRES_PASSWORD` only for a new DB volume, `JWT__Key`),
  `ConnectionStrings__DefaultConnection` is always rebuilt from `POSTGRES_*`.
  SMTP settings must be filled in manually.
* `APP_IMAGE_TAG`: `--tag`, otherwise the current one from `.env`, otherwise the newest `main` commit that has an image.
* An existing certificate covering the domain (`bookhub.null-land.org` or `*.null-land.org`) is reused,
  otherwise a separate one is requested.
* nginx: `/etc/nginx/sites-available/<domain>` is overwritten (backup `*.bak.<date>` next to it),
  plus `/etc/nginx/conf.d/bookhub.conf` (rate limit zone + WebSocket map with unique names).
* Options: `./server-setup.sh --help`.

## Setup Manually
### 1. Prepare the app directory
   ```bash
   mkdir -p /home/deploy/apps/BookHub/BookHub/artifacts
   cd /home/deploy/apps/BookHub/BookHub
   # the app container runs as uid 64198 with a read-only filesystem
   mkdir -p app_data_protection && sudo chown 64198:64198 app_data_protection
   ```
### 2. Copy compose and DB image files (from your machine)
   ```bash
   scp BookHub/docker-compose.prod.yml BookHub/db.Dockerfile \
     deploy@<server>:/home/deploy/apps/BookHub/BookHub/
   ```
   * `db.Dockerfile` is needed because the `sql` image is built on the server.
   * Keep the name `docker-compose.prod.yml` — the CI deploy step calls it with `-f docker-compose.prod.yml`.
   * The app port is bound to localhost only (`127.0.0.1:8080:8080`) — don't change it to `8080:8080`,
     otherwise the app is reachable from the internet on `http://<server-ip>:8080`, bypassing nginx (Docker bypasses ufw).
### 3. Create secrets (`.env` file) like `.env.example` next to the `docker-compose.prod.yml` file
   * `APP_IMAGE_TAG` must be an existing image tag: `sha-<full 40-char commit sha>`,
     e.g. `sha-a5b68c9026a12018e30e60ac1732c94b9392c718`.
     CI does not push `latest`. Tags like `sha256-<digest>.sig` are Cosign signatures, not images.
     Get the hash with `git log -1 --format=%H origin/main` and check that it exists:
   ```bash
   docker pull ghcr.io/psssart/book-hub-app:sha-<commit sha>
   ```
   * Replace all example secrets (`POSTGRES_PASSWORD` + the password in `ConnectionStrings__DefaultConnection`, `JWT__Key`).
     Postgres sets the password only when the volume is created — changing `.env` later does not change it.
   * Protect the file:
   ```bash
   chmod 600 .env
   ```
   * If the GHCR package is private, login once (PAT with `read:packages`) as the user that runs the service (root):
   ```bash
   sudo docker login ghcr.io -u psssart
   ```
### 4. Run as a systemd service (auto-start on server boot)
   Create `/etc/systemd/system/bookhub.service`:
   ```ini
   [Unit]
   Description=BookHub (docker compose)
   Requires=docker.service
   After=docker.service network-online.target
   Wants=network-online.target

   [Service]
   Type=oneshot
   RemainAfterExit=yes
   WorkingDirectory=/home/deploy/apps/BookHub/BookHub
   ExecStart=/usr/bin/docker compose -f docker-compose.prod.yml up -d sql app
   ExecStop=/usr/bin/docker compose -f docker-compose.prod.yml stop
   TimeoutStartSec=0

   [Install]
   WantedBy=multi-user.target
   ```
   Enable and start it (check the docker path with `which docker`):
   ```bash
   sudo systemctl enable docker
   sudo systemctl daemon-reload
   sudo systemctl enable --now bookhub
   systemctl status bookhub
   ```
   * `migrator` is not started by the service — the app migrates the DB on startup,
     and CI runs the migrator when the EF bundle changes.
   * `ExecStop` uses `stop` (not `down`), containers and the DB volume are kept.
   * The first start builds the `sql` image and pulls the app image, the first app start seeds the DB.

   Check that the app answers locally:
   ```bash
   docker compose -f docker-compose.prod.yml ps -a   # app: 127.0.0.1:8080->8080, sql: healthy
   curl -s http://127.0.0.1:8080/health              # Healthy
   ```
### 5. DNS (Cloudflare)
   * The domain uses Cloudflare nameservers, so records are added **only in Cloudflare**
     (DNS records in Namecheap are ignored).
   * Add an `A` record `bookhub` → server IP.
   * If the record is proxied (orange cloud), SSL/TLS mode must be **Full (strict)** —
     `Flexible` causes an endless redirect loop with the nginx HTTP → HTTPS redirect.
   * Check:
   ```bash
   getent hosts bookhub.null-land.org   # must match the server IP (or Cloudflare IPs if proxied)
   ```
### 6. TLS certificate
   The main `null-land.org` config already serves `/.well-known/acme-challenge/` on port 80 for `*.null-land.org`.
   Check that the certificate covers the subdomain:
   ```bash
   sudo certbot certificates
   ```
   If `bookhub.null-land.org` (or `*.null-land.org`) is not listed, expand it — list **all** existing domains plus the new one
   (works only after DNS from step 5 resolves):
   ```bash
   sudo certbot certonly --webroot -w /var/www/letsencrypt --cert-name null-land.org --expand \
     -d null-land.org -d www.null-land.org -d bookhub.null-land.org
   ```
### 7. nginx reverse proxy
   Create `/etc/nginx/sites-available/bookhub.null-land.org`:
   ```nginx
   # WebSocket upgrade for SignalR (define the map only once per nginx)
   map $http_upgrade $connection_upgrade {
       default upgrade;
       ''      close;
   }

   # HTTP → HTTPS and ACME challenge are handled by the main null-land.org config (*.null-land.org)

   server {
       listen 443 ssl http2;
       server_name bookhub.null-land.org;

       # certificate + dhparam are included here
       include snippets/ssl-params.conf;

       # avatars up to 2 MB, nginx default is 1 MB
       client_max_body_size 10m;

       location / {
           limit_req zone=perip burst=40 nodelay;

           proxy_pass         http://127.0.0.1:8080;
           proxy_http_version 1.1;
           proxy_set_header   Host              $host;
           proxy_set_header   X-Real-IP         $remote_addr;
           proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
           proxy_set_header   X-Forwarded-Proto $scheme;
           proxy_set_header   Upgrade           $http_upgrade;
           proxy_set_header   Connection        $connection_upgrade;
           proxy_read_timeout 90;
       }

       # SignalR (/hubs/discussion): long-lived connections, no rate limit
       location /hubs/ {
           proxy_pass         http://127.0.0.1:8080;
           proxy_http_version 1.1;
           proxy_set_header   Host              $host;
           proxy_set_header   X-Forwarded-For   $proxy_add_x_forwarded_for;
           proxy_set_header   X-Forwarded-Proto $scheme;
           proxy_set_header   Upgrade           $http_upgrade;
           proxy_set_header   Connection        $connection_upgrade;
           proxy_read_timeout 1h;
           proxy_send_timeout 1h;
       }
   }
   ```
   Enable it:
   ```bash
   sudo ln -s /etc/nginx/sites-available/bookhub.null-land.org /etc/nginx/sites-enabled/
   sudo nginx -t && sudo systemctl reload nginx
   ```
   Notes:
   * nginx older than 1.25.1 does not know `http2 on;` — use `listen 443 ssl http2;` (check with `nginx -v`).
   * If `snippets/ssl-params.conf` does not contain `ssl_certificate`/`ssl_dhparam`, add them to the server block.
     Do not duplicate `ssl_dhparam` — nginx fails with `directive is duplicate`.
   * Do not include `snippets/security-headers.conf` blindly: the app already sends HSTS and `X-Frame-Options`,
     and a strict `Content-Security-Policy` breaks Bootstrap / jQuery / SignalR loaded from CDN.
   * `X-Forwarded-Proto` is trusted by the app because `.env` has `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`.
   * When pasting the config over SSH, long lines can get cut off or contain non-breaking spaces
     (`invalid number of arguments in "proxy_set_header"`). Find broken lines:
   ```bash
   F=/etc/nginx/sites-available/bookhub.null-land.org
   grep -nvE '(;|\{|\}|^\s*#.*|^\s*)$' "$F"   # lines without ; { }
   sudo sed -i 's/\xC2\xA0/ /g' "$F"          # replace non-breaking spaces
   ```
### 8. Verify
   ```bash
   curl -sI http://127.0.0.1:8080/ -H 'X-Forwarded-Proto: https'                               # app directly
   curl -skI https://bookhub.null-land.org/ --resolve bookhub.null-land.org:443:127.0.0.1    # nginx, bypassing DNS/cert
   curl -sI https://bookhub.null-land.org/                                                     # full chain
   echo | openssl s_client -connect 127.0.0.1:443 -servername bookhub.null-land.org 2>/dev/null \
     | openssl x509 -noout -subject -ext subjectAltName                                        # certificate names
   ```
   If the first two answer `200` but the last one fails — the problem is DNS or the certificate (steps 5–6).

## Security checklist
* Postgres is bound to `127.0.0.1:5432` — not reachable from the internet, the app connects via the `bookhub-net` network.
* App port is `127.0.0.1:8080` — only nginx can reach it (check `docker compose -f docker-compose.prod.yml ps`).
* `.env` has real secrets and `chmod 600`.
* Production `SeedData.Users` is empty (`appsettings.Production.json`) — no default accounts are created.
* Check from outside (e.g. phone on mobile data) that `http://<server-ip>:8080/health` does **not** respond.

## Service commands
   ```bash
   sudo systemctl restart bookhub
   sudo systemctl stop bookhub
   systemctl status bookhub
   journalctl -u bookhub
   ```

## Updating
* Automatically: push to `main` — CI builds the image, uploads the EF bundle and `db.Dockerfile`,
  sets `APP_IMAGE_TAG` in `.env`, runs migrations if the bundle changed and restarts `app`.
  After a server reboot the service starts the last deployed version.
* Manually: set `APP_IMAGE_TAG` in `.env` to the new tag, then
   ```bash
   docker compose -f docker-compose.prod.yml pull app
   docker compose -f docker-compose.prod.yml up -d app
   ```
* Manual migrations (optional), requires `artifacts/efbundle` uploaded by CI:
   ```bash
   docker compose -f docker-compose.prod.yml run --rm migrator
   ```

## Useful commands
### 1. Check Logs
* All containers
   ```bash
   docker compose -f docker-compose.prod.yml ps -a
   ```
* App
   ```bash
   docker compose -f docker-compose.prod.yml logs -f app
   ```
* Migrator
   ```bash
   docker compose -f docker-compose.prod.yml logs -f migrator
   ```
* nginx
   ```bash
   sudo tail -f /var/log/nginx/error.log
   ```
* Resource consumption
  ```bash
  docker stats
  ```
* Inspect docker network
  ```bash
  docker network inspect bookhub_bookhub-net
  ```

### 2. Reset
* Stop containers (service)
  ```bash
  sudo systemctl stop bookhub
  ```
* Stop and remove containers
  ```bash
  docker compose -f docker-compose.prod.yml down
  ```
* Stop and delete containers, networks, images, and volumes from a project (**deletes the database**)
  ```bash
  docker compose -f docker-compose.prod.yml down --rmi all --volumes
  ```
* Remove dangling resources
  ```bash
  docker system prune -f
  ```
* Delete all unused, including volumes (**deletes the database if containers are stopped**)
  ```bash
  docker system prune -a --volumes -f
  ```
* Clear build cache
  ```bash
  docker builder prune -f
  ```
