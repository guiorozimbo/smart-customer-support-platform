# Customer Support Platform

Backend monorepo em **.NET 10**: API Gateway (YARP), microserviços, SQL Server e **serviço de integração HTTP** para canais de conversação (ex.: automações com `POST` JSON).

## Structure

```
customer-support-platform/
├── api-gateway          # YARP (port 5000)
├── services/
│   ├── order-service    # Orders API (5001)
│   ├── customer-service # Customers API (5002)
│   └── ticket-service   # Tickets API (5003)
├── shared/
│   ├── domain           # Entities
│   ├── application      # DTOs, interfaces
│   ├── infrastructure   # EF Core, repositórios
│   └── hosting          # Correlation id, logs, exceções (API + integração)
├── database             # DbContext, migrations
├── chatbot-integration  # REST /api/chatbot/* + Minimal API /api/integration/* (5004)
└── tests/
```

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server ou LocalDB

## Database

1. Ajuste as connection strings nos `appsettings.json` se precisar (default: LocalDB).
2. Na pasta do monorepo:

```bash
cd customer-support-platform
dotnet ef database update --project database --startup-project services/order-service
```

(Se ainda não existir migration: `dotnet ef migrations add InitialCreate --project database --startup-project services/order-service` e depois `database update`.)

## Run (5 terminais)

```bash
cd customer-support-platform
dotnet run --project api-gateway
dotnet run --project services/order-service
dotnet run --project services/customer-service
dotnet run --project services/ticket-service
dotnet run --project chatbot-integration
```

Portas padrão: **5000** (gateway), **5001–5004** (serviços).

## API Gateway (`http://localhost:5000`)

| Prefixo | Destino |
|--------|---------|
| `/api/orders/*` | Order Service |
| `/api/customers/*` | Customer Service |
| `/api/tickets/*` | Ticket Service |
| `/api/chatbot/*` | Integração — dados (clientes, pedidos, tickets) |
| `/api/integration/*` | Integração — Minimal API (mensagens curtas, demo) |

## Integração HTTP (`chatbot-integration`)

### Minimal API (conversa / demo)

- **POST** `http://localhost:5000/api/integration/messages` (via gateway) ou `http://localhost:5004/api/integration/messages` (direto)
- **Body:** `{ "text": "projeto" }` ou `{ "text": "status" }`
- **Resposta:** `{ "type": "text/plain", "content": "..." }`
- Código consolidado em `chatbot-integration/Integration/ChatIntegrationApi.cs`

### REST `/api/chatbot/*` (dados reais)

| Method | Path | Descrição |
|--------|------|-----------|
| GET | `/api/chatbot/customer?email=` | Cliente por email (dados completos) |
| GET | `/api/chatbot/customer/email?email=` | Só o email confirmado (`{"email":"..."}`) |
| GET | `/api/chatbot/orders?email=` | Pedidos |
| GET | `/api/chatbot/order/{orderNumber}` | Pedido por número |
| GET | `/api/chatbot/tickets?email=` | Tickets |
| POST | `/api/chatbot/ticket` | Criar ticket (JSON no body) |

`GET /health` no serviço de integração verifica a base de dados (EF).

### Segurança opcional

- Se `Chatbot:SharedSecret` estiver preenchido em `chatbot-integration/appsettings.json`, envie header **`X-Chatbot-Api-Key`** com o mesmo valor.

### Testes manuais

- Ficheiro **`DevApi.http`** na raiz do monorepo (extensão REST Client no VS Code/Cursor).
- Script opcional: **`scripts/DevApi.ps1`**.

## Tests

```bash
dotnet test
```

## Tech stack

- ASP.NET Core 10  
- Entity Framework Core 10, SQL Server  
- Clean Architecture (Domain / Application / Infrastructure)  
- YARP, Swagger, DI, health checks

---

## Publicar no GitHub (passo a passo)

1. **Git init** (se ainda não for repositório):
   ```bash
   cd customer-support-platform
   git init
   ```
