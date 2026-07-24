FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TaxManager.Classic.slnx ./
COPY src/TaxManager.Domain/TaxManager.Domain.csproj src/TaxManager.Domain/
COPY src/TaxManager.Application/TaxManager.Application.csproj src/TaxManager.Application/
COPY src/TaxManager.Infrastructure/TaxManager.Infrastructure.csproj src/TaxManager.Infrastructure/
COPY src/TaxManager.Api/TaxManager.Api.csproj src/TaxManager.Api/
RUN dotnet restore src/TaxManager.Api/TaxManager.Api.csproj

COPY src/ src/
RUN dotnet publish src/TaxManager.Api/TaxManager.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaxManager.Api.dll"]
