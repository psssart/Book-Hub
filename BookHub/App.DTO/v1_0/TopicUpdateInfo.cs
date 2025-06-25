namespace App.DTO.v1_0;

public class TopicUpdateInfo
{
    public Guid Id { get; set; }
    public string Tittle { get; set; } = default!;
    public string Content { get; set; } = default!;
}