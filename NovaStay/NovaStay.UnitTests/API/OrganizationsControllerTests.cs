using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class OrganizationsControllerTests
{
    [Fact]
    public async Task GetResidents_ReturnsOk_WhenOrganizationExists()
    {
        var service = new FakeOrganizationResidentService
        {
            Residents = [new OrganizationResidentDto { ResidentId = Guid.NewGuid() }]
        };
        var controller = new OrganizationsController(service);

        var result = await controller.GetResidents(Guid.NewGuid(), "ACTIVE");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.Residents, ok.Value);
    }

    [Fact]
    public async Task GetResidents_ReturnsNotFound_WhenOrganizationDoesNotExist()
    {
        var controller = new OrganizationsController(new FakeOrganizationResidentService { Residents = null });

        var result = await controller.GetResidents(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetResidentInvitations_ReturnsOk_WhenOrganizationExists()
    {
        var service = new FakeOrganizationResidentService
        {
            Invitations = [new OrganizationResidentInvitationDto { MembershipId = Guid.NewGuid() }]
        };
        var controller = new OrganizationsController(service);

        var result = await controller.GetResidentInvitations(Guid.NewGuid(), "PENDING");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(service.Invitations, ok.Value);
    }

    [Fact]
    public async Task GetResidentInvitations_ReturnsNotFound_WhenOrganizationDoesNotExist()
    {
        var controller = new OrganizationsController(new FakeOrganizationResidentService { Invitations = null });

        var result = await controller.GetResidentInvitations(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task InviteResident_ReturnsCreated_WhenServiceSucceeds()
    {
        var response = new ResidentMembershipResponse { MembershipId = Guid.NewGuid() };
        var controller = new OrganizationsController(new FakeOrganizationResidentService
        {
            InviteResponse = response
        });

        var result = await controller.InviteResident(Guid.NewGuid(), new InviteResidentToOrganizationRequest());

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Equal($"/api/resident-memberships/{response.MembershipId}", created.Location);
        Assert.Same(response, created.Value);
    }

    [Fact]
    public async Task InviteResident_ReturnsNotFound_WhenServiceCannotFindEntity()
    {
        var controller = new OrganizationsController(new FakeOrganizationResidentService
        {
            InviteException = new KeyNotFoundException("not found")
        });

        var result = await controller.InviteResident(Guid.NewGuid(), new InviteResidentToOrganizationRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task InviteResident_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var controller = new OrganizationsController(new FakeOrganizationResidentService
        {
            InviteException = new InvalidOperationException("invalid")
        });

        var result = await controller.InviteResident(Guid.NewGuid(), new InviteResidentToOrganizationRequest());

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
