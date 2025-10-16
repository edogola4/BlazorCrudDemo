# Blazor CRUD Demo

A comprehensive Blazor Server application demonstrating CRUD (Create, Read, Update, Delete) operations with a clean, modern UI. This project showcases best practices for building data-driven web applications using .NET and Blazor.

![Blazor CRUD Demo](https://img.shields.io/badge/Blazor-5C2D91?logo=blazor&logoColor=white) ![.NET](https://img.shields.io/badge/.NET-5C2D91?logo=.net&logoColor=white) ![C#](https://img.shields.io/badge/C%23-239120?logo=c-sharp&logoColor=white)

## Features

- **Product Management**
  - Create, Read, Update, and Delete products
  - Product search and filtering
  - Category-based organization
  - Image upload support

- **Dashboard**
  - Real-time statistics and metrics
  - Activity feed
  - Quick actions

- **Advanced Search**
  - Multi-category filtering
  - Price range sliders
  - Date range selection
  - Tag-based filtering

- **Responsive Design**
  - Works on desktop and mobile devices
  - Clean, modern UI with Bootstrap 5
  - Font Awesome icons

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [VS Code](https://code.visualstudio.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or full version)

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/edogola4/BlazorCrudDemo.git
   cd BlazorCrudDemo
   ```

2. **Configure the database**
   - Update the connection string in `appsettings.json` to point to your SQL Server instance
   - Run the database migrations:
     ```bash
     cd BlazorCrudDemo.Data
     dotnet ef database update
     ```

3. **Run the application**
   ```bash
   cd BlazorCrudDemo.Web
   dotnet watch run
   ```

4. **Access the application**
   - Open a web browser and navigate to `https://localhost:5001`
   - The default admin credentials are:
     - Username: `admin@example.com`
     - Password: `Admin@123`

## Project Structure

```
BlazorCrudDemo/
├── BlazorCrudDemo.Data/          # Data access layer
│   ├── Configurations/          # Entity Framework configurations
│   ├── Contexts/                # Database context
│   └── Interfaces/              # Repository interfaces
│
├── BlazorCrudDemo.Shared/       # Shared models and DTOs
│   ├── DTOs/                   # Data Transfer Objects
│   ├── Models/                 # Domain models
│   └── Validators/             # FluentValidation validators
│
└── BlazorCrudDemo.Web/          # Blazor Server application
    ├── Components/             # Reusable UI components
    │   ├── Dashboard/         # Dashboard components
    │   ├── Layout/            # Layout components
    │   ├── Products/          # Product-related components
    │   └── Search/            # Search and filter components
    │
    ├── Pages/                  # Application pages
    │   └── Products/          # Product management pages
    │
    ├── Services/               # Application services
    └── wwwroot/                # Static files
```

## Technologies Used

- **Frontend**
  - Blazor Server
  - Bootstrap 5
  - Font Awesome 6
  - Syncfusion Blazor Components
  - JavaScript Interop for client-side functionality

- **Backend**
  - .NET 8.0
  - Entity Framework Core 8.0
  - ASP.NET Core Identity
  - AutoMapper
  - FluentValidation

- **Database**
  - SQL Server
  - Entity Framework Core Migrations

## API Documentation

The application exposes a RESTful API for programmatic access to the data. The API is documented using Swagger/OpenAPI.

To access the API documentation:
1. Run the application
2. Navigate to `/swagger`

## Testing

To run the unit tests:

```bash
dotnet test
```

## Deployment

The application can be deployed to various hosting environments including:
- Azure App Service
- Docker containers
- IIS
- Linux with Kestrel

### Docker Support

```bash
# Build the Docker image
docker build -t blazorcruddemo .

# Run the container
docker run -d -p 5000:80 --name blazor-app blazorcruddemo
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) - .NET web framework
- [Bootstrap](https://getbootstrap.com/) - CSS framework
- [Syncfusion Blazor Components](https://www.syncfusion.com/blazor-components) - UI component library
- [Font Awesome](https://fontawesome.com/) - Icon library

## Support

If you encounter any issues or have questions, please [open an issue](https://github.com/edogola4/BlazorCrudDemo/issues).

---

<div align="center">
  Made with ❤️ by Bran Don
  <br>
  <a href="https://twitter.com/intent/tweet?text=Check%20out%20this%20awesome%20Blazor%20CRUD%20demo!&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_X-000000?style=for-the-badge&logo=x&logoColor=white" alt="Share on X">
  </a>
  <a href="https://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fgithub.com%2Fedogola4%2FBlazorCrudDemo">
    <img src="https://img.shields.io/badge/Share_on_LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white" alt="Share on LinkedIn">
  </a>
</div>
