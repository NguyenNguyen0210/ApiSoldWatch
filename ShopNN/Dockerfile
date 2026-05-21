# Dùng môi trường ASP.NET Core Runtime để chạy app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Dùng .NET SDK để build code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShopNN.csproj", "./"]
RUN dotnet restore "./ShopNN.csproj"
COPY . .
RUN dotnet build "ShopNN.csproj" -c Release -o /app/build

# Publish ứng dụng ra thư mục chạy
FROM build AS publish
RUN dotnet publish "ShopNN.csproj" -c Release -o /app/publish

# Copy file đã build vào môi trường Runtime tối giản và chạy ứng dụng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopNN.dll"]
