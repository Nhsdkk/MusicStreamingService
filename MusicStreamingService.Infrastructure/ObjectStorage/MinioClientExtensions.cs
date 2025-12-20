using System.Reactive;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using MusicStreamingService.Common.Result;

namespace MusicStreamingService.Infrastructure.ObjectStorage;

public static class MinioClientExtensions
{
    public static async Task<Result<MemoryStream>> GetObjectStream(
        this IMinioClient minioClient,
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        var objectExists =
            await minioClient.DoesObjectExist(bucketName, objectName, cancellationToken: cancellationToken);

        if (!objectExists)
        {
            return new Exception("File does not exist");
        }

        var memoryStream = new MemoryStream();

        var args = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithCallbackStream(x => x.CopyTo(memoryStream));

        try
        {
            await minioClient.GetObjectAsync(args, cancellationToken);
            return memoryStream;
        }
        catch (ObjectNotFoundException e)
        {
            return e;
        }
    }

    public static async Task<Result<string>> GetPresignedUrl(
        this IMinioClient minioClient,
        string bucketName,
        string objectName,
        int expiryInSeconds,
        CancellationToken cancellationToken = default)
    {
        var objectExists = await minioClient.DoesObjectExist(bucketName, objectName, cancellationToken);

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
        string objectName,
        CancellationToken cancellationToken = default)
    {
        var args = new StatObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName);

        try
        {
            await minioClient.StatObjectAsync(args, cancellationToken);
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
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(args, cancellationToken);
    }

    public static async Task<Result<Unit>> RemoveObjects(
        this IMinioClient minioClient,
        string bucketName,
        List<string> objectNames,
        CancellationToken cancellationToken = default)
    {
        var args = new RemoveObjectsArgs()
            .WithBucket(bucketName)
            .WithObjects(objectNames);

        var errors = await minioClient.RemoveObjectsAsync(args, cancellationToken);

        if (errors != null && errors.Any())
        {
            return new AggregateException(
                errors
                    .Where(x => x != null)
                    .Select(x => new Exception(x.Message))
                    .ToList());
        }

        return Unit.Default;
    }
}