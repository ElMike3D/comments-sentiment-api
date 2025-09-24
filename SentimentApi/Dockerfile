# Usamos la imagen oficial de .NET SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos csproj y restauramos dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiamos todo el código y hacemos build
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Imagen runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Exponemos puerto
EXPOSE 80

# Comando por defecto
ENTRYPOINT ["dotnet", "SentimentApi.dll"]
