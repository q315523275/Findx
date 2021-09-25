#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 8888

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/Findx.WebHost/Findx.WebHost.csproj", "src/Findx.WebHost/"]
COPY ["src/Findx.Discovery.Consul/Findx.Discovery.Consul.csproj", "src/Findx.Discovery.Consul/"]
COPY ["src/Findx.Discovery/Findx.Discovery.csproj", "src/Findx.Discovery/"]
COPY ["src/Findx.Common/Findx.csproj", "src/Findx.Common/"]
COPY ["src/Findx.EventBus.RabbitMQ/Findx.EventBus.RabbitMQ.csproj", "src/Findx.EventBus.RabbitMQ/"]
COPY ["src/Findx.EventBus/Findx.EventBus.csproj", "src/Findx.EventBus/"]
COPY ["src/Findx.RabbitMQ/Findx.RabbitMQ.csproj", "src/Findx.RabbitMQ/"]
COPY ["src/Findx.Mapster/Findx.Mapster.csproj", "src/Findx.Mapster/"]
COPY ["src/Findx.WebApiClient/Findx.WebApiClient.csproj", "src/Findx.WebApiClient/"]
COPY ["src/Findx.NLog/Findx.NLog.csproj", "src/Findx.NLog/"]
COPY ["src/Findx.Security/Findx.Security.csproj", "src/Findx.Security/"]
COPY ["src/Findx.AspNetCore/Findx.AspNetCore.csproj", "src/Findx.AspNetCore/"]
COPY ["src/Findx.WebSocketCore/Findx.WebSocketCore.csproj", "src/Findx.WebSocketCore/"]
COPY ["src/Findx.MailKit/Findx.MailKit.csproj", "src/Findx.MailKit/"]
COPY ["src/Findx.Redis/Findx.Redis.csproj", "src/Findx.Redis/"]
COPY ["src/Findx.Swagger/Findx.Swagger.csproj", "src/Findx.Swagger/"]
COPY ["src/Findx.DinkToPdf/Findx.DinkToPdf.csproj", "src/Findx.DinkToPdf/"]
COPY ["src/Findx.SqlSugar/Findx.SqlSugar.csproj", "src/Findx.SqlSugar/"]
COPY ["src/Findx.ImageSharp/Findx.ImageSharp.csproj", "src/Findx.ImageSharp/"]
RUN dotnet restore "src/Findx.WebHost/Findx.WebHost.csproj"
COPY . .
WORKDIR "/src/src/Findx.WebHost"
RUN dotnet build "Findx.WebHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Findx.WebHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Findx.WebHost.dll"]