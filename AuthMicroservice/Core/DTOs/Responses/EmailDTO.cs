namespace AuthMicroservice.Core.DTOs.Responses;

public class EmailDTO<T>
{
    public bool EmailSend { get; set; }
    public T? EmailContent { get; set; }
    public string EmailToSend { get; set; } = string.Empty;
    public string TemplateID { get; set; } = string.Empty;
}
