# 🏦 Payment Switch API

> **Interbank payment routing system** built with **.NET 8**, **Clean Architecture**, **CQRS + MediatR**, and **event-driven messaging**. Simulates a real-world payment switch processing transactions between banks with idempotency, circuit breaking, and full audit trails.
>
> *Inspired by production experience building Pakistan's 1LINK interbank switch, processing 1M+ daily transactions at 99.9% uptime.*

[![.NET](https://img.shields.io/badge/.NET_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23_12-239120?style=for-the-badge&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp)
[![Architecture](https://img.shields.io/badge/Clean_Architecture-1B3A6B?style=for-the-badge)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![CQRS](https://img.shields.io/badge/CQRS_+_MediatR-FF6B35?style=for-the-badge)](https://github.com/jbogard/MediatR)
[![Tests](https://img.shields.io/badge/xUnit_Tests-green?style=for-the-badge)](tests/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

---

## ✨ Features

| Feature | Details |
|---------|---------|
| 💳 **Idempotent Transactions** | `Idempotency-Key` header prevents duplicate processing |
| 🏗️ **Clean Architecture** | Domain → Application → Infrastructure → API, strict dependency rule |
| ⚡ **CQRS + MediatR** | Commands and Queries fully separated, pipeline behaviours |
| 📨 **Event-Driven** | Domain events published on state transitions (RabbitMQ-ready) |
| 🔒 **Domain Rules** | Amount limits, state machine transitions enforced in domain |
| 🧪 **Tested** | xUnit unit tests for all domain business rules |
| 🛡️ **Global Error Handling** | Middleware catches and maps exceptions to structured responses |
| 🐳 **Docker Ready** | Multi-stage Dockerfile, single command run |
| 📖 **Swagger UI** | Full API documentation at `/swagger` |
| 🔄 **Swap-Ready** | InMemory repo/publisher → plug in EF Core + RabbitMQ for production |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      API Layer                          │
│  TransactionsController  HealthController               │
│  ExceptionMiddleware     Swagger                        │
└──────────────────┬──────────────────────────────────────┘
                   │ MediatR
                   ▼
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                      │
│  CreateTransactionCommand  →  Handler                   │
│  ProcessTransactionCommand →  Handler                   │
│  GetTransactionQuery       →  Handler                   │
│                                                         │
│  ITransactionRepository  IMessagePublisher (interfaces) │
└──────────────────┬──────────────────────────────────────┘
                   │ depends on Domain
                   ▼
┌─────────────────────────────────────────────────────────┐
│                   Domain Layer                          │
│  Transaction (Aggregate Root)                           │
│  ├── Create() — factory method, validates rules         │
│  ├── MarkProcessing() — state transition                │
│  ├── MarkCompleted() — raises TransactionCompletedEvent │
│  └── MarkFailed(reason) — raises TransactionFailedEvent │
│                                                         │
│  IDomainEvent  TransactionCreated/Completed/Failed      │
│  ITransactionRepository (interface only)                │
└─────────────────────────────────────────────────────────┘
                   ▲ implements
┌─────────────────────────────────────────────────────────┐
│               Infrastructure Layer                      │
│  InMemoryTransactionRepository  (swap → EF Core)        │
│  InMemoryMessagePublisher       (swap → RabbitMQ)       │
└─────────────────────────────────────────────────────────┘
```

### Transaction State Machine

```
              ┌─────────┐
              │ PENDING │ ←── IncrementRetry()
              └────┬────┘
                   │ MarkProcessing()
                   ▼
           ┌────────────┐
           │ PROCESSING │
           └─────┬──────┘
          ┌──────┴──────┐
          │             │
          ▼             ▼
    ┌──────────┐  ┌────────┐
    │COMPLETED │  │ FAILED │
    └──────────┘  └────────┘
```

---

## 📁 Project Structure

```
payment-switch-api/
├── src/
│   ├── PaymentSwitch.Domain/           # Enterprise business rules
│   │   ├── Entities/Transaction.cs     # Aggregate root + state machine
│   │   ├── Enums/TransactionStatus.cs
│   │   ├── Enums/TransactionType.cs
│   │   ├── Events/TransactionEvents.cs # Domain events
│   │   └── Interfaces/ITransactionRepository.cs
│   │
│   ├── PaymentSwitch.Application/      # Use cases
│   │   ├── Commands/CreateTransactionCommand.cs
│   │   ├── Commands/ProcessTransactionCommand.cs
│   │   ├── Queries/GetTransactionQuery.cs
│   │   ├── DTOs/TransactionDtos.cs
│   │   └── Interfaces/IMessagePublisher.cs
│   │
│   ├── PaymentSwitch.Infrastructure/   # Adapters
│   │   ├── Repositories/InMemoryTransactionRepository.cs
│   │   └── Messaging/InMemoryMessagePublisher.cs
│   │
│   └── PaymentSwitch.API/              # Delivery mechanism
│       ├── Controllers/TransactionsController.cs
│       ├── Middleware/ExceptionMiddleware.cs
│       └── Program.cs
│
└── tests/
    └── PaymentSwitch.UnitTests/
        └── TransactionTests.cs         # 8 domain rule tests
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run Locally

```bash
git clone https://github.com/yasirmalik36/payment-switch-api.git
cd payment-switch-api

dotnet run --project src/PaymentSwitch.API
```

API at `http://localhost:5000` · Swagger UI at `http://localhost:5000/swagger`

### Run with Docker

```bash
docker build -t payment-switch-api .
docker run -p 5000:8080 payment-switch-api
```

### Run Tests

```bash
dotnet test tests/PaymentSwitch.UnitTests --verbosity normal
```

---

## 📡 API Usage

### 1. Create a Transaction (Idempotent)

```bash
curl -X POST "http://localhost:5000/api/v1/transactions" \
  -H "Content-Type: application/json" \
  -H "Idempotency-Key: unique-key-001" \
  -d '{
    "senderAccountNumber": "1234567890",
    "senderBankCode": "HBL",
    "receiverAccountNumber": "0987654321",
    "receiverBankCode": "UBL",
    "amount": 25000.00,
    "transactionType": "InterBankTransfer",
    "currency": "PKR"
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "switchReferenceId": "SW-20250607-A1B2C3D4",
    "senderBankCode": "HBL",
    "receiverBankCode": "UBL",
    "amount": 25000.00,
    "currency": "PKR",
    "status": "Pending",
    "type": "InterBankTransfer",
    "createdAt": "2025-06-07T10:30:00Z"
  }
}
```

### 2. Process (Route) the Transaction

```bash
curl -X POST "http://localhost:5000/api/v1/transactions/{id}/process"
```

### 3. Check Status

```bash
curl "http://localhost:5000/api/v1/transactions/{id}"
```

### 4. Idempotency — Same Key Returns Same Result

```bash
# Sending the same Idempotency-Key twice returns the original transaction, not a duplicate
curl -X POST "http://localhost:5000/api/v1/transactions" \
  -H "Idempotency-Key: unique-key-001" \  # same key
  -d '{ ... }'
# Returns the existing transaction — no duplicate created
```

---

## 🧪 Test Results

```
PaymentSwitch.UnitTests.TransactionTests
  ✓ Create_ValidTransaction_ShouldHavePendingStatus
  ✓ Create_NegativeAmount_ShouldThrow
  ✓ Create_ExceedsLimit_ShouldThrow
  ✓ MarkProcessing_FromPending_ShouldSucceed
  ✓ MarkCompleted_FromProcessing_ShouldSucceed
  ✓ MarkProcessing_AlreadyProcessing_ShouldThrow
  ✓ MarkFailed_FromCompleted_ShouldThrow
  ✓ IdempotencyKey_ShouldBeStored

Passed: 8 | Failed: 0 | Duration: <1s
```

---

## 🔄 Extending for Production

### Swap InMemory → EF Core + SQL Server

```csharp
// In Program.cs, replace:
builder.Services.AddSingleton<ITransactionRepository, InMemoryTransactionRepository>();
// With:
builder.Services.AddDbContext<PaymentSwitchDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddScoped<ITransactionRepository, EfCoreTransactionRepository>();
```

### Swap InMemory Publisher → RabbitMQ

```csharp
// Replace:
builder.Services.AddSingleton<IMessagePublisher, InMemoryMessagePublisher>();
// With:
builder.Services.AddSingleton<IMessagePublisher, RabbitMqMessagePublisher>();
```

The `IMessagePublisher` interface is already defined — just implement it with your RabbitMQ connection.

### Add PCI-DSS Patterns (Production Checklist)

- [ ] TLS 1.2+ enforcement on all endpoints
- [ ] Mask PANs in logs (`1234****5678`)
- [ ] Audit log every state transition to immutable store
- [ ] Rate limiting per client IP + bank code
- [ ] Request signing (HMAC-SHA256)
- [ ] Field-level encryption for account numbers

---

## 🔑 Key Design Decisions

**Why Clean Architecture over MVC?**
Domain logic is completely isolated from infrastructure. Swap EF Core → Dapper, RabbitMQ → Kafka, without touching a single line of business logic. This is how payment systems survive 10 years.

**Why CQRS?**
Payment systems have extreme read/write asymmetry. Reads (balance checks, status queries) can be served from read replicas. Writes (initiating payments) go through strict validation. Separating them enables independent scaling.

**Why Idempotency Keys?**
Network failures are inevitable. Without idempotency, a client timeout followed by a retry creates duplicate charges. The `Idempotency-Key` header ensures exactly-once processing regardless of network conditions.

**Why Domain Events?**
State changes (transaction completed, failed) need to trigger downstream actions (notify bank, update ledger, send SMS). Domain events decouple the trigger from the reaction — add new consumers without touching the Transaction class.

---

## 🤝 Author

**Yasir Mehmood** — Senior Full-Stack & AI Engineer | Rawalpindi, Pakistan

*Built systems processing 1M+ daily interbank transactions at 99.9% uptime with full PCI-DSS compliance.*

[![Portfolio](https://img.shields.io/badge/Portfolio-Visit-1B3A6B?style=flat-square)](https://yasir-portfolio-omega.vercel.app)
[![LinkedIn](https://img.shields.io/badge/LinkedIn-Connect-0077B5?style=flat-square&logo=linkedin)](https://linkedin.com/in/yasir-mehmood-53549a1b3)

---

## 📄 License

MIT License — see [LICENSE](LICENSE) for details.
