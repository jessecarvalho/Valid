using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Handlers.Profile.Commands;

namespace Tests.Handlers.Profile.Queries
{
    public class GetProfileParametersHandlerTests
    {
        private readonly Mock<ILogger<GetProfileParametersHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly GetProfileParametersHandler _handler;

        public GetProfileParametersHandlerTests()
        {
            _loggerMock = new Mock<ILogger<GetProfileParametersHandler>>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new GetProfileParametersHandler(
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnProfile_WhenProfileExists()
        {
            // Arrange
            var profileName = "Existing Profile";
            var expectedProfile = new Application.Domain.Entities.Profile(profileName, new Dictionary<string, string>());
            var profiles = new List<Application.Domain.Entities.Profile> { expectedProfile };
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            var request = new GetProfileParametersRequest(profileName);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(expectedProfile, result);
            _loggerMock.VerifyLog(l => l.LogInformation($"Perfil {profileName} foi procurado e encontrado"));
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenProfileDoesNotExist()
        {
            // Arrange
            var profileName = "Non-existent Profile";
            var profiles = new List<Application.Domain.Entities.Profile>();
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            var request = new GetProfileParametersRequest(profileName);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
            _loggerMock.VerifyLog(l => l.LogInformation($"Perfil {profileName} foi procurado mas não foi encontrado"));
        }

        [Fact]
        public async Task Handle_ShouldReturnCorrectProfile_WhenMultipleProfilesExist()
        {
            // Arrange
            var profile1 = new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"param1", "value1"} });
            var profile2 = new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"param2", "value2"} });
            var profiles = new List<Application.Domain.Entities.Profile> { profile1, profile2 };
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            var request = new GetProfileParametersRequest("Profile2");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(profile2, result);
            Assert.Equal("value2", result.Parameters["param2"]);
        }

        [Fact]
        public async Task Handle_ShouldReturnProfileWithParameters_WhenProfileHasParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                {"param1", "value1"},
                {"param2", "value2"}
            };
            var profileName = "ProfileWithParams";
            var expectedProfile = new Application.Domain.Entities.Profile(profileName, parameters);
            var profiles = new List<Application.Domain.Entities.Profile> { expectedProfile };
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            var request = new GetProfileParametersRequest(profileName);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Parameters.Count);
            Assert.Equal("value1", result.Parameters["param1"]);
            Assert.Equal("value2", result.Parameters["param2"]);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenMediatorThrowsException()
        {
            // Arrange
            var profileName = "Test Profile";
            var request = new GetProfileParametersRequest(profileName);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldReturnProfile_WhenProfileNameCaseIsDifferent()
        {
            // Arrange
            var profileName = "CaseSensitiveProfile";
            var expectedProfile = new Application.Domain.Entities.Profile(profileName, new Dictionary<string, string>());
            var profiles = new List<Application.Domain.Entities.Profile> { expectedProfile };
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            var request = new GetProfileParametersRequest("casesensitiveprofile");

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
        }
    }
}