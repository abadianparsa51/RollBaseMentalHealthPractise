# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# 👇 اینجا باید مسیر csproj پروژه API را دقیق کنی
# مثال: NeuroEase/NeuroEase.Api/NeuroEase.Api.csproj
RUN dotnet restore "PATH_TO_API_CSPROJ"

RUN dotnet publish "PATH_TO_API_CSPROJ" -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "YOUR_API_DLL_NAME.dll"]
