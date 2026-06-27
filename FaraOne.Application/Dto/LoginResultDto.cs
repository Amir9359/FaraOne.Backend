namespace FaraOne.Application.Dto
{
    public class LoginResultDto
    {
        public bool IsSucces { get; set; }
        public string Message { get; set; }
        public LoginDataDto Data { get; set; }
    }

    public class LoginDataDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
    public class RequestLoginDto
    {
        public string phone { get; set; }
        public string Code { get; set; }
    }
}