# Stage 1: build the Vite client bundle (outputs to wwwroot/dist)
FROM node:22-alpine AS client
WORKDIR /src/JiujitsuGymApp/ClientApp
COPY JiujitsuGymApp/ClientApp/package*.json ./
RUN npm ci
COPY JiujitsuGymApp/ClientApp/ ./
RUN npm run build

# Stage 2: build and publish the ASP.NET Core app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY JiujitsuGymApp/JiujitsuGymApp.csproj JiujitsuGymApp/
RUN dotnet restore JiujitsuGymApp/JiujitsuGymApp.csproj
COPY JiujitsuGymApp/ JiujitsuGymApp/
RUN dotnet publish JiujitsuGymApp/JiujitsuGymApp.csproj -c Release -o /app/publish

# Stage 3: runtime image (multi-arch: works on linux/arm64 for Raspberry Pi)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=client /src/JiujitsuGymApp/wwwroot/dist ./wwwroot/dist
EXPOSE 8080
ENTRYPOINT ["dotnet", "JiujitsuGymApp.dll"]
