namespace SQTWeb.DTOModels;

public class ValidationDTO<T>
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}