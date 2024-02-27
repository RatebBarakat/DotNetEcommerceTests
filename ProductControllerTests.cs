using ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Dtos;
using ecommerce.Validators;

namespace Ecommerce.Test
{
    public class CategoryTests
    {
        private readonly ecommerce.Controllers.Admin.ProductController _controller;
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly AppDbContext _Context;

        public CategoryTests()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            SeedInMemoryDatabase();

            var dbContext = new AppDbContext(_options);
            _Context = dbContext;
            _controller = new ecommerce.Controllers.Admin.ProductController(dbContext,
                new ProductValidator(dbContext),
                new ExcelValidator(),
                new ecommerce.Services.ImageHelper()
                );
        }
        [Fact]
        public async Task CreateProduct_WithInvalidModel_ReturnsValidationError()
        {
            // Act
            var result = await _controller.CreateProduct(new CreateProductDTO()) as OkObjectResult;

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var validationErrors = Assert.IsType<SerializableError>(badRequestResult.Res);
            Assert.True(validationErrors.ContainsKey("SmallDescription"));
        }

        [Fact]
        public async Task CreateProduct_WithValidModel_ReturnsOkResult()
        {
            // Arrange
            var productDTO = new CreateProductDTO
            {
                Name = "Test Product",
                Quantity = 10,
                Price = 20,
                SmallDescription = "Test Description",
                Description = "Test Description",
                CategoryId = 1,
                Images = new List<Microsoft.AspNetCore.Http.IFormFile>()
            };

            // Act
            var result = await _controller.CreateProduct(productDTO);

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(result);
            var createdProduct = Assert.IsType<Product>(okObjectResult.Value);
            Assert.Equal(productDTO.Name, createdProduct.Name);
        }
        private void SeedInMemoryDatabase()
        {
            using (var context = new AppDbContext(_options))
            {
                var category = new Category { Name = "Category 1" };
                context.Categories.Add(category);
                context.Products.Add(new Product
                {
                    Name = "Product 1",
                    Price = 10.0m,
                    Description = "Product 1 Description",
                    SmallDescription = "Small description of Product 1",
                    Category = category
                });
                context.SaveChanges();
            }
        }
    }
}
