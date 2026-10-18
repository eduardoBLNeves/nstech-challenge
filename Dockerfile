FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Nstech.OrderService.Domain/Nstech.OrderService.Domain.csproj src/Nstech.OrderService.Domain/
COPY src/Nstech.OrderService.Application/Nstech.OrderService.Application.csproj src/Nstech.OrderService.Application/
COPY src/Nstech.OrderService.Infrastructure/Nstech.OrderService.Infrastructure.csproj src/Nstech.OrderService.Infrastructure/
COPY src/Nstech.OrderService.Api/Nstech.OrderService.Api.csproj src/Nstech.OrderService.Api/
RUN dotnet restore src/Nstech.OrderService.Api/Nstech.OrderService.Api.csproj
COPY . .
RUN dotnet publish src/Nstech.OrderService.Api/Nstech.OrderService.Api.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nstech.OrderService.Api.dll"]
