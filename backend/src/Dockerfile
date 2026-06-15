FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY *.sln .
COPY todoApp.Core/*.csproj ./todoApp.Core/
COPY todoApp.Data/*.csproj ./todoApp.Data/
COPY *Web*/*.csproj ./todoApp.Web/
COPY *UnitTests*/*.csproj ./todoApp.UnitTests/
COPY *DI*/*.csproj ./todoApp.DI/
RUN dotnet restore

COPY . .
WORKDIR /src/todoApp.Web
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS migrations
WORKDIR /src

COPY --from=build /src/. .

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir -p /app/data

WORKDIR /src/todoApp.Web

RUN dotnet ef migrations add InitialCreate --context AppDbContext -o Migrations || true
RUN dotnet ef database update --context AppDbContext || true

RUN cp /src/todoApp.Web/*.db* /app/data/ 2>/dev/null || true

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

RUN apt-get update && \
    apt-get install -y curl libsqlite3-0 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

COPY --from=migrations /app/data/todos.db* ./data/ 

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "todoApp.Web.dll"]