# Hypesoft API

## Introdução
Sistema de gerenciamento de produtos e categorias desenvolvido com Clean Architecture, DDD e CQRS.  
Funcionalidades incluem cadastro completo de produtos e categorias, controle de estoque, busca/filtragem, paginação e dashboard com métricas.

[Video de Apresentação do Projeto]()

Tecnologias principais:
- **Backend:** .NET 9, C#, Clean Architecture, DDD, CQRS, FluentValidation
- **Banco de dados:** MongoDB
- **Frontend:** React, TypeScript, TailwindCSS, Shadcn/ui, React Query, Axios, Zod
- **Autenticação:** Keycloak, JWT
- **Outras ferramentas:** Docker, Swagger

---

## Arquitetura
O projeto segue os princípios de:
- **Clean Architecture:** Separação clara de responsabilidades em camadas
- **Domain-Driven Design (DDD):** Modelagem focada no domínio do negócio
- **CQRS:** Separação entre comandos (escrita) e queries (leitura)

---

## O que foi implementado

### Backend
- [x] CRUD completo de produtos (GET, POST, PUT, DELETE, PATCH)
- [x] CRUD completo de categorias
- [x] Listagem de produtos por categoria
- [x] Busca de produtos por nome
- [x] Paginação de produtos
- [x] Controle de estoque (atualização via PATCH)
- [x] Listagem de produtos com estoque baixo
- [x] Dashboard com métricas e estatísticas
- [x] Soft delete para produtos e categorias
- [x] Integração com MongoDB
- [x] Autenticação com Keycloak e JWT
- [x] Proteção de rotas com autorização
- [x] Swagger documentando a API
- [x] Validações com FluentValidation

### Frontend
- [x] Listagem de produtos e categorias
- [x] Cadastro de produtos e categorias
- [x] Edição de produtos e categorias
- [x] Dashboard com métricas (estoque, total de produtos, categorias)
- [x] Gráfico por categoria
- [x] Busca e filtros de produtos
- [x] Paginação de listagens
- [x] Autenticação com Keycloak
- [x] Proteção de rotas
- [x] Controle de estoque baixo

### Infraestrutura
- [x] Docker Compose rodando backend, frontend, MongoDB e Keycloak
- [x] População de dados de exemplo
- [x] Configuração de JWT e autorização

---

## Rotas API

### Produtos

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/Products` | Lista todos os produtos | Requerida |
| POST | `/api/Products` | Cria um novo produto | Requerida |
| GET | `/api/Products/{id}` | Retorna um produto por ID | Requerida |
| PUT | `/api/Products/{id}` | Atualiza um produto existente | Requerida |
| DELETE | `/api/Products/{id}` | Remove um produto (soft delete) | Requerida |
| PATCH | `/api/Products/{id}/stock` | Atualiza o estoque de um produto | Requerida |
| GET | `/api/Products/search?name=term` | Busca produtos por nome | Requerida |
| GET | `/api/Products/category/{categoryId}` | Filtra produtos por categoria | Requerida |
| GET | `/api/Products/low-stock` | Lista produtos com estoque baixo | Requerida |
| GET | `/api/Products/paged?page=1&pageSize=10` | Lista produtos com paginação | Requerida |

### Categorias

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/Categories` | Lista todas as categorias | Requerida |
| POST | `/api/Categories` | Cria uma nova categoria | Requerida |
| GET | `/api/Categories/{id}` | Retorna uma categoria por ID | Requerida |
| PUT | `/api/Categories/{id}` | Atualiza uma categoria existente | Requerida |
| DELETE | `/api/Categories/{id}` | Remove uma categoria (soft delete) | Requerida |

### Dashboard

| Método | Rota | Descrição | Autenticação |
|--------|------|-----------|--------------|
| GET | `/api/Dashboard` | Retorna métricas do dashboard | Requerida |

---

## Exemplos de requisição

### Autenticação
Todas as rotas da API requerem autenticação via JWT. O token deve ser enviado no header:
```
Authorization: Bearer {seu_token_jwt}
```

### Criar um produto

**POST /api/Products**
```json
{
  "name": "Headset",
  "description": "Readragon Zeus pro",
  "price": 899.90,
  "categoryId": "507f1f77bcf86cd799439011",
  "stock": 25
}
```

