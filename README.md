# HSEGozon - Микросервисная система заказов и платежей

## Описание

Система для онлайн-магазина "Gogon", реализующая асинхронное межсервисное взаимодействие с использованием паттернов Transactional Outbox и Transactional Inbox.

## Архитектура

Система состоит из трех основных компонентов:

1. **API Gateway** - маршрутизация запросов к микросервисам
2. **Orders Service** - управление заказами
3. **Payments Service** - управление счетами и платежами

### Технологический стек

- **.NET 8.0** - платформа разработки
- **ASP.NET Core** - веб-фреймворк
- **PostgreSQL** - база данных
- **RabbitMQ** - брокер сообщений
- **Docker** - контейнеризация
- **Entity Framework Core** - ORM

## Паттерны и принципы

### Реализованные паттерны:

1. **Transactional Outbox** (Order Service и Payments Service)
   - Гарантирует доставку сообщений даже при сбоях
   - Сообщения сохраняются в БД в той же транзакции, что и бизнес-данные

2. **Transactional Inbox** (Payments Service)
   - Обеспечивает идемпотентную обработку входящих сообщений
   - Предотвращает дублирование обработки

3. **Exactly-Once Semantics**
   - Достигается через идемпотентную обработку сообщений
   - Использование уникальных идентификаторов сообщений

4. **Atomic Operations**
   - Использование `FOR UPDATE` для блокировки строк при операциях с балансом
   - Транзакции БД для обеспечения консистентности

### Принципы SOLID:

- **Single Responsibility** - каждый сервис отвечает за свою область
- **Open/Closed** - расширяемость через интерфейсы
- **Liskov Substitution** - корректное использование абстракций
- **Interface Segregation** - разделение интерфейсов по назначению
- **Dependency Inversion** - зависимость от абстракций, а не от конкретных реализаций

## Функциональность

### Payments Service

- Создание счета (максимум один счет на пользователя)
- Пополнение счета
- Просмотр баланса счета
- Обработка платежей за заказы (асинхронно)

### Orders Service

- Создание заказа (асинхронно запускает процесс оплаты)
- Просмотр списка заказов
- Просмотр статуса заказа
- Обновление статуса заказа на основе результатов оплаты

## Запуск системы

### Требования

- Docker и Docker Compose

### Запуск через Docker Compose

```bash
docker-compose up -d --build
```

Система будет доступна по следующим адресам:

- **Веб-интерфейс (Frontend)**: http://localhost:5000
- **API Gateway**: http://localhost:5000
- **Orders Service**: http://localhost:5002
- **Payments Service**: http://localhost:5001
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **PostgreSQL Orders**: localhost:5434
- **PostgreSQL Payments**: localhost:5433

### Веб-интерфейс

После запуска доступен веб-интерфейс для работы с системой:

- **Главная страница**: http://localhost:5000

Веб-интерфейс предоставляет возможность:
- Создания и управления счетами пользователей
- Пополнения счетов
- Просмотра баланса
- Создания заказов
- Просмотра списка заказов и их статусов

### Swagger документация

После запуска доступна Swagger документация:

- API Gateway: http://localhost:5000/swagger
- Orders Service: http://localhost:5002/swagger
- Payments Service: http://localhost:5001/swagger

## API Endpoints

### Payments Service

#### Создание счета
```
POST /api/accounts
Body: { "userId": "guid" }
```

#### Пополнение счета
```
POST /api/accounts/{userId}/topup
Body: { "amount": 100.00 }
```

#### Просмотр баланса
```
GET /api/accounts/{userId}
```

### Orders Service

#### Создание заказа
```
POST /api/orders
Body: {
  "userId": "guid",
  "amount": 100.00,
  "description": "Order description"
}
```

#### Список заказов пользователя
```
GET /api/orders/user/{userId}
```

#### Статус заказа
```
GET /api/orders/{orderId}
```

### API Gateway

Все запросы можно делать через API Gateway по тем же путям:
```
POST /api/accounts
POST /api/accounts/{userId}/topup
GET /api/accounts/{userId}
POST /api/orders
GET /api/orders/user/{userId}
GET /api/orders/{orderId}
```

## Сценарий работы системы

1. **Создание заказа**:
   - Пользователь создает заказ через API Gateway
   - Orders Service создает заказ в БД со статусом NEW
   - В той же транзакции создается сообщение в Outbox для обработки платежа
   - Фоновый сервис OutboxProcessorService публикует сообщение в RabbitMQ

