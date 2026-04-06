# Hướng dẫn Testing

## 📋 Tổng quan

Dự án Clean Architecture sử dụng comprehensive testing strategy bao gồm Unit Tests, Integration Tests, và Code Coverage. Tài liệu này hướng dẫn cách viết, chạy và maintain tests.

## 🏗️ Testing Architecture

### Testing Pyramid
```
    ┌─────────────────┐
    │   E2E Tests     │  ← 10% (Manual/Automated UI Tests)
    ├─────────────────┤
    │ Integration     │  ← 20% (API, Database, External Services)
    │ Tests           │
    ├─────────────────┤
    │   Unit Tests    │  ← 70% (Business Logic, Services, Utilities)
    └─────────────────┘
```

### Test Projects Structure
```
src/
├── CleanArchitecture.UnitTests/          # Unit Tests
│   ├── TestCase/                        # Test cases
│   ├── Extension/                       # Test utilities
│   └── ExternalService/                 # External service tests
├── CleanArchitecture.IntegrationTest/   # Integration Tests
│   ├── TestCase/                        # Integration test cases
│   ├── Shared/                          # Shared test utilities
│   └── Config/                          # Test configuration
└── CleanArchitecture/                   # Main application
```

## 🧪 Unit Testing

### Cấu trúc Unit Test

#### 1. Test Class Structure
```csharp
public class BookServiceTest : SetupTest
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private BookService _bookService;

    public BookServiceTest()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _bookService = new BookService(_unitOfWorkMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task BookService_GetById_ShouldReturnBook_WhenValidId()
    {
        // Arrange
        int bookId = 1;
        var expectedBook = new Book { Id = 1, Title = "Test Book" };
        var expectedDto = new BookDTO { Id = 1, Title = "Test Book" };

        _unitOfWorkMock.Setup(u => u.BookRepository.FirstOrDefaultAsync(b => b.Id == bookId, null))
                      .ReturnsAsync(expectedBook);
        _mapperMock.Setup(m => m.Map<BookDTO>(expectedBook)).Returns(expectedDto);

        // Act
        var result = await _bookService.Get(bookId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDto.Id, result.Id);
        Assert.Equal(expectedDto.Title, result.Title);
    }
}
```

#### 2. Test Naming Convention
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public async Task BookService_GetById_ShouldReturnBook_WhenValidId()
[Fact]
public async Task BookService_GetById_ShouldThrowException_WhenInvalidId()
[Fact]
public async Task BookService_Create_ShouldReturnCreatedBook_WhenValidData()
```

#### 3. AAA Pattern
```csharp
[Fact]
public async Task BookService_Create_ShouldReturnCreatedBook_WhenValidData()
{
    // Arrange - Setup test data and mocks
    var createRequest = new CreateBookRequest
    {
        Title = "New Book",
        Description = "Book Description",
        Price = 29.99
    };
    
    var expectedBook = new Book { Id = 1, Title = "New Book" };
    var expectedDto = new BookDTO { Id = 1, Title = "New Book" };

    _unitOfWorkMock.Setup(u => u.BookRepository.AddAsync(It.IsAny<Book>()))
                  .ReturnsAsync(expectedBook);
    _mapperMock.Setup(m => m.Map<Book>(createRequest)).Returns(expectedBook);
    _mapperMock.Setup(m => m.Map<BookDTO>(expectedBook)).Returns(expectedDto);

    // Act - Execute the method under test
    var result = await _bookService.Create(createRequest);

    // Assert - Verify the results
    Assert.NotNull(result);
    Assert.Equal(expectedDto.Id, result.Id);
    Assert.Equal(expectedDto.Title, result.Title);
    
    // Verify interactions
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}
```

### Test Utilities và Helpers

#### 1. SetupTest Base Class
```csharp
[ExcludeFromCodeCoverage]
public class SetupTest : IDisposable
{
    protected readonly IMapper _mapperConfig;
    protected readonly Fixture _fixture;
    protected readonly Mock<IUnitOfWork> _unitOfWorkMock;
    protected readonly ApplicationDbContext _dbContext;

