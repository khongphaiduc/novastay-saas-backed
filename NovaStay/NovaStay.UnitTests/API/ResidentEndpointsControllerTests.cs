using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class ResidentEndpointsControllerTests
{
    [Fact]
    public async Task GetActiveAccommodations_ReturnsUnauthorized_WhenTokenHasNoValidAccountId()
    {
        var controller = new ResidentAccommodationsController(new FakeResidentService())
        {
            ControllerContext = TestControllerContext.WithUser("invalid")
        };

        var result = await controller.GetActiveAccommodations();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetActiveAccommodations_ReturnsOk_WhenTokenIsValid()
    {
        var service = new FakeResidentService
        {
            Accommodations = [new ResidentAccommodationDto { MembershipId = Guid.NewGuid() }]
        };
        var controller = new ResidentAccommodationsController(service)
        {
            ControllerContext = TestControllerContext.WithUser(Guid.NewGuid())
        };

        var result = await controller.GetActiveAccommodations();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.Accommodations, ok.Value);
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsUnauthorized_WhenTokenHasNoValidAccountId()
    {
        var controller = new ResidentInvitationsController(new FakeResidentInvitationService())
        {
            ControllerContext = TestControllerContext.WithUser("invalid")
        };

        var result = await controller.GetPendingInvitations();

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetPendingInvitations_ReturnsOk_WhenTokenIsValid()
    {
        var service = new FakeResidentInvitationService
        {
            Invitations = [new ResidentInvitationDto { MembershipId = Guid.NewGuid() }]
        };
        var controller = new ResidentInvitationsController(service)
        {
            ControllerContext = TestControllerContext.WithUser(Guid.NewGuid())
        };

        var result = await controller.GetPendingInvitations();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.Invitations, ok.Value);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsUnauthorized_WhenTokenHasNoValidAccountId()
    {
        var controller = new ResidentMembershipsController(new FakeOrganizationResidentService())
        {
            ControllerContext = TestControllerContext.WithUser("invalid")
        };

        var result = await controller.AcceptInvitation(Guid.NewGuid());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsOk_AndPassesDecision()
    {
        var service = new FakeOrganizationResidentService
        {
            AcceptResponse = new ResidentMembershipResponse { MembershipId = Guid.NewGuid() }
        };
        var accountId = Guid.NewGuid();
        var membershipId = Guid.NewGuid();
        var controller = new ResidentMembershipsController(service)
        {
            ControllerContext = TestControllerContext.WithUser(accountId)
        };

        var result = await controller.AcceptInvitation(
            membershipId,
            new ResidentInvitationDecisionRequest { IsAccepted = false });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.AcceptResponse, ok.Value);
        Assert.Equal(accountId, service.AcceptAccountId);
        Assert.Equal(membershipId, service.AcceptMembershipId);
        Assert.False(service.AcceptIsAccepted);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsNotFound_WhenMembershipIsMissing()
    {
        var controller = new ResidentMembershipsController(new FakeOrganizationResidentService
        {
            AcceptException = new KeyNotFoundException("missing")
        })
        {
            ControllerContext = TestControllerContext.WithUser(Guid.NewGuid())
        };

        var result = await controller.AcceptInvitation(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task AcceptInvitation_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var controller = new ResidentMembershipsController(new FakeOrganizationResidentService
        {
            AcceptException = new InvalidOperationException("invalid")
        })
        {
            ControllerContext = TestControllerContext.WithUser(Guid.NewGuid())
        };

        var result = await controller.AcceptInvitation(Guid.NewGuid());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
