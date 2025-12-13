using System.Net.Mime;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MusicStreamingService.Infrastructure.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public static class MinioClientExtensions
{
    public static async Task<Result<string>> GetPresignedUrl(
        this IMinioClient minioClient,
        string bucketName,
        string objectName,
        int expiryInSeconds)
    {
        var objectExists = await minioClient.DoesObjectExist(bucketName, objectName);
        
        if (!objectExists)
        {
            return new Exception("File does not exist");
        }
        
        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiryInSeconds);
        
        try
        {
            var url = await minioClient.PresignedGetObjectAsync(args);
            if (url is null)
            {
                return new Exception("Failed to generate presigned URL");
            }

            return url;
        }
        catch (ObjectNotFoundException e)
        {
            return e;
        }
    }

    private static async Task<bool> DoesObjectExist(
        this IMinioClient minioClient,
        string bucketName,
        string objectName)
    {
        var args = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);
        
        try
        {
            await minioClient.StatObjectAsync(args);
            return true;
        }
        catch (Exception e)
        {
            if (e is ObjectNotFoundException or BucketNotFoundException)
            {
                return false;   
            }

            throw;
        }
    }
    
    public static async Task UploadObject(
        this IMinioClient minioClient,
        string bucketName,
        string objectName,
        Stream data,
        string contentType)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);
        
        await minioClient.PutObjectAsync(args);
    }
}