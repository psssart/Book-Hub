# Use the same version as your production compose
FROM postgres:16.4

# Install PostGIS binaries
# We use 'postgresql-16-postgis-3' to match the Postgres version
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    postgresql-16-postgis-3 \
    && rm -rf /var/lib/apt/lists/*