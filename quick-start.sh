#!/bin/bash

# 🚀 FinanceApp - Quick Start Script
# Este script prepara e executa o backend

echo "======================================"
echo "🚀 FinanceApp Backend - Quick Start"
echo "======================================"
echo ""

# Cores para output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Função para verificar se comando foi bem-sucedido
check_status() {
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✅ $1${NC}"
    else
        echo -e "${RED}❌ Erro: $1${NC}"
        exit 1
    fi
}

# Passo 1: Limpar projeto
echo -e "${YELLOW}📦 Passo 1: Limpando projeto...${NC}"
dotnet clean > /dev/null 2>&1
check_status "Projeto limpo"

# Passo 2: Restaurar pacotes
echo -e "${YELLOW}📦 Passo 2: Restaurando pacotes NuGet...${NC}"
dotnet restore
check_status "Pacotes restaurados"

# Passo 3: Compilar
echo -e "${YELLOW}🔨 Passo 3: Compilando projeto...${NC}"
dotnet build --no-restore
check_status "Projeto compilado com sucesso"

# Passo 4: Verificar se dotnet-ef está instalado
echo -e "${YELLOW}🔧 Passo 4: Verificando dotnet-ef...${NC}"
if ! command -v dotnet-ef &> /dev/null; then
    echo "dotnet-ef não encontrado. Instalando..."
    dotnet tool install --global dotnet-ef
    check_status "dotnet-ef instalado"
else
    echo -e "${GREEN}✅ dotnet-ef já está instalado${NC}"
fi

# Passo 5: Criar migration (se não existir)
echo -e "${YELLOW}🗄️  Passo 5: Verificando migrations...${NC}"
if [ ! -d "FinanceApp.Infrastructure/Migrations" ]; then
    echo "Criando migration inicial..."
    dotnet ef migrations add InitialCreate --project FinanceApp.Infrastructure --startup-project FinanceApp.API
    check_status "Migration criada"
else
    echo -e "${GREEN}✅ Migrations já existem${NC}"
fi

# Passo 6: Aplicar migration
echo -e "${YELLOW}🗄️  Passo 6: Aplicando migrations ao banco...${NC}"
dotnet ef database update --project FinanceApp.Infrastructure --startup-project FinanceApp.API
check_status "Banco de dados atualizado"

echo ""
echo -e "${GREEN}======================================"
echo "✅ Setup completo!"
echo "======================================${NC}"
echo ""
echo -e "${YELLOW}Para executar a API:${NC}"
echo "  cd FinanceApp.API"
echo "  dotnet run"
echo ""
echo -e "${YELLOW}Swagger UI estará disponível em:${NC}"
echo "  https://localhost:5001/swagger"
echo ""
echo -e "${YELLOW}Leia o guia de testes em:${NC}"
echo "  TESTING_GUIDE.md"
echo ""
