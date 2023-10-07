FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["RabbitMqInitializer/RabbitMqInitializer.csproj", "RabbitMqInitializer/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "RabbitMqInitializer/RabbitMqInitializer.csproj"
COPY . .
WORKDIR "/src/RabbitMqInitializer"
RUN dotnet build "RabbitMqInitializer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RabbitMqInitializer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RabbitMqInitializer.dll"]
