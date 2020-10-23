using GerwimFeiken.Publishing.Tasks.Tests.Fakes;
using GerwimFeiken.Publishing.Tasks.Repositories;
using Moq;
using Xunit;

namespace GerwimFeiken.Publishing.Tasks.Tests
{
    public class SetWindowsReleaseVersionEnvironmentVariableTests
    {

        [Fact]
        public void ReturnsDataWithoutFakeRegistry()
        {
            // Arrange
           var sut = new SetWindowsReleaseVersionEnvironmentVariable()
            {
                BuildEngine = new FakeBuildEngine()
            };

            // Act
            bool result = sut.Execute();

            // Assert
            Assert.True(result);
            Assert.True(sut.Output.Length > 0);
        }

        [Theory]
        [InlineData("1607", "ltsc2016")]
        [InlineData("1809", "ltsc2019")]
        public void ConvertsToLtscVersions(string version, string expected)
        {
            // Arrange
            var registry = new Mock<IRegistry>();
            registry.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<string>())).Returns(version);

            var sut = new SetWindowsReleaseVersionEnvironmentVariable(registry.Object)
            {
                BuildEngine = new FakeBuildEngine()
            };

            // Act
            bool result = sut.Execute();

            // Assert
            Assert.True(result);
            Assert.Equal(expected, sut.Output);
        }
    }
}
