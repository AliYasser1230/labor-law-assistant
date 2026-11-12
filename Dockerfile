FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/LaborLawAssistant/LaborLawAssistant.csproj src/LaborLawAssistant/
RUN dotnet restore src/LaborLawAssistant/LaborLawAssistant.csproj
COPY src/LaborLawAssistant/ src/LaborLawAssistant/
RUN dotnet publish src/LaborLawAssistant/LaborLawAssistant.csproj --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data /app/App_Data && chown -R $APP_UID /app/data /app/App_Data
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/laborlaw.db"
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "LaborLawAssistant.dll"]
