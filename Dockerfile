# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY TazaOrda.sln ./
COPY Core/TazaOrda.Domain/TazaOrda.Domain.csproj ./Core/TazaOrda.Domain/
COPY Infrastructure/TazaOrda.Infrastructure/TazaOrda.Infrastructure.csproj ./Infrastructure/TazaOrda.Infrastructure/
COPY Presentation/TazaOrda.API/TazaOrda.API.csproj ./Presentation/TazaOrda.API/
COPY Presentation/TazaOrda.TelegramBot/TazaOrda.TelegramBot.csproj ./Presentation/TazaOrda.TelegramBot/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
WORKDIR /src/Presentation/TazaOrda.API
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080

# Run the application
ENTRYPOINT ["dotnet", "TazaOrda.API.dll"]
