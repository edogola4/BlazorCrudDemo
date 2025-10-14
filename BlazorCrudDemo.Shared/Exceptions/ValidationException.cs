using System;
using System.Collections.Generic;

namespace BlazorCrudDemo.Shared.Exceptions
{
    public class ValidationException : Exception
    {
        public Dictionary<string, string[]> Errors { get; }

        public ValidationException(Dictionary<string, string[]> errors) : base("Validation failed")
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
