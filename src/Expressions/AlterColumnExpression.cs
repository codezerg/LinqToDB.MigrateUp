using System;
using System.Linq;
using System.Linq.Expressions;
using LinqToDB.MigrateUp.Abstractions;
using LinqToDB.MigrateUp.Schema;
using LinqToDB.Mapping;

namespace LinqToDB.MigrateUp.Expressions;

/// <summary>
/// Represents an expression for altering a database column.
/// </summary>
/// <typeparam name="TEntity">The type representing the database table.</typeparam>
public sealed class AlterColumnExpression<TEntity> : IMigrationTask, IAlterColumnExpression<TEntity>
    where TEntity : class
{
    private string? _columnName;
    private string? _newDataType;
    private bool? _isNullable;
    private string? _defaultValue;
    private readonly MigrationProfile _profile;

    /// <summary>
    /// Gets the migration profile associated with this expression.
    /// </summary>
    public MigrationProfile Profile => _profile;

    /// <inheritdoc/>
    public Type EntityType => typeof(TEntity);

    /// <summary>
    /// Initializes a new instance of the <see cref="AlterColumnExpression{TEntity}"/> class.
    /// </summary>
    /// <param name="profile">The migration profile associated with this expression.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="profile"/> is null.</exception>
    public AlterColumnExpression(MigrationProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    /// <inheritdoc/>
    public IAlterColumnExpression<TEntity> Column<TProperty>(Expression<Func<TEntity, TProperty>> propertySelector)
    {
        if (propertySelector == null)
            throw new ArgumentNullException(nameof(propertySelector));

        // Extract property name from the expression
        if (propertySelector.Body is MemberExpression memberExpression)
        {
            _columnName = memberExpression.Member.Name;
            
            // Check for Column attribute to get the actual database column name
            var columnAttr = memberExpression.Member
                .GetCustomAttributes(typeof(ColumnAttribute), true)
                .FirstOrDefault() as ColumnAttribute;
            
            if (columnAttr?.Name != null)
            {
                _columnName = columnAttr.Name;
            }
        }
        else
        {
            throw new ArgumentException("Expression must be a property selector", nameof(propertySelector));
        }

        return this;
    }

    /// <inheritdoc/>
    public IAlterColumnExpression<TEntity> ToType(string newDataType)
    {
        if (string.IsNullOrWhiteSpace(newDataType))
            throw new ArgumentException("Data type cannot be null or empty", nameof(newDataType));

        _newDataType = newDataType;
        return this;
    }

    /// <inheritdoc/>
    public IAlterColumnExpression<TEntity> Nullable()
    {
        _isNullable = true;
        return this;
    }

    /// <inheritdoc/>
    public IAlterColumnExpression<TEntity> NotNullable()
    {
        _isNullable = false;
        return this;
    }

    /// <inheritdoc/>
    public IAlterColumnExpression<TEntity> WithDefault(string defaultValue)
    {
        _defaultValue = defaultValue;
        return this;
    }

    /// <summary>
    /// Executes the column alteration task.
    /// </summary>
    /// <param name="provider">The migration provider to use for executing the task.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="provider"/> is null.</exception>
    void IMigrationTask.Run(IMigrationProvider provider)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        if (string.IsNullOrWhiteSpace(_columnName))
            throw new InvalidOperationException("Column name must be specified using Column() method");

        var tableName = provider.Migration.GetEntityName<TEntity>();

        // Build the new column definition if any changes are specified
        if (_newDataType != null || _isNullable.HasValue || _defaultValue != null)
        {
            var column = BuildColumnDefinition(provider);
            provider.AlterColumn<TEntity>(tableName, _columnName, column);
        }
    }

    private TableColumn BuildColumnDefinition(IMigrationProvider provider)
    {
        // Get current column info if we need to preserve some attributes
        var tableName = provider.Migration.GetEntityName<TEntity>();
        var currentColumns = provider.SchemaService.GetTableColumns(tableName);
        var currentColumn = currentColumns.FirstOrDefault(c => 
            string.Equals(c.ColumnName, _columnName, StringComparison.OrdinalIgnoreCase));

        // Use new values if specified, otherwise keep current values
        var dataType = _newDataType ?? currentColumn?.DataType ?? "TEXT";
        var isNullable = _isNullable ?? currentColumn?.IsNullable ?? true;

        return new TableColumn(_columnName!, dataType, isNullable);
    }
}