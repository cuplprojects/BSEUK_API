using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEUK.Models
{
    public class Candidate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CandidateID { get; set; }

        public string CandidateName { get; set; }

        public string Group { get; set; }

        public string RollNumber { get; set; }

        public string FName { get; set; }

        public string MName { get; set; }

        public string DOB { get; set; }

        public string InstitutionName { get; set; }

        public int SemID { get; set; }

        public int SesID { get; set; }

        public string Category { get; set; }

        public string PapersOpted { get; set; }

        public int? Dist_Code { get; set; }
    }
}
