#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["InformationSystems.API/InformationSystems.API.csproj", "InformationSystems.API/"]
RUN dotnet restore "InformationSystems.API/InformationSystems.API.csproj"
COPY . .
WORKDIR "/src/InformationSystems.API"
RUN dotnet build "InformationSystems.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InformationSystems.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InformationSystems.API.dll"]
