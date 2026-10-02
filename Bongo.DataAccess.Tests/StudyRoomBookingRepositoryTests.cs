using NUnit.Framework;
using Bongo.Models.Model;
using Bongo.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace Bongo.DataAccess
{
    [TestFixture]
    public class StudyRoomBookingRepositoryTests
    {
        private StudyRoomBooking studyRoomBookingOne;
        private StudyRoomBooking studyRoomBookingTwo;
        private DbContextOptions<ApplicationDbContext> options;

        public StudyRoomBookingRepositoryTests()
        {
            studyRoomBookingOne = new()
            {
                FirstName = "Ben1",
                LastName = "Spark1",
                Date = new DateTime(2023, 1, 1),
                Email = "ben1@gmail.com",
                BookingId = 11,
                StudyRoomId = 1
            };

            studyRoomBookingTwo = new()
            {
                FirstName = "Ben2",
                LastName = "Spark2",
                Date = new DateTime(2023, 2, 2),
                Email = "ben2@gmail.com",
                BookingId = 22,
                StudyRoomId = 2
            };

            options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "temp_Bongo").Options;
        }

        [SetUp]
        public void SetUp()
        {
            using var context = new ApplicationDbContext(options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        [Test]
        public void SaveBooking_BookingOne_CheckTheValuesFromDatabase()
        {
            //act
            using(var context = new ApplicationDbContext(options))
            {
                var repository = new StudyRoomBookingRepository(context);
                repository.Book(studyRoomBookingOne);
            }

            //assert
            using(var context = new ApplicationDbContext(options))
            {
                var bookingFromDb = context.StudyRoomBookings.FirstOrDefault(x => x.BookingId == 11);
                
                Assert.Multiple(() =>
                {
                    Assert.IsNotNull(bookingFromDb);
                    Assert.AreEqual(studyRoomBookingOne.BookingId, bookingFromDb.BookingId);
                    Assert.AreEqual(studyRoomBookingOne.FirstName, bookingFromDb.FirstName);
                    Assert.AreEqual(studyRoomBookingOne.LastName, bookingFromDb.LastName);
                    Assert.AreEqual(studyRoomBookingOne.Date, bookingFromDb.Date);
                    Assert.AreEqual(studyRoomBookingOne.Email, bookingFromDb.Email);
                    Assert.AreEqual(studyRoomBookingOne.StudyRoomId, bookingFromDb.StudyRoomId);
                });
            }
        }

        [Test]
        public void GetAllBooking_BookingOneAndTwo_CheckBothBookingFromDatabase()
        {
            //arrange
            var expectedResult = new List<StudyRoomBooking>
            {
                studyRoomBookingOne,
                studyRoomBookingTwo
            };

            using (var context = new ApplicationDbContext(options))
            {
                var repository = new StudyRoomBookingRepository(context);
                repository.Book(studyRoomBookingOne);
                repository.Book(studyRoomBookingTwo);
            }

            //act
            List<StudyRoomBooking> actualList;
            using (var context = new ApplicationDbContext(options))
            {
                var repository = new StudyRoomBookingRepository(context);
                actualList = repository.GetAll(null).ToList();
            }


            using (var context = new ApplicationDbContext(options))
            {
                CollectionAssert.AreEqual(expectedResult, actualList, new BookingCompare());
            }
        }

        private class BookingCompare : IComparer
        {
            public int Compare(object x, object y)
            {
                var booking1 = (StudyRoomBooking)x;
                var booking2 = (StudyRoomBooking)y;

                return (booking1.BookingId != booking2.BookingId) ? 1 : 0;
            }
        }
    }
}
