using System;

namespace StarCorp.Logger
{
    public interface IStarCorpLogger<T>
    {
        void LogInformation(string message, params object[] args);

        void LogWarning(string message, params object[] args);
        void LogWarning(Exception ex, string message, params object[] args);

        void LogError(string message, params object[] args);
        void LogError(Exception ex, string message, params object[] args);
    }
}