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

# Mudou aqui: porta 5000 em vez de 80
EXPOSE 5000

RUN mkdir -p /app/logs

# Define a porta 5000 como padrão
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["dotnet", "Hypesoft.API.dll"]