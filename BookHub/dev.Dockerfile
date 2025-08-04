# Use the full SDK image to enable hot-reload, build and debugging
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev

# Set working directory inside the container
WORKDIR /src

# Copy only project and solution files first to leverage layer caching
COPY *.sln ./
COPY WebApp/*.csproj ./WebApp/
COPY App.BLL/*.csproj ./App.BLL/
COPY App.BLL.DTO/*.csproj ./App.BLL.DTO/
COPY App.Contracts.BLL/*.csproj ./App.Contracts.BLL/
COPY App.Contracts.DAL/*.csproj ./App.Contracts.DAL/
COPY App.DAL.DTO/*.csproj ./App.DAL.DTO/
COPY App.DAL.EF/*.csproj ./App.DAL.EF/
COPY App.DTO/*.csproj ./App.DTO/
COPY App.Domain/*.csproj ./App.Domain/
COPY App.Test/*.csproj ./App.Test/
COPY Base.BLL/*.csproj ./Base.BLL/
COPY Base.Contracts.BLL/*.csproj ./Base.Contracts.BLL/
COPY Base.Contracts.DAL/*.csproj ./Base.Contracts.DAL/
COPY Base.Contracts.Domain/*.csproj ./Base.Contracts.Domain/
COPY Base.DAL.EF/*.csproj ./Base.DAL.EF/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Tests/*.csproj ./Base.Tests/
COPY Helpers/*.csproj ./Helpers/

# Restore dependencies
RUN dotnet restore

# Settings for development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER=true

# Expose the development ports
EXPOSE 80 443
