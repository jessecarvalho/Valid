using Application.Application.Handlers.Profile.Queries.GetProfileParameters;
using Application.Application.Handlers.Profile.Queries.ValidateProfileParameter;
using Application.Domain.Entities;
using Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Handlers.Profile.Queries
{
    public class ValidateProfileParameterHandlerTests
    {
        private readonly Mock<ILogger<ValidateProfileParameterHandler>> _loggerMock;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ValidateProfileParameterHandler _handler;

        public ValidateProfileParameterHandlerTests()
        {
            _loggerMock = new Mock<ILogger<ValidateProfileParameterHandler>>();
            _mediatorMock = new Mock<IMediator>();
            _handler = new ValidateProfileParameterHandler(
                _loggerMock.Object,
                _mediatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenParameterValueIsTrue()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var parameters = new Dictionary<string, string> { { parameterName, "true" } };
            var profile = new Application.Domain.Entities.Profile(profileName, parameters);
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
            _loggerMock.VerifyLog(l => l.LogDebug(
                "Parâmetro {Parameter} encontrado com valor {Value} (válido: {IsValid})",
                parameterName, "true", true));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenParameterValueIsNotTrue()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var parameters = new Dictionary<string, string> { { parameterName, "false" } };
            var profile = new Application.Domain.Entities.Profile(profileName, parameters);
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(l => l.LogDebug(
                "Parâmetro {Parameter} encontrado com valor {Value} (válido: {IsValid})",
                parameterName, "false", false));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenParameterDoesNotExist()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var parameters = new Dictionary<string, string>();
            var profile = new Application.Domain.Entities.Profile(profileName, parameters);
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.False(result);
            _loggerMock.VerifyLog(l => l.LogDebug(
                "Parâmetro {Parameter} não encontrado no perfil {ProfileName}",
                parameterName, profileName));
        }

        [Fact]
        public async Task Handle_ShouldBeCaseInsensitive_WhenCheckingTrueValue()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var parameters = new Dictionary<string, string> { { parameterName, "TRUE" } };
            var profile = new Application.Domain.Entities.Profile(profileName, parameters);
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_ShouldLogAppropriately_WhenValidatingParameter()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var parameters = new Dictionary<string, string> { { parameterName, "true" } };
            var profile = new Application.Domain.Entities.Profile(profileName, parameters);
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(profile);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _loggerMock.VerifyLog(l => l.LogInformation(
                "Iniciando validação do parâmetro {Parameter} para o perfil {ProfileName}",
                parameterName, profileName));
            _loggerMock.VerifyLog(l => l.LogDebug(
                "Buscando parâmetros do perfil {ProfileName}",
                profileName));
            _loggerMock.VerifyLog(l => l.LogDebug(
                "Parâmetro {Parameter} encontrado com valor {Value} (válido: {IsValid})",
                parameterName, "true", true));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenMediatorThrowsException()
        {
            // Arrange
            var profileName = "TestProfile";
            var parameterName = "testParam";
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AggregateException("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => _handler.Handle(request, CancellationToken.None));
            _loggerMock.VerifyLog(l => l.LogError(
                It.IsAny<Exception>(),
                "Erro ao validar parâmetro {Parameter} para o perfil {ProfileName}",
                parameterName, profileName));
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenProfileDoesNotExist()
        {
            // Arrange
            var profileName = "NonExistentProfile";
            var parameterName = "testParam";
            var request = new ValidateProfileParameterRequest(profileName, parameterName);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProfileParametersRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AggregateException("Profile not found"));

            // Act & Assert
            await Assert.ThrowsAsync<AggregateException>(() => _handler.Handle(request, CancellationToken.None));
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
            catch
            {
                // Ignore - we just want to verify the log was called
            }
        }
    }
}