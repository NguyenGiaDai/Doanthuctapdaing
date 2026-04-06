# Curriculum Training Clean Architecture

## 📋 Tổng quan

Curriculum này được thiết kế để training nhân sự mới về Clean Architecture, .NET 8, và các best practices trong phát triển phần mềm. Thời gian training: **4-8 tuần** (40 giờ/tuần).

## 🎯 Mục tiêu học tập

Sau khi hoàn thành curriculum, học viên sẽ có thể:
- Hiểu và áp dụng Clean Architecture principles
- Phát triển ứng dụng .NET 8 với best practices
- Viết và maintain comprehensive tests
- Sử dụng Docker cho containerization
- Implement security và authentication
- Debug và troubleshoot applications
- Làm việc với team trong môi trường production

## 📚 Cấu trúc Curriculum

### **Tuần 1-2: Foundation & Clean Architecture**
**Mục tiêu**: Hiểu cơ bản về Clean Architecture và .NET 8

#### Tuần 1: .NET 8 Fundamentals


**Ngày 1-2: .NET 8 Basics**
- [ ] Cài đặt .NET 8 SDK và development tools
- [ ] Tạo console application đầu tiên
- [ ] Hiểu về C# 12 features
- [ ] Dependency Injection cơ bản
- [ ] Async/await patterns

**Thực hành**:
```csharp
// Tạo console app với DI
var services = new ServiceCollection();
services.AddScoped<IUserService, UserService>();
var provider = services.BuildServiceProvider();
```

**Ngày 3-4: ASP.NET Core Web API**
- [ ] Tạo Web API project
- [ ] Controllers và routing
- [ ] Model binding và validation
- [ ] Middleware pipeline
- [ ] Configuration management

**Thực hành**:
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsers()
    {
        // Implementation
    }
}
```

**Ngày 5: Entity Framework Core**
- [ ] Code First approach
- [ ] DbContext và DbSet
- [ ] Migrations
- [ ] Relationships (One-to-Many, Many-to-Many)
- [ ] Query optimization

**Thực hành**:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
    }
}
```

#### Tuần 2: Clean Architecture Principles


**Ngày 1-2: Architecture Layers**
- [ ] Domain Layer: Entities, Value Objects, Domain Services
- [ ] Application Layer: Use Cases, Interfaces, DTOs
- [ ] Infrastructure Layer: Data Access, External Services
- [ ] Web Layer: Controllers, Middleware, Extensions

**Thực hành**: Tạo project structure theo Clean Architecture

**Ngày 3-4: Design Patterns**
- [ ] Repository Pattern
- [ ] Unit of Work Pattern
- [ ] CQRS (Command Query Responsibility Segregation)
- [ ] Dependency Inversion Principle
- [ ] Interface Segregation

**Thực hành**:
```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IBookRepository Books { get; }
    Task<int> SaveChangesAsync();
}
```

**Ngày 5: Project Setup**
- [ ] Tạo solution structure
- [ ] Configure dependency injection
- [ ] Setup Entity Framework
- [ ] Configure logging
- [ ] Setup Swagger/OpenAPI

**Thực hành**: Setup dự án Clean Architecture hoàn chỉnh

### **Tuần 3-4: Business Logic & Data Access**
**Mục tiêu**: Implement business logic và data access layer

#### Tuần 3: Domain & Application Layer


**Ngày 1-2: Domain Layer Implementation**
- [ ] Entity design và relationships
- [ ] Value Objects
- [ ] Domain Events
- [ ] Business rules validation
- [ ] Domain Services

**Thực hành**:
```csharp
public class User : BaseEntity
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Status Status { get; set; }
    
    // Business methods
    public void ChangePassword(string newPassword)
    {
        // Business logic
    }
    
    public void Activate()
    {
        if (Status == Status.Inactive)
        {
            Status = Status.Active;
        }
    }
}
```

**Ngày 3-4: Application Layer Implementation**
- [ ] Service classes
- [ ] DTOs và mapping
- [ ] Use cases implementation
- [ ] Exception handling
- [ ] Validation logic

