﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Users.Api/Users.Api.csproj", "Users.Api/"]
COPY ["Infrastructure.Authentication/Infrastructure.Authentication.csproj", "Infrastructure.Authentication/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Users.Api/Users.Api.csproj"
COPY . .
WORKDIR "/src/Users.Api"
RUN dotnet build "Users.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Users.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Users.Api.dll"]