2. Crie um `.gitignore` para .NET (Visual Studio template ou [github/gitignore](https://github.com/github/gitignore)) — ignore `bin/`, `obj/`, `.vs/`, `*.user`.
3. **Commit**:
   ```bash
   git add .
   git commit -m "Customer Support Platform: .NET 10 monorepo com gateway e integração HTTP"
   ```
4. No GitHub: **New repository** (público para portfólio), sem README duplicado se já tiveres local.
5. **Liga o remoto e envia** (HTTPS; podes usar SSH se preferires `git@github.com:...`):

   ```bash
   git remote add origin https://github.com/SEU_USUARIO/SEU_REPO.git
   git branch -M main
   git push -u origin main
   ```

6. No repositório: abre **About** e coloca **topics**, por exemplo: `dotnet`, `csharp`, `microservices`, `yarp`, `ef-core`, `clean-architecture`, `api-gateway`.

7. **Opcional:** adiciona **LICENSE** (MIT é comum em portfólio) e **Security** → dependabot (GitHub sugere).

*Não commitares segredos:* mantém `Chatbot:SharedSecret` vazio no exemplo ou usa variáveis de ambiente em produção.

---

## Post no LinkedIn (sugestão honesta e profissional)

1. **Uma frase de impacto:** o que é (plataforma de suporte com gateway e integração HTTP).
2. **Stack em 1 linha:** .NET 10, YARP, EF Core, SQL Server, Minimal API + REST.
3. **O que demonstra:** ponto de entrada único, microserviços, rastreio de pedidos, camada pronta para canal de conversação via HTTP.
4. **Link:** para o repositório GitHub (e, se tiveres, um diagrama ou screenshot do Swagger).
5. **Tom:** projeto de estudo/portfólio; evita dizer “parceria” com marcas de terceiros sem ser verdade.

Exemplo curto (adapta ao teu nome e link):

> Partilho no GitHub uma plataforma de customer support em .NET 10: API Gateway com YARP, microserviços para pedidos/clientes/tickets, EF Core + SQL Server e um serviço de integração HTTP (Minimal API + REST) para ligar a canais de conversação. [link do repo] `#dotnet` `#backend` `#microservices`

---

## IDE (Cursor / VS Code) e “erros a vermelho”

Se o código **compila** (`dotnet build`) mas o editor mostra erros:

1. Guarda todos os ficheiros.
2. **Reload Window** (Command Palette) ou fecha e abre a pasta do monorepo.
3. Garante que abriste a pasta **`customer-support-platform`** (onde está o `.sln`), não só uma subpasta.

Isso sincroniza o language service com os ficheiros e referências do projeto.

---

## `.gitignore`

Na raiz do monorepo existe **`.gitignore`** para .NET (pastas `bin/`, `obj/`, `.vs/`, ficheiros de utilizador, logs de teste, `.env`, etc.).  
Antes do primeiro commit: `git status` deve mostrar só código-fonte e configs “seguros” (sem `appsettings` com passwords — usa LocalDB ou variáveis de ambiente).

---

## Passo a passo — correr tudo e “fazer acontecer” (para demo / vídeo)

### 0. Pronto para gravar

- Fecha instâncias antigas dos serviços (evita erro de DLL bloqueada ao compilar).
- Abre **SQL Server LocalDB** (ou altera connection strings).
- Terminal na pasta **`customer-support-platform`**.

### 1. Base de dados

```bash
dotnet ef database update --project database --startup-project services/order-service
```

### 2. Cinco terminais (PowerShell)

Em cada um: `cd` para `customer-support-platform`, depois **um** destes por janela:

```bash
dotnet run --project api-gateway
dotnet run --project services/order-service
dotnet run --project services/customer-service
dotnet run --project services/ticket-service
dotnet run --project chatbot-integration
```

Espera até aparecer “Now listening on …” em cada um (portas **5000**, **5001–5004**).

### 3. Provar que responde (rápido)

**Gateway vivo:** (PowerShell: `curl.exe http://localhost:5000/`)

```bash
curl.exe http://localhost:5000/
```

**Minimal API (mensagem demo):**

No **PowerShell** usa `curl.exe` (o alias `curl` não é o mesmo) ou:

```powershell
Invoke-RestMethod -Method Post -Uri "http://localhost:5000/api/integration/messages" `
  -Body '{"text":"projeto"}' -ContentType "application/json; charset=utf-8"
```

Em **CMD** ou Git Bash:

```bash
curl.exe -X POST http://localhost:5000/api/integration/messages -H "Content-Type: application/json" -d "{\"text\":\"projeto\"}"
```

**Swagger (bonito no vídeo):** abre no browser `http://localhost:5004/swagger` (serviço de integração) ou o que cada projeto expuser em Development.

**Ficheiro `DevApi.http`:** na raiz do repo — extensão **REST Client** no VS Code/Cursor; envia os pedidos **Send Request**.

### 4. Gravar vídeo ou capturar ecrã (Windows)

| Ferramenta | Uso |
|------------|-----|
| **Win + G** | Barra de jogo → **Gravar** (clip do ecrã + áudio). |
| **Win + Shift + S** | Captura parte do ecrã (colar no Paint / LinkedIn como imagem). |
| **OBS Studio** | Gravação profissional (ecrã inteiro + micro). |
| **Clipchamp** (Windows 11) | Edição simples do `.mp4`. |

**Roteiro sugerido (2–3 min):** mostrar o `README` ou estrutura → `dotnet ef database update` (rápido) → os 5 terminais a correr → `curl` ou Swagger → mensagem `projeto` / `status` na Minimal API.

### 5. Partilhar

- **GitHub:** faz **push** do repo com o `.gitignore`; opcionalmente cola o link do vídeo (YouTube não listado, ou upload MP4 no release).
- **LinkedIn:** post com 1 screenshot do Swagger ou do terminal + link do repositório + hashtags (`#dotnet`, `#backend`).
