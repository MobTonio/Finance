FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проектов
COPY FinanceTracker.ConsoleApp/FinanceTracker.ConsoleApp.csproj ./FinanceTracker.ConsoleApp/
COPY FinanceTracker.Data/FinanceTracker.Data.csproj ./FinanceTracker.Data/

# Восстанавливаем зависимости
RUN dotnet restore FinanceTracker.ConsoleApp/FinanceTracker.ConsoleApp.csproj

# Копируем весь код
COPY . .

# Публикуем
WORKDIR /src/FinanceTracker.ConsoleApp
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV LANG=C.UTF-8
ENTRYPOINT ["dotnet", "FinanceTracker.ConsoleApp.dll"]