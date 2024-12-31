namespace BSEUK.Models
{
    public class Paper
    {
        public int PaperID { get; set; }
        
        public string PaperName { get; set; }

        public int? PaperCode { get; set; }

        public int PaperType { get; set; }

        public int? TheoryPaperMaxMarks { get; set; }

        public int? InteralMaxMarks { get; set; }

        public int? PracticalMaxMarks { get; set; }

        public int SemID { get; set; }

        public int TotalMarks { get; set; }

    }
}
