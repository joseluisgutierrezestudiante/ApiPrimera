namespace ApiPrimera.Interfaces;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(string base64Image);
    Task<string> UploadImageAsync(Stream imageStream, string fileName);
    Task<bool> DeleteImageByUrlAsync(string imageUrl);
}
