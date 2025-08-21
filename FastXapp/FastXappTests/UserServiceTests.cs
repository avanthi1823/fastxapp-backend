using FastX.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NewFastBus.Tests
{
    public class UserServiceTest
    {
        private FastXContext _dbContext;
        private IUserRepository _userRepository;
        private IRepository<int, Bookings> _bookingRepository;
        private UserService _userService;

        [SetUp]
        public async Task Setup()
        {
            var dbOptions = new DbContextOptionsBuilder<FastXContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new FastXContext(dbOptions);
            _userRepository = new UserRepository(_dbContext);
            _bookingRepository = new BookingRepository(_dbContext);

            var role = new RoleMaster { RoleId = 1, RoleName = "User" };
            var user = new UserMaster
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@example.com",
                Gender = "Male",
                Phone = "1234567890",
                Role = role,
                Password = "hashedpassword123"
            };

            _dbContext.RoleMasters.Add(role);
            _dbContext.UserMasters.Add(user);
            await _dbContext.SaveChangesAsync();

            _userService = new UserService(_userRepository, _bookingRepository, _dbContext);
        }

        [Test]
        public async Task GetProfileAsync_ShouldReturnUserProfile()
        {
            // Act
            var profile = await _userService.GetProfileAsync(1);

            // Assert
            Assert.That(profile.FullName, Is.EqualTo("Test User"));
            Assert.That(profile.RoleName, Is.EqualTo("User"));
        }

        [Test]
        public async Task UpdateProfileAsync_ShouldUpdateUserDetails()
        {
            // Arrange
            var updateDto = new UserUpdateDto
            {
                FullName = "Updated User",
                Gender = "Female",
                Phone = "0987654321"
            };

            // Act
            await _userService.UpdateProfileAsync(1, updateDto);
            var updatedUser = await _userRepository.GetById(1);

            // Assert
            Assert.That(updatedUser.FullName, Is.EqualTo("Updated User"));
            Assert.That(updatedUser.Gender, Is.EqualTo("Female"));
            Assert.That(updatedUser.Phone, Is.EqualTo("0987654321"));
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
    }
}
