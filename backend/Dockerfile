FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TableOrder.Backend/TableOrder.Backend.csproj", "src/TableOrder.Backend/"]
RUN dotnet restore "src/TableOrder.Backend/TableOrder.Backend.csproj"
COPY . .
WORKDIR "/src/src/TableOrder.Backend"
RUN dotnet build "TableOrder.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TableOrder.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create self-signed certificate for HTTPS in development
RUN apt-get update && apt-get install -y openssl && rm -rf /var/lib/apt/lists/*
RUN openssl req -x509 -newkey rsa:4096 -keyout /app/key.pem -out /app/cert.pem -days 365 -nodes -subj "/C=US/ST=State/L=City/O=Organization/CN=localhost"

ENTRYPOINT ["dotnet", "TableOrder.Backend.dll"]
