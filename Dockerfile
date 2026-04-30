# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Job.Marketplace.API/Job.Marketplace.API.csproj", "src/Job.Marketplace.API/"]
COPY ["src/Job.Marketplace.Domain/Job.Marketplace.Domain.csproj", "src/Job.Marketplace.Domain/"]
COPY ["src/Job.Marketplace.Infrastructure/Job.Marketplace.Infrastructure.csproj", "src/Job.Marketplace.Infrastructure/"]
RUN dotnet restore "src/Job.Marketplace.API/Job.Marketplace.API.csproj"
COPY . .
WORKDIR /src/src/Job.Marketplace.API
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Job.Marketplace.API.dll"]