### Atualizar estoque

**PATCH /api/Products/{id}/stock**
```json
{
  "quantity": 10,
}
```

### Criar categoria

**POST /api/Categories**
```json
{
  "name": "Eletrônicos",
  "description": "Produtos eletrônicos diversos"
}
```

---

## Rotas Frontend

### Dashboard
| Caminho | Página | Descrição |
|---------|--------|-----------|
| `/` | `Dashboard` | Página inicial com métricas gerais (produtos, estoque, categorias, gráficos) |

### Produtos
| Caminho | Página | Descrição |
|---------|--------|-----------|
| `/products` | `ListProducts` | Lista todos os produtos com paginação |
| `/products/new` | `CreateProduct` | Formulário para cadastrar um novo produto |
| `/products/:id` | `ShowProduct` | Exibe detalhes de um produto específico |
| `/products/:id/update` | `UpdateProduct` | Formulário para editar um produto existente |
| `/products/low-stock` | `LowStock` | Lista produtos com estoque baixo |

### Categorias
| Caminho | Página | Descrição |
|---------|--------|-----------|
| `/categories` | `ListCategories` | Lista todas as categorias |
| `/categories/new` | `CreateCategory` | Formulário para criar uma nova categoria |
| `/categories/:id/update` | `UpdateCategory` | Formulário para editar uma categoria existente |

### Autenticação
| Caminho | Página | Descrição |
|---------|--------|-----------|
| `/login` | `Login` | Página de login com Keycloak |
| `/logout` | - | Encerra a sessão do usuário |

---

## Instalação e execução

### Pré-requisitos
- Node.js 18+
- npm ou yarn
- .NET 9 SDK
- MongoDB
- Docker e Docker Compose

### Execução com Docker Compose

```bash
# Clone o repositório
git clone https://github.com/Erick-Diego/hypesoft-challenge.git
cd hypesoft-challenge

# Execute toda a aplicação com Docker Compose
docker-compose up -d

# Aguarde alguns segundos para os serviços iniciarem
# Verifique se todos os containers estão rodando
docker-compose ps
```

### URLs de Acesso
- **Frontend:** http://localhost:3000
- **API:** http://localhost:5000
- **Swagger:** http://localhost:5000/swagger
- **MongoDB Express:** http://localhost:8081
- **Keycloak:** http://localhost:8080

### Desenvolvimento Local

#### Backend
```bash
cd backend/src/Hypesoft.API
dotnet restore
dotnet run
```

#### Frontend
```bash
cd frontend
npm install
npm run dev
```

---

## Configuração do Keycloak

### Configuração Inicial
1. Acesse o Keycloak em http://localhost:8080
2. Faça login com as credenciais de administrador
3. Crie um novo Realm (ex: `hypesoft`)
4. Crie um Client para a API
5. Configure as URLs de redirecionamento
6. Crie usuários de teste

### Variáveis de Ambiente
Configure as seguintes variáveis no arquivo `.env`:

```env
# Keycloak
KEYCLOAK_URL=http://localhost:8080
KEYCLOAK_REALM=hypesoft
KEYCLOAK_CLIENT_ID=hypesoft-api
KEYCLOAK_CLIENT_SECRET=seu-client-secret

# JWT
JWT_AUTHORITY=http://localhost:8080/realms/hypesoft
JWT_AUDIENCE=hypesoft-api
```

---

## Schemas da API

### ProductDto
```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "price": 0.0,
  "categoryId": "string",
  "categoryName": "string",
  "stock": 0,
  "createdAt": "2025-01-16T00:00:00Z",
  "updatedAt": "2025-01-16T00:00:00Z"
}
```

### CategoryDto
```json
{
  "id": "string",
  "name": "string",
  "description": "string",
  "createdAt": "2025-01-16T00:00:00Z",
  "updatedAt": "2025-01-16T00:00:00Z"
}
```

### DashboardDto
```json
{
  "totalProducts": 0,
  "totalCategories": 0,
  "totalStock": 0,
  "lowStockProducts": 0,
  "categoryStats": [
    {
      "categoryName": "string",
      "productCount": 0,
      "totalStock": 0
    }
  ]
}
```
