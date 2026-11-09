# ---------- Stage 1: build and publish ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy only the project file first so the restore layer is cached
COPY raceday_mvc.csproj ./
RUN dotnet restore

# copy the rest of the source and publish a release build
COPY . ./
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Stage 2: small runtime image ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./

# the app listens on 8080 and finds the API through ApiSettings__BaseUrl
# (host.docker.internal = the computer Docker is running on, where the API runs)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ApiSettings__BaseUrl=http://host.docker.internal:5080
EXPOSE 8080

# do not run as root
USER $APP_UID

ENTRYPOINT ["dotnet", "raceday_mvc.dll"]
