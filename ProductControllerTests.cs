using ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Dtos;
using ecommerce.Validators;
using Microsoft.AspNetCore.Http;
using System.Text;

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

            var dbContext = new AppDbContext(_options);
            _Context = dbContext;

            SeedInMemoryDatabase();

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
            var result = await _controller.CreateProduct(new CreateProductDTO());
            var data = result.Result;
            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(data);
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
                SmallDescription = GenerateRandomString(50, 100),
                Description = GenerateRandomString(500, 1000),
                CategoryId = 1,
            };

            // Act
            var result = await _controller.CreateProduct(productDTO);

            var data = result.Result;

            // Assert
            var okObjectResult = Assert.IsType<OkObjectResult>(data);
            var createdProduct = Assert.IsType<Product>(okObjectResult.Value);
            Assert.Equal(productDTO.Name, createdProduct.Name);
        }

        private string GenerateRandomString(int minLength, int maxLength)
        {
            var random = new Random();
            var length = random.Next(minLength, maxLength + 1);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private void SeedInMemoryDatabase()
        {
            var category = new Category { Name = "Category 1" };
            _Context.Categories.Add(category);
            _Context.Products.Add(new Product
            {
                Name = "Product 1",
                Price = 10.0m,
                Description = "Product 1 Description",
                SmallDescription = "Small description of Product 1",
                Category = category
            });
            _Context.SaveChanges();
        }
    }
}

