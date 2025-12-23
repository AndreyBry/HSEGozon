FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/HSEGozon.PaymentsService/HSEGozon.PaymentsService.csproj", "src/HSEGozon.PaymentsService/"]
RUN dotnet restore "src/HSEGozon.PaymentsService/HSEGozon.PaymentsService.csproj"
COPY . .
WORKDIR "/src/src/HSEGozon.PaymentsService"
RUN dotnet build "HSEGozon.PaymentsService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/src/HSEGozon.PaymentsService"
RUN dotnet publish "HSEGozon.PaymentsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HSEGozon.PaymentsService.dll"]
