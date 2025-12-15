using System;

namespace Check_IT.Interfaces
{
    public interface IAppLogger
    {
        void Information(string message);
        void Debug(string message);
        void Warning(string message);
        void Error(Exception? ex, string message);
    }
}