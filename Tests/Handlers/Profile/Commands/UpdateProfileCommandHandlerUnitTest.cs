using Application.Application.Handlers.Profile.Commands.UpdateProfile;
using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Handlers.Profile.Commands
{
    public class UpdateProfileCommandHandlerTests
    {
        private readonly Mock<IMemoryStorage> _memoryStorageMock;
        private readonly Mock<ILogger<UpdateProfileCommandHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly UpdateProfileCommandHandler _handler;

        public UpdateProfileCommandHandlerTests()
        {
            _memoryStorageMock = new Mock<IMemoryStorage>();
            _loggerMock = new Mock<ILogger<UpdateProfileCommandHandler>>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new UpdateProfileCommandHandler(
                _memoryStorageMock.Object,
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldThrowKeyNotFoundException_WhenOldProfileDoesNotExist()
        {
            // Arrange
            var oldProfileName = "Old Profile";
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string>());
            var request = new UpdateProfileCommandRequest(oldProfileName, newProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Application.Domain.Entities.Profile)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(request, CancellationToken.None));
            _loggerMock.VerifyLog(l => l.LogWarning("Perfil antigo {OldProfileName} não encontrado", oldProfileName));
        }

        [Fact]
        public async Task Handle_ShouldUpdateProfile_WhenProfileExists()
        {
            // Arrange
            var oldProfile = new Application.Domain.Entities.Profile("Old Profile", new Dictionary<string, string> { {"key", "oldValue"} });
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string> { {"key", "newValue"} });
            var request = new UpdateProfileCommandRequest(oldProfile.ProfileName, newProfile);
            var profiles = new List<Application.Domain.Entities.Profile> { oldProfile };

            _mediatorMock.Setup(m => m.Send(It.Is<GetProfileParametersRequest>(r => r.ProfileName == oldProfile.ProfileName), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            _memoryStorageMock.Setup(m => m.Remove("profile"));
            _memoryStorageMock.Setup(m => m.Set<List<Application.Domain.Entities.Profile>>("profile", It.IsAny<List<Application.Domain.Entities.Profile>>()));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(newProfile, result);
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profile", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => 
                    p.Count == 1 && 
                    p[0].ProfileName == newProfile.ProfileName &&
                    p[0].Parameters["key"] == "newValue")), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogWarning_WhenOldProfileNotInList()
        {
            // Arrange
            var oldProfile = new Application.Domain.Entities.Profile("Old Profile", new Dictionary<string, string>());
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string>());
            var request = new UpdateProfileCommandRequest(oldProfile.ProfileName, newProfile);
            var profiles = new List<Application.Domain.Entities.Profile>(); // Empty list

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(newProfile, result);
            _loggerMock.VerifyLog(l => l.LogWarning(
                "Perfil {OldProfileName} não estava na lista para remoção", 
                oldProfile.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldMaintainOtherProfiles_WhenUpdatingOne()
        {
            // Arrange
            var profile1 = new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"p1", "v1"} });
            var profile2 = new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"p2", "v2"} });
            var newProfile1 = new Application.Domain.Entities.Profile("Updated Profile1", new Dictionary<string, string> { {"p1", "updated"} });
            var request = new UpdateProfileCommandRequest(profile1.ProfileName, newProfile1);
            var profiles = new List<Application.Domain.Entities.Profile> { profile1, profile2 };

            _mediatorMock.Setup(m => m.Send(It.Is<GetProfileParametersRequest>(r => r.ProfileName == profile1.ProfileName), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile1);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profile", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => 
                    p.Count == 2 &&
                    p.Any(pr => pr.ProfileName == "Updated Profile1" && pr.Parameters["p1"] == "updated") &&
                    p.Any(pr => pr.ProfileName == "Profile2" && pr.Parameters["p2"] == "v2"))), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldLogAppropriately_WhenProfileUpdated()
        {
            // Arrange
            var oldProfile = new Application.Domain.Entities.Profile("Old Profile", new Dictionary<string, string>());
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string>());
            var request = new UpdateProfileCommandRequest(oldProfile.ProfileName, newProfile);
            var profiles = new List<Application.Domain.Entities.Profile> { oldProfile };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _loggerMock.VerifyLog(l => l.LogInformation(
                "Iniciando atualização do perfil: {OldProfileName} para {NewProfileName}", 
                oldProfile.ProfileName, newProfile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogDebug("Buscando perfil antigo: {OldProfileName}", oldProfile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogDebug("Obtendo lista completa de perfis"));
            _loggerMock.VerifyLog(l => l.LogInformation(
                "Substituindo perfil {OldProfileName} na lista de {ProfileCount} perfis",
                oldProfile.ProfileName, 1));
            _loggerMock.VerifyLog(l => l.LogDebug("Novo perfil {NewProfileName} adicionado à lista", newProfile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogDebug("Atualizando storage de perfis"));
            _loggerMock.VerifyLog(l => l.LogInformation(
                "Perfil {OldProfileName} atualizado para {NewProfileName} com sucesso",
                oldProfile.ProfileName, newProfile.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var oldProfile = new Application.Domain.Entities.Profile("Old Profile", new Dictionary<string, string>());
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string>());
            var request = new UpdateProfileCommandRequest(oldProfile.ProfileName, newProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => _handler.Handle(request, CancellationToken.None));
            _loggerMock.VerifyLog(l => l.LogError(
                It.IsAny<AggregateException>(), 
                "Falha ao atualizar perfil {OldProfileName} para {NewProfileName}",
                oldProfile.ProfileName, 
                newProfile.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldUpdateParameters_WhenProfileUpdated()
        {
            // Arrange
            var oldProfile = new Application.Domain.Entities.Profile("Old Profile", new Dictionary<string, string> { {"param", "old"} });
            var newProfile = new Application.Domain.Entities.Profile("New Profile", new Dictionary<string, string> { {"param", "new"} });
            var request = new UpdateProfileCommandRequest(oldProfile.ProfileName, newProfile);
            var profiles = new List<Application.Domain.Entities.Profile> { oldProfile };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(oldProfile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("new", result.Parameters["param"]);
        }
    }
}