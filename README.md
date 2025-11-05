# Blazor CRUD Demo

A comprehensive Blazor Server application demonstrating enterprise-level CRUD operations with JWT authentication, role-based authorization, audit logging, and real-time features. This project showcases modern web development best practices using .NET 8 and Blazor Server.

![Blazor CRUD Demo](https://img.shields.io/badge/Blazor-5C2D91?logo=blazor&logoColor=white) 
![.NET](https://img.shields.io/badge/.NET_8-5C2D91?logo=.net&logoColor=white) 
![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white) 
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white) 
![JWT](https://img.shields.io/badge/JWT-black?logo=json-web-tokens) 
![SignalR](https://img.shields.io/badge/SignalR-%235C2D91?logo=microsoft%20azure&logoColor=white)

## Features

### 🔐 Authentication & Authorization
- **Hybrid Authentication**: Cookie-based (Blazor Server) + JWT (API endpoints)
- **User Registration & Login**: Complete authentication system with email/password
- **Password Recovery**: Forgot password functionality with email reset
- **User Management**: Admin panel for user administration
- **Role-Based Access Control (RBAC)**: Admin and User roles with policy-based authorization
- **Profile Management**: User profile editing and settings
- **Token Refresh**: Secure refresh token mechanism with automatic renewal
- **Account Security**: Account lockout protection, password hashing (PBKDF2)
- **Audit Logging**: Complete tracking of all authentication events

📖 **[Complete Authentication Documentation](AUTHENTICATION_README.md)** | **[Testing Guide](AUTHENTICATION_TESTING.md)**

### 📊 Dashboard
- **Real-time Statistics**: Live metrics and data visualization
- **Activity Feed**: Recent user and system activities
- **Quick Actions**: Fast access to common tasks
- **Responsive Design**: Optimized for all screen sizes

### 🏷️ Product Management
- **Complete CRUD Operations**: Create, Read, Update, Delete products
- **Advanced Search & Filtering**:
  - Multi-category filtering with checkboxes
  - Price range sliders
  - Date range selection
  - Real-time search with instant results
  - Tag-based filtering system
- **Image Upload**: Product image management with preview
- **Rich Text Editor**: HTML content support for product descriptions
- **Bulk Operations**: Multi-select and batch actions

### 📁 Category Management
- **Hierarchical Categories**: Parent-child category relationships
- **Category Organization**: Drag-and-drop ordering
- **Icon Support**: Custom icons for visual categorization
- **Display Management**: Category visibility and ordering controls

### 👥 Admin Panel
- **User Administration**: Complete user management interface
- **Audit Logging**: Comprehensive system audit trail
- **System Monitoring**: Performance metrics and health checks
- **Configuration Management**: Application settings interface

### 🔍 Advanced Features
- **Real-time Notifications**: SignalR-powered live updates
- **Background Services**: Automated maintenance and cleanup tasks
- **Error Handling**: Global exception handling with user-friendly error pages
- **Offline Support**: Network status detection and offline mode
- **State Management**: Global application state container
- **Audit Trail**: Complete activity logging for compliance
- **Data Validation**: Comprehensive client and server-side validation
- **Currency Input**: International currency formatting and validation

### 🚀 Technical Features
- **Clean Architecture**: Layered architecture with separation of concerns
- **Dependency Injection**: Comprehensive DI container setup
- **Middleware Pipeline**: Request/response logging and global error handling
- **Entity Framework Core**: Advanced ORM with migrations and seeding
- **AutoMapper**: Object-to-object mapping for DTOs
- **FluentValidation**: Model validation with custom rules
- **Serilog**: Structured logging with detailed context
- **Responsive Design**: Bootstrap 5 with mobile-first approach
- **Component Library**: Reusable UI components and controls

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / JetBrains Rider
- SQLite (included) or SQL Server
- Node.js (for frontend dependencies)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/BlazorCrudDemo.git
cd BlazorCrudDemo
```

2. **Database Setup**
   - SQLite is used by default (no additional setup required)
   - For SQL Server, update the connection string in `appsettings.json`
   - Run database migrations (see [Database Setup](DATABASE_SETUP.md))

3. **Run the application**
```bash
# Start the application
cd BlazorCrudDemo.Web
dotnet run
```

The database file (`blazorcrud.db`) will be created automatically with seed data.

### 4. Run the application
```bash
cd BlazorCrudDemo.Web
dotnet watch
```

### 5. Access the application
- Open your browser and navigate to `http://localhost:5120`
- **Default Admin Account**: 
  - Email: `admin@demo.com`
  - Password: `Admin123!`
- The dashboard will load with sample data for demonstration

## Project Structure

```
BlazorCrudDemo/
├── BlazorCrudDemo.Data/          # Data access layer
│   ├── Configurations/          # Entity Framework configurations
│   ├── Contexts/                # Database context & interceptors
│   ├── Interfaces/              # Repository interfaces
│   └── Repositories/            # Data repository implementations
│
├── BlazorCrudDemo.Shared/       # Shared models and utilities
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Models/                 # Domain models (Product, Category, User, Audit)
│   ├── Validators/             # Input validation rules
│   └── Exceptions/             # Custom exception types
│
└── BlazorCrudDemo.Web/          # Blazor Server application
    ├── BackgroundServices/      # Automated background tasks
    │   └── MaintenanceServices.cs
    │
    ├── Components/             # Reusable UI components
    │   ├── Categories/         # Category management components
    │   ├── Dashboard/         # Dashboard widgets and charts
    │   ├── Layout/            # Navigation and layout components
    │   ├── Products/          # Product form and display components
    │   ├── Search/            # Advanced search and filter components
    │   └── Shared/            # Common UI elements (inputs, modals, etc.)
    │
    ├── Configuration/         # Application configuration
    │   └── IdentityServerConfig.cs
    │
    ├── Hubs/                  # SignalR hubs for real-time features
    │   └── NotificationHub.cs
    │
    ├── Middleware/            # Custom middleware
    │   ├── GlobalExceptionHandlerMiddleware.cs
    │   └── RequestResponseLoggingMiddleware.cs
    │
    ├── Pages/                 # Application pages
    │   ├── Account/           # User profile and settings
    │   ├── Admin/             # Administrative functions
    │   ├── Auth/              # Authentication pages (Login, Register, etc.)
    │   ├── Categories.razor   # Category management
    │   ├── Dashboard/         # Main dashboard
    │   ├── Error.razor        # Error handling
    │   └── Products/          # Product management
    │
    ├── Services/              # Business logic services
    │   ├── AuthenticationService.cs
    │   ├── AuditService.cs
    │   ├── CategoryService.cs
    │   ├── ProductService.cs
    │   ├── UserService.cs
    │   └── NotificationService.cs
    │
    └── wwwroot/               # Static assets
        ├── css/              # Custom stylesheets
        ├── js/               # JavaScript files
        └── lib/              # Third-party libraries (Bootstrap, Font Awesome)
```

## Technologies Used

### Frontend
- **Blazor Server** - .NET web framework for interactive web UIs
- **Bootstrap 5** - Responsive CSS framework with mobile-first design
- **Font Awesome 6** - Comprehensive icon library
- **JavaScript Interop** - Client-side functionality integration

### Backend
- **.NET 8.0** - Cross-platform development framework
- **ASP.NET Core** - Web framework for hosting and middleware
- **Entity Framework Core 8.0** - Object-relational mapping with advanced features
- **SignalR** - Real-time web functionality for live updates
- **AutoMapper** - Object-to-object mapping for DTOs
- **FluentValidation** - Model validation with custom business rules

### Database & Storage
- **SQLite** - Lightweight, file-based database with full EF Core support
- **Entity Framework Core Migrations** - Database schema management and versioning
- **Audit Trail** - Complete activity logging and change tracking

### Authentication & Security
- **ASP.NET Core Identity** - User management and authentication
- **Duende IdentityServer** - OAuth 2.0 and OpenID Connect framework
- **Password Hashing** - Secure password storage and validation
- **Role-Based Authorization** - Permission-based access control

### Development & Monitoring
- **Serilog** - Structured logging with rich context information
- **Background Services** - Automated tasks and maintenance operations
- **Error Boundaries** - Graceful error handling and recovery
- **Request/Response Logging** - HTTP traffic monitoring and analysis
- **Network Status Detection** - Online/offline state management

### Additional Libraries
- **System.Text.Json** - High-performance JSON processing
- **Microsoft.Extensions** - Dependency injection and configuration
- **Humanizer** - Human-friendly text formatting
- **Blazor Components** - Rich UI component library

## Development

### Code Quality
- Comprehensive error handling with custom middleware
- Structured logging with detailed context information
- Input validation using FluentValidation
- Clean architecture with separation of concerns

### Performance
- Efficient database queries with Entity Framework
- Background services for maintenance tasks
- Optimized component rendering and state management

## Deployment

The application can be deployed to various hosting environments:

### Development Deployment
```bash
cd BlazorCrudDemo.Web
dotnet publish -c Release
```

### Production Deployment Options
- **IIS (Windows)**: Traditional Windows hosting with process isolation
- **Kestrel (Cross-platform)**: Lightweight cross-platform deployment
- **Docker containers**: Containerized deployment with consistent environments
- **Azure App Service**: Cloud hosting with scaling and monitoring
- **Linux hosting**: Native Linux deployment with systemd

### Docker Support
```bash
# Build the Docker image
docker build -t blazorcruddemo -f BlazorCrudDemo.Web/Dockerfile .

# Run the container
docker run -d -p 5120:80 --name blazor-app blazorcruddemo
```

### Environment Configuration
- **Development**: Uses `appsettings.Development.json` with relaxed security
- **Production**: Uses `appsettings.Production.json` with enhanced security settings
- **Connection Strings**: Configurable for different database providers
- **Logging**: Environment-specific logging levels and outputs

## Contributing

Contributions are welcome! Here's how to get started:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Make your changes** with clear, descriptive commit messages
4. **Test your changes** thoroughly across different scenarios
5. **Update documentation** for any new features or changes
6. **Submit a pull request** with a detailed description

### Development Guidelines
- Follow the existing code style and architectural patterns
- Add comprehensive tests for new functionality
- Update documentation for any new features
- Ensure all tests pass before submitting
- Follow security best practices for authentication and data handling

### Code Quality Standards
- Use meaningful variable and method names
- Add XML documentation to public APIs
- Handle exceptions gracefully with appropriate logging
- Follow SOLID principles and clean code practices
- Implement proper separation of concerns

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- **[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** - .NET web framework for building interactive web UIs
- **[Bootstrap](https://getbootstrap.com/)** - Responsive CSS framework for modern web design
- **[Font Awesome](https://fontawesome.com/)** - Comprehensive icon library and toolkit
- **[Serilog](https://serilog.net/)** - Structured logging framework for .NET applications
- **[SQLite](https://www.sqlite.org/)** - Self-contained, serverless SQL database engine
- **[Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)** - Object-relational mapping framework
- **[ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)** - Membership system for authentication
- **[SignalR](https://docs.microsoft.com/en-us/aspnet/core/signalr/)** - Real-time web functionality framework
- **[AutoMapper](https://automapper.org/)** - Object-to-object mapping library
- **[FluentValidation](https://fluentvalidation.net/)** - Validation library for .NET

## Support

If you encounter any issues or have questions:

- **📚 Documentation**: Check the [Wiki](https://github.com/edogola4/BlazorCrudDemo/wiki) for detailed guides
- **🐛 Bug Reports**: [Report issues](https://github.com/edogola4/BlazorCrudDemo/issues) on GitHub
- **💬 Discussions**: Start a [Discussion](https://github.com/edogola4/BlazorCrudDemo/discussions) for questions
- **📧 Email**: Contact the maintainers for private inquiries

---

<div align="center">
  <strong>Built with ❤️ using Blazor and .NET 8</strong>
  <br><br>
  <p>Show your support by starring this repository! ⭐</p>

  <a href="https://twitter.com/intent/tweet?text=Check%20out%20this%20awesome%20Blazor%20CRUD%20demo!&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_X-000000?style=for-the-badge&logo=x&logoColor=white" alt="Share on X">
  </a>
  <a href="https://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="Share on LinkedIn">
  </a>
</div>
