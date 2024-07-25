using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToDB.MigrateUp.Schema
{
    public class TableIndexColumn : IComparable<TableIndexColumn>, IEquatable<TableIndexColumn>
    {
        public string ColumnName { get; }
        public bool IsAscending { get; }

        public TableIndexColumn(string columnName, bool isAscending)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentNullException(nameof(columnName));

            ColumnName = columnName;
            IsAscending = isAscending;
        }

        public int CompareTo(TableIndexColumn other)
        {
            if (other == null) return 1; // Current instance comes after null

            int columnNameComparison = string.Compare(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase);
            if (columnNameComparison != 0) return columnNameComparison;

            return IsAscending.CompareTo(other.IsAscending);
        }

        public bool Equals(TableIndexColumn other)
        {
            if (other == null) return false;
            return string.Equals(ColumnName, other.ColumnName, StringComparison.OrdinalIgnoreCase) &&
                   IsAscending == other.IsAscending;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TableIndexColumn);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + ColumnName.GetHashCode();
            hash = hash * 31 + IsAscending.GetHashCode();
            return hash;
        }

        public override string ToString()
        {
            return $"{ColumnName} ({(IsAscending ? "ASC" : "DESC")})";
        }
    }
}
