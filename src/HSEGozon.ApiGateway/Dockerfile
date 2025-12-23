FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HSEGozon.ApiGateway/HSEGozon.ApiGateway.csproj", "src/HSEGozon.ApiGateway/"]
RUN dotnet restore "src/HSEGozon.ApiGateway/HSEGozon.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/HSEGozon.ApiGateway"
RUN dotnet build "HSEGozon.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/src/HSEGozon.ApiGateway"
RUN dotnet publish "HSEGozon.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HSEGozon.ApiGateway.dll"]
