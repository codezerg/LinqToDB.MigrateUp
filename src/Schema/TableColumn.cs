using System;

namespace LinqToDB.MigrateUp.Schema
{
    public class TableColumn : IComparable<TableColumn>, IEquatable<TableColumn>
    {
        public string ColumnName { get; }
        public string DataType { get; }
        public bool IsNullable { get; }

        public TableColumn(string columnName, string dataType, bool isNullable)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));
            if (string.IsNullOrEmpty(dataType))
                throw new ArgumentNullException(nameof(dataType));

            ColumnName = columnName;
            DataType = dataType;
            IsNullable = isNullable;
        }

        public int CompareTo(TableColumn? other)
        {
            if (other == null) return 1;
            return string.Compare(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(TableColumn? other)
        {
            if (other == null) return false;
            return string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(DataType, other.DataType, StringComparison.OrdinalIgnoreCase) &&
                   IsNullable == other.IsNullable;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TableColumn);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + ColumnName.GetHashCode();
            hash = hash * 31 + DataType.GetHashCode();
            hash = hash * 31 + IsNullable.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"{ColumnName} ({DataType}){(IsNullable ? " NULL" : " NOT NULL")}";
        }
    }
}
