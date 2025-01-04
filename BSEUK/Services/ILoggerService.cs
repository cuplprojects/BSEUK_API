namespace BSEUK.Services
{
    public interface ILoggerService
    {
        void LogChangeInMarks(string message, string Category, int oldMarks, int newMarks,int userID);
    }
}
