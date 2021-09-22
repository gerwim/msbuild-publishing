using System;
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
        [InlineData("1607", 10, 14393, "ltsc2016")]
        [InlineData("1809", 10, 17763, "ltsc2019")]
        [InlineData("21H2", 10, 20348, "ltsc2022")]
        [InlineData("21H2", 10, 22000, "ltsc2022")] // Windows 11 build number
        [InlineData("21H2", 10, 11111, "21H2")] // fake build number
        [InlineData("25H2", 05, 11111, "25H2")] // fake major version
        public void ReturnsCorrectVersions(string releaseId, int major, int buildNumber, string expected)
        {
            // Arrange
            var registryMock = new Mock<IRegistry>();
            registryMock.Setup(x => x.Read(It.IsAny<string>(), It.IsAny<string>())).Returns(releaseId);

            var versionMock = new Mock<IVersion>();
            versionMock.Setup(x => x.GetOsVersion()).Returns(new System.Version(major, 0, buildNumber));


            var sut = new SetWindowsReleaseVersionEnvironmentVariable(registryMock.Object, versionMock.Object)
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
