using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class ManagementPaginationControllerTests
{
    private static OrganizationsController Controller(Guid org) => new(new FakeOrganizationResidentService
    {
        Residents = [new() { ResidentId = Guid.NewGuid() }]
    })
    {
        ControllerContext = new()
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("organizationId", org.ToString())], "test"))
            }
        }
    };

    [Fact]
    public async Task ResidentsPaged_ExposesMetadata_WhileLegacyReturnsArray()
    {
        var org = Guid.NewGuid(); var controller = Controller(org);
        var paged = await controller.GetResidentsPaged(org, 1, 10);
        var data = Assert.IsType<PagedResult<OrganizationResidentDto>>(Assert.IsType<OkObjectResult>(paged.Result).Value);
        Assert.Single(data.Items); Assert.Equal(1, data.TotalCount); Assert.Equal(1, data.TotalPages);
        var legacy = await controller.GetResidents(org);
        Assert.IsAssignableFrom<IReadOnlyList<OrganizationResidentDto>>(Assert.IsType<OkObjectResult>(legacy.Result).Value);
    }

    [Fact]
    public async Task ResidentsPaged_RejectsDifferentOrganizationAndInvalidPagination()
    {
        var org = Guid.NewGuid(); var controller = Controller(org);
        Assert.IsType<ForbidResult>((await controller.GetResidentsPaged(Guid.NewGuid())).Result);
        Assert.IsType<BadRequestObjectResult>((await controller.GetResidentsPaged(org, 0)).Result);
        Assert.IsType<BadRequestObjectResult>((await controller.GetResidentsPaged(org, 1, 1001)).Result);
        Assert.IsType<BadRequestObjectResult>((await controller.GetResidents(org, page: 0)).Result);
    }
}
