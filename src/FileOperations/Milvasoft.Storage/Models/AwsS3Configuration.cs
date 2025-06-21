namespace Milvasoft.Storage.Models;

/// <summary>
/// Aws S3 configuration.
/// </summary>
public class AwsS3Configuration
{
    /// <summary>
    /// Access key of the user who has access to the bucket.
    /// </summary>
    public string AccessKey { get; set; }

    /// <summary>
    /// Secret key of the user who has access to the bucket.
    /// </summary>
    public string SecretKey { get; set; }

    /// <summary>
    /// The name of the bucket from which the object will be deleted.
    /// </summary>
    public string BucketName { get; set; }

    /// <summary>
    /// Bucket region. 
    /// </summary>
    public string Region { get; set; }
}