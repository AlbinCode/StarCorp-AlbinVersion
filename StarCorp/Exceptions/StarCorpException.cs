using System;

namespace StarCorp.Exceptions
{
    public abstract class StarCorpException : Exception
    {
        protected StarCorpException(string message) : base(message)
        {
        }
    }
}