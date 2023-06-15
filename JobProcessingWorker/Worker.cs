using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace JobProcessingWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public Worker(ILogger<Worker> logger, IHostApplicationLifetime hostApplicationLifetime) => (_logger, _hostApplicationLifetime) = (logger, hostApplicationLifetime);

        protected override async Task<string> ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                try
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(10000, stoppingToken);
                    string parameter1 = Environment.GetEnvironmentVariable("ACS");
                    _logger.LogInformation($"ContainerOverrides 2.0: {parameter1}");
                    await UploadDataToS3Bucket();
                    return ProcessJobParameters() + parameter1;
                }
                finally
                {
                    _hostApplicationLifetime.StopApplication();
                }


            }

            return null;
        }

        public string ProcessJobParameters()
        {
            string customData = Environment.GetEnvironmentVariable("inputPayload");

            // Use the job data for processing

            _logger.LogInformation($"customData 2.0: {customData}");

            return customData;
        }

        public async Task UploadDataToS3Bucket()
        {
            // Configure your AWS credentials and region
            var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            var bucketName = "acsworkers3";
            var region = RegionEndpoint.USEast1;

            string customData = Environment.GetEnvironmentVariable("inputPayload");

            // Create an S3 client
            var s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, region);

            // Specify the JSON data to upload
            var jsonData = "{ \"name\": \"John\", \"customData\": " + customData + " }";
            var key = string.Concat("ACS", Environment.GetEnvironmentVariable("ExecutionKey"), ".json");

            // Create a request object
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = jsonData
            };

            // Upload the JSON data to S3
            await s3Client.PutObjectAsync(request);

        }
    }
}