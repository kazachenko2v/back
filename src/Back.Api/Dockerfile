# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Back.Api/Back.Api.csproj", "src/Back.Api/"]
COPY ["src/Back.Application/Back.Application.csproj", "src/Back.Application/"]
COPY ["src/Back.Domain/Back.Domain.csproj", "src/Back.Domain/"]
COPY ["src/Back.Infrastructure/Back.Infrastructure.csproj", "src/Back.Infrastructure/"]
RUN dotnet restore "src/Back.Api/Back.Api.csproj"

COPY . .
RUN dotnet publish "src/Back.Api/Back.Api.csproj" \
    --configuration $BUILD_CONFIGURATION \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Back.Api.dll"]
