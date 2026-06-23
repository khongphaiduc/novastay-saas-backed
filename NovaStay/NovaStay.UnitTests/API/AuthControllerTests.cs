using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task RegisterOrganization_ReturnsCreated_WhenServiceSucceeds()
    {
        var auth = new FakeAuthService
        {
            RegisterResponse = new RegisterOrganizationAccountResponse { OrganizationId = Guid.NewGuid() }
        };
        var controller = CreateController(auth);

        var result = await controller.RegisterOrganization(new RegisterOrganizationAccountRequest());

        var created = Assert.IsType<CreatedResult>(result.Result);
        Assert.Same(auth.RegisterResponse, created.Value);
    }

    [Fact]
    public async Task RegisterOrganization_ReturnsConflict_WhenServiceRejectsRequest()
    {
        var controller = CreateController(new FakeAuthService
        {
            RegisterException = new InvalidOperationException("duplicate")
        });

        var result = await controller.RegisterOrganization(new RegisterOrganizationAccountRequest());

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task LoginBusiness_ReturnsOk_WhenCredentialsAreValid()
    {
        var auth = new FakeAuthService
        {
            LoginBusinessResponse = new LoginBusinessAccountResponse { AccountId = Guid.NewGuid() }
        };
        var controller = CreateController(auth);

        var result = await controller.LoginBusiness(new LoginBusinessAccountRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(auth.LoginBusinessResponse, ok.Value);
    }

    [Fact]
    public async Task LoginBusiness_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var controller = CreateController(new FakeAuthService
        {
            LoginBusinessException = new UnauthorizedAccessException("bad login")
        });

        var result = await controller.LoginBusiness(new LoginBusinessAccountRequest());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task LoginResident_ReturnsOk_WhenCredentialsAreValid()
    {
        var residentAuth = new FakeResidentAuthService
        {
            Response = new LoginResidentAccountResponse { AccountId = Guid.NewGuid() }
        };
        var controller = CreateController(residentAuthService: residentAuth);

        var result = await controller.LoginResident(new LoginResidentAccountRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(residentAuth.Response, ok.Value);
    }

    [Fact]
    public async Task LoginResident_ReturnsUnauthorized_WhenCredentialsAreInvalid()
    {
        var controller = CreateController(residentAuthService: new FakeResidentAuthService
        {
            Exception = new UnauthorizedAccessException("bad login")
        });

        var result = await controller.LoginResident(new LoginResidentAccountRequest());

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent()
    {
        var logout = new FakeLogoutService();
        var controller = CreateController(logoutService: logout);

        var result = await controller.Logout(new LogoutRequest { RefreshToken = "refresh" });

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("refresh", logout.Request?.RefreshToken);
    }

    [Fact]
    public async Task ChangePassword_ReturnsUnauthorized_WhenTokenHasNoValidAccountId()
    {
        var controller = CreateController();
        controller.ControllerContext = TestControllerContext.WithUser("not-a-guid");

        var result = await controller.ChangePassword(new ChangePasswordRequest());

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task ChangePassword_ReturnsNoContent_WhenServiceSucceeds()
    {
        var accountId = Guid.NewGuid();
        var changePassword = new FakeChangePasswordService();
        var controller = CreateController(changePasswordService: changePassword);
        controller.ControllerContext = TestControllerContext.WithUser(accountId);

        var result = await controller.ChangePassword(new ChangePasswordRequest());

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(accountId, changePassword.AccountId);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var controller = CreateController(changePasswordService: new FakeChangePasswordService
        {
            Exception = new InvalidOperationException("invalid")
        });
        controller.ControllerContext = TestControllerContext.WithUser(Guid.NewGuid());

        var result = await controller.ChangePassword(new ChangePasswordRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    private static AuthController CreateController(
        FakeAuthService? authService = null,
        FakeChangePasswordService? changePasswordService = null,
        FakeLogoutService? logoutService = null,
        FakeResidentAuthService? residentAuthService = null)
    {
        return new AuthController(
            authService ?? new FakeAuthService(),
            changePasswordService ?? new FakeChangePasswordService(),
            logoutService ?? new FakeLogoutService(),
            residentAuthService ?? new FakeResidentAuthService())
        {
            ControllerContext = TestControllerContext.WithHttpContext()
        };
    }
}
