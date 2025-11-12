# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["BlazorCrudDemo.sln", "./"]
COPY ["BlazorCrudDemo.Data/BlazorCrudDemo.Data.csproj", "BlazorCrudDemo.Data/"]
COPY ["BlazorCrudDemo.Shared/BlazorCrudDemo.Shared.csproj", "BlazorCrudDemo.Shared/"]
COPY ["BlazorCrudDemo.Web/BlazorCrudDemo.Web.csproj", "BlazorCrudDemo.Web/"]

# Restore dependencies
RUN dotnet restore "BlazorCrudDemo.sln"

# Copy everything else and build
COPY . .
WORKDIR "/src/BlazorCrudDemo.Web"
RUN dotnet build "BlazorCrudDemo.Web.csproj" -c Release -o /app/build

# Publish the app
FROM build AS publish
RUN dotnet publish "BlazorCrudDemo.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose ports
EXPOSE 80
EXPOSE 443

# Copy the published app
COPY --from=publish /app/publish .

# Set entry point
ENTRYPOINT ["dotnet", "BlazorCrudDemo.Web.dll"]
