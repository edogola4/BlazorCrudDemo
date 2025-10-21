# Blazor CRUD Demo

A comprehensive Blazor Server application demonstrating CRUD (Create, Read, Update, Delete) operations with a modern, responsive UI. This project showcases best practices for building data-driven web applications using .NET 8 and Blazor Server.

![Blazor CRUD Demo](https://img.shields.io/badge/Blazor-5C2D91?logo=blazor&logoColor=white) ![.NET](https://img.shields.io/badge/.NET_8-5C2D91?logo=.net&logoColor=white) ![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white) ![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite&logoColor=white)

## Features

### Dashboard
- Real-time statistics and metrics display
- Recent activity feed with timestamps
- Quick action buttons for common tasks
- Responsive design for all screen sizes

### Product Management
- Complete CRUD Operations: Create, Read, Update, Delete products
- Advanced Search & Filtering:
  - Multi-category filtering with checkboxes
  - Price range sliders
  - Date range selection
  - Tag-based filtering system
- Image Upload Support: Product image management
- Category Organization: Hierarchical product categorization

### Category Management
- Full category CRUD operations
- Hierarchical category structure
- Category-based product organization

### Advanced Search System
- Multi-Filter Support: Combine multiple search criteria
- Real-time Filtering: Instant results as you type
- Tag System: Advanced tagging for products
- Responsive Search UI: Works seamlessly on all devices

### Technical Features
- Error Handling: Comprehensive error boundaries and user-friendly error pages
- Logging: Structured logging with Serilog
- Background Services: Automated maintenance, cache cleanup, and data synchronization
- Middleware: Request/response logging and global exception handling
- Responsive Design: Bootstrap 5 with mobile-first approach
- Real-time Updates: SignalR integration for live data updates

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/), [VS Code](https://code.visualstudio.com/), or any .NET IDE
- No external database required (uses SQLite)

## Getting Started

### 1. Clone the repository
```bash
git clone https://github.com/edogola4/BlazorCrudDemo.git
cd BlazorCrudDemo
```

### 2. Database Setup
The application uses SQLite with automatic database creation and migrations:

```bash
cd BlazorCrudDemo.Web
dotnet watch
```

The database file (`blazorcrud.db`) will be created automatically in the `BlazorCrudDemo.Web` directory.

### 3. Run the application
```bash
cd BlazorCrudDemo.Web
dotnet watch
```

### 4. Access the application
- Open your browser and navigate to `http://localhost:5120`
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
│   ├── Models/                 # Domain models (Product, Category)
│   ├── Validators/             # Input validation rules
│   └── Exceptions/             # Custom exception types
│
└── BlazorCrudDemo.Web/          # Blazor Server application
    ├── BackgroundServices/      # Automated background tasks
    │   ├── CacheCleanupBackgroundService.cs
    │   ├── DataSyncBackgroundService.cs
    │   └── MaintenanceBackgroundService.cs
    │
    ├── Components/             # Reusable UI components
    │   ├── Categories/         # Category management components
    │   ├── Dashboard/         # Dashboard widgets and charts
    │   ├── Layout/            # Navigation and layout components
    │   ├── Products/          # Product form and display components
    │   ├── Search/            # Advanced search and filter components
    │   └── Shared/            # Common UI elements
    │
    ├── Hubs/                  # SignalR hubs for real-time features
    │   └── NotificationHub.cs
    │
    ├── Middleware/            # Custom middleware
    │   ├── GlobalExceptionHandlerMiddleware.cs
    │   └── RequestResponseLoggingMiddleware.cs
    │
    ├── Pages/                 # Application pages
    │   ├── Categories.razor   # Category management page
    │   ├── Dashboard/         # Main dashboard page
    │   ├── Error.razor        # Error handling page
    │   └── ProductListPage.razor # Product listing and search
    │
    ├── Services/              # Business logic services
    │   ├── CategoryService.cs
    │   ├── ProductService.cs
    │   ├── NotificationService.cs
    │   └── Error handling services
    │
    └── wwwroot/               # Static assets
        ├── css/              # Stylesheets
        ├── js/               # JavaScript files
        └── lib/              # Third-party libraries
```

## Technologies Used

### Frontend
- Blazor Server - .NET web framework for interactive web UIs
- Bootstrap 5 - Responsive CSS framework
- Font Awesome 6 - Icon library
- JavaScript Interop - Client-side functionality integration

### Backend
- .NET 8.0 - Cross-platform development framework
- ASP.NET Core - Web framework for hosting and middleware
- Entity Framework Core 8.0 - Object-relational mapping
- SignalR - Real-time web functionality
- AutoMapper - Object-to-object mapping
- FluentValidation - Model validation

### Database & Storage
- SQLite - Lightweight, file-based database
- Entity Framework Core Migrations - Database schema management

### Development & Monitoring
- Serilog - Structured logging
- Background Services - Automated tasks and maintenance
- Error Boundaries - Graceful error handling
- Request/Response Logging - HTTP traffic monitoring

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
- IIS (Windows)
- Kestrel (Cross-platform)
- Docker containers
- Azure App Service
- Linux hosting

### Docker Support
```bash
# Build the Docker image
docker build -t blazorcruddemo -f BlazorCrudDemo.Web/Dockerfile .

# Run the container
docker run -d -p 5120:80 --name blazor-app blazorcruddemo
```

## Contributing

Contributions are welcome! Here's how to get started:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes with clear, descriptive commit messages
4. Test your changes thoroughly
5. Submit a pull request with a detailed description

### Development Guidelines
- Follow the existing code style and patterns
- Add tests for new functionality
- Update documentation for any new features
- Ensure all tests pass before submitting

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) - .NET web framework
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Font Awesome](https://fontawesome.com/) - Icon library
- [Serilog](https://serilog.net/) - Logging framework
- [SQLite](https://www.sqlite.org/) - Database engine

## Support

If you encounter any issues or have questions:

- Check the [Wiki](https://github.com/edogola4/BlazorCrudDemo/wiki) for detailed documentation
- [Report issues](https://github.com/edogola4/BlazorCrudDemo/issues) on GitHub
- Start a [Discussion](https://github.com/edogola4/BlazorCrudDemo/discussions)

---

<div align="center">
  <strong>Made with ❤️ by Bran Don</strong>
  <br><br>
  <a href="https://twitter.com/intent/tweet?text=Check%20out%20this%20awesome%20Blazor%20CRUD%20demo!&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_X-000000?style=for-the-badge&logo=x&logoColor=white" alt="Share on X">
  </a>
  <a href="https://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="Share on LinkedIn">
  </a>
</div>
