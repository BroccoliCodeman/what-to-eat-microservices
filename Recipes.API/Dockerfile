#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Recipes.API/Recipes.API.csproj", "Recipes.API/"]
COPY ["Recipes.BLL/Recipes.BLL.csproj", "Recipes.BLL/"]
COPY ["Recipes.DAL/Recipes.DAL.csproj", "Recipes.DAL/"]
COPY ["Recipes.Data/Recipes.Data.csproj", "Recipes.Data/"]
RUN dotnet restore "./Recipes.API/Recipes.API.csproj"
COPY . .
WORKDIR "/src/Recipes.API"
RUN dotnet build "./Recipes.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Recipes.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Recipes.API/Certificates/certificate.pfx /Certificates/
ENTRYPOINT ["dotnet", "Recipes.API.dll"]

ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="hecysgsplf"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=Certificates/certificate.pfx 

RUN apt-get update && \
    apt-get install -y ca-certificates && \
    update-ca-certificates