FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Создаем папки заранее
RUN mkdir -p MyFormixApp.Domain MyFormixApp.Application MyFormixApp.Infrastructure MyFormixApp.UI

# Копируем csproj-файлы
COPY MyFormixApp.Domain/MyFormixApp.Domain.csproj MyFormixApp.Domain/
COPY MyFormixApp.Application/MyFormixApp.Application.csproj MyFormixApp.Application/
COPY MyFormixApp.Infrastructure/MyFormixApp.Infrastructure.csproj MyFormixApp.Infrastructure/
COPY MyFormixApp.UI/MyFormixApp.UI.csproj MyFormixApp.UI/


# Восстанавливаем зависимости
RUN dotnet restore MyFormixApp.UI/MyFormixApp.UI.csproj

# Копируем остальное
COPY . .

WORKDIR /src/MyFormixApp.UI
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MyFormixApp.UI.dll"]
