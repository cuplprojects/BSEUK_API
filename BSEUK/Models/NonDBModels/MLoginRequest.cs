using System.ComponentModel;

namespace BSEUK.Models.NonDBModels
{
    public class MLoginRequest
    {
        public string UserName { get; set; }
        [PasswordPropertyText]
        public string Password { get; set; }

    }
}
