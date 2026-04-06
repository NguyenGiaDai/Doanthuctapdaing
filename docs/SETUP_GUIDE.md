# Hướng dẫn Setup và Chạy Dự án

## 📋 Yêu cầu hệ thống

### Phần mềm cần thiết
- **.NET 8.0 SDK** hoặc mới hơn
- **Docker Desktop** (cho containerization)
- **SQL Server** (local development) hoặc **SQL Server in Docker**
- **Visual Studio 2022** hoặc **Visual Studio Code**
- **Git** (để clone repository)

### Kiểm tra phiên bản
```bash
# Kiểm tra .NET version
dotnet --version

# Kiểm tra Docker version
docker --version

# Kiểm tra Git version
git --version
```

## 🚀 Cài đặt nhanh

### 1. Clone Repository
```bash
git clone https://github.com/your-org/clean-architecture.git
cd clean-architecture
```

### 2. Chạy với Docker (Khuyến nghị)
```bash
# Build và chạy tất cả services
docker-compose up --build

# Chạy ở background
docker-compose up -d --build
```

### 3. Truy cập ứng dụng
- **API**: http://localhost:3001
- **Swagger UI**: http://localhost:3001/swagger/index.html
- **Health Check**: http://localhost:3001/health

## 🛠️ Cài đặt chi tiết

### Bước 1: Cài đặt .NET 8.0 SDK

