using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class UserAuth
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UaID { get; set; }

        public int UserID { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
