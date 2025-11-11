using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorCrudDemo.Data.Contexts;
using BlazorCrudDemo.Data.Models;
using BlazorCrudDemo.Data.Repositories;
using BlazorCrudDemo.Tests.TestHelpers;
using BlazorCrudDemo.Web.Mapping;
using BlazorCrudDemo.Web.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlazorCrudDemo.Tests.Services
{
    public class ProductServiceTests : IClassFixture<TestFixture<Program>>, IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ILogger<ProductService>> _loggerMock;

        public ProductServiceTests(TestFixture<Program> factory)
        {
            // Create a new service provider and in-memory database for each test
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            // Create a new options instance using an in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            // Create a mock logger
            _loggerMock = new Mock<ILogger<ProductService>>();

            // Create the ApplicationDbContext
            _context = new ApplicationDbContext(
                new DbContextOptions<ApplicationDbContext>(),
                new AuditInterceptor(),
                new SoftDeleteInterceptor()
            );

            // Create the repositories and unit of work
            var productRepository = new ProductRepository(_context, _loggerMock.Object);
            var categoryRepository = new CategoryRepository(_context, Mock.Of<ILogger<CategoryRepository>>());
            
            // Initialize AutoMapper
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }).CreateMapper();

            _unitOfWork = new UnitOfWork(_context);
            _productService = new ProductService(
                _unitOfWork,
                productRepository,
                categoryRepository,
                mapper,
                _loggerMock.Object
            );

            // Seed the database with test data
            DatabaseInitializer.Initialize(_context);
        }

        [Fact]
        public async Task GetAllProductsAsync_ShouldReturnAllProducts()
        {
            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // We added 2 products in the initializer
            result.Should().Contain(p => p.Name == "Laptop");
            result.Should().Contain(p => p.Name == "Smartphone");
        }

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            const int productId = 1;

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(productId);
            result.Name.Should().Be("Laptop");
        }

        [Fact]
        public async Task CreateProductAsync_WithValidData_ShouldCreateProduct()
        {
            // Arrange
            var newProduct = new Product
            {
                Name = "New Product",
                Description = "New Product Description",
                Price = 199.99m,
                CategoryId = 1,
                StockQuantity = 5,
                IsActive = true
            };

            // Act
            var result = await _productService.CreateProductAsync(newProduct);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
            result.Name.Should().Be("New Product");

            // Verify the product was added to the database
            var productInDb = await _context.Products.FindAsync(result.Id);
            productInDb.Should().NotBeNull();
            productInDb.Name.Should().Be("New Product");
        }

        [Fact]
        public async Task UpdateProductAsync_WithValidData_ShouldUpdateProduct()
        {
            // Arrange
            var productToUpdate = await _context.Products.FindAsync(1);
            productToUpdate.Name = "Updated Laptop";
            productToUpdate.Price = 1099.99m;

            // Act
            var result = await _productService.UpdateProductAsync(productToUpdate);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Laptop");
            result.Price.Should().Be(1099.99m);

            // Verify the product was updated in the database
            var updatedProduct = await _context.Products.FindAsync(1);
            updatedProduct.Name.Should().Be("Updated Laptop");
            updatedProduct.Price.Should().Be(1099.99m);
        }

        [Fact]
        public async Task DeleteProductAsync_WithValidId_ShouldMarkAsInactive()
        {
            // Arrange
            const int productId = 1;

            // Act
            var result = await _productService.DeleteProductAsync(productId);

            // Assert
            result.Should().BeTrue();

            // Verify the product was marked as inactive (soft delete)
            var deletedProduct = await _context.Products.FindAsync(productId);
            deletedProduct.IsActive.Should().BeFalse();
        }

        [Fact]
        public async Task GetProductsByCategoryAsync_WithValidCategoryId_ShouldReturnFilteredProducts()
        {
            // Arrange
            const int categoryId = 1; // Electronics

            // Act
            var result = await _productService.GetProductsByCategoryAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // We have 2 products in Electronics category
            result.Should().OnlyContain(p => p.CategoryId == categoryId);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
