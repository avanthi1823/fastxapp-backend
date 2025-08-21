using Microsoft.EntityFrameworkCore;
using NewFastBus.Exceptions;
using NewFastBus.Interfaces;
using NewFastBus.Models.DTOs;
using NewFastBus.Models.Entities;
using NewFastBus.Services;
using NUnit.Framework;
using System.Threading.Tasks;

namespace NewFastBusTest
{
    [TestFixture]
    public class PaymentServiceTests
    {
        private FastXContext _context = null!;
        private IPaymentRepository _paymentRepository = null!;
        private PaymentService _paymentService = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<FastXContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
                .Options;

            _context = new FastXContext(options);
            _paymentRepository = new PaymentRepository(_context);
            _paymentService = new PaymentService(_paymentRepository);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task RecordPaymentAsync_ShouldAddPaymentAndReturnDto()
        {
            // Arrange
            int bookingId = 1;
            decimal amount = 1234m;

            // Act
            var result = await _paymentService.RecordPaymentAsync(bookingId, amount);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(bookingId, result.BookingId);
            Assert.AreEqual(amount, result.Amount);

            
            var paymentInDb = await _context.Payments.FirstOrDefaultAsync(p => p.BookingId == bookingId);
            Assert.IsNotNull(paymentInDb);
            Assert.AreEqual(amount, paymentInDb.Amount);
        }

        [Test]
        public async Task GetPaymentDetailsAsync_ShouldReturnPaymentDto_WhenPaymentExists()
        {
            // Arrange
            int bookingId = 2;
            decimal amount = 2000m;

         
            _context.Payments.Add(new Payments
            {
                BookingId = bookingId,
                Amount = amount,
                Status = "Confiru"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _paymentService.GetPaymentDetailsAsync(bookingId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(bookingId, result!.BookingId);
            Assert.AreEqual(amount, result.Amount);
        }


        [Test]
        public void GetPaymentDetailsAsync_ShouldThrowEntityNotFoundException_WhenPaymentDoesNotExist()
        {
            // Arrange
            int nonExistentBookingId = 999;

            // Act & Assert
            var ex = Assert.ThrowsAsync<EntityNotFoundException>(async () =>
                await _paymentService.GetPaymentDetailsAsync(nonExistentBookingId));

            Assert.That(ex.Message, Is.EqualTo($"Payment details for booking ID {nonExistentBookingId} not found."));
        }
    }
    }

