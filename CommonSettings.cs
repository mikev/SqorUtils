using System.Configuration;

namespace Sqor.Utils
{
    public class CommonSettings
    {
        public static readonly string RabbitMqHost = ConfigurationManager.AppSettings["RabbitMqHost"];
        public static readonly string RabbitMqUserName = ConfigurationManager.AppSettings["RabbitMqUserName"];
        public static readonly string RabbitMqPassword = ConfigurationManager.AppSettings["RabbitMqPassword"];         
        public static readonly string S3UgcAssetsBucketHost = ConfigurationManager.AppSettings["S3UgcAssetsBucketHost"];
        public static readonly string S3UgcAssetsBucket = ConfigurationManager.AppSettings["S3UgcAssetsBucket"];
        public static readonly string S3ProfileImagesBucketHost = ConfigurationManager.AppSettings["S3ProfileImagesBucketHost"];
        public static readonly string S3ProfileImagesBucket = ConfigurationManager.AppSettings["S3ProfileImagesBucket"];
        public static readonly string S3UgcVideosRegionHost = ConfigurationManager.AppSettings["S3UgcVideosRegionHost"];
        public static readonly string S3UgcAssetsBucketCdn = ConfigurationManager.AppSettings["S3UgcAssetsBucketCdn"];
        public static readonly string S3ProfileImagesBucketCdn = ConfigurationManager.AppSettings["S3ProfileImagesBucketCdn"];
        public static readonly string S3UgcVideosThumbnailsBucket = ConfigurationManager.AppSettings["S3UgcVideosThumbnailsBucket"];
        public static readonly string S3UgcVideosThumbnailsBucketCdn = ConfigurationManager.AppSettings["S3UgcVideosThumbnailsBucketCdn"];
        public static readonly string S3UgcVideosOutputBucket = ConfigurationManager.AppSettings["S3UgcVideosOutputBucket"];
        public static readonly string S3UgcVideosBucketCdn = ConfigurationManager.AppSettings["S3UgcVideosBucketCdn"];

        public const string DatabaseUpdateExchange = "db-updates-exchange";
        public const string DatabaseUpdateQueue = "db-updates-queue";

        public const string DataUpdateExchange = "data-updates-exchange";
        public const string DataUpdateQueue = "data-updates-queue";
    }
}