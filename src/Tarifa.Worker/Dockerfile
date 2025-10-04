FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o csproj (contexto já é ../src)
COPY Tarifa.Worker/Tarifa.Worker.csproj Tarifa.Worker/
RUN dotnet restore Tarifa.Worker/Tarifa.Worker.csproj

# Copia todo o código (tudo que está dentro de src/)
COPY . .

WORKDIR "/src/Tarifa.Worker"
RUN dotnet build Tarifa.Worker.csproj -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Tarifa.Worker.csproj -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tarifa.Worker.dll"]