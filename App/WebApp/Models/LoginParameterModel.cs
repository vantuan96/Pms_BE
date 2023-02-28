namespace DrFee.Models
{
    public class LoginParameterModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string captcha { get; set; }
        public bool Validate()
        {
            if (string.IsNullOrEmpty(this.username) || string.IsNullOrEmpty(this.password))
            {
                return false;
            }
            return true;
        }
    }
}