2. **Обработка платежа**:
   - Payments Service получает сообщение из очереди
   - Сообщение сохраняется в Inbox (Transactional Inbox - часть 1)
   - Обрабатывается платеж: проверка счета, списание средств (Transactional Inbox - часть 2)
   - Создается сообщение в Outbox для отправки статуса (Transactional Outbox - часть 1)
   - Фоновый сервис публикует статус в RabbitMQ (Transactional Outbox - часть 2)

3. **Обновление статуса заказа**:
   - Orders Service получает сообщение о статусе платежа
   - Обновляет статус заказа (NEW → FINISHED или CANCELLED)
   - Операция идемпотентна - безопасна для повторной обработки

## Гарантии доставки

- **At-Least-Once Delivery**: RabbitMQ гарантирует доставку сообщений минимум один раз
- **Exactly-Once Processing**: Достигается через идемпотентную обработку с использованием уникальных идентификаторов сообщений

## Тестирование

Для тестирования можно использовать Swagger UI.

### Пример сценария:

1. Создать счет для пользователя:
```json
POST /api/accounts
{
  "userId": "123e4567-e89b-12d3-a456-426614174000"
}
```

2. Пополнить счет:
```json
POST /api/accounts/123e4567-e89b-12d3-a456-426614174000/topup
{
  "amount": 1000.00
}
```

3. Создать заказ:
```json
POST /api/orders
{
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "amount": 500.00,
  "description": "Test order"
}
```

4. Проверить статус заказа:
```
GET /api/orders/{orderId}
```

## Структура проекта

```
HSEGozon/
├── frontend/                          # Веб-интерфейс
│   ├── index.html                    # Главная страница
│   ├── style.css                     # Стили
│   └── app.js                         # JavaScript логика
├── src/
│   ├── HSEGozon.ApiGateway/
│   │   ├── Api/
│   │   │   ├── Controllers/
│   │   │   ├── Examples/
│   │   │   └── Filters/
│   │   └── Domain/
│   │       └── DTOs/
│   ├── HSEGozon.OrdersService/
│   │   ├── Abstractions/
│   │   ├── Api/
│   │   │   ├── Controllers/
│   │   │   ├── Examples/
│   │   │   └── Filters/
│   │   ├── Application/
│   │   │   └── OrderService.cs
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   ├── DTOs/
│   │   │   └── Messages/
│   │   └── Infrastructure/
│   │       ├── Data/
│   │       ├── Messaging/
│   │       ├── Repositories/
│   │       └── BackgroundServices/
│   └── HSEGozon.PaymentsService/
│       ├── Abstractions/
│       ├── Api/
│       │   ├── Controllers/
│       │   ├── Examples/
│       │   └── Filters/
│       ├── Application/
│       │   ├── AccountService.cs
│       │   └── PaymentProcessingService.cs
│       ├── Domain/
│       │   ├── Entities/
│       │   ├── DTOs/
│       │   └── Messages/
│       └── Infrastructure/
│           ├── Data/
│           ├── Messaging/
│           ├── Repositories/
│           └── BackgroundServices/
├── docker-compose.yml
└── README.md
```

### Архитектурные слои

Каждый микросервис следует принципам **Clean Architecture**:

- **Abstractions** - интерфейсы для всех зависимостей
- **Api** - слой представления:
  - **Controllers** - API endpoints
  - **Examples** - примеры для Swagger документации
  - **Filters** - фильтры Swagger
- **Application** - слой приложения (бизнес-логика, сервисы)
- **Domain** - доменный слой:
  - **Entities** - доменные сущности
  - **DTOs** - объекты передачи данных (запросы и ответы)
  - **Messages** - сообщения для межсервисного взаимодействия
- **Infrastructure** - инфраструктурный слой (внешние зависимости):
  - **Data** - конфигурация базы данных
  - **Messaging** - интеграция с RabbitMQ
  - **Repositories** - реализация доступа к данным (Entity Framework)
  - **BackgroundServices** - фоновые задачи (обработка очередей)

## Особенности реализации

1. **Идемпотентность**: Все операции обработки сообщений идемпотентны
2. **Атомарность**: Операции с балансом используют блокировки строк БД
3. **Надежность**: Использование транзакций БД для обеспечения консистентности
4. **Масштабируемость**: Асинхронная обработка через очереди сообщений
5. **Единообразные ответы**: Все ошибки (400, 404, 500) возвращаются в формате JSON
6. **Swagger документация**: Автоматическое заполнение примеров в Request Body при использовании "Try it out"
7. **Repository Pattern**: Разделение логики доступа к данным и бизнес-логики