namespace EcommrceApi.DTO.Auth
{
    public class RefreshRequest
    {
        public string Token { get; set; }=string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
