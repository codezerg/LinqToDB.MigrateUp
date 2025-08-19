using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace LinqToDB.MigrateUp.Caching;

/// <summary>
/// Provides utilities for generating hashes of migration task configurations.
/// </summary>
public static class MigrationTaskHasher
{
    /// <summary>
    /// Generates a hash for a migration task based on its configuration.
    /// </summary>
    /// <param name="task">The migration task.</param>
    /// <returns>A hash string representing the task configuration.</returns>
    public static string GenerateHash(IMigrationTask task)
    {
        if (task == null)
            return string.Empty;

        var taskData = GetTaskData(task);
        return ComputeHash(taskData);
    }

    /// <summary>
    /// Gets the data representation of a migration task for hashing.
    /// </summary>
    /// <param name="task">The migration task.</param>
    /// <returns>String representation of the task configuration.</returns>
    private static string GetTaskData(IMigrationTask task)
    {
        var taskType = task.GetType();
        var sb = new StringBuilder();
        
        sb.Append(taskType.FullName);
        
        // Add generic type information if available
        if (taskType.IsGenericType)
        {
            foreach (var genericArg in taskType.GetGenericArguments())
            {
                sb.Append($"|{genericArg.FullName}");
            }
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Computes a SHA256 hash of the given input.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>The SHA256 hash as a hexadecimal string.</returns>
    private static string ComputeHash(string input)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = sha256.ComputeHash(bytes);
            
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            
            return sb.ToString();
        }
    }
}