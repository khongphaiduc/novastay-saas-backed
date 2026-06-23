FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NovaStay.sln ./
COPY NovaStay.API/NovaStay.API.csproj NovaStay.API/
COPY NovaStay.Application/NovaStay.Application.csproj NovaStay.Application/
COPY NovaStay.Domain/NovaStay.Domain.csproj NovaStay.Domain/
COPY NovaStay.Infrastructure/NovaStay.Infrastructure.csproj NovaStay.Infrastructure/
COPY NovaStay.UnitTests/NovaStay.UnitTests.csproj NovaStay.UnitTests/

RUN dotnet restore NovaStay.sln

COPY . .

FROM build AS test
RUN dotnet test NovaStay.UnitTests/NovaStay.UnitTests.csproj \
    --configuration Release \
    --no-restore

FROM build AS publish
RUN dotnet publish NovaStay.API/NovaStay.API.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NovaStay.API.dll"]
