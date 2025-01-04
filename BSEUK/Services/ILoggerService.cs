namespace BSEUK.Services
{
    public interface ILoggerService
    {
        void LogChangeInMarks(string message, string Category, int oldMarks, int newMarks,int userID);
        void LogChangeInAbsent(string message, string Category, bool oldValue, bool newValue, int UserID);
    }
}
