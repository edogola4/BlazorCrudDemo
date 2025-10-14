using System.Collections.Generic;

namespace BlazorCrudDemo.Shared.Models
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the collection of validation errors.
        /// </summary>
        public Dictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class with the specified validity.
        /// </summary>
        /// <param name="isValid">A value indicating whether the validation was successful.</param>
        public ValidationResult(bool isValid)
        {
            IsValid = isValid;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class with the specified validity and errors.
        /// </summary>
        /// <param name="isValid">A value indicating whether the validation was successful.</param>
        /// <param name="errors">The collection of validation errors.</param>
        public ValidationResult(bool isValid, Dictionary<string, string[]> errors)
        {
            IsValid = isValid;
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
