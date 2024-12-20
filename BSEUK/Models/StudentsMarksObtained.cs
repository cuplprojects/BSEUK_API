using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class StudentsMarksObtained
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SmoID { get; set; }

        public int CandidateID { get; set; }

        public int PaperID { get; set; }

        public int? TheoryPaperMarks { get; set; }

        public int? InteralMarks { get; set; }

        public int? PracticalMaxMarks { get; set; }
    }
}
