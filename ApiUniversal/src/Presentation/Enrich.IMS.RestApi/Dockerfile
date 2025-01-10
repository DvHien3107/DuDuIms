FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY ./bin/Release/net6.0/publish ./

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /App
COPY --from=build-env /App .

ENTRYPOINT ["dotnet", "Enrich.IMS.RestApi.dll"]