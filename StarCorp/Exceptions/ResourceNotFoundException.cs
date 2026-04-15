using System;

namespace StarCorp.Exceptions
{
    public class ResourceNotFoundException : StarCorpException
    {
        public ResourceNotFoundException(string resourceName, Guid id)
            : base($"The {resourceName.ToLower()} with ID '{id}' could not be found in the database.")
        {
        }
    }
}