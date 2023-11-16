namespace ApiReview.Common.Utils;

public class ResponseDto<T>
{
    public bool Status { get; set; } = true;
    public string Message { get; set; }
    public T Data { get; set; }
}