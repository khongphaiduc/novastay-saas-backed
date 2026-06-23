using Microsoft.AspNetCore.Http;
using NovaStay.Application.DTOs;
using System.IO;

namespace NovaStay.API.Extensions;

public static class FormFileExtensions
{
    public static UploadRoomImageRequest ToUploadRequest(this IFormFile? file, bool isCover)
    {
        if (file == null)
        {
            return new UploadRoomImageRequest
            {
                ImageStream = Stream.Null,
                FileName = string.Empty,
                ContentType = "image/jpeg",
                FileSize = 0,
                IsCover = isCover
            };
        }

        return new UploadRoomImageRequest
        {
            ImageStream = file.OpenReadStream(),
            FileName = Path.GetFileName(file.FileName),
            ContentType = file.ContentType,
            FileSize = file.Length,
            IsCover = isCover
        };
    }
}
