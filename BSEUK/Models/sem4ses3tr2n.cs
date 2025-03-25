using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class Sem4Ses3Tr2N
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Sno { get; set; }
        public string Group { get; set; }
        public int RollNumber { get; set; }
        public string CandidateName { get; set; }
        public string MotherName { get; set; }
        public string FatherName { get; set; }
        public int Dist_Code { get; set; }
        public string InstituteName { get; set; }
        public string Sem1_TotalTheoryInternal { get; set; }
        public string Sem1_TotalPractical { get; set; }
        public string Sem1_TotalMarks { get; set; }
        public string Sem1_Remarks { get; set; }
        public string Sem2_TotalTheoryInternal { get; set; }
        public string Sem2_TotalPractical { get; set; }
        public string Sem2_TotalMarks { get; set; }
        public string Sem2_Remarks { get; set; }
        public string Sem3_TotalTheoryInternal { get; set; }
        public string Sem3_TotalPractical { get; set; }
        public string Sem3_TotalMarks { get; set; }
        public string Sem3_Remarks { get; set; }
        public string Sem4_TotalTheoryInternal { get; set; }
        public string Sem4_TotalPractical { get; set; }
        public string Sem4_TotalMarks { get; set; }
        public string Sem4_Remarks { get; set; }
        public int Grand_TotalTheoryInternal { get; set; }
        public int Grand_TotalPractical { get; set; }
        public int Grand_TotalMarks { get; set; }
        public string Final_Remarks { get; set; }
        public string Final_Remark_Short { get; set; }
        public string Rank { get; set; }
        public string Comments { get; set; }
        public string AwardsheetNumber { get; set; }
    }
}