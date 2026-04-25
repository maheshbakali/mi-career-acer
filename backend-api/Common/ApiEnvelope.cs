namespace MiCareerAcer.Api.Common;

public class ApiEnvelope<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }

    public static ApiEnvelope<T> Ok(T data) => new() { Success = true, Data = data, Error = null };

    public static ApiEnvelope<T> Fail(string error) => new() { Success = false, Data = default, Error = error };
}
