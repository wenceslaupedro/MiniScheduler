# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project files
COPY ["MiniScheduler.csproj", "./"]
RUN dotnet restore "MiniScheduler.csproj"

# Copy the rest of the source code
COPY . .
RUN dotnet build "MiniScheduler.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "MiniScheduler.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set environment variables for Google Cloud Run
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "MiniScheduler.dll"]
