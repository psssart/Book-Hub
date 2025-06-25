namespace App.DTO.v1_0;

public class MessageUpdateInfo
{
    public Guid Id { get; set; }
    public string Content { get; set; } = default!;
}