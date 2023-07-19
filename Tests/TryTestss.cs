using CSharpExtensions.Monads;
using NUnit.Framework;

namespace Monads.Tests
{
    [TestFixture]
    public class TryTestss
    {
        [Test]
        public void Try_SuccessfulExecution_ReturnsResult()
        {
            // Arrange
            var expectedResult = 42;
            var tryFunc = () => new Either<Exception, int>(expectedResult);

            // Act
            var tryResult = new Try<int>(tryFunc);

            // Assert
            Assert.AreEqual(expectedResult, tryResult.Result._right);
        }

        [Test]
        public void Try_ExceptionThrown_ReturnsException()
        {
            // Arrange
            var expectedException = new Exception("Something went wrong");
            Func<Either<Exception, int>> tryFunc = () => throw expectedException;

            // Act
            var tryResult = new Try<int>(tryFunc);

            // Assert
            Assert.AreEqual(expectedException, tryResult.Result._left);
        }

        [Test]
        public void Select_SuccessfulExecution_ReturnsTransformedResult()
        {
            // Arrange
            var initialResult = 10;
            var expectedTransformedResult = "10";
            Func<int, string> transformFunc = (x) => x.ToString();
            var tryInstance = new Try<int>(() => initialResult);

            // Act
            var transformedTry = tryInstance.Select(transformFunc);

            // Assert
            Assert.AreEqual(expectedTransformedResult, transformedTry.Result._right);
        }

        [Test]
        public void Select_ExceptionThrown_ReturnsException()
        {
            // Arrange
            var initialResult = 10;
            var expectedException = new Exception("Something went wrong");
            Func<int, string> transformFunc = (x) => throw expectedException;
            var tryInstance = new Try<int>(() => initialResult);

            // Act
            var transformedTry = tryInstance.Select(transformFunc);

            // Assert
            Assert.AreEqual(expectedException, transformedTry.Result._left);
        }

    }
}
