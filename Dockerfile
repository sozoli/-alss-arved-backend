# Define base image
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.20 AS builder

# Copy project files
WORKDIR /source
COPY ./*.csproj .

# Restore
RUN dotnet restore

# Copy all source code
COPY . .

# Publish
WORKDIR /source
RUN dotnet publish "ALSS_invoice_back.csproj" -c Release -o /publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.20
WORKDIR /app
COPY --from=builder /publish .

EXPOSE 8080

RUN addgroup -S runner && adduser -S runner -G runner
USER runner
ENTRYPOINT ["dotnet", "ALSS_invoice_back.dll"]
