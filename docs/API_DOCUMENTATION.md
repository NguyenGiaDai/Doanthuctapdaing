# API Documentation

## 📋 Tổng quan

Dự án Clean Architecture cung cấp RESTful API với các endpoints cho authentication, user management, và book management. API được document đầy đủ với Swagger/OpenAPI.

## 🔗 Base URLs

- **Local Development**: `https://localhost:5240`
- **Docker**: `http://localhost:3001`
- **Swagger UI**: `{baseUrl}/swagger/index.html`
- **Health Check**: `{baseUrl}/health`

## 🔐 Authentication

### JWT Token Flow
1. **Login** để lấy access token
2. **Include token** trong Authorization header
3. **Refresh token** khi access token hết hạn

### Headers Required
```http
Authorization: Bearer {access_token}
Content-Type: application/json
```

## 📚 API Endpoints

### 1. Authentication Endpoints

#### POST `/api/auth/register`
Đăng ký user mới

**Request Body:**
```json
{
  "userName": "string",
  "name": "string", 
  "email": "string",
  "password": "string"
}
```

**Validation Rules:**
- `userName`: Required, max 100 characters
- `name`: Required, max 100 characters
- `email`: Required, valid email format, max 100 characters
- `password`: Required, min 8 characters, must contain uppercase, lowercase, digit, special character

**Response:**
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "guid",
    "userName": "string",
    "email": "string"
  }
}
```

#### POST `/api/auth/login`
Đăng nhập user

**Request Body:**
```json
{
  "userName": "string",
  "password": "string",
  "rememberMe": boolean
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": 3600,
    "user": {
      "id": "guid",
      "userName": "string",
      "email": "string",
      "name": "string"
    }
  }
}
```

#### POST `/api/auth/refresh-token`
Làm mới access token

**Request Body:**
```json
{
  "refreshToken": "string"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "string",
    "refreshToken": "string",
    "expiresIn": 3600
  }
}
```

#### POST `/api/auth/logout`
Đăng xuất user

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "success": true,
  "message": "Logout successful"
}
```

### 2. User Management Endpoints

#### GET `/api/users/profile`
Lấy thông tin profile của user hiện tại

**Headers:** `Authorization: Bearer {token}`

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "userName": "string",
    "email": "string",
    "name": "string",
    "status": "Active",
    "avatarId": 123,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### PUT `/api/users/profile`
Cập nhật thông tin profile

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "name": "string",
  "email": "string"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": "guid",
    "userName": "string",
    "email": "string",
    "name": "string",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### POST `/api/users/change-password`
Thay đổi mật khẩu

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "currentPassword": "string",
  "newPassword": "string",
  "confirmPassword": "string"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Password changed successfully"
}
```

### 3. Book Management Endpoints

#### GET `/api/books`
Lấy danh sách books với pagination

**Query Parameters:**
- `page`: Số trang (default: 1)
- `pageSize`: Số items per page (default: 10)
- `search`: Tìm kiếm theo title hoặc description
- `sortBy`: Sắp xếp theo field (title, price, createdAt)
- `sortOrder`: Thứ tự sắp xếp (asc, desc)

**Example:** `GET /api/books?page=1&pageSize=10&search=csharp&sortBy=title&sortOrder=asc`

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "C# Programming",
        "description": "A comprehensive guide to C# programming",
        "price": 29.99,
        "createdAt": "2024-01-01T00:00:00Z",
        "updatedAt": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

#### GET `/api/books/{id}`
Lấy thông tin book theo ID

**Path Parameters:**
- `id`: Book ID (integer)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "C# Programming",
    "description": "A comprehensive guide to C# programming",
    "price": 29.99,
    "createdAt": "2024-01-01T00:00:00Z",
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### POST `/api/books`
Tạo book mới

**Headers:** `Authorization: Bearer {token}`

**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "price": 0.0
}
```

**Validation Rules:**
- `title`: Required, max 200 characters
- `description`: Required, max 1000 characters
- `price`: Required, must be >= 0

