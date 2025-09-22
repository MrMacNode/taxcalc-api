# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["TaxcalcApi/TaxcalcApi.csproj", "TaxcalcApi/"]
RUN dotnet restore "TaxcalcApi/TaxcalcApi.csproj"

COPY . .
WORKDIR "/src/TaxcalcApi"
RUN dotnet publish "TaxcalcApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

# Final runtime image
FROM mcr.microsoft.com/dotnet/runtime-deps:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["./TaxcalcApi"]
