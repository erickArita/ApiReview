namespace ApiReview.Services.GCS;

public record GCPConfig
{
    public string ProjectId  { get; set; }
    public string BucketName  { get; set; }
};