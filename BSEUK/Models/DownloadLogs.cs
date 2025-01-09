namespace BSEUK.Models
{
    public class DownloadLogs
    {

        public int LogID { get; set; }

        public int CandidateID { get; set; }

        public int UserID { get; set; }

        public DateTime LoggedAt { get; set; } = DateTime.Now;


    }
}
