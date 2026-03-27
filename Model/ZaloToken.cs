public class ZaloTokenEntity
{
    public int Id { get; set; }

    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public int? Temp { get; set; } 
    public DateTime ExpiredAt { get; set; }
}