**Thực hành**:
```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    
    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<UserDTO> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"User with ID {id} not found");
            
        return _mapper.Map<UserDTO>(user);
    }
}
```

**Ngày 5: AutoMapper Configuration**
- [ ] Profile setup
- [ ] Custom value resolvers
- [ ] Conditional mapping
- [ ] Nested object mapping
- [ ] Performance optimization

**Thực hành**:
```csharp
public class MapProfile : Profile
{
    public MapProfile()
    {
        CreateMap<User, UserDTO>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
            
        CreateMap<CreateUserRequest, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => HashPassword(src.Password)));
    }
}
```

#### Tuần 4: Infrastructure & Data Access


**Ngày 1-2: Repository Implementation**
- [ ] Generic repository
- [ ] Specific repositories
- [ ] Query optimization
- [ ] Async operations
- [ ] Error handling

**Thực hành**:
```csharp
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<List<User>> GetActiveUsersAsync()
    {
        return await _context.Users
            .Where(u => u.Status == Status.Active)
            .ToListAsync();
    }
}
```

**Ngày 3-4: Unit of Work Pattern**
- [ ] Transaction management
- [ ] Multiple repository coordination
- [ ] Rollback handling
- [ ] Performance optimization
- [ ] Testing considerations

**Thực hành**:
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories;
    
    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }
    
    public IUserRepository Users => GetRepository<IUserRepository, UserRepository>();
    public IBookRepository Books => GetRepository<IBookRepository, BookRepository>();
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
    
    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }
}
```

**Ngày 5: External Services Integration**
- [ ] HTTP client setup
- [ ] Service interfaces
- [ ] Error handling
- [ ] Retry policies
- [ ] Circuit breaker pattern

**Thực hành**:
```csharp
public class ExternalApiService : IExternalApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiService> _logger;
    
    public async Task<ExternalData> GetDataAsync(string id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/data/{id}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ExternalData>(content);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get data from external API");
            throw new ExternalServiceException("External API is unavailable", ex);
        }
    }
}
```

### **Tuần 5-6: Testing & Quality Assurance**
**Mục tiêu**: Học cách viết và maintain comprehensive tests

#### Tuần 5: Unit Testing

**Ngày 1-2: Testing Fundamentals**
- [ ] xUnit framework
- [ ] Test structure (Arrange, Act, Assert)
- [ ] Naming conventions
- [ ] Test categories
- [ ] Mocking với Moq

**Thực hành**:
```csharp
public class UserServiceTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly UserService _userService;
    
    public UserServiceTest()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _userService = new UserService(_userRepositoryMock.Object, _mapperMock.Object);
    }
    
    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var userId = 1;
        var user = new User { Id = userId, UserName = "testuser" };
        var userDto = new UserDTO { Id = userId, UserName = "testuser" };
        
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId))
                          .ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserDTO>(user))
                  .Returns(userDto);
        
        // Act
        var result = await _userService.GetByIdAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        Assert.Equal("testuser", result.UserName);
    }
}
```

**Ngày 3-4: Advanced Testing Techniques**
- [ ] AutoFixture cho test data generation
- [ ] Test data builders
- [ ] Parameterized tests
- [ ] Test fixtures
- [ ] Integration testing setup

**Thực hành**:
```csharp
public class BookServiceTest : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly BookService _bookService;
    
    public BookServiceTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _bookService = new BookService(_fixture.UnitOfWork, _fixture.Mapper);
    }
    
    [Theory]
    [InlineData("C# Programming", 29.99)]
    [InlineData("Clean Architecture", 39.99)]
    public async Task CreateAsync_ShouldCreateBook_WithValidData(string title, double price)
    {
        // Arrange
        var request = new CreateBookRequest { Title = title, Price = price };
        
        // Act
        var result = await _bookService.CreateAsync(request);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.Equal(price, result.Price);
    }
}
```

**Ngày 5: Code Coverage & Quality**
- [ ] Code coverage analysis
- [ ] Coverage reports
- [ ] Quality gates
- [ ] Static code analysis
- [ ] Performance testing

**Thực hành**: Setup code coverage và generate reports

#### Tuần 6: Integration Testing


**Ngày 1-2: API Integration Testing**
- [ ] TestServer setup
- [ ] HTTP client testing
- [ ] Authentication testing
- [ ] End-to-end scenarios
- [ ] Database integration

**Thực hành**:
```csharp
public class BookControllerIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    
    public BookControllerIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }
    
    [Fact]
    public async Task GetBooks_ShouldReturnBooks_WhenCalled()
    {
        // Act
        var response = await _client.GetAsync("/api/books");
        
        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var books = JsonSerializer.Deserialize<ApiResponse<List<BookDTO>>>(content);
        
        Assert.NotNull(books);
        Assert.True(books.Success);
    }
}
```

**Ngày 3-4: Database Integration Testing**
- [ ] In-memory database
- [ ] Test database setup
- [ ] Transaction testing
- [ ] Migration testing
- [ ] Performance testing

**Thực hành**:
```csharp
public class DatabaseIntegrationTest : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    
    public DatabaseIntegrationTest()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _context = new ApplicationDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }
    
    [Fact]
    public async Task UserRepository_ShouldWorkWithRealDatabase()
    {
        // Arrange
        var user = new User { UserName = "testuser", Email = "test@example.com" };
        
        // Act
        var createdUser = await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        var retrievedUser = await _unitOfWork.Users.GetByIdAsync(createdUser.Id);
        
        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal("testuser", retrievedUser.UserName);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
}
```

**Ngày 5: External Service Testing**
- [ ] Mock external services
- [ ] Contract testing
- [ ] Error scenario testing
- [ ] Performance testing
- [ ] Monitoring và alerting

**Thực hành**: Test external API integrations

### **Tuần 7-8: Security, DevOps & Production**
**Mục tiêu**: Học về security, deployment và production readiness

#### Tuần 7: Security & Authentication


**Ngày 1-2: Authentication & Authorization**
- [ ] JWT token implementation
- [ ] ASP.NET Core Identity
- [ ] Role-based authorization
- [ ] Policy-based authorization
- [ ] Refresh token mechanism

**Thực hành**:
```csharp
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    
    public async Task<AuthResult> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new UnauthorizedException("Invalid credentials");
        }
        
        var token = await _tokenService.GenerateTokenAsync(user);
        var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user);
        
        return new AuthResult
        {
            AccessToken = token,
            RefreshToken = refreshToken,
            ExpiresIn = 3600
        };
    }
}
```

**Ngày 3-4: Security Best Practices**
- [ ] Input validation
- [ ] SQL injection prevention
- [ ] XSS protection
- [ ] CSRF protection
- [ ] HTTPS enforcement

**Thực hành**:
```csharp
public class RegisterRequestValidation : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidation()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MaximumLength(100).WithMessage("Username must not exceed 100 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");
            
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}
```

**Ngày 5: File Upload Security**
- [ ] File type validation
- [ ] File size limits
- [ ] Virus scanning
- [ ] Secure file storage
- [ ] Access control

**Thực hành**: Implement secure file upload functionality

#### Tuần 8: DevOps & Production


**Ngày 1-2: Docker & Containerization**
- [ ] Docker basics
- [ ] Multi-stage builds
- [ ] Docker Compose
- [ ] Container optimization
- [ ] Health checks

**Thực hành**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CleanArchitecture/CleanArchitecture.csproj", "src/CleanArchitecture/"]
RUN dotnet restore "src/CleanArchitecture/CleanArchitecture.csproj"
COPY . .
WORKDIR "/src/src/CleanArchitecture"
RUN dotnet build "CleanArchitecture.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CleanArchitecture.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CleanArchitecture.dll"]
```

