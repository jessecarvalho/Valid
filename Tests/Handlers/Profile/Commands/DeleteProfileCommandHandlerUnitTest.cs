using Application.Application.Handlers.Profile.Commands.DeleteProfile;
using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ListAllProfiles;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Handlers.Profile.Commands
{
    public class DeleteProfileCommandHandlerTests
    {
        private readonly Mock<IMemoryStorage> _memoryStorageMock;
        private readonly Mock<ILogger<DeleteProfileCommandHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly DeleteProfileCommandHandler _handler;

        public DeleteProfileCommandHandlerTests()
        {
            _memoryStorageMock = new Mock<IMemoryStorage>();
            _loggerMock = new Mock<ILogger<DeleteProfileCommandHandler>>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new DeleteProfileCommandHandler(
                _memoryStorageMock.Object,
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProfileDoesNotExist()
        {
            // Arrange
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest("Non-existent Profile");
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Application.Domain.Entities.Profile)null);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(l => l.LogWarning("Perfil {ProfileName} não encontrado - exclusão cancelada", request.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldDeleteProfile_WhenProfileExists()
        {
            // Arrange
            var profile = new Application.Domain.Entities.Profile("Existing Profile", new Dictionary<string, string>());
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest(profile.ProfileName);
            var profiles = new List<Application.Domain.Entities.Profile> { profile };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            _memoryStorageMock.Setup(m => m.Remove("profile"));
            _memoryStorageMock.Setup(m => m.Set<List<Application.Domain.Entities.Profile>>("profile", It.IsAny<List<Application.Domain.Entities.Profile>>()));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profile", 
                It.Is<List<Application.Domain.Entities.Profile>>(p => !p.Contains(profile))), Times.Once);
            _loggerMock.VerifyLog(l => l.LogInformation("Perfil {ProfileName} excluído com sucesso", request.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProfileNotInList()
        {
            // Arrange
            var profile = new Application.Domain.Entities.Profile("Existing Profile", new Dictionary<string, string>());
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest(profile.ProfileName);
            var profiles = new List<Application.Domain.Entities.Profile>(); // Empty list

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(l => l.LogWarning("Perfil {ProfileName} não encontrado na lista durante a remoção", request.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldMaintainOtherProfiles_WhenDeletingOne()
        {
            // Arrange
            var profile1 = new Application.Domain.Entities.Profile("Profile1", new Dictionary<string, string> { {"p1", "v1"} });
            var profile2 = new Application.Domain.Entities.Profile("Profile2", new Dictionary<string, string> { {"p2", "v2"} });
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest(profile1.ProfileName);
            var profiles = new List<Application.Domain.Entities.Profile> { profile1, profile2 };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile1);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
            _memoryStorageMock.Verify(m => m.Set<List<Application.Domain.Entities.Profile>>("profile",
                It.Is<List<Application.Domain.Entities.Profile>>(p =>
                    p.Count == 1 &&
                    p[0].ProfileName == "Profile2")));
        }

        [Fact]
        public async Task Handle_ShouldLogAppropriately_WhenProfileDeleted()
        {
            // Arrange
            var profile = new Application.Domain.Entities.Profile("Test Profile", new Dictionary<string, string>());
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest(profile.ProfileName);
            var profiles = new List<Application.Domain.Entities.Profile> { profile };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<ListAllProfilesQueryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profiles);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _loggerMock.VerifyLog(l => l.LogInformation("Iniciando exclusão do perfil: {ProfileName}", profile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogDebug("Buscando parâmetros do perfil {ProfileName}", profile.ProfileName));
            _loggerMock.VerifyLog(l => l.LogDebug("Obtendo lista completa de perfis"));
            _loggerMock.VerifyLog(l => l.LogInformation("Removendo perfil {ProfileName} da lista de {ProfileCount} perfis", 
                profile.ProfileName, 1));
            _loggerMock.VerifyLog(l => l.LogDebug("Atualizando perfis"));
            _loggerMock.VerifyLog(l => l.LogInformation("Perfil {ProfileName} excluído com sucesso", profile.ProfileName));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenErrorOccurs()
        {
            // Arrange
            var request = new Application.Application.Handlers.Profile.Commands.DeleteProfile.DeleteProfileCommandRequest("Test Profile");
            
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AggregateException("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => _handler.Handle(request, CancellationToken.None));
            _loggerMock.VerifyLog(l => l.LogError(It.IsAny<AggregateException>(), "Erro ao excluir perfil {ProfileName}", request.ProfileName));
        }
    }
}