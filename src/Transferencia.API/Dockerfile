FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish Transferencia.API/Transferencia.API.csproj -c Release -o /out

FROM base AS final_transferencia
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Transferencia.API.dll"]