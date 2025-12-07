using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public static class MinioClientExtensions
{
    public static async Task<Result<string, Exception>> GetPresignedUrl(
        this IMinioClient minioClient,
        string bucketName,
        string objectName,
        int expiryInSeconds)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);
        
        try
        {
            var url = await minioClient.PresignedGetObjectAsync(args);
            if (url == null)
            {
                return new Exception("Failed to generate presigned URL.");
            }

            return url;
        }
        catch (ObjectNotFoundException e)
        {
            return e;
        }
    }
}