using System;
using System.Text.RegularExpressions;

namespace LinqToDB.MigrateUp.Helpers
{
    /// <summary>
    /// Provides centralized validation logic for the migration library.
    /// </summary>
    public static class ValidationHelper
    {
        private static readonly Regex SqlIdentifierRegex = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);
        private static readonly string[] ReservedKeywords = new[] 
        { 
            "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "WHERE", "JOIN", "LEFT", "RIGHT", 
            "INNER", "OUTER", "CREATE", "ALTER", "DROP", "TABLE", "INDEX", "DATABASE", 
            "COLUMN", "PRIMARY", "KEY", "FOREIGN", "REFERENCES", "CONSTRAINT"
        };

        /// <summary>
        /// Validates a SQL identifier (table name, column name, index name, etc.).
        /// </summary>
        /// <param name="identifier">The identifier to validate.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentException">Thrown when the identifier is invalid.</exception>
        public static void ValidateSqlIdentifier(string identifier, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException($"The {parameterName} cannot be null or whitespace.", parameterName);
            }

            if (identifier.Length > 128)
            {
                throw new ArgumentException($"The {parameterName} '{identifier}' exceeds the maximum length of 128 characters.", parameterName);
            }

            if (!SqlIdentifierRegex.IsMatch(identifier))
            {
                throw new ArgumentException($"The {parameterName} '{identifier}' contains invalid characters. SQL identifiers must start with a letter or underscore and contain only letters, numbers, and underscores.", parameterName);
            }

            if (Array.Exists(ReservedKeywords, k => k.Equals(identifier, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"The {parameterName} '{identifier}' is a reserved SQL keyword.", parameterName);
            }
        }

        /// <summary>
        /// Validates that a string is not null or whitespace.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentException">Thrown when the value is null or whitespace.</exception>
        public static void ValidateNotNullOrWhiteSpace(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"The {parameterName} cannot be null or whitespace.", parameterName);
            }
        }

        /// <summary>
        /// Validates that an object is not null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
        public static void ValidateNotNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}