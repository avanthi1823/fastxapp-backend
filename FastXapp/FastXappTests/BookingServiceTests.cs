

using Microsoft.EntityFrameworkCore;
using NewFastBus.Exceptions;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NewFastBus.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewFastBusTest
{
    [TestFixture]
    public class BookingServiceTests
    {
        private FastXContext _context = null!;
        private BookingService _bookingService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastXContext>()
                .UseInMemoryDatabase("TestDb_" + Guid.NewGuid())
                .Options;

            _context = new FastXContext(options);

            
            _context.Seats.AddRange(new List<Seats>
            {
                new Seats { SeatId = 1, ScheduleId = 1, SeatNumber = "A1", IsBooked = false },
                new Seats { SeatId = 2, ScheduleId = 1, SeatNumber = "A2", IsBooked = false },
                new Seats { SeatId = 3, ScheduleId = 1, SeatNumber = "A3", IsBooked = true }
            });

       
            var user = new UserMaster
            {
                UserId = 1,
                FullName = "Test User",
                Email = "user@example.com",
                Gender = "M",
                Password = "hashedpass",
                Phone = "9837284282"
            };
            _context.UserMasters.Add(user);

            var busOperator = new BusOperator
            {
                OperatorId = 1,
                OperatorName = "Test Operator",
                Email = "operator@example.com",
                Password = "dummyPassword123",
                Phone = "9382934393"
            };
            _context.BusOperators.Add(busOperator);

           
            var bus = new Buses
            {
                BusId = 1,
                BusName = "Test Bus",
                BusNumber = "TN01AB1234",
                OperatorId = busOperator.OperatorId,
                NumberOfSeats = 40
            };
            _context.Buses.Add(bus);

          
            var route = new RouteMaster
            {
                RouteId = 1,
                Origin = "CityA",
                Destination = "CityB"
            };
            _context.RouteMasters.Add(route);

            
            var schedule = new Schedules
            {
                ScheduleId = 1,
                BusId = bus.BusId,
                Bus = bus,
                RouteId = route.RouteId,
                Route = route,
                DepartureTime = DateTime.UtcNow.AddDays(1),
                Fare = 500m
            };
            _context.Schedules.Add(schedule);

            _context.SaveChanges();

            
            _bookingService = new BookingService(null, null, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task BookTicketsAsync_WithAvailableSeats_Succeeds()
        {
            var request = new BookingRequestDto
            {
                UserId = 1,
                ScheduleId = 1,
                SeatNumbers = new List<string> { "A1", "A2" }
            };

            var result = await _bookingService.BookTicketsAsync(request);

            Assert.IsNotNull(result);
            Assert.That(result.SeatNumber, Does.Contain("A1"));
            Assert.That(result.SeatNumber, Does.Contain("A2"));

            
            var seats = _context.Seats.Where(s => s.ScheduleId == 1 && request.SeatNumbers.Contains(s.SeatNumber));
            Assert.IsTrue(seats.All(s => s.IsBooked));
        }

        [Test]
        public void BookTicketsAsync_WithAlreadyBookedSeat_ThrowsException()
        {
            var request = new BookingRequestDto
            {
                UserId = 1,
                ScheduleId = 1,
                SeatNumbers = new List<string> { "A3" } 
            };

            Assert.ThrowsAsync<SeatAlreadyBookedException>(async () =>
                await _bookingService.BookTicketsAsync(request));
        }

        [Test]
        public async Task GetBookingSummaryAsync_ReturnsCorrectData()
        {
            var bookingRequest = new BookingRequestDto
            {
                UserId = 1,
                ScheduleId = 1,
                SeatNumbers = new List<string> { "A1" }
            };

            var bookingSummary = await _bookingService.BookTicketsAsync(bookingRequest);

            var summary = await _bookingService.GetBookingSummaryAsync(bookingSummary.BookingId);

            Assert.IsNotNull(summary);
            Assert.AreEqual(bookingSummary.BookingId, summary.BookingId);
            Assert.AreEqual("Test Bus", summary.BusName);
            Assert.That(summary.SeatNumber, Does.Contain("A1"));
            Assert.AreEqual("CityA ? CityB", summary.Route);
        }

        [Test]
        public async Task GetBookingsByOperatorIdAsync_ReturnsBookings()
        {
            var bookingRequest = new BookingRequestDto
            {
                UserId = 1,
                ScheduleId = 1,
                SeatNumbers = new List<string> { "A2" }
            };

            var bookingSummary = await _bookingService.BookTicketsAsync(bookingRequest);

            var bookings = await _bookingService.GetBookingsByOperatorIdAsync(1);

            Assert.IsNotNull(bookings);
            Assert.IsTrue(bookings.Any(b => b.BookingId == bookingSummary.BookingId));
        }
    }
}
