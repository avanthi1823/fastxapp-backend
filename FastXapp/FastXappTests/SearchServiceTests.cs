using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewFastBus.Tests
{
    [TestFixture]
    public class SearchServiceTests
    {
        private FastXContext _dbContext;
        private IMapper _mapper;
        private SearchService _searchService;

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<FastXContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new FastXContext(options);

            var busType = new BusTypeMaster { BusTypeId = 1, TypeName = "AC Sleeper" };
            
                var bus = new Buses
                {
                    BusId = 1,
                    BusName = "SuperBus",
                    BusNumber = "TN01AB1234",
                    BusTypeId = 1,
                    BusType = busType
                };

            var route = new RouteMaster { RouteId = 1, Origin = "Chennai", Destination = "Bangalore" };

            var seats = new List<Seats>
            {
                new Seats { SeatId = 1, ScheduleId = 1, SeatNumber = "1A", IsBooked = false },
                new Seats { SeatId = 2, ScheduleId = 1, SeatNumber = "1B", IsBooked = true },
                new Seats { SeatId = 3, ScheduleId = 1, SeatNumber = "1C", IsBooked = false }
            };

            var schedule = new Schedules
            {
                ScheduleId = 1,
                BusId = 1,
                Bus = bus,
                RouteId = 1,
                Route = route,
                DepartureTime = new DateTime(2025, 08, 09, 10, 0, 0),
                ArrivalTime = new DateTime(2025, 08, 09, 16, 0, 0),
                Fare = 500m,
                Seats = seats
            };

            await _dbContext.BusTypeMasters.AddAsync(busType);
            await _dbContext.Buses.AddAsync(bus);
            await _dbContext.RouteMasters.AddAsync(route);
            await _dbContext.Schedules.AddAsync(schedule);
            await _dbContext.Seats.AddRangeAsync(seats);
            await _dbContext.SaveChangesAsync();

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<SearchMapperProfile>();
            });

            _mapper = mapperConfig.CreateMapper();

            _searchService = new SearchService(_dbContext, _mapper);
        }

        [Test]
        public async Task SearchAsync_WithMatchingCriteria_ReturnsResults()
        {
            var request = new SearchRequestDto
            {
                Origin = "Chennai",
                Destination = "Bangalore",
                TravelDate = new DateTime(2025, 08, 09)
            };

            var results = await _searchService.SearchAsync(request);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count(), Is.EqualTo(1));

            var first = results.First();
            Assert.That(first.ScheduleId, Is.EqualTo(1));
            Assert.That(first.BusName, Is.EqualTo("SuperBus"));
            Assert.That(first.BusType, Is.EqualTo("AC Sleeper"));
            Assert.That(first.DepartureTime, Is.EqualTo(new DateTime(2025, 08, 09, 10, 0, 0).ToString("g")));
            Assert.That(first.ArrivalTime, Is.EqualTo(new DateTime(2025, 08, 09, 16, 0, 0).ToString("g")));
            Assert.That(first.AvailableSeats, Is.EqualTo(2));
        }

        [Test]
        public async Task SearchAsync_WithNoMatchingCriteria_ReturnsEmpty()
        {
            var request = new SearchRequestDto
            {
                Origin = "Mumbai",
                Destination = "Delhi",
                TravelDate = new DateTime(2025, 08, 09)
            };

            var results = await _searchService.SearchAsync(request);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }
    }
}
