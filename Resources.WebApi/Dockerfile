FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Resources.WebApi/Resources.WebApi.csproj", "Resources.WebApi/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Resources.WebApi/Resources.WebApi.csproj"
COPY . .
WORKDIR "/src/Resources.WebApi"
RUN dotnet build "Resources.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Resources.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Resources.WebApi.dll"]
