using Moq;
using NUnit.Framework;
using Bongo.Models.Model;
using Bongo.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Bongo.Core.Services.IServices;
using Bongo.Models.Model.VM;

namespace Bongo.Web.Tests
{
    [TestFixture]
    public class RoomBookingControllerTests
    {
        private RoomBookingController _bookingController;
        private Mock<IStudyRoomBookingService> _studyRoomBookingService;

        [SetUp]
        public void Setup()
        {
            _studyRoomBookingService = new Mock<IStudyRoomBookingService>();
            _bookingController = new RoomBookingController(_studyRoomBookingService.Object);
        }

        [Test]
        public void IndexPage_CallRequest_VerifyGetAllInvoked()
        {
            _bookingController.Index();
            _studyRoomBookingService.Verify(x => x.GetAllBooking(), Times.Once());
        }

        [Test]
        public void BookRoomCheck_ModelStateInvalid_ReturnView()
        {
            _bookingController.ModelState.AddModelError("Error", "Test");
            var result = _bookingController.Book(new StudyRoomBooking()) as ViewResult;
            Assert.AreEqual("Book", result?.ViewName);
        }

        [Test]
        public void BookRoomCheck_NotSuccessfull_NoRoomCode()
        {
            _studyRoomBookingService.Setup(x => x.BookStudyRoom(It.IsAny<StudyRoomBooking>()))
                .Returns(new StudyRoomBookingResult()
                {
                    Code = StudyRoomBookingCode.NoRoomAvailable
                });

            var result = _bookingController.Book(new StudyRoomBooking()) as ViewResult;
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.AreEqual("No Study Room available for selected date", result.ViewData["Error"]);
        }

        [Test]
        public void BookRoomCheck_Successfull_SuccessCodeAndRedirect()
        {
            //arrange
            _studyRoomBookingService.Setup(x => x.BookStudyRoom(It.IsAny<StudyRoomBooking>()))
                .Returns((StudyRoomBooking booking) => new StudyRoomBookingResult
                {
                    Code = StudyRoomBookingCode.Success,
                    FirstName = booking.FirstName,
                    LastName = booking.LastName,
                    Date = booking.Date,
                    Email = booking.Email
                });

            //act
            var result = _bookingController.Book(new StudyRoomBooking()
            {
                FirstName = "Ben",
                LastName = "Spark",
                Date = DateTime.Now,
                Email = "test@gmail.com"
            }) as RedirectToActionResult;

            //assert
            Assert.IsInstanceOf<RedirectToActionResult>(result);
            Assert.AreEqual("Ben", result.RouteValues["firstName"]);
            Assert.AreEqual(StudyRoomBookingCode.Success, result.RouteValues["Code"]);
        }

    }
}
