using Xunit;
using ecommerce.Data;
using ecommerce.Controllers.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ecommerce.Models;
using ecommerce.Dtos;

namespace Ecommerce.Test
{
    public class UnitTest1
    {
        private readonly ProductController _controller;
        private readonly DbContextOptions<AppDbContext> _options;

        public UnitTest1()
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            SeedInMemoryDatabase();

            var dbContext = new AppDbContext(_options);
            _controller = new ProductController(dbContext);
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
            var categories = result.Value as IEnumerable<object>;

            // Assert
            Assert.NotNull(categories);
            Assert.NotEmpty(categories);

            var firstCategory = (categories.First() as IDictionary<string, object>)["Category"] as IDictionary<string, object>;
            var categoryName = firstCategory?["Name"]?.ToString();

            Assert.NotNull(categoryName);
            Assert.NotEmpty(categoryName);

            var products = firstCategory?["Products"] as IEnumerable<object>;

            Assert.NotNull(products);
            Assert.NotEmpty(products);

            var firstProduct = (products.First() as IDictionary<string, object>);
            var productName = firstProduct?["Name"]?.ToString();

            Assert.Equal("Product 1", productName);
        }

        private void SeedInMemoryDatabase()
        {
            using (var context = new AppDbContext(_options))
            {
                context.Categories.Add(new Category { Name = "Category 1" });
                context.Products.Add(new Product
                {
                    Name = "Product 1",
                    Price = 10.0m,
                    Description = "Product 1 Description",
                    SmallDescription = "Small description of Product 1"
                });
                context.SaveChanges();
            }
        }
    }
}
