using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class Institute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public string InstituteNameHindi { get; set; }

        public string InstituteNameEnglish { get; set; }

        public string Dist_Code { get; set; }
    }
}
