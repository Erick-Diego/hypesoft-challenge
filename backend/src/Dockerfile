FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Hypesoft.API/Hypesoft.API.csproj", "Hypesoft.API/"]
COPY ["src/Hypesoft.Application/Hypesoft.Application.csproj", "Hypesoft.Application/"]
COPY ["src/Hypesoft.Domain/Hypesoft.Domain.csproj", "Hypesoft.Domain/"]
COPY ["src/Hypesoft.Infrastructure/Hypesoft.Infrastructure.csproj", "Hypesoft.Infrastructure/"]

RUN dotnet restore "Hypesoft.API/Hypesoft.API.csproj"

COPY src/ .

WORKDIR "/src/Hypesoft.API"
RUN dotnet build "Hypesoft.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Hypesoft.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 80

RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Hypesoft.API.dll"]