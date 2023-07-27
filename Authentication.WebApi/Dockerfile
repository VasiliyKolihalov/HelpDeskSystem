FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Authentication.WebApi/Authentication.WebApi.csproj", "Authentication.WebApi/"]
COPY ["Authentication.Infrastructure/Authentication.Infrastructure.csproj", "Authentication.Infrastructure/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Authentication.WebApi/Authentication.WebApi.csproj"
COPY . .
WORKDIR "/src/Authentication.WebApi"
RUN dotnet build "Authentication.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Authentication.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Authentication.WebApi.dll"]
