using ecommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ecommerce.Models;
using ecommerce.Dtos;

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

            SeedInMemoryDatabase();

            var dbContext = new AppDbContext(_options);
            _Context = dbContext;
            _controller = new ecommerce.Controllers.Users.ProductController(dbContext);
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
