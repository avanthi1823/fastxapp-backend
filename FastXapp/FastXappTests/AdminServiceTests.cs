using Microsoft.EntityFrameworkCore;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NewFastBus.Tests
{
    public class AdminServiceTests
    {
        private FastXContext _dbContext;
        private AdminService _adminService;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<FastXContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new FastXContext(options);

          
            var role = new RoleMaster { RoleId = 1, RoleName = "User" };
            var user = new UserMaster
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com",
                Gender = "Male",
                Phone = "1234567890",
                Role = role,
                Password = "hashed"
            };

            _dbContext.RoleMasters.Add(role);
            _dbContext.UserMasters.Add(user);

            var booking = new Bookings
            {
                BookingId = 1,
                User = user,
                BookingDate = DateTime.UtcNow
            };

            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            _adminService = new AdminService(_dbContext);
        }

        [Test]
        public async Task GetAllUsersAsync_ShouldReturnUsers()
        {
            var result = await _adminService.GetAllUsersAsync();
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FullName, Is.EqualTo("Test User"));
        }

        [Test]
        public async Task DeleteUserAsync_ShouldRemoveUser()
        {
            var deleted = await _adminService.DeleteUserAsync(1);
            Assert.That(deleted, Is.True);
            Assert.That(_dbContext.UserMasters.Any(), Is.False);
        }

        [Test]
        public async Task DeleteBookingAsync_ShouldRemoveBooking()
        {
            var deleted = await _adminService.DeleteBookingAsync(1);
            Assert.That(deleted, Is.True);
            Assert.That(_dbContext.Bookings.Any(), Is.False);
        }

        [Test]
        public async Task AddRouteAsync_ShouldAddNewRoute()
        {
            var dto = new RouteDto { Origin = "CityA", Destination = "CityB" };
            var route = await _adminService.AddRouteAsync(dto);
            Assert.That(route, Is.Not.Null);
            Assert.That(_dbContext.RouteMasters.Count(), Is.EqualTo(1));
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
    }
}
