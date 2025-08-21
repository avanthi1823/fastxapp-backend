using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NewFastBus.Interfaces;
using NewFastBus.Services;

namespace NewFastBusTest
{
    [TestFixture]
    public class ScheduleServiceTests
    {
        private Mock<IScheduleRepository> _mockScheduleRepo;
        private Mock<IRepository<int, Buses>> _mockBusRepo;
        private Mock<ISeatService> _mockSeatService;
        private ScheduleService _service;

        [SetUp]
        public void Setup()
        {
            _mockScheduleRepo = new Mock<IScheduleRepository>();
            _mockBusRepo = new Mock<IRepository<int, Buses>>();
            _mockSeatService = new Mock<ISeatService>();

            _service = new ScheduleService(
                _mockScheduleRepo.Object,
                _mockBusRepo.Object,
                _mockSeatService.Object
            );
        }

        [Test]
        public async Task CreateScheduleAsync_ValidBus_AddsScheduleAndGeneratesSeats()
        {
            // Arrange
            var bus = new Buses { BusId = 1, OperatorId = 100, NumberOfSeats = 40, BusName = "SuperBus" };
            var dto = new ScheduleCreateDto
            {
                BusId = 1,
                RouteId = 5,
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(5),
                Fare = 500
            };

            var savedSchedule = new Schedules
            {
                ScheduleId = 10,
                BusId = 1,
                RouteId = 5,
                DepartureTime = dto.DepartureTime,
                ArrivalTime = dto.ArrivalTime,
                Fare = dto.Fare
            };

            _mockBusRepo.Setup(r => r.GetById(1)).ReturnsAsync(bus);
            _mockScheduleRepo.Setup(r => r.Add(It.IsAny<Schedules>())).ReturnsAsync(savedSchedule);

            // Act
            var result = await _service.CreateScheduleAsync(dto, 100);

            // Assert
            Assert.That(result.ScheduleId, Is.EqualTo(10));
            _mockScheduleRepo.Verify(r => r.Add(It.Is<Schedules>(s => s.BusId == 1 && s.RouteId == 5)), Times.Once);
            _mockSeatService.Verify(s => s.GenerateSeatsForScheduleAsync(10, 40), Times.Once);
        }

        [Test]
        public void CreateScheduleAsync_BusNotOwnedByOperator_ThrowsUnauthorized()
        {
            // Arrange
            var bus = new Buses { BusId = 1, OperatorId = 200 };
            var dto = new ScheduleCreateDto { BusId = 1, RouteId = 5, Fare = 500 };

            _mockBusRepo.Setup(r => r.GetById(1)).ReturnsAsync(bus);

            // Act & Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.CreateScheduleAsync(dto, 100));
        }

        [Test]
        public async Task GetSchedulesByOperatorAsync_ReturnsMappedDtos()
        {
            // Arrange
            var schedules = new List<Schedules>
            {
                new Schedules
                {
                    ScheduleId = 1,
                    Bus = new Buses { BusName = "BusA" },
                    Route = new RouteMaster { Origin = "CityA", Destination = "CityB" },
                    DepartureTime = DateTime.Today,
                    ArrivalTime = DateTime.Today.AddHours(3),
                    Fare = 250
                }
            };

            _mockScheduleRepo.Setup(r => r.GetByOperatorIdAsync(99)).ReturnsAsync(schedules);

            // Act
            var result = (await _service.GetSchedulesByOperatorAsync(99)).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].BusName, Is.EqualTo("BusA"));
            Assert.That(result[0].Route, Is.EqualTo("CityA to CityB"));
        }

        [Test]
        public async Task UpdateScheduleAsync_ValidOwnership_UpdatesAndReturnsTrue()
        {
            // Arrange
            var existingSchedule = new Schedules
            {
                ScheduleId = 5,
                Bus = new Buses { OperatorId = 101 },
                Fare = 100
            };

            var dto = new ScheduleCreateDto
            {
                DepartureTime = DateTime.Today,
                ArrivalTime = DateTime.Today.AddHours(2),
                Fare = 150,
                RouteId = 2
            };

            _mockScheduleRepo.Setup(r => r.GetById(5)).ReturnsAsync(existingSchedule);

            // Act
            var result = await _service.UpdateScheduleAsync(5, dto, 101);

            // Assert
            Assert.That(result, Is.True);
            _mockScheduleRepo.Verify(r => r.Update(5, existingSchedule), Times.Once);
        }

        [Test]
        public async Task UpdateScheduleAsync_NotOwner_ReturnsFalse()
        {
            var schedule = new Schedules { Bus = new Buses { OperatorId = 300 } };
            _mockScheduleRepo.Setup(r => r.GetById(1)).ReturnsAsync(schedule);

            var dto = new ScheduleCreateDto();
            var result = await _service.UpdateScheduleAsync(1, dto, 999);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteScheduleAsync_ValidOwnership_DeletesAndReturnsTrue()
        {
            var schedule = new Schedules { Bus = new Buses { OperatorId = 500 } };
            _mockScheduleRepo.Setup(r => r.GetById(2)).ReturnsAsync(schedule);

            var result = await _service.DeleteScheduleAsync(2, 500);

            Assert.That(result, Is.True);
            _mockScheduleRepo.Verify(r => r.Delete(2), Times.Once);
        }

        [Test]
        public async Task DeleteScheduleAsync_NotOwner_ReturnsFalse()
        {
            var schedule = new Schedules { Bus = new Buses { OperatorId = 777 } };
            _mockScheduleRepo.Setup(r => r.GetById(3)).ReturnsAsync(schedule);

            var result = await _service.DeleteScheduleAsync(3, 100);

            Assert.That(result, Is.False);
        }
    }
}
