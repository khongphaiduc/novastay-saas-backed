using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;

namespace NovaStay.UnitTests.API;

public sealed class RoomsControllerTests
{
    [Fact]
    public async Task GetRooms_ReturnsOk_WhenServiceSucceeds()
    {
        var service = new FakeRoomService { Rooms = [CreateRoomDto()] };
        var controller = new RoomsController(service);
        var propertyId = Guid.NewGuid();

        var result = await controller.GetRooms(propertyId, "101", "Available");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResult = Assert.IsType<PagedResult<RoomDto>>(ok.Value);
        Assert.Same(service.Rooms, pagedResult.Items);
        Assert.Equal(propertyId, service.GetRoomsPropertyId);
        Assert.Equal("101", service.GetRoomsSearch);
        Assert.Equal("Available", service.GetRoomsStatus);
    }

    [Fact]
    public async Task GetRooms_ReturnsBadRequest_WhenServiceThrowsArgumentException()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            GetRoomsException = new ArgumentException("invalid")
        });

        var result = await controller.GetRooms(Guid.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task CreateRoom_ReturnsCreatedAtAction()
    {
        var room = CreateRoomDto();
        var controller = new RoomsController(new FakeRoomService { CreateResponse = room });

        var result = await controller.CreateRoom(new CreateRoomRequest { PropertyId = room.PropertyId });

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(RoomsController.GetRooms), created.ActionName);
        Assert.Same(room, created.Value);
    }

    [Fact]
    public async Task UpdateRoom_ReturnsOk_WhenServiceSucceeds()
    {
        var room = CreateRoomDto();
        var controller = new RoomsController(new FakeRoomService { UpdateResponse = room });

        var result = await controller.UpdateRoom(room.Id, new UpdateRoomRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(room, ok.Value);
    }

    [Fact]
    public async Task UpdateRoom_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UpdateException = new KeyNotFoundException("missing")
        });

        var result = await controller.UpdateRoom(Guid.NewGuid(), new UpdateRoomRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeleteRoom_ReturnsNoContent_WhenServiceSucceeds()
    {
        var service = new FakeRoomService();
        var controller = new RoomsController(service);
        var roomId = Guid.NewGuid();

        var result = await controller.DeleteRoom(roomId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(roomId, service.DeleteRoomId);
    }

    [Fact]
    public async Task DeleteRoom_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            DeleteException = new KeyNotFoundException("missing")
        });

        var result = await controller.DeleteRoom(Guid.NewGuid());

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UploadImage_ReturnsOk_WhenServiceSucceeds()
    {
        var image = new RoomImageDto { Id = Guid.NewGuid() };
        var controller = new RoomsController(new FakeRoomService { UploadResponse = image });
        var file = new FormFile(new MemoryStream([1, 2, 3]), 0, 3, "image", "room.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var result = await controller.UploadImage(Guid.NewGuid(), file, true);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(image, ok.Value);
    }

    [Fact]
    public async Task UploadImage_ReturnsBadRequest_WhenServiceRejectsRequest()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UploadArgumentException = new ArgumentException("invalid")
        });
        var file = new FormFile(new MemoryStream([1]), 0, 1, "image", "room.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var result = await controller.UploadImage(Guid.NewGuid(), file);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UploadImage_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UploadException = new KeyNotFoundException("missing")
        });
        var file = new FormFile(new MemoryStream([1]), 0, 1, "image", "room.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        var result = await controller.UploadImage(Guid.NewGuid(), file);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdatePrice_ReturnsOk_WhenServiceSucceeds()
    {
        var room = CreateRoomDto();
        var controller = new RoomsController(new FakeRoomService { UpdatePriceResponse = room });

        var result = await controller.UpdatePrice(room.Id, new UpdateBasePriceRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(room, ok.Value);
    }

    [Fact]
    public async Task UpdatePrice_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UpdatePriceException = new KeyNotFoundException("missing")
        });

        var result = await controller.UpdatePrice(Guid.NewGuid(), new UpdateBasePriceRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateOccupants_ReturnsOk_WhenServiceSucceeds()
    {
        var room = CreateRoomDto();
        var controller = new RoomsController(new FakeRoomService { UpdateOccupantsResponse = room });

        var result = await controller.UpdateOccupants(room.Id, new UpdateMaxOccupantsRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(room, ok.Value);
    }

    [Fact]
    public async Task UpdateOccupants_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UpdateOccupantsException = new KeyNotFoundException("missing")
        });

        var result = await controller.UpdateOccupants(Guid.NewGuid(), new UpdateMaxOccupantsRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateAmenities_ReturnsOk_WhenServiceSucceeds()
    {
        var room = CreateRoomDto();
        var controller = new RoomsController(new FakeRoomService { UpdateAmenitiesResponse = room });

        var result = await controller.UpdateAmenities(room.Id, new UpdateAmenitiesRequest());

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(room, ok.Value);
    }

    [Fact]
    public async Task UpdateAmenities_ReturnsNotFound_WhenRoomIsMissing()
    {
        var controller = new RoomsController(new FakeRoomService
        {
            UpdateAmenitiesException = new KeyNotFoundException("missing")
        });

        var result = await controller.UpdateAmenities(Guid.NewGuid(), new UpdateAmenitiesRequest());

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    private static RoomDto CreateRoomDto()
    {
        return new RoomDto
        {
            Id = Guid.NewGuid(),
            PropertyId = Guid.NewGuid(),
            RoomNumber = "101",
            Status = "Available"
        };
    }
}
