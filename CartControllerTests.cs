using ecommerce.Controllers.Users;
using ecommerce.Data;
using ecommerce.Dtos;
using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Ecommerce.Test
{
    public class CartControllerTests
    {
        private readonly CartController _controller;
        private readonly AppDbContext _context;

        public CartControllerTests()
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

/*            var mockUserManager = MockUserManager<User>();
            mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync((ClaimsPrincipal principal) => _context.Users.FirstOrDefault(u => u.Email == principal.Identity.Name));
*/
            var repo = new CartRepository(_context, mockHttpContextAccessor.Object);

            _controller = new CartController(repo)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContext
                }
            };
        }

        private void SeedInMemoryDatabase()
        {
            var category = new Category { Name = "Category 1" };

            _context.Users.Add(new User
            {
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
            _context.SaveChanges();
        }

        [Fact]
        public async Task Test_Add_To_Cart_Success()
        {
            var data = await _controller.AddToCart(new ecommerce.Dtos.ProductCartDto
            {
                ProductId = 1,
                Quantity = 1,
            });

            Assert.IsType<NoContentResult>(data);
        }

        [Fact]
        public async Task Test_Get_users_Cart_ReturnData()
        {
            await _controller.AddToCart(new ecommerce.Dtos.ProductCartDto
            {
                ProductId = 1,
                Quantity = 1,
            });

            var data = await _controller.GetCarts();
            var result = data as OkObjectResult;
            var carts = result.Value;
            Assert.IsType<OkObjectResult>(data);
            Assert.NotNull(carts);
            Assert.IsAssignableFrom<IEnumerable<CartDto>>(carts);
        }

        private void TruncateAllTables()
        {
            _context.Users.RemoveRange(_context.Users);

            _context.Categories.RemoveRange(_context.Categories);

            _context.Products.RemoveRange(_context.Products);

            _context.SaveChanges();
        }
    }
}
