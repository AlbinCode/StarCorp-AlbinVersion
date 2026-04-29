using Microsoft.Extensions.Logging;
using System;

namespace StarCorp.Logger
{
    public class StarCorpLogger<T> : IStarCorpLogger<T>
    {
        private readonly ILogger<T> _logger;

        public StarCorpLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogWarning(Exception ex, string message, params object[] args)
        {
            _logger.LogWarning(ex, message, args);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }
    }
}