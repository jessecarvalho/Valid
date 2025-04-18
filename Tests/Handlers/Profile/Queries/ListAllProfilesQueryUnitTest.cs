using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Handlers.Profile.Commands;

namespace Tests.Handlers.Profile.Queries
{
    public class ListAllProfilesQueryHandlerTests
    {
        private readonly Mock<IMemoryStorage> _memoryStorageMock;
        private readonly Mock<ILogger<ListAllProfilesQueryHandler>> _loggerMock;
        private readonly ListAllProfilesQueryHandler _handler;

        public ListAllProfilesQueryHandlerTests()
        {
            _memoryStorageMock = new Mock<IMemoryStorage>();
            _loggerMock = new Mock<ILogger<ListAllProfilesQueryHandler>>();
            _handler = new ListAllProfilesQueryHandler(
                _memoryStorageMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProfiles_WhenProfilesExist()
        {
            // Arrange
            var expectedProfiles = new List<Application.Domain.Entities.Profile>
            {
                new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"param1", "value1"} }),
                new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"param2", "value2"} })
            };

            _memoryStorageMock.Setup(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"))
                .Returns(expectedProfiles);

            var request = new ListAllProfilesQueryRequest();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(expectedProfiles, result);
            Assert.Equal(2, result?.Count);
            _memoryStorageMock.Verify(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoProfilesExist()
        {
            // Arrange
            var emptyProfiles = new List<Application.Domain.Entities.Profile>();

            _memoryStorageMock.Setup(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"))
                .Returns(emptyProfiles);

            var request = new ListAllProfilesQueryRequest();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Empty(result);
            _loggerMock.VerifyLog(l => l.LogInformation("Nenhum perfil encontrado na memoria após busca todos os perfils"));
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenProfilesNotInStorage()
        {
            // Arrange
            _memoryStorageMock.Setup(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"))
                .Returns((List<Application.Domain.Entities.Profile>?)null);

            var request = new ListAllProfilesQueryRequest();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Null(result);
            _loggerMock.VerifyLog(l => l.LogInformation("Nenhum perfil encontrado na memoria após busca todos os perfils"));
        }

        [Fact]
        public async Task Handle_ShouldReturnProfilesWithParameters_WhenProfilesHaveParameters()
        {
            // Arrange
            var expectedProfiles = new List<Application.Domain.Entities.Profile>
            {
                new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"param1", "value1"} }),
                new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"param2", "value2"} })
            };

            _memoryStorageMock.Setup(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"))
                .Returns(expectedProfiles);

            var request = new ListAllProfilesQueryRequest();

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("value1", result?[0].Parameters["param1"]);
            Assert.Equal("value2", result?[1].Parameters["param2"]);
        }

        [Fact]
        public async Task Handle_ShouldNotLog_WhenProfilesExist()
        {
            // Arrange
            var profiles = new List<Application.Domain.Entities.Profile>
            {
                new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string>())
            };

            _memoryStorageMock.Setup(m => m.Get<List<Application.Domain.Entities.Profile>?>("profiles"))
                .Returns(profiles);

            var request = new ListAllProfilesQueryRequest();

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }
    }
}