FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SupportTicketWebApi/SupportTicketWebApi.csproj", "SupportTicketWebApi/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "SupportTicketWebApi/SupportTicketWebApi.csproj"
COPY . .
WORKDIR "/src/SupportTicketWebApi"
RUN dotnet build "SupportTicketWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SupportTicketWebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportTicketWebApi.dll"]
