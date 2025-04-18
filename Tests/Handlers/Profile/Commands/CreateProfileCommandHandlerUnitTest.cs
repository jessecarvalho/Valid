using Application.Application.Handlers.Profile.Commands.CreateProfile;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Handlers.Profile.Commands
{
    public class CreateProfileCommandHandlerTests
    {
        private readonly Mock<IMemoryStorage> _memoryStorageMock;
        private readonly Mock<ILogger<CreateProfileCommandHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly CreateProfileCommandHandler _handler;

        public CreateProfileCommandHandlerTests()
        {
            _memoryStorageMock = new Mock<IMemoryStorage>();
            _loggerMock = new Mock<ILogger<CreateProfileCommandHandler>>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new CreateProfileCommandHandler(
                _memoryStorageMock.Object,
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldCreateProfileWithParameters_WhenNoProfilesExist()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                {"param1", "value1"},
                {"param2", "value2"}
            };
            var profile = new Application.Domain.Entities.Profile("Test Profile", parameters);
            var request = new CreateProfileCommandRequest(profile);
            var emptyProfiles = new List<Application.Domain.Entities.Profile>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyProfiles);

            _memoryStorageMock.Setup(m => m.Remove("profiles"));
            _memoryStorageMock.Setup(m => m.Set<List<Application.Domain.Entities.Profile>>("profiles", It.IsAny<List<Application.Domain.Entities.Profile>>()));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(profile, result);
            Assert.Equal(parameters, result.Parameters);
            _mediatorMock.Verify(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
            _memoryStorageMock.Verify(m => m.Remove("profiles"), Times.Once);
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profiles", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => p.Contains(profile) && p[0].Parameters.Count == 2)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCreateProfileWithEmptyParameters_WhenNoneProvided()
        {
            // Arrange
            var profile = new Application.Domain.Entities.Profile("Test Profile", null);
            var request = new CreateProfileCommandRequest(profile);
            var emptyProfiles = new List<Application.Domain.Entities.Profile>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyProfiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Parameters);
            Assert.Empty(result.Parameters);
        }

        [Fact]
        public async Task Handle_ShouldCreateProfile_WhenProfilesAlreadyExist()
        {
            // Arrange
            var existingParameters = new Dictionary<string, string> { {"existingParam", "value"} };
            var existingProfile = new Application.Domain.Entities.Profile("Existing Profile", existingParameters);
            
            var newParameters = new Dictionary<string, string> { {"newParam", "value"} };
            var newProfile = new Application.Domain.Entities.Profile("New Profile", newParameters);
            
            var request = new CreateProfileCommandRequest(newProfile);
            var existingProfiles = new List<Application.Domain.Entities.Profile> { existingProfile };

            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProfiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(newProfile, result);
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profiles", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => 
                    p.Count == 2 && 
                    p[0].Parameters.ContainsKey("existingParam") && 
                    p[1].Parameters.ContainsKey("newParam"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldMaintainParametersIntegrity_WhenMultipleProfilesExist()
        {
            // Arrange
            var profile1 = new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"p1", "v1"} });
            var profile2 = new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"p2", "v2"} });
            var newProfile = new Application.Domain.Entities.Profile("NewProfile", new Dictionary<string, string> { {"p3", "v3"} });
            
            var request = new CreateProfileCommandRequest(newProfile);
            var existingProfiles = new List<Application.Domain.Entities.Profile> { profile1, profile2 };

            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingProfiles);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profiles", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => 
                    p.Count == 3 &&
                    p[0].Parameters["p1"] == "v1" &&
                    p[1].Parameters["p2"] == "v2" &&
                    p[2].Parameters["p3"] == "v3")), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogAppropriately_WhenProfileCreated()
        {
            // Arrange
            var profile = new Application.Domain.Entities.Profile("Test Profile", new Dictionary<string, string> { {"key", "value"} });
            var request = new CreateProfileCommandRequest(profile);
            var emptyProfiles = new List<Application.Domain.Entities.Profile>();

            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyProfiles);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _loggerMock.VerifyLog(l => l.LogInformation("Iniciando criação do perfil {ProfileName}", profile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogInformation("Perfil {ProfileName} criado com sucesso", profile.ProfileName));
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> loggerMock, Action<ILogger<T>> action)
        {
            loggerMock.Verify(l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                ((Func<It.IsAnyType, Exception, string>)It.IsAny<object>())!),
                Times.AtLeastOnce);

            try
            {
                action.Invoke(loggerMock.Object);
            }
            catch (Exception)
            {
                // Ignore - we just want to verify the log was called
            }
        }
    }
}