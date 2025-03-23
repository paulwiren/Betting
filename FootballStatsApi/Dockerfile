# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.




# Use the ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build and publish your app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FootballStatsApi/FootballStatsApi.csproj", "FootballStatsApi/"]
#COPY ["FootballStatsApi/FootballStatsApi.csproj", "FootballStatsApi/"]
RUN dotnet restore "FootballStatsApi/FootballStatsApi.csproj"
COPY . .
WORKDIR "/src/FootballStatsApi"
RUN dotnet publish "FootballStatsApi.csproj" -c Release -o /app/publish

# Final stage: Run the app
FROM base AS final
RUN apt-get update && apt-get install -y wget gnupg ca-certificates \
    && wget -qO- https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list' \
    && apt-get update && apt-get install -y google-chrome-stable
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FootballStatsApi.dll"]


#
## This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
#WORKDIR /app
#EXPOSE 80
##EXPOSE 8080
##EXPOSE 8081
##ENV ASPNETCORE_URLS=http://+:80
#
## This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["FootballStatsApi/FootballStatsApi.csproj", "FootballStatsApi/"]
#COPY ["BettingEngine/BettingEngine.csproj", "BettingEngine/"]
#RUN dotnet restore "./FootballStatsApi/FootballStatsApi.csproj"
#COPY . .
#WORKDIR "/src/FootballStatsApi"
#RUN dotnet build "./FootballStatsApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## This stage is used to publish the service project to be copied to the final stage
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./FootballStatsApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "FootballStatsApi.dll"]