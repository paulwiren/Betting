# === Build stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

# === Runtime stage: Azure Functions .NET 8 isolated ===
FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated8.0

# Installera Chromium + fonter (Debian/Ubuntu-baserad image)
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        chromium \
        fonts-liberation \
        libnss3 \
        libasound2 \
        libxss1 \
        libatk-bridge2.0-0 \
        libatk1.0-0 \
        libx11-xcb1 \
        libxcomposite1 \
        libxdamage1 \
        libxfixes3 \
        libxrandr2 \
        libgbm1 \
        libpango-1.0-0 \
        libcairo2 \
        libatspi2.0-0 && \
    rm -rf /var/lib/apt/lists/*

# Miljövariabler (hjälper Puppeteer/Chromium i container)
ENV PUPPETEER_EXECUTABLE_PATH=/usr/bin/chromium
ENV CHROMIUM_PATH=/usr/bin/chromium

# Kopiera publicerade funktioner
WORKDIR /home/site/wwwroot
COPY --from=build /app/publish .

# Kör som root eller lägg till --no-sandbox (vi lägger flaggor i koden)
