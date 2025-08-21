using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFastBus.Models.Entities;
using NewFastBus.Models.DTOs;
using NewFastBus.Services;

namespace NewFastBusTest
{
    [TestFixture]
    public class BusServiceTests
    {
        private Mock<IBusRepository> _busRepoMock;
        private BusService _busService;

        [SetUp]
        public void Setup()
        {
            _busRepoMock = new Mock<IBusRepository>();
            _busService = new BusService(_busRepoMock.Object);
        }

        [Test]
        public async Task AddBusAsync_ShouldReturnCreatedBus()
        {
            // Arrange
            var dto = new BusCreateDto
            {
                BusName = "Luxury Express",
                BusNumber = "LX123",
                BusTypeId = 1,
                NumberOfSeats = 50,
                OperatorId = 10
            };
            var busEntity = new Buses
            {
                BusName = dto.BusName,
                BusNumber = dto.BusNumber,
                BusTypeId = dto.BusTypeId,
                NumberOfSeats = dto.NumberOfSeats,
                OperatorId = dto.OperatorId
            };

            _busRepoMock.Setup(r => r.Add(It.IsAny<Buses>()))
                        .ReturnsAsync(busEntity);

            // Act
            var result = await _busService.AddBusAsync(10, dto);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Luxury Express", result.BusName);
        }

        [Test]
        public async Task GetMyBusesAsync_ShouldReturnMappedDtos()
        {
            // Arrange
            var buses = new List<Buses>
            {
                new Buses { BusName = "Bus 1", BusNumber = "B001", BusTypeId = 1, OperatorId = 5 },
                new Buses { BusName = "Bus 2", BusNumber = "B002", BusTypeId = 2, OperatorId = 5 }
            };

            _busRepoMock.Setup(r => r.GetByOperatorIdAsync(5))
                        .ReturnsAsync(buses);

            // Act
            var result = await _busService.GetMyBusesAsync(5);

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(b => b is BusCreateDto));
        }

        [Test]
        public async Task UpdateBusAsync_ShouldReturnTrue_WhenBusExistsAndOperatorMatches()
        {
            // Arrange
            var existingBus = new Buses { BusName = "Old Name", OperatorId = 7, BusTypeId = 1 };
            var dto = new BusCreateDto { BusName = "New Name", BusNumber = "B123", BusTypeId = 1, OperatorId = 7 };

            _busRepoMock.Setup(r => r.GetById(1))
                        .ReturnsAsync(existingBus);
         

            // Act
            var result = await _busService.UpdateBusAsync(7, 1, dto);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("New Name", existingBus.BusName);
        }

        [Test]
        public async Task UpdateBusAsync_ShouldReturnFalse_WhenBusNotFound()
        {
            // Arrange
            _busRepoMock.Setup(r => r.GetById(1))
                        .ReturnsAsync((Buses)null);

            // Act
            var result = await _busService.UpdateBusAsync(7, 1, new BusCreateDto());

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task DeleteBusAsync_ShouldReturnTrue_WhenBusExistsAndOperatorMatches()
        {
            // Arrange
            var existingBus = new Buses { OperatorId = 3 };
            _busRepoMock.Setup(r => r.GetById(2))
                        .ReturnsAsync(existingBus);
           

            // Act
            var result = await _busService.DeleteBusAsync(3, 2);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task DeleteBusAsync_ShouldReturnFalse_WhenBusNotFound()
        {
            // Arrange
            _busRepoMock.Setup(r => r.GetById(2))
                        .ReturnsAsync((Buses)null);

            // Act
            var result = await _busService.DeleteBusAsync(3, 2);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