    public SetupTest()
    {
        // AutoMapper configuration
        var mappingConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new MapProfile());
        });
        _mapperConfig = mappingConfig.CreateMapper();

        // AutoFixture for test data generation
        _fixture = new Fixture();

        // Mock setup
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        // In-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose() => _dbContext.Dispose();
}
```

#### 2. MockService Helper
```csharp
public static class MockService
{
    public static ILogger<T> GetMockLogger<T>()
    {
        var mockLogger = new Mock<ILogger<T>>();
        mockLogger.Setup(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<object>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<object, Exception, string>>()))
            .Verifiable();
        return mockLogger.Object;
    }

    public static IHttpClientFactory GetMockHttpFactory(HttpStatusCode statusCode, string content = "")
    {
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };

        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponse);

        var httpClient = new HttpClient(mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://mockedurl.com")
        };

        var mockFactory = new Mock<IHttpClientFactory>();
        mockFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);
        return mockFactory.Object;
    }
}
```

### Testing Different Layers

#### 1. Service Layer Tests
```csharp
public class AuthServiceTest : SetupTest
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IPasswordHasher<ApplicationUser>> _passwordHasherMock;
    private AuthService _authService;

    [Fact]
    public async Task Login_ShouldReturnToken_WhenValidCredentials()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            UserName = "testuser",
            Password = "TestPass123!"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        _userRepositoryMock.Setup(u => u.GetByUserNameAsync(loginRequest.UserName))
                          .ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, loginRequest.Password))
                          .Returns(PasswordVerificationResult.Success);
        _tokenServiceMock.Setup(t => t.GenerateToken(user))
                        .Returns("mock-jwt-token");

        // Act
        var result = await _authService.Login(loginRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("mock-jwt-token", result.AccessToken);
    }
}
```

#### 2. Repository Layer Tests
```csharp
public class BookRepositoryTest : SetupTest
{
    private BookRepository _bookRepository;

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBook_WhenBookExists()
    {
        // Arrange
        var book = new Book { Id = 1, Title = "Test Book" };
        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        _bookRepository = new BookRepository(_dbContext);

        // Act
        var result = await _bookRepository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal(book.Title, result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBookNotExists()
    {
        // Arrange
        _bookRepository = new BookRepository(_dbContext);

        // Act
        var result = await _bookRepository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }
}
```

#### 3. Utility Tests
```csharp
public class PasswordHelperTest
{
    [Theory]
    [InlineData("Password123!", true)]
    [InlineData("weak", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidPassword_ShouldReturnCorrectResult(string password, bool expected)
    {
        // Act
        var result = PasswordHelper.IsValidPassword(password);

        // Assert
        Assert.Equal(expected, result);
    }
}
```

## 🔗 Integration Testing

### API Integration Tests

#### 1. Test Controller với HttpClient
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
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/books");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var books = JsonSerializer.Deserialize<ApiResponse<List<BookDTO>>>(content);
        
        Assert.NotNull(books);
        Assert.True(books.Success);
    }

    [Fact]
    public async Task CreateBook_ShouldReturnCreatedBook_WhenValidData()
    {
        // Arrange
        var createRequest = new CreateBookRequest
        {
            Title = "Integration Test Book",
            Description = "Test Description",
            Price = 29.99
        };

        var json = JsonSerializer.Serialize(createRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/books", content);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<BookDTO>>(responseContent);
        
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal(createRequest.Title, result.Data.Title);
    }
}
```

#### 2. Database Integration Tests
```csharp
public class DatabaseIntegrationTest : IClassFixture<DatabaseFixture>
{
    private readonly ApplicationDbContext _context;
    private readonly DatabaseFixture _fixture;

    public DatabaseIntegrationTest(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _context = _fixture.Context;
    }

    [Fact]
    public async Task BookRepository_ShouldWorkWithRealDatabase()
    {
        // Arrange
        var repository = new BookRepository(_context);
        var book = new Book
        {
            Title = "Database Test Book",
            Description = "Test Description",
            Price = 29.99
        };

        // Act
        var createdBook = await repository.AddAsync(book);
        await _context.SaveChangesAsync();
        var retrievedBook = await repository.GetByIdAsync(createdBook.Id);

        // Assert
        Assert.NotNull(retrievedBook);
        Assert.Equal(book.Title, retrievedBook.Title);
        Assert.Equal(book.Price, retrievedBook.Price);
    }
}

public class DatabaseFixture : IDisposable
{
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
```

### External Service Integration Tests

#### 1. Health Check Tests
```csharp
public class HealthCheckIntegrationTest
{
    [Fact]
    public async Task GithubHealthCheck_ShouldBeHealthy_WhenGithubIsAccessible()
    {
        // Arrange
        var logger = MockService.GetMockLogger<GithubHealthCheck>();
        var httpClientFactory = MockService.GetMockHttpFactory(HttpStatusCode.OK);
        var healthCheck = new GithubHealthCheck(httpClientFactory, logger);
        var healthContext = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthContext);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("Github is healthy", result.Description);
    }

    [Fact]
    public async Task GithubHealthCheck_ShouldBeUnhealthy_WhenGithubIsDown()
    {
        // Arrange
        var logger = MockService.GetMockLogger<GithubHealthCheck>();
        var httpClientFactory = MockService.GetMockHttpFactory(HttpStatusCode.InternalServerError);
        var healthCheck = new GithubHealthCheck(httpClientFactory, logger);
        var healthContext = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(healthContext);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("Github is unhealthy", result.Description);
    }
}
```

## 📊 Code Coverage

### 1. Chạy Code Coverage

#### Sử dụng Coverlet
```bash
# Cài đặt coverlet tool
dotnet tool install --global coverlet.console

# Chạy tests với coverage
coverlet src/CleanArchitecture.UnitTests/bin/Debug/net8.0/CleanArchitecture.UnitTests.dll \
  --target "dotnet" \
  --targetargs "test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj --no-build" \
  --output "./coverage/" \
  --format opencover
```

#### Sử dụng Script có sẵn
```bash
# Windows
./code-coverage.bat

# Linux/macOS
./code-coverage.sh
```

### 2. Generate HTML Report
```bash
# Cài đặt reportgenerator
dotnet tool install --global dotnet-reportgenerator-globaltool

# Generate HTML report
reportgenerator \
  "-reports:./coverage/coverage.opencover.xml" \
  "-targetdir:./coverage/html" \
  -reporttypes:Html

# Mở report
start ./coverage/html/index.html
```

### 3. Coverage Configuration
```xml
<!-- trong .csproj file -->
<PropertyGroup>
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>opencover</CoverletOutputFormat>
  <CoverletOutput>./coverage/</CoverletOutput>
  <Exclude>[*.Tests]*,[*.Test]*</Exclude>
</PropertyGroup>
```

## 🎯 Testing Best Practices

### 1. Test Organization
```csharp
// ✅ Good: Group related tests
public class BookServiceTest
{
    [Fact]
    public async Task GetById_ShouldReturnBook_WhenValidId() { }
    
    [Fact]
    public async Task GetById_ShouldReturnNull_WhenInvalidId() { }
    
    [Fact]
    public async Task GetById_ShouldThrowException_WhenDatabaseError() { }
}

// ❌ Bad: Mixed concerns
public class MixedTest
{
    [Fact]
    public async Task BookService_GetById_ShouldWork() { }
    
    [Fact]
    public async Task UserService_Login_ShouldWork() { }
}
```

### 2. Test Data Management
```csharp
// ✅ Good: Use AutoFixture
public class BookServiceTest
{
    private readonly Fixture _fixture;

    public BookServiceTest()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public async Task Create_ShouldWork_WithValidData()
    {
        // Arrange
        var request = _fixture.Create<CreateBookRequest>();
        // ... rest of test
    }
}

// ✅ Good: Use Builder Pattern
public class BookBuilder
{
    private string _title = "Default Title";
    private string _description = "Default Description";
    private double _price = 0.0;

    public BookBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookBuilder WithPrice(double price)
    {
        _price = price;
        return this;
    }

    public Book Build()
    {
        return new Book
        {
            Title = _title,
            Description = _description,
            Price = _price
        };
    }
}
```

### 3. Mocking Best Practices
```csharp
// ✅ Good: Verify interactions
[Fact]
public async Task Create_ShouldCallRepository_WhenValidData()
{
    // Arrange
    var request = new CreateBookRequest { Title = "Test" };
    var book = new Book { Id = 1, Title = "Test" };

    _unitOfWorkMock.Setup(u => u.BookRepository.AddAsync(It.IsAny<Book>()))
                  .ReturnsAsync(book);

    // Act
    await _bookService.Create(request);

    // Assert
    _unitOfWorkMock.Verify(u => u.BookRepository.AddAsync(It.IsAny<Book>()), Times.Once);
    _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
}

// ❌ Bad: Over-mocking
[Fact]
public async Task Create_ShouldWork()
{
    // Don't mock everything - use real objects when possible
    var realMapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile<MapProfile>()));
    // ...
}
```

### 4. Assertion Best Practices
```csharp
// ✅ Good: Specific assertions
[Fact]
public async Task GetById_ShouldReturnCorrectBook()
{
    // Act
    var result = await _bookService.GetById(1);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("Expected Title", result.Title);
    Assert.True(result.Price > 0);
}

// ❌ Bad: Vague assertions
[Fact]
public async Task GetById_ShouldWork()
{
    // Act
    var result = await _bookService.GetById(1);

    // Assert
    Assert.NotNull(result); // Too vague
}
```

## 🚀 CI/CD Integration

### 1. GitHub Actions
```yaml
name: Tests

on: [push, pull_request]

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
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3
      with:
        file: ./coverage/coverage.opencover.xml
```

### 2. Azure DevOps
```yaml
trigger:
- main

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration)'

- task: DotNetCoreCLI@2
  displayName: 'Test'
  inputs:
    command: 'test'
    projects: '**/*.csproj'
    arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage" --logger trx --results-directory $(Agent.TempDirectory)'

- task: PublishTestResults@2
  displayName: 'Publish test results'
  inputs:
    testResultsFormat: 'VSTest'
    testResultsFiles: '**/*.trx'
    searchFolder: '$(Agent.TempDirectory)'

- task: PublishCodeCoverageResults@1
  displayName: 'Publish code coverage'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

## 📈 Performance Testing

### 1. Load Testing với k6
```javascript
// test.js
import http from 'k6/http';
import { check } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
};

export default function () {
  let response = http.get('http://localhost:3001/api/books');
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 500ms': (r) => r.timings.duration < 500,
  });
}
```

### 2. Chạy Performance Tests
```bash
# Cài đặt k6
# Windows: choco install k6
# macOS: brew install k6
# Linux: https://k6.io/docs/getting-started/installation/

# Chạy test
k6 run test.js
```

## 🔍 Debugging Tests

### 1. Visual Studio
- Set breakpoints trong test methods
- Sử dụng Test Explorer để run specific tests
- Attach debugger khi cần

### 2. Visual Studio Code
- Sử dụng C# extension
- Set breakpoints và run tests với debugger
- Sử dụng Test Explorer extension

### 3. Command Line Debugging
```bash
# Chạy specific test với debug info
dotnet test --logger "console;verbosity=detailed" --filter "ClassName=BookServiceTest"

# Chạy với debug symbols
dotnet test --configuration Debug
```

## 📚 Tài liệu tham khảo

- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [AutoFixture Documentation](https://autofixture.github.io/)
- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ASP.NET Core Testing](https://docs.microsoft.com/en-us/aspnet/core/test/)
- [Entity Framework Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
