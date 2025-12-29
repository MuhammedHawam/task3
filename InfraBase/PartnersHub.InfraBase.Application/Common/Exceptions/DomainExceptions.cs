namespace PartnersHub.InfraBase.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when domain validation rules are violated
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with id '{key}' was not found")
    {
    }
}
