# build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY WebApp/*.csproj WebApp/
COPY App.BLL/*.csproj App.BLL/
COPY App.BLL.DTO/*.csproj App.BLL.DTO/
COPY App.Contracts.BLL/*.csproj App.Contracts.BLL/
COPY App.Contracts.DAL/*.csproj App.Contracts.DAL/
COPY App.DAL.DTO/*.csproj App.DAL.DTO/
COPY App.DAL.EF/*.csproj App.DAL.EF/
COPY App.DTO/*.csproj App.DTO/
COPY App.Domain/*.csproj App.Domain/
COPY Base.BLL/*.csproj Base.BLL/
COPY Base.Contracts.BLL/*.csproj Base.Contracts.BLL/
COPY Base.Contracts.DAL/*.csproj Base.Contracts.DAL/
COPY Base.Contracts.Domain/*.csproj Base.Contracts.Domain/
COPY Base.DAL.EF/*.csproj Base.DAL.EF/
COPY Base.Domain/*.csproj Base.Domain/
COPY Helpers/*.csproj Helpers/

RUN dotnet restore WebApp/WebApp.csproj \
    -r linux-x64 \
    -p:PublishReadyToRun=true

COPY . .

RUN dotnet publish WebApp/WebApp.csproj -c Release -o /app/publish \
    -r linux-x64 \
    /p:UseAppHost=false \
    /p:PublishReadyToRun=true \
    --no-restore

# runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

COPY --chown=64198:64198 --from=build /app/publish ./
USER 64198:64198

EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApp.dll"]
