# Clean Architecture Documentation

## 📋 Tổng quan

Dự án này là một template Clean Architecture hoàn chỉnh được xây dựng trên .NET 8, phục vụ mục đích training nhân sự mới về kiến trúc phần mềm và best practices.

## 🏗️ Kiến trúc tổng thể

```
┌─────────────────────────────────────────────────────────────┐
│                        Web Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │   Controllers   │  │   Middlewares   │  │ Extensions  │  │
│  └─────────────────┘  └─────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │    Services     │  │   Repositories  │  │   DTOs      │  │
│  └─────────────────┘  └─────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                      Domain Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │    Entities     │  │   Value Objects │  │  Services   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────┐
│                   Infrastructure Layer                      │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐  │
│  │   Data Access   │  │ External APIs   │  │   Logging   │  │
│  └─────────────────┘  └─────────────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## 📁 Cấu trúc thư mục

### 1. **Domain Layer** (`src/CleanArchitecture/Domain/`)
- **Entities**: Các thực thể nghiệp vụ
  - `ApplicationUser.cs` - User entity với Identity
  - `Book.cs` - Book entity
  - `Media.cs` - Media entity
  - `RefreshToken.cs` - Refresh token entity

- **Constants**: Các hằng số và error codes
  - `ErrorRespondCode.cs` - HTTP error codes
  - `ErrorMessage.cs` - Error messages
  - `ApplicationConstants.cs` - App constants

- **Authorization**: Custom authorization
  - `HasScopeHandler.cs` - Scope-based authorization
  - `HasScopeRequirement.cs` - Authorization requirements

### 2. **Application Layer** (`src/CleanArchitecture/Application/`)
- **Services**: Business logic services
  - `AuthService.cs` - Authentication logic
  - `BookService.cs` - Book management
  - `UserService.cs` - User management
  - `MediaService.cs` - File/media handling

- **Repositories**: Data access interfaces
  - `IBookRepository.cs` - Book data access
  - `IUserRepository.cs` - User data access
  - `GenericRepository.cs` - Generic CRUD operations

- **Common**: Shared utilities
  - `AppSettings.cs` - Configuration model
  - `Exceptions/` - Custom exceptions
  - `Interfaces/` - Common interfaces
  - `Utilities/` - Helper utilities

### 3. **Infrastructure Layer** (`src/CleanArchitecture/Infrastructure/`)
- **Data**: Entity Framework implementation
  - `ApplicationDbContext.cs` - Main DbContext
  - `Migrations/` - Database migrations
  - `UnitOfWork.cs` - Unit of Work pattern

- **External Services**: Third-party integrations
  - `HealthCheck/` - Health check implementations
  - `Interface/` - External service interfaces

### 4. **Web Layer** (`src/CleanArchitecture/Web/`)
- **Controllers**: API endpoints
  - `AuthController.cs` - Authentication endpoints
  - `BookController.cs` - Book management endpoints
  - `UserController.cs` - User management endpoints

- **Extensions**: Configuration extensions
  - `AuthenticationExtension.cs` - JWT setup
  - `SwaggerExtension.cs` - API documentation
  - `CorsExtension.cs` - CORS configuration

- **Middlewares**: Custom middlewares
  - `GlobalExceptionMiddleware.cs` - Exception handling
  - `PerformanceMiddleware.cs` - Performance monitoring
  - `LoggingMiddleware.cs` - Request logging

- **Validations**: Input validation
  - `RegisterRequestValidation.cs` - Registration validation
  - `LoginRequestValidation.cs` - Login validation
  - `FileUploadValidator.cs` - File upload validation

## 🔄 Dependency Flow

### Dependency Direction
```
Web → Application → Domain
Infrastructure → Application → Domain
```

### Key Principles
1. **Dependency Inversion**: High-level modules không phụ thuộc vào low-level modules
2. **Interface Segregation**: Clients không phụ thuộc vào interfaces không sử dụng
3. **Single Responsibility**: Mỗi class có một lý do duy nhất để thay đổi
4. **Open/Closed**: Open for extension, closed for modification

## 🛠️ Design Patterns

### 1. **Repository Pattern**
```csharp
public interface IBookRepository
{
    Task<Book> GetByIdAsync(int id);
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book> AddAsync(Book book);
    Task UpdateAsync(Book book);
    Task DeleteAsync(int id);
}
```

### 2. **Unit of Work Pattern**
```csharp
public interface IUnitOfWork
{
    IBookRepository BookRepository { get; }
    IUserRepository UserRepository { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### 3. **Dependency Injection**
```csharp
// Service registration
services.AddScoped<IBookService, BookService>();
services.AddScoped<IBookRepository, BookRepository>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

### 4. **CQRS (Command Query Responsibility Segregation)**
- **Commands**: Thay đổi state (Create, Update, Delete)
- **Queries**: Đọc data (Get, List, Search)

## 🔐 Security Architecture

### Authentication Flow
1. User gửi credentials qua `/api/auth/login`
2. Server validate credentials
3. Generate JWT token với claims
4. Return token cho client
5. Client sử dụng token trong subsequent requests

### Authorization Levels
- **Public**: Không cần authentication
- **Authenticated**: Cần valid JWT token
- **Scoped**: Cần specific scope permissions
  - `user_read`: Read operations
  - `user_write`: Write operations

### Security Features
- JWT token với expiration
- Refresh token mechanism
- Password hashing với BCrypt
- CORS configuration
- Input validation với FluentValidation

## 📊 Data Flow

### Request Flow
1. **Controller** nhận HTTP request
2. **Validation** kiểm tra input
3. **Service** xử lý business logic
4. **Repository** truy cập data
5. **Database** lưu trữ/retrieve data
6. **Response** trả về cho client

### Error Handling Flow
1. **Exception** xảy ra trong bất kỳ layer nào
2. **GlobalExceptionMiddleware** catch exception
3. **Logging** ghi lại error details
4. **Error Response** format và trả về client

## 🧪 Testing Strategy

### Unit Testing
- **Service Layer**: Test business logic
- **Repository Layer**: Test data access
- **Utility Classes**: Test helper methods

### Integration Testing
- **API Endpoints**: Test full request/response cycle
- **Database Operations**: Test với real database
- **External Services**: Test third-party integrations

### Test Structure
```
Tests/
├── Unit/
│   ├── Services/
│   ├── Repositories/
│   └── Utilities/
├── Integration/
│   ├── Controllers/
│   ├── Database/
│   └── External/
└── Shared/
    ├── Fixtures/
    └── Mocks/
```

## 🚀 Performance Considerations

### Caching Strategy
- **Memory Cache**: Cho frequently accessed data
- **Distributed Cache**: Cho multi-instance scenarios
- **Response Caching**: Cho static content

### Database Optimization
- **Connection Pooling**: Tối ưu database connections
- **Query Optimization**: Sử dụng proper indexing
- **Lazy Loading**: Load data khi cần thiết

### Monitoring
- **Health Checks**: Monitor application health
- **Performance Middleware**: Track response times
- **Logging**: Structured logging cho debugging

## 📈 Scalability

### Horizontal Scaling
- **Stateless Design**: Không lưu state trong memory
- **Database Sharding**: Phân chia data across databases
- **Load Balancing**: Distribute requests across instances

### Vertical Scaling
- **Resource Optimization**: Tối ưu memory và CPU usage
- **Async/Await**: Non-blocking operations
- **Background Services**: Process heavy tasks asynchronously

## 🔧 Configuration Management

### Environment-based Configuration
- **Development**: Local development settings
- **Docker**: Container-specific settings
- **Production**: Production environment settings

### Configuration Sources
1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific
3. Environment variables - Runtime overrides
4. User secrets - Development secrets

## 📝 Best Practices

### Code Organization
- **Single Responsibility**: Mỗi class có một purpose
- **DRY Principle**: Don't Repeat Yourself
- **SOLID Principles**: Follow SOLID design principles
- **Clean Code**: Readable và maintainable code

### Error Handling
- **Custom Exceptions**: Specific exception types
- **Global Exception Handler**: Centralized error handling
- **Logging**: Comprehensive logging strategy
- **User-friendly Messages**: Clear error messages

### Security
- **Input Validation**: Validate all inputs
- **Authentication**: Secure authentication flow
- **Authorization**: Proper permission checking
- **Data Protection**: Protect sensitive data

## 🎯 Learning Objectives

Sau khi hoàn thành training với dự án này, nhân sự sẽ:

1. **Hiểu Clean Architecture**: Các layer và dependency flow
2. **Thực hành Design Patterns**: Repository, UoW, DI
3. **Làm việc với .NET 8**: Modern C# features
4. **Viết Tests**: Unit và Integration tests
5. **Docker**: Containerization và deployment
6. **Security**: Authentication và Authorization
7. **Best Practices**: Code quality và maintainability

## 📚 Tài liệu tham khảo

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [Docker Documentation](https://docs.docker.com/)
