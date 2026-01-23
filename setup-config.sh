#!/bin/bash

# FinanceApp Configuration Setup Script
echo "========================================="
echo "  FinanceApp - Configuration Setup"
echo "========================================="
echo ""

# Check if appsettings.Development.json exists
if [ -f "FinanceApp.API/appsettings.Development.json" ]; then
    echo "✓ appsettings.Development.json already exists"
    read -p "Do you want to overwrite it? (y/n): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Setup cancelled."
        exit 0
    fi
fi

# Generate a random JWT secret key (64 characters)
JWT_SECRET=$(openssl rand -base64 48 | tr -d '\n')

# Create appsettings.Development.json
cat > FinanceApp.API/appsettings.Development.json << EOF
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=financeapp_dev;Username=postgres;Password=YOUR_PASSWORD_HERE"
  },
  "JwtSettings": {
    "SecretKey": "$JWT_SECRET",
    "Issuer": "FinanceApp",
    "Audience": "FinanceAppUsers",
    "ExpirationInMinutes": 1440
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console" ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ]
  }
}
EOF

echo ""
echo "✓ Configuration file created successfully!"
echo ""
echo "⚠️  IMPORTANT: Update the database password in:"
echo "   FinanceApp.API/appsettings.Development.json"
echo ""
echo "JWT Secret Key has been generated automatically."
echo ""
echo "Next steps:"
echo "1. Update the PostgreSQL password in appsettings.Development.json"
echo "2. Run migrations: cd FinanceApp.Infrastructure && dotnet ef database update"
echo "3. Start the application: cd FinanceApp.API && dotnet run"
echo ""
