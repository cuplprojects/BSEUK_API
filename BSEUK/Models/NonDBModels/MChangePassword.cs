using System.ComponentModel;

namespace BSEUK.Models.NonDBModels
{
    public class MChangePassword
    {
        [PasswordPropertyText]
        public string OldPassword { get; set; }

        [PasswordPropertyText]
        public string NewPassword { get; set; }
    }
}
