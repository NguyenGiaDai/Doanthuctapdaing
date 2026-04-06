# Clean Architecture Training Documentation

# Project Thực Tập - .NET 8 Clean Architecture

**Tiến độ hoàn thành bài tập:** ![Coverage](https://img.shields.io/badge/Progress-0%25-brightgreen)

## 📚 Tổng quan

Đây là bộ tài liệu training hoàn chỉnh cho dự án Clean Architecture .NET 8, được thiết kế để training nhân sự mới về kiến trúc phần mềm và best practices trong phát triển ứng dụng enterprise.

## 🎯 Mục tiêu

- Cung cấp template Clean Architecture hoàn chỉnh cho .NET 8
- Hướng dẫn training nhân sự mới từ cơ bản đến nâng cao
- Thực hành với các công nghệ và patterns hiện đại
- Chuẩn bị cho môi trường production thực tế

## 📖 Tài liệu có sẵn

### 1. [Kiến trúc Clean Architecture](./docs/ARCHITECTURE.md)

- Tổng quan về Clean Architecture principles
- Cấu trúc layers và dependency flow
- Design patterns được sử dụng
- Security architecture
- Performance considerations

### 2. [API Documentation](./docs/API_DOCUMENTATION.md)

- RESTful API endpoints chi tiết
- Request/Response formats
- Authentication & Authorization
- Error handling
- Example usage với cURL và JavaScript

### 3. [Hướng dẫn Setup](./docs/SETUP_GUIDE.md)

- Yêu cầu hệ thống
- Cài đặt từng bước
- Cấu hình database và JWT
- Chạy với Docker hoặc local
- Troubleshooting common issues

### 4. [Hướng dẫn Testing](./docs/TESTING_GUIDE.md)

- Unit testing với xUnit và Moq
- Integration testing
- Code coverage
- Performance testing
- CI/CD integration

### 5. [Curriculum Training](./docs/TRAINING_CURRICULUM.md)

- 8 tuần training program
- Từ cơ bản đến nâng cao
- Thực hành hands-on
- Assessment và evaluation
- Career development path

## 🚀 Bắt đầu nhanh

### 1. Clone Repository

```bash
git clone https://github.com/TechSphereX-Edu/CleanArchitecture.git
cd CleanArchitecture
```

### 2. Chạy với Docker

```bash
docker-compose up --build
```

### 3. Truy cập ứng dụng

- **API**: http://localhost:3001
- **Swagger**: http://localhost:3001/swagger/index.html
- **Health Check**: http://localhost:3001/health

## 🏗️ Cấu trúc dự án

```
clean-architecture/
├── src/
│   ├── CleanArchitecture/              # Main Web API
│   ├── CleanArchitecture.Shared/       # Shared models
│   ├── CleanArchitecture.UnitTests/    # Unit tests
│   └── CleanArchitecture.IntegrationTest/ # Integration tests
├── docs/                              # Documentation
├── docker-compose.yml                 # Docker configuration
├── Directory.Packages.props           # Package management
└── README.md
```

## 🛠️ Công nghệ sử dụng

### **Backend**

- **.NET 8** - Framework chính
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation

### **Testing**

- **xUnit** - Testing framework
- **Moq** - Mocking library
- **AutoFixture** - Test data generation
- **Coverlet** - Code coverage

### **DevOps**

- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **GitHub Actions** - CI/CD pipeline

### **Security**

- **JWT Bearer** - Authentication
- **ASP.NET Core Identity** - User management
- **BCrypt** - Password hashing
- **CORS** - Cross-origin resource sharing

## 📊 Tính năng chính

### **Authentication & Authorization**

- User registration và login
- JWT token management
- Role-based access control
- Password reset functionality

### **User Management**

- User profile management
- Avatar upload
- Password change
- Account status management

### **Book Management**

- CRUD operations
- Search và pagination
- File upload support
- Media management

### **API Features**

- RESTful API design
- Swagger/OpenAPI documentation
- Global exception handling
- Request/Response logging
- Performance monitoring

### **Testing**

- Unit tests cho business logic
- Integration tests cho API endpoints
- Code coverage reporting
- Automated testing pipeline

## 🎓 Training Path

### **Tuần 1-2: Foundation**

- .NET 8 fundamentals
- Clean Architecture principles
- Project setup và configuration

### **Tuần 3-4: Implementation**

- Domain và Application layer
- Data access layer
- Business logic implementation

### **Tuần 5-6: Testing**

- Unit testing strategies
- Integration testing
- Code coverage và quality

### **Tuần 7-8: Production**

- Security implementation
- Docker containerization
- CI/CD pipeline
- Monitoring và logging

## 📈 Learning Outcomes

Sau khi hoàn thành training, học viên sẽ có thể:

### **Technical Skills**

- [ ] Implement Clean Architecture từ đầu
- [ ] Viết comprehensive tests
- [ ] Sử dụng Docker cho containerization
- [ ] Implement security best practices
- [ ] Debug và troubleshoot applications

### **Soft Skills**

- [ ] Code review và feedback
- [ ] Documentation writing
- [ ] Team collaboration
- [ ] Problem-solving
- [ ] Continuous learning

## 🔧 Development Tools

### **Required**

- .NET 8.0 SDK
- Docker Desktop
- Visual Studio 2022 hoặc VS Code
- Git

### **Recommended**

- Postman hoặc Thunder Client
- SQL Server Management Studio
- GitHub Desktop
- Resharper (Optional)

## 📚 Additional Resources

### **Books**

- "Clean Architecture" by Robert C. Martin
- "Clean Code" by Robert C. Martin
- "Pro ASP.NET Core" by Adam Freeman

### **Online Courses**

- Microsoft Learn .NET
- Pluralsight .NET courses
- Udemy Clean Architecture courses

### **Communities**

- .NET Community
- Clean Architecture Facebook Group
- Stack Overflow
- GitHub Discussions

## 🤝 Contributing

### **How to Contribute**

1. Fork repository
2. Tạo feature branch
3. Commit changes
4. Push to branch
5. Tạo Pull Request

### **Code Standards**

- Follow C# coding conventions
- Write comprehensive tests
- Update documentation
- Use meaningful commit messages

## 📞 Support

### **Training Support**

- **Email**: support@techspherex.com
- **Slack**: #clean-architecture-training
- **Office Hours**: 2-4 PM mỗi ngày

### **Technical Support**

- **GitHub Issues**: Tạo issue trên repository
- **Documentation**: Kiểm tra docs/ folder
- **FAQ**: Xem troubleshooting guide

## 📄 License

Dự án này được license under MIT License. Xem file [LICENSE](LICENSE) để biết thêm chi tiết.

## 🙏 Acknowledgments

- Clean Architecture principles by TechSphereX
- .NET team cho excellent framework
- Community contributors
- Training team và mentors

---

**Chúc mừng! Bạn đã sẵn sàng để bắt đầu hành trình Clean Architecture! 🚀**

Hãy bắt đầu với [Setup Guide](./docs/SETUP_GUIDE.md) để cài đặt và chạy dự án, sau đó theo [Training Curriculum](./docs/TRAINING_CURRICULUM.md) để học từng bước một cách có hệ thống.