#### Windows
1. Tải .NET 8.0 SDK từ [Microsoft](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Chạy installer và làm theo hướng dẫn
3. Restart terminal/command prompt

#### macOS
```bash
# Sử dụng Homebrew
brew install --cask dotnet-sdk

# Hoặc tải từ Microsoft
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Linux (Ubuntu/Debian)
```bash
# Thêm Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Cài đặt .NET 8.0 SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### Bước 2: Cài đặt Docker Desktop

#### Windows
1. Tải Docker Desktop từ [Docker](https://www.docker.com/products/docker-desktop)
2. Chạy installer
3. Restart máy tính
4. Mở Docker Desktop và đợi khởi động

#### macOS
```bash
# Sử dụng Homebrew
brew install --cask docker

# Hoặc tải từ Docker
# https://www.docker.com/products/docker-desktop
```

#### Linux
```bash
# Cài đặt Docker Engine
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Thêm user vào docker group
sudo usermod -aG docker $USER

# Logout và login lại
```

### Bước 3: Cài đặt SQL Server (Optional - nếu không dùng Docker)

#### Windows
1. Tải SQL Server Express từ [Microsoft](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. Chạy installer và chọn "Basic" installation
3. Ghi nhớ password cho SA account

#### macOS/Linux
```bash
# Sử dụng Docker cho SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
   -p 1433:1433 --name sql1 --hostname sql1 \
   -d mcr.microsoft.com/mssql/server:2019-latest
```

## 🔧 Cấu hình dự án

### 1. Cấu hình Database Connection

#### Sử dụng Docker (Khuyến nghị)
Không cần cấu hình gì thêm, Docker sẽ tự động setup database.

#### Sử dụng SQL Server Local
Chỉnh sửa file `src/CleanArchitecture/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitecture;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

#### Sử dụng SQL Server với Username/Password
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CleanArchitecture;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"
  }
}
```

### 2. Cấu hình JWT Settings

Chỉnh sửa file `src/CleanArchitecture/appsettings.json`:

```json
{
  "Identity": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "CleanArchitecture",
    "Audience": "CleanArchitecture-Users",
    "ExpireTimeInMinutes": 60,
    "RefreshTokenExpireTimeInDays": 7,
    "IsLocal": true,
    "ValidateHttps": false,
    "ScopeBaseDomain": "CleanArchitecture"
  }
}
```

### 3. Cấu hình File Storage

#### Local Storage (Mặc định)
```json
{
  "FileStorageSettings": {
    "LocalStorage": true,
    "MaxFileSize": 5242880,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".pdf", ".mp4"],
    "UploadPath": "uploads"
  }
}
```

#### Cloudinary Storage
```json
{
  "FileStorageSettings": {
    "LocalStorage": false,
    "Cloudinary": {
      "CloudName": "your-cloud-name",
      "ApiKey": "your-api-key",
      "ApiSecret": "your-api-secret"
    }
  }
}
```

## 🚀 Chạy dự án

### Phương pháp 1: Docker (Khuyến nghị)

#### Chạy tất cả services
```bash
# Từ thư mục gốc của dự án
docker-compose up --build

# Chạy ở background
docker-compose up -d --build

# Xem logs
docker-compose logs -f

# Dừng services
docker-compose down
```

#### Chỉ chạy database
```bash
# Chạy chỉ SQL Server
docker-compose up sqlserver

# Chạy database ở background
docker-compose up -d sqlserver
```

### Phương pháp 2: Local Development

#### Bước 1: Restore packages
```bash
# Từ thư mục gốc
dotnet restore

# Hoặc từ thư mục solution
dotnet restore CleanArchitecture.sln
```

#### Bước 2: Chạy database migrations
```bash
# Từ thư mục src/CleanArchitecture
dotnet ef database update

# Hoặc nếu chưa có migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Bước 3: Chạy ứng dụng
```bash
# Từ thư mục src/CleanArchitecture
dotnet run

# Hoặc với specific environment
dotnet run --environment Development

# Hoặc với hot reload
dotnet watch run
```

### Phương pháp 3: Visual Studio

1. Mở file `CleanArchitecture.sln` trong Visual Studio
2. Set `CleanArchitecture` project làm Startup Project
3. Nhấn F5 hoặc click "Start Debugging"
4. Visual Studio sẽ tự động restore packages và chạy migrations

## 🧪 Chạy Tests

### Unit Tests
```bash
# Chạy tất cả unit tests
dotnet test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj

# Chạy với coverage
dotnet test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj --collect:"XPlat Code Coverage"

# Chạy specific test class
dotnet test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj --filter "ClassName=BookServiceTest"
```

### Integration Tests
```bash
# Chạy integration tests
dotnet test src/CleanArchitecture.IntegrationTest/CleanArchitecture.IntegrationTest.csproj

# Chạy với specific test
dotnet test src/CleanArchitecture.IntegrationTest/CleanArchitecture.IntegrationTest.csproj --filter "ClassName=BookTest"
```

### Code Coverage
```bash
# Sử dụng script có sẵn
./code-coverage.bat

# Hoặc manual
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet tool install --global coverlet.console

# Chạy tests với coverage
coverlet src/CleanArchitecture.UnitTests/bin/Debug/net8.0/CleanArchitecture.UnitTests.dll \
  --target "dotnet" \
  --targetargs "test src/CleanArchitecture.UnitTests/CleanArchitecture.UnitTests.csproj --no-build" \
  --output "./coverage/" \
  --format opencover

# Generate HTML report
reportgenerator \
  "-reports:./coverage/coverage.opencover.xml" \
  "-targetdir:./coverage/html" \
  -reporttypes:Html
```

## 🔍 Kiểm tra ứng dụng

### 1. Health Check
```bash
# Kiểm tra health của ứng dụng
curl http://localhost:3001/health

# Hoặc mở browser
# http://localhost:3001/health
```

### 2. Swagger Documentation
Mở browser và truy cập:
- **Swagger UI**: http://localhost:3001/swagger/index.html
- **OpenAPI JSON**: http://localhost:3001/swagger/v1/swagger.json

### 3. Test API Endpoints

#### Register User
```bash
curl -X POST "http://localhost:3001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "name": "Test User",
    "email": "test@example.com",
    "password": "TestPass123!"
  }'
```

#### Login
```bash
curl -X POST "http://localhost:3001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "testuser",
    "password": "TestPass123!",
    "rememberMe": true
  }'
```

#### Get Books
```bash
curl -X GET "http://localhost:3001/api/books" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

## 🐛 Troubleshooting

### Lỗi thường gặp

#### 1. Port đã được sử dụng
```bash
# Kiểm tra port đang được sử dụng
netstat -ano | findstr :3001

# Kill process sử dụng port
taskkill /PID <PID_NUMBER> /F

# Hoặc thay đổi port trong docker-compose.yml
ports:
  - "3002:8080"  # Thay 3001 thành 3002
```

#### 2. Database connection failed
```bash
# Kiểm tra SQL Server có chạy không
docker ps | grep sqlserver

# Restart SQL Server container
docker-compose restart sqlserver

# Kiểm tra logs
docker-compose logs sqlserver
```

#### 3. Migration errors
```bash
# Xóa database và tạo lại
dotnet ef database drop
dotnet ef database update

# Hoặc xóa migrations và tạo lại
rm -rf src/CleanArchitecture/Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 4. Package restore failed
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Hoặc xóa bin/obj folders
rm -rf src/*/bin src/*/obj
dotnet restore
```

#### 5. Docker build failed
```bash
# Clean Docker cache
docker system prune -a

# Rebuild without cache
docker-compose build --no-cache

# Hoặc xóa images cũ
docker rmi $(docker images -q)
```

### Debug Mode

#### Visual Studio
1. Set breakpoints trong code
2. Nhấn F5 để start debugging
3. Sử dụng Debug Console để inspect variables

#### Visual Studio Code
1. Mở Command Palette (Ctrl+Shift+P)
2. Chọn "Debug: Start Debugging"
3. Chọn ".NET Core" configuration

#### Command Line
```bash
# Chạy với debug symbols
dotnet run --configuration Debug

# Attach debugger
dotnet run --configuration Debug --launch-profile "Development"
```

## 📊 Monitoring và Logging

### Application Logs
```bash
# Xem logs trong Docker
docker-compose logs -f api

# Xem logs của specific service
docker-compose logs -f sqlserver
```

### Performance Monitoring
- **Health Check UI**: http://localhost:3001/healthcheck-ui
- **Application Metrics**: Check console output for performance middleware logs

### Database Monitoring
```bash
# Connect to SQL Server container
docker exec -it clean-architecture_sqlserver_1 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'MyPass@word'

# List databases
SELECT name FROM sys.databases;

# Use CleanArchitecture database
USE CleanArchitecture;

# List tables
SELECT name FROM sys.tables;
```

## 🔧 Development Tools

### Recommended Extensions (VS Code)
- **C# for Visual Studio Code**
- **Docker**
- **REST Client**
- **Thunder Client**
- **GitLens**

### Recommended Extensions (Visual Studio)
- **Resharper** (Optional)
- **Git Extensions**
- **SQL Server Management Studio**

### Postman Collection
Import file `CleanArchitecture.postman_collection.json` vào Postman để test API endpoints.

## 📚 Tài liệu tham khảo

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Docker Documentation](https://docs.docker.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/)

## 🆘 Hỗ trợ

Nếu gặp vấn đề trong quá trình setup:

1. **Kiểm tra logs**: Xem logs của Docker containers
2. **Kiểm tra ports**: Đảm bảo ports 3001, 1433 không bị conflict
3. **Kiểm tra firewall**: Đảm bảo firewall không block ports
4. **Restart services**: Thử restart Docker hoặc ứng dụng
5. **Clean build**: Xóa bin/obj folders và rebuild

### Liên hệ hỗ trợ
- **Email**: support@yourcompany.com
- **Slack**: #clean-architecture-support
- **GitHub Issues**: Tạo issue trên repository
