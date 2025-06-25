namespace App.DTO.v1_0;

public class RatingUpdateInfo
{
    public Guid Id { get; set; }
    public string Value { get; set; } = default!;
    public string Comment { get; set; } = default!;
}