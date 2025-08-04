FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ContactService/ContactService.csproj", "ContactService/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "ContactService/ContactService.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/ContactService"
RUN dotnet build "ContactService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ContactService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ContactService.dll"]