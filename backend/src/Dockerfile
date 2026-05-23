
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.sln .
COPY todoApp.Core/*.csproj ./todoApp.Core/
COPY todoApp.Data/*.csproj ./todoApp.Data/
COPY todoApp.Web/*.csproj ./todoApp.Web/
RUN dotnet restore

COPY . .
WORKDIR /src/todoApp.Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y libsqlite3-0 sqlite3 && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY --from=build /src/todoApp.Web/todos.db ./todos.db 2>/dev/null || true


EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "todoApp.Web.dll"]