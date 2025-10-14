namespace BlazorCrudDemo.Data.Exceptions;

/// <summary>
/// Base exception for repository-related errors.
/// </summary>
public class RepositoryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the RepositoryException class.
    /// </summary>
    public RepositoryException() : base() { }

    /// <summary>
    /// Initializes a new instance of the RepositoryException class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RepositoryException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the RepositoryException class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RepositoryException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : RepositoryException
{
    /// <summary>
    /// Gets the entity ID that was not found.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    /// Gets the entity type that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class.
    /// </summary>
    /// <param name="entityType">The type of entity that was not found.</param>
    /// <param name="entityId">The ID of the entity that was not found.</param>
    public EntityNotFoundException(string entityType, int entityId)
        : base($"{entityType} with ID {entityId} was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public EntityNotFoundException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the EntityNotFoundException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when a duplicate entity is found.
/// </summary>
public class DuplicateEntityException : RepositoryException
{
    /// <summary>
    /// Gets the entity type that has a duplicate.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the duplicate value.
    /// </summary>
    public string DuplicateValue { get; }

    /// <summary>
    /// Gets the field name that has the duplicate value.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class.
    /// </summary>
    /// <param name="entityType">The type of entity that has a duplicate.</param>
    /// <param name="fieldName">The field name that has the duplicate value.</param>
    /// <param name="duplicateValue">The duplicate value.</param>
    public DuplicateEntityException(string entityType, string fieldName, string duplicateValue)
        : base($"{entityType} with {fieldName} '{duplicateValue}' already exists.")
    {
        EntityType = entityType;
        FieldName = fieldName;
        DuplicateValue = duplicateValue;
    }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DuplicateEntityException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the DuplicateEntityException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DuplicateEntityException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : RepositoryException
{
    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public IDictionary<string, string[]> ValidationErrors { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="validationErrors">The validation errors.</param>
    public ValidationException(IDictionary<string, string[]> validationErrors)
        : base("Validation failed.")
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="validationErrors">The validation errors.</param>
    public ValidationException(string message, IDictionary<string, string[]> validationErrors)
        : base(message)
    {
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Initializes a new instance of the ValidationException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }
}

/// <summary>
/// Exception thrown when a concurrency conflict occurs.
/// </summary>
public class ConcurrencyException : RepositoryException
{
    /// <summary>
    /// Gets the entity type that had the concurrency conflict.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Gets the entity ID that had the concurrency conflict.
    /// </summary>
    public int EntityId { get; }

    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="entityType">The type of entity that had the concurrency conflict.</param>
    /// <param name="entityId">The ID of the entity that had the concurrency conflict.</param>
    public ConcurrencyException(string entityType, int entityId)
        : base($"Concurrency conflict occurred for {entityType} with ID {entityId}.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public ConcurrencyException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the ConcurrencyException class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }
}
