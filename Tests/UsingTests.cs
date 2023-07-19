using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using CSharpExtensions.Monads;

namespace Monads.Tests
{

    [TestFixture]
    public class UsingTests
    {
        [Test]
        public void Using_ResourceDisposed_AfterExecution()
        {
            // Arrange
            var resourceMock = new Mock<IDisposable>();
            var usingCallbackMock = new Mock<Action<IDisposable>>();
            var usingInstance = new Using<IDisposable>(resourceMock.Object, usingCallbackMock.Object);

            // Act
            var result = usingInstance.Result;

            // Assert
            resourceMock.Verify(r => r.Dispose(), Times.Once);
        }

        [Test]
        public void Using_ResourceDisposed_EvenWhenExceptionThrown()
        {
            // Arrange
            var resourceMock = new Mock<IDisposable>();
            var usingCallbackMock = new Mock<Action<IDisposable>>();
            usingCallbackMock.Setup(u => u(It.IsAny<IDisposable>())).Throws<InvalidOperationException>();
            var usingInstance = new Using<IDisposable>(resourceMock.Object, usingCallbackMock.Object);

            // Act
            var result = usingInstance.Result;

            // Assert
            resourceMock.Verify(r => r.Dispose(), Times.Once);
        }

        [Test]
        public void Using_UsingCallbackExecuted_WithResource()
        {
            // Arrange
            var resourceMock = new Mock<IDisposable>();
            var usingCallbackMock = new Mock<Action<IDisposable>>();
            var usingInstance = new Using<IDisposable>(resourceMock.Object, usingCallbackMock.Object);

            // Act
            var result = usingInstance.Result;

            // Assert
            usingCallbackMock.Verify(cb => cb(resourceMock.Object), Times.Once);
        }

        [Test]
        public void Using_ReturnsUnit()
        {
            // Arrange
            var resourceMock = new Mock<IDisposable>();
            var usingCallbackMock = new Mock<Action<IDisposable>>();
            var usingInstance = new Using<IDisposable>(resourceMock.Object, usingCallbackMock.Object);

            // Act
            var result = usingInstance.Result;

            // Assert
            Assert.AreEqual(Unit.Default, result._right);
        }
    }

}