**Response:**
```json
{
  "success": true,
  "message": "Book created successfully",
  "data": {
    "id": 1,
    "title": "string",
    "description": "string",
    "price": 0.0,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

#### PUT `/api/books/{id}`
Cập nhật book

**Headers:** `Authorization: Bearer {token}`

**Path Parameters:**
- `id`: Book ID (integer)

**Request Body:**
```json
{
  "title": "string",
  "description": "string",
  "price": 0.0
}
```

**Response:**
```json
{
  "success": true,
  "message": "Book updated successfully",
  "data": {
    "id": 1,
    "title": "string",
    "description": "string",
    "price": 0.0,
    "updatedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### DELETE `/api/books/{id}`
Xóa book

**Headers:** `Authorization: Bearer {token}`

**Path Parameters:**
- `id`: Book ID (integer)

**Response:**
```json
{
  "success": true,
  "message": "Book deleted successfully"
}
```

### 4. Media Management Endpoints

#### POST `/api/media/upload`
Upload file

**Headers:** `Authorization: Bearer {token}`

**Request:** `multipart/form-data`
- `file`: File to upload (max 5MB)
- `mediaType`: Type of media (Image, Document, Video)

**Supported File Types:**
- Images: .jpg, .jpeg, .png
- Documents: .pdf
- Videos: .mp4, .avi

**Response:**
```json
{
  "success": true,
  "message": "File uploaded successfully",
  "data": {
    "id": 1,
    "fileName": "string",
    "fileUrl": "string",
    "fileSize": 1024,
    "mediaType": "Image",
    "uploadedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### GET `/api/media/{id}`
Lấy thông tin media

**Path Parameters:**
- `id`: Media ID (integer)

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "fileName": "string",
    "fileUrl": "string",
    "fileSize": 1024,
    "mediaType": "Image",
    "uploadedAt": "2024-01-01T00:00:00Z"
  }
}
```

#### DELETE `/api/media/{id}`
Xóa media

**Headers:** `Authorization: Bearer {token}`

**Path Parameters:**
- `id`: Media ID (integer)

**Response:**
```json
{
  "success": true,
  "message": "Media deleted successfully"
}
```

## 📊 Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... },
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error message",
  "errors": [
    {
      "code": "VALIDATION_ERROR",
      "message": "Field validation failed",
      "property": "email",
      "value": "invalid-email"
    }
  ],
  "timestamp": "2024-01-01T00:00:00Z"
}
```

## 🔍 Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `VALIDATION_ERROR` | 400 | Input validation failed |
| `UNAUTHORIZED` | 401 | Authentication required |
| `FORBIDDEN` | 403 | Insufficient permissions |
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource already exists |
| `INTERNAL_ERROR` | 500 | Server internal error |

## 🔒 Authorization Scopes

### User Scopes
- `user_read`: Đọc thông tin user
- `user_write`: Tạo/cập nhật user
- `book_read`: Đọc thông tin books
- `book_write`: Tạo/cập nhật books
- `media_read`: Đọc thông tin media
- `media_write`: Upload/delete media

### Role-based Access
- **Admin**: Full access to all endpoints
- **User**: Limited access to own resources
- **Guest**: Read-only access to public resources

## 📈 Rate Limiting

- **Authentication endpoints**: 5 requests per minute per IP
- **General API**: 100 requests per minute per user
- **File upload**: 10 requests per minute per user

## 🧪 Testing Endpoints

### Health Check
```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "checks": [
    {
      "name": "Database",
      "status": "Healthy",
      "duration": "00:00:00.1234567"
    },
    {
      "name": "External API",
      "status": "Healthy", 
      "duration": "00:00:00.2345678"
    }
  ],
  "totalDuration": "00:00:00.3456789"
}
```

### Swagger Documentation
- **Swagger UI**: `{baseUrl}/swagger/index.html`
- **OpenAPI JSON**: `{baseUrl}/swagger/v1/swagger.json`

## 📝 Example Usage

### cURL Examples

#### Register User
```bash
curl -X POST "https://localhost:5240/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john_doe",
    "name": "John Doe",
    "email": "john@example.com",
    "password": "SecurePass123!"
  }'
```

#### Login
```bash
curl -X POST "https://localhost:5240/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "john_doe",
    "password": "SecurePass123!",
    "rememberMe": true
  }'
```

#### Get Books
```bash
curl -X GET "https://localhost:5240/api/books?page=1&pageSize=10" \
  -H "Authorization: Bearer {access_token}"
```

#### Create Book
```bash
curl -X POST "https://localhost:5240/api/books" \
  -H "Authorization: Bearer {access_token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Advanced C# Programming",
    "description": "Deep dive into advanced C# concepts",
    "price": 49.99
  }'
```

### JavaScript Examples

#### Using Fetch API
```javascript
// Login
const loginResponse = await fetch('/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    userName: 'john_doe',
    password: 'SecurePass123!',
    rememberMe: true
  })
});

const loginData = await loginResponse.json();
const accessToken = loginData.data.accessToken;

// Get Books
const booksResponse = await fetch('/api/books', {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});

const booksData = await booksResponse.json();
```

#### Using Axios
```javascript
// Setup axios instance
const api = axios.create({
  baseURL: 'https://localhost:5240/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

// Add token to requests
api.interceptors.request.use(config => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Usage
const books = await api.get('/books');
const newBook = await api.post('/books', {
  title: 'New Book',
  description: 'Book description',
  price: 29.99
});
```

## 🔧 Configuration

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Server=localhost;Database=CleanArchitecture;Trusted_Connection=true;"

# JWT
Identity__Key="your-secret-key-here"
Identity__Issuer="CleanArchitecture"
Identity__Audience="CleanArchitecture-Users"

# File Storage
FileStorageSettings__LocalStorage=true
FileStorageSettings__MaxFileSize=5242880
```

### CORS Configuration
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://localhost:3000"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE"],
    "AllowedHeaders": ["*"]
  }
}
```

## 📚 SDK và Client Libraries

### .NET Client
```csharp
// Install NuGet package
Install-Package CleanArchitecture.Client

// Usage
var client = new CleanArchitectureClient("https://localhost:5240");
var books = await client.Books.GetAllAsync();
```

### JavaScript/TypeScript Client
```bash
# Install npm package
npm install @cleanarchitecture/client

# Usage
import { CleanArchitectureClient } from '@cleanarchitecture/client';

const client = new CleanArchitectureClient('https://localhost:5240');
const books = await client.books.getAll();
```

## 🚀 Performance Tips

1. **Use Pagination**: Luôn sử dụng pagination cho list endpoints
2. **Cache Responses**: Implement client-side caching
3. **Compress Requests**: Enable gzip compression
4. **Batch Operations**: Group multiple operations when possible
5. **Async Operations**: Use async/await for better performance

## 🔍 Debugging

### Enable Detailed Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "CleanArchitecture": "Debug"
    }
  }
}
```

### Request/Response Logging
Tất cả requests và responses được log với correlation ID để dễ dàng debug.

### Health Check Monitoring
Monitor application health qua `/health` endpoint để đảm bảo service availability.
