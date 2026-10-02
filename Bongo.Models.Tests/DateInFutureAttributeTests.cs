using Bongo.Models.ModelValidations;
using NUnit.Framework;

namespace Bongo.Models
{
    [TestFixture]
    public class DateInFutureAttributeTests
    {
        [Test]
        [TestCase(100, ExpectedResult = true)]
        [TestCase(-100, ExpectedResult = false)]
        [TestCase(0, ExpectedResult = false)]
        public bool DateValidator_InputExpectedDateRange_DateValidity(int seconds)
        {
            return new DateInFutureAttribute().IsValid(DateTime.Now.AddSeconds(seconds));
        }

        [Test]
        public void DateValidator_AnyDate_ReturnErrorMessage()
        {
            Assert.AreEqual("Date must be in the future", new DateInFutureAttribute().ErrorMessage);
        }
    }
}
