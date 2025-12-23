FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HSEGozon.OrdersService/HSEGozon.OrdersService.csproj", "src/HSEGozon.OrdersService/"]
RUN dotnet restore "src/HSEGozon.OrdersService/HSEGozon.OrdersService.csproj"
COPY . .
WORKDIR "/src/src/HSEGozon.OrdersService"
RUN dotnet build "HSEGozon.OrdersService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/src/HSEGozon.OrdersService"
RUN dotnet publish "HSEGozon.OrdersService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HSEGozon.OrdersService.dll"]
