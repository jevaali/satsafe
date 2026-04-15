#FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY global.json .
COPY Base.Contracts/*.csproj ./Base.Contracts/
COPY Base.Domain/*.csproj ./Base.Domain/
COPY Base.Helpers/*.csproj ./Base.Helpers/
COPY App.Domain/*.csproj ./App.Domain/
COPY App.Application/*.csproj ./App.Application/
COPY App.Infrastructure/*.csproj ./App.Infrastructure/
COPY App.Resources/*.csproj ./App.Resources/
COPY WebApp/*.csproj ./WebApp/
RUN dotnet restore

# copy everything else and build app
COPY Base.Contracts/. ./Base.Contracts/
COPY Base.Domain/. ./Base.Domain/
COPY Base.Helpers/. ./Base.Helpers/
COPY App.Domain/. ./App.Domain/
COPY App.Application/. ./App.Application/
COPY App.Infrastructure/. ./App.Infrastructure/
COPY App.Resources/. ./App.Resources/
COPY WebApp/. ./WebApp/
WORKDIR /app/WebApp
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/WebApp/out ./
ENTRYPOINT ["dotnet", "WebApp.dll"]
