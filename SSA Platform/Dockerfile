FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all project files for restore (maximises layer cache)
COPY "SSA Platform.sln" .
COPY global.json .
COPY Base.Contracts/Base.Contracts.csproj           Base.Contracts/
COPY Base.Domain/Base.Domain.csproj                 Base.Domain/
COPY Base.Helpers/Base.Helpers.csproj               Base.Helpers/
COPY App.Domain/App.Domain.csproj                   App.Domain/
COPY App.Application/App.Application.csproj         App.Application/
COPY App.Infrastructure/App.Infrastructure.csproj   App.Infrastructure/
COPY App.Resources/App.Resources.csproj             App.Resources/
COPY WebApp/WebApp.csproj                           WebApp/
RUN dotnet restore "WebApp/WebApp.csproj"

# Copy everything else and publish
COPY . .
RUN dotnet publish "WebApp/WebApp.csproj" -c Release -o /app/publish --no-restore


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApp.dll"]