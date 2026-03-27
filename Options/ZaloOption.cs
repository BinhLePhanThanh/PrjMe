public class ZaloOptions
{
    public string AppId { get; set; }

    public string Code { get; set; }              // dùng 1 lần
    public string SecretKey { get; set; }
    public string CodeVerifier { get; set; }      // PKCE
    public string RefreshToken { get; set; }      // dùng lâu dài
}