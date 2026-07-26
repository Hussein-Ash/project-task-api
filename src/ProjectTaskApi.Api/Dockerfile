FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/ProjectTaskApi.Domain/*.csproj src/ProjectTaskApi.Domain/
COPY src/ProjectTaskApi.Application/*.csproj src/ProjectTaskApi.Application/
COPY src/ProjectTaskApi.Infrastructure/*.csproj src/ProjectTaskApi.Infrastructure/
COPY src/ProjectTaskApi.Api/*.csproj src/ProjectTaskApi.Api/
RUN dotnet restore src/ProjectTaskApi.Api/ProjectTaskApi.Api.csproj

COPY src/ src/
RUN dotnet publish src/ProjectTaskApi.Api/ProjectTaskApi.Api.csproj \
    --configuration Release --no-restore --output /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProjectTaskApi.Api.dll"]
