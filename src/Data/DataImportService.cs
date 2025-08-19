using System;
using LinqToDB.MigrateUp.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Data;

/// <summary>
/// Default implementation of IDataImportService for data import operations.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class DataImportService<TEntity> : IDataImportService<TEntity>
    where TEntity : class
{
    /// <inheritdoc/>
    public bool ShouldImport(DataImportConfiguration config, IMigrationStateManager stateManager, string tableName, bool tableHasData)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        
        if (stateManager == null)
            throw new ArgumentNullException(nameof(stateManager));

        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be null or whitespace.", nameof(tableName));

        if (config.ImportAlways)
            return true;

        var tableCreated = stateManager.IsTableCreated(tableName);
        var tableEmpty = !tableHasData;

        return (tableEmpty && config.WhenTableEmpty) || (tableCreated && config.WhenTableCreated);
    }

    /// <inheritdoc/>
    public IEnumerable<TEntity> GetItemsToInsert(
        IEnumerable<TEntity> sourceItems, 
        IEnumerable<TEntity> existingItems,
        IEnumerable<Func<TEntity, bool>> matchFunctions)
    {
        if (sourceItems == null)
            yield break;

        var existingList = existingItems?.ToList() ?? new List<TEntity>();
        var matchFunctionsList = matchFunctions?.ToList() ?? new List<Func<TEntity, bool>>();

        foreach (var sourceItem in sourceItems)
        {
            var shouldInsert = true;

            foreach (var matchFunc in matchFunctionsList)
            {
                if (existingList.Any(matchFunc))
                {
                    shouldInsert = false;
                    break;
                }
            }

            if (shouldInsert)
                yield return sourceItem;
        }
    }
}