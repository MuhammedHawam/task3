using MediatR;
using Microsoft.AspNetCore.Mvc;

using Moq;
using PartnersHub.InnovationHub.Apis.Controllers.ChallengeRequest;


namespace PartnersHub.Services.InnovationHub.Apis.Tests;

public class InnovationHubControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<ILogger<ChallengeRequestController>> _loggerMock = null!;
    private ChallengeRequestController _controller = null!;

    [SetUp]
    public void Setup() {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ChallengeRequestController>>();
      //  _controller = new ChallengeRequestController();
    }


}
