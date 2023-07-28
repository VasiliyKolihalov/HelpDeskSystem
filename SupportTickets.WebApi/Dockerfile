FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SupportTickets.WebApi/SupportTickets.WebApi.csproj", "SupportTickets.WebApi/"]
COPY ["Authentication.Infrastructure/Authentication.Infrastructure.csproj", "Authentication.Infrastructure/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "SupportTickets.WebApi/SupportTickets.WebApi.csproj"
COPY . .
WORKDIR "/src/SupportTickets.WebApi"
RUN dotnet build "SupportTickets.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SupportTickets.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportTickets.WebApi.dll"]