**Ngày 3-4: CI/CD Pipeline**
- [ ] GitHub Actions
- [ ] Azure DevOps
- [ ] Automated testing
- [ ] Code quality gates
- [ ] Deployment strategies

**Thực hành**:
```yaml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    - name: Upload coverage
      uses: codecov/codecov-action@v3
```

**Ngày 5: Monitoring & Logging**
- [ ] Structured logging
- [ ] Application Insights
- [ ] Health checks
- [ ] Performance monitoring
- [ ] Error tracking

**Thực hành**:
```csharp
public class PerformanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMiddleware> _logger;
    private readonly Stopwatch _stopwatch;

    public PerformanceMiddleware(RequestDelegate next, ILogger<PerformanceMiddleware> logger, Stopwatch stopwatch)
    {
        _next = next;
        _logger = logger;
        _stopwatch = stopwatch;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _stopwatch.Restart();
        
        await _next(context);
        
        _stopwatch.Stop();
        
        if (_stopwatch.ElapsedMilliseconds > 1000)
        {
            _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                _stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 🎯 Assessment & Evaluation

### **Weekly Assessments**
- **Week 1-2**: Code review và architecture understanding
- **Week 3-4**: Business logic implementation và code quality
- **Week 5-6**: Test coverage và quality assurance
- **Week 7-8**: Security implementation và production readiness

### **Final Project**
Học viên sẽ implement một mini-project sử dụng tất cả concepts đã học:
- Clean Architecture structure
- Complete CRUD operations
- Authentication & Authorization
- Comprehensive testing
- Docker containerization
- CI/CD pipeline

### **Evaluation Criteria**
- **Code Quality**: Clean code, SOLID principles
- **Architecture**: Proper layer separation, dependency management
- **Testing**: Unit tests, integration tests, coverage
- **Security**: Authentication, validation, best practices
- **DevOps**: Docker, CI/CD, monitoring

## 📚 Learning Resources

### **Books**
- "Clean Architecture" by Robert C. Martin
- "Clean Code" by Robert C. Martin
- "Design Patterns" by Gang of Four
- "Pro ASP.NET Core" by Adam Freeman

### **Online Resources**
- [Microsoft .NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)

### **Tools & Extensions**
- Visual Studio 2022 / Visual Studio Code
- Postman / Thunder Client
- Docker Desktop
- Git / GitHub
- xUnit / NUnit
- Moq / NSubstitute

## 🏆 Certification

Sau khi hoàn thành curriculum, học viên sẽ nhận được:
- **Certificate of Completion**
- **Code Portfolio** (GitHub repository)
- **Project Documentation**
- **Performance Evaluation Report**

## 📞 Support & Mentoring

### **Mentoring Sessions**
- **Daily Standups**: 15 phút mỗi ngày
- **Weekly Reviews**: 1 giờ mỗi tuần
- **Code Reviews**: 2 lần/tuần
- **Q&A Sessions**: Theo yêu cầu

### **Support Channels**
- **Slack Channel**: #clean-architecture-training
- **Email Support**: support@techspherex.com
- **Office Hours**: 2-4 PM mỗi ngày
- **Emergency Support**: 24/7 cho critical issues

## 🔄 Continuous Learning

### **Post-Training**
- **Advanced Topics**: Microservices, Event Sourcing, CQRS
- **Cloud Platforms**: Azure, AWS, Google Cloud
- **Advanced Testing**: Contract Testing, Load Testing
- **Architecture Patterns**: Hexagonal, Onion, Event-Driven

### **Career Development**
- **Code Review Participation**
- **Mentoring Junior Developers**
- **Technical Blog Writing**
- **Conference Speaking**
- **Open Source Contributions**

## 📈 Success Metrics

### **Technical Skills**
- [ ] Có thể implement Clean Architecture từ đầu
- [ ] Viết được comprehensive tests (Unit + Integration)
- [ ] Setup và maintain Docker containers
- [ ] Implement security best practices
- [ ] Debug và troubleshoot production issues

### **Soft Skills**
- [ ] Code review và feedback
- [ ] Documentation writing
- [ ] Team collaboration
- [ ] Problem-solving
- [ ] Continuous learning

### **Project Deliverables**
- [ ] Working Clean Architecture application
- [ ] Complete test suite với >80% coverage
- [ ] Docker containerization
- [ ] CI/CD pipeline
- [ ] Production-ready documentation

---

**Chúc mừng! Bạn đã hoàn thành Clean Architecture Training Curriculum! 🎉**

Bây giờ bạn đã sẵn sàng để trở thành một .NET Developer chuyên nghiệp với kiến thức sâu về Clean Architecture và best practices!
