using Moq;
using NUnit.Framework;
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
    public class SeatServiceTests
    {
        private Mock<ISeatRepository> _mockSeatRepo;
        private SeatService _seatService;

        [SetUp]
        public void Setup()
        {
            _mockSeatRepo = new Mock<ISeatRepository>();
            _seatService = new SeatService(_mockSeatRepo.Object);
        }

        [Test]
        public async Task GenerateSeatsForScheduleAsync_AddsCorrectNumberOfSeats()
        {
            // Arrange
            var addedSeats = new List<Seats>();

            _mockSeatRepo
                .Setup(r => r.AddRangeAsync(It.IsAny<List<Seats>>()))
                .Callback<List<Seats>>(seats => addedSeats.AddRange(seats))
                .Returns(Task.CompletedTask);

            int scheduleId = 42;
            int totalSeats = 3;

            // Act
            await _seatService.GenerateSeatsForScheduleAsync(scheduleId, totalSeats);

            // Assert
            Assert.AreEqual(totalSeats, addedSeats.Count);
            Assert.That(addedSeats, Has.All.Matches<Seats>(s => s.ScheduleId == scheduleId));
        }

        [Test]
        public async Task GetSeatsForScheduleAsync_ReturnsSeatsFromRepo()
        {
            // Arrange
            var seatDtos = new List<SeatDto>
            {
                new SeatDto { SeatNumber = "S1", IsBooked = false }
            };

            _mockSeatRepo
                .Setup(r => r.GetSeatsByScheduleAsync(10))
                .ReturnsAsync(seatDtos);

            // Act
            var result = await _seatService.GetSeatsForScheduleAsync(10);

            // Assert
            Assert.IsNotNull(result);
            var listResult = result.ToList();
            Assert.AreEqual(1, listResult.Count);
            Assert.AreEqual("S1", listResult[0].SeatNumber);
            Assert.IsFalse(listResult[0].IsBooked);
        }
    }
}
