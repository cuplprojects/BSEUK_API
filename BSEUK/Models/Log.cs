using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }

        public int oldMarks { get; set; }

        public int newMarks { get; set; }

        public int UserID { get; set; }
    }
}
