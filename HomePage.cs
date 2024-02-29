using ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Dtos;
using Microsoft.AspNetCore.Http;
using Moq;
using ecommerce.Interfaces;

namespace Ecommerce.Test
{
    public class HomePage
    {
        private readonly ecommerce.Controllers.Users.ProductController _controller;
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly AppDbContext _Context;

        public HomePage()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            var dbContext = new AppDbContext(_options);
            _Context = dbContext;

            SeedInMemoryDatabase();

            var httpContext = new DefaultHttpContext
            {
                Request =
                {
                    Scheme = "http",
                    Host = new HostString("example.com")
                }
            };

            var mockCache = new Mock<IRedis>();
            mockCache.Setup(m => m.GetCachedDataAsync<string>("HomePageRandomCategories")).Returns(() => _Context.Categories.ToList());

            mockCache.Setup(m => m.GetCachedDataAsync<string>("categories")).Returns(() => _Context.Categories.ToList());

            _controller = new ecommerce.Controllers.Users.ProductController(dbContext, mockCache.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]
        public async Task Test_Ok_Result()
        {
            // Act
            var result = await _controller.GetHomePageData();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task TestFirstProductOfFirstCategory()
        {
            // Act
            var result = await _controller.GetHomePageData() as OkObjectResult;
            var categories = result.Value as IEnumerable<CategoryDto>;

            // Assert
            Assert.NotNull(categories);
            Assert.NotEmpty(categories);

            var firstCategory = categories.First();
            var categoryName = firstCategory.Name;

            Assert.NotNull(categoryName);
            Assert.NotEmpty(categoryName);

            var product = firstCategory.Products.First();

            Assert.NotNull(product);

            var productName = product.Name;

            Assert.Equal("Product 1", productName);
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
