using Moq;
using Bongo.DataAccess.Repository.IRepository;
using NUnit.Framework;
using Bongo.Core.Services;
using Bongo.Models.Model;
using Bongo.Models.Model.VM;

namespace Bongo.Core
{
    [TestFixture]
    public class StudyRoomBookingServiceTests
    {
        private StudyRoomBooking _request;
        private List<StudyRoom> _avalibaleStudyRooms;
        private StudyRoomBookingService _bookingService;
        private Mock<IStudyRoomRepository> _studyRoomMock;
        private Mock<IStudyRoomBookingRepository> _studyRoomBookingMock;

        [SetUp]
        public void Setup()
        {
            _request = new StudyRoomBooking
            {
                FirstName = "Ben",
                LastName = "Spark",
                Email = "ben@gmail.com",
                Date = new DateTime(2024, 1, 1)
            };

            _avalibaleStudyRooms = new List<StudyRoom>
            {
                new StudyRoom { Id = 10, RoomName = "Michigan", RoomNumber = "A202" },
            };

            _studyRoomMock = new Mock<IStudyRoomRepository>();
            _studyRoomMock.Setup(repo => repo.GetAll()).Returns(_avalibaleStudyRooms);

            _studyRoomBookingMock = new Mock<IStudyRoomBookingRepository>();
            _bookingService = new StudyRoomBookingService(_studyRoomBookingMock.Object, _studyRoomMock.Object);
        }

        [Test]
        public void GetAllBooking_InvokeMethod_CheckRepoIsCalled()
        {
            // Act
            _bookingService.GetAllBooking();
            // Assert
            _studyRoomBookingMock.Verify(repo => repo.GetAll(null), Times.Once());
        }

        [Test]
        public void BookingException_NullRequest_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => _bookingService.BookStudyRoom(null));
            Assert.AreEqual("request", exception.ParamName);
        }

        [Test]
        public void StudyRoomBooking_SaveBookingWithAvalibleRoom_ReturnResultWithAllValues()
        {
            StudyRoomBooking? studyRoomBooking = null;
            _studyRoomBookingMock.Setup(x => x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(booking => studyRoomBooking = booking);

            //act
            _bookingService.BookStudyRoom(_request);

            //assert
            _studyRoomBookingMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Once());
            Assert.IsNotNull(studyRoomBooking);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(_request.FirstName, studyRoomBooking.FirstName);
                Assert.AreEqual(_request.LastName, studyRoomBooking.LastName);
                Assert.AreEqual(_request.Email, studyRoomBooking.Email);
                Assert.AreEqual(_request.Date, studyRoomBooking.Date);
                Assert.AreEqual(_avalibaleStudyRooms[0].Id, studyRoomBooking.StudyRoomId);
            });
        }

        [Test]
        public void StudyRoomBookingResultCheck_InputRequest_ValuesMatchInResult()
        {
            var result = _bookingService.BookStudyRoom(_request);

            Assert.NotNull(result);
            Assert.AreEqual(_request.FirstName, result.FirstName);
            Assert.AreEqual(_request.LastName, result.LastName);
            Assert.AreEqual(_request.Email, result.Email);
            Assert.AreEqual(_request.Date, result.Date);
        }

        [TestCase(true, ExpectedResult = StudyRoomBookingCode.Success)]
        [TestCase(false, ExpectedResult = StudyRoomBookingCode.NoRoomAvailable)]
        public StudyRoomBookingCode  ResultCodeSuccess_RoomAvalibality_ReturnsSuccessCode(bool isRoomAvailable)
        {
            if(!isRoomAvailable)
            {
                _avalibaleStudyRooms.Clear();
            }

            return _bookingService.BookStudyRoom(_request).Code;
        }

        [TestCase(0, false)]
        [TestCase(55, true)]
        public void StudyRoomBooking_BookRoomWithAviliability_ReturnsBookId(int expectedBookId, bool isRoomAvailable)
        {
            if (!isRoomAvailable)
            {
                _avalibaleStudyRooms.Clear();
            }

            StudyRoomBooking? studyRoomBooking = null;
            _studyRoomBookingMock.Setup(x => x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(booking => { booking.BookingId = 55; });

            //act
            var result = _bookingService.BookStudyRoom(_request);
            Assert.AreEqual(expectedBookId, result.BookingId);
        }

        [Test]
        public void BookNotInvoked_SaveBookingWithoutAvaliableRoom_BookNotInvoked()
        {
            _avalibaleStudyRooms.Clear();
            _bookingService.BookStudyRoom(_request);
            _studyRoomBookingMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Never());
        }
    }
}
