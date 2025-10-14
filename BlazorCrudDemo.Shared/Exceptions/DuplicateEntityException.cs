using System;

namespace BlazorCrudDemo.Shared.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public string EntityType { get; }
        public string FieldName { get; }
        public string DuplicateValue { get; }

        public DuplicateEntityException() { }

        public DuplicateEntityException(string message) : base(message) { }

        public DuplicateEntityException(string message, Exception inner) : base(message, inner) { }

        public DuplicateEntityException(string entityType, string fieldName, string duplicateValue)
            : base($"Duplicate {entityType} found with {fieldName}: {duplicateValue}")
        {
            EntityType = entityType;
            FieldName = fieldName;
            DuplicateValue = duplicateValue;
        }
    }
}
