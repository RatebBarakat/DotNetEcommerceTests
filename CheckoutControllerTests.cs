using ecommerce.Controllers.Users;
using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace Ecommerce.Test
{
    public class CheckoutControllerTests
    {
        private readonly CheckoutController _controller;
        private readonly AppDbContext _context;

        public CheckoutControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new AppDbContext(options);
            TruncateAllTables();
            SeedInMemoryDatabase();

            var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, "rateb@example.com"),
            }, "mock"));

            var httpContext = new DefaultHttpContext
            {
                User = userClaims
            };

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

            var repo = new OrderRepository(new OrderItemRepository(),_context, mockHttpContextAccessor.Object);

            _controller = new CheckoutController(repo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        [Fact]

        public async Task Test_Fail_On_CartEmpty()
        {
            var result = await _controller.Checkout(new CreateOrderDto
            {
                Address = "test",
            });

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Test_Success_On_Cart_Not_Empty_AndCheck_Total()
        {
            _context.Carts.Add(new Cart
            {
                ProductId = 1,
                UserId = "sdsd",
                Quantity = 1,
            });

            _context.Carts.Add(new Cart
            {
                ProductId = 2,
                UserId = "sdsd",
                Quantity = 3,
            });

            var result = await _controller.Checkout(new CreateOrderDto
            {
                Address = "test",
            });

            Assert.IsType<NoContentResult>(result);

            var order = _context.Orders.Last();

            Assert.Equal(25m, order.Total);
        }


        private void SeedInMemoryDatabase()
        {
            var category = new Category { Name = "Category 1" };

            _context.Users.Add(new User
            {
                Id = "sdsd",
                UserName = "rateb",
                Email = "rateb@example.com",
                PasswordHash = "2392dsds",
                EmailConfirmed = true,
            });

            _context.Categories.Add(category);

            _context.Products.Add(new Product
            {
                Id = 1,
                Name = "Product 1",
                Price = 10.0m,
                Description = "Product 1 Description",
                SmallDescription = "Small description of Product 1",
                Category = category
            });

            _context.Products.Add(new Product
            {
                Id = 2,
                Name = "Product 2",
                Price = 5.0m,
                Description = "Product 2 Description",
                SmallDescription = "Small description of Product 2",
                Category = category
            });

            _context.SaveChanges();
        }

        private void TruncateAllTables()
        {
            _context.Users.RemoveRange(_context.Users);

            _context.Categories.RemoveRange(_context.Categories);

            _context.Products.RemoveRange(_context.Products);

            _context.Carts.RemoveRange(_context.Carts);

            _context.Orders.RemoveRange(_context.Orders);

            _context.SaveChanges();
        }
    }
}
