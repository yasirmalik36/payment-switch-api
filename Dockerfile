FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PaymentSwitch.API/PaymentSwitch.API.csproj", "src/PaymentSwitch.API/"]
COPY ["src/PaymentSwitch.Application/PaymentSwitch.Application.csproj", "src/PaymentSwitch.Application/"]
COPY ["src/PaymentSwitch.Domain/PaymentSwitch.Domain.csproj", "src/PaymentSwitch.Domain/"]
COPY ["src/PaymentSwitch.Infrastructure/PaymentSwitch.Infrastructure.csproj", "src/PaymentSwitch.Infrastructure/"]
RUN dotnet restore "src/PaymentSwitch.API/PaymentSwitch.API.csproj"
COPY . .
RUN dotnet build "src/PaymentSwitch.API/PaymentSwitch.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/PaymentSwitch.API/PaymentSwitch.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentSwitch.API.dll"]
