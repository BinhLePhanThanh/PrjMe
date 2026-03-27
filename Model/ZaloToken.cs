using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ZaloTokenEntity
{
    [Key] // 🔥 BẮT BUỘC nên có
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string AccessToken { get; set; } = null!;
    public string? RefreshToken { get; set; } // 🔥 nên nullable
    public int? Temp { get; set; }

    public DateTime ExpiredAt { get; set; }
}