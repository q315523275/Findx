#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 10020

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
#WORKDIR /src
COPY . .
#COPY ["modules/Findx.Admin.WebHost/Findx.Admin.WebHost.csproj", "modules/Findx.Admin.WebHost/"]
#COPY ["src/Findx.ImageSharp/Findx.ImageSharp.csproj", "src/Findx.ImageSharp/"]
#COPY ["src/Findx.Common/Findx.csproj", "src/Findx.Common/"]
#COPY ["src/Findx.NLog/Findx.NLog.csproj", "src/Findx.NLog/"]
#COPY ["src/Findx.Swagger/Findx.Swagger.csproj", "src/Findx.Swagger/"]
#COPY ["src/Findx.AspNetCore/Findx.AspNetCore.csproj", "src/Findx.AspNetCore/"]
#COPY ["src/Findx.WebSocketCore/Findx.WebSocketCore.csproj", "src/Findx.WebSocketCore/"]
#COPY ["modules/Findx.Module.EleAdmin/Findx.Module.EleAdmin.csproj", "modules/Findx.Module.EleAdmin/"]
#COPY ["src/Findx.FreeSql/Findx.FreeSql.csproj", "src/Findx.FreeSql/"]
#COPY ["src/Findx.MailKit/Findx.MailKit.csproj", "src/Findx.MailKit/"]
#COPY ["src/Findx.Security/Findx.Security.csproj", "src/Findx.Security/"]
#COPY ["src/Findx.Mapster/Findx.Mapster.csproj", "src/Findx.Mapster/"]
#RUN dotnet restore "modules/Findx.Admin.WebHost/Findx.Admin.WebHost.csproj"
#COPY . .
#WORKDIR "/src/modules/Findx.Admin.WebHost"
#RUN dotnet build "Findx.Admin.WebHost.csproj" -c Release -o /app/build
#
#FROM build AS publish
#RUN dotnet publish "Findx.Admin.WebHost.csproj" -c Release -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Findx.Admin.WebHost.dll"]
