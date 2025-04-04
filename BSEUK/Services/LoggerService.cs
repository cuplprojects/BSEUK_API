﻿using BSEUK.Data;
using BSEUK.Models;

namespace BSEUK.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly AppDbContext _appDbContext;

        public LoggerService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public void LogChangeInMarks(string message,string category, int oldMarks, int newMarks, int userID)
        {
            var log = new Log
            {
                Message = message,
                Category = category,
                oldMarks = oldMarks,
                newMarks = newMarks,
                UserID = userID

            };
            _appDbContext.MarksLogs.Add(log);
            _appDbContext.SaveChanges();
        }

        public void LogChangeInAbsent(string message, string category, bool oldMarks, bool newMarks, int userID)
        {
            var log = new Log
            {
                Message = message,
                Category = category,
                oldMarks = oldMarks==true?1:0,
                newMarks = newMarks==true?1:0,
                UserID = userID

            };
            _appDbContext.MarksLogs.Add(log);
            _appDbContext.SaveChanges();
        }
    }
}
