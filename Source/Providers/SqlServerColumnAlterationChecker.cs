using LinqToDB.MigrateUp.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToDB.MigrateUp.Providers
{
    internal class SqlServerColumnAlterationChecker
    {
        private static readonly Dictionary<string, HashSet<string>> safeAlterations = new Dictionary<string, HashSet<string>>
        {
            ["varchar"] = new HashSet<string> { "varchar", "nvarchar", "text", "ntext" },
            ["char"] = new HashSet<string> { "char", "varchar", "nchar", "nvarchar", "text", "ntext" },
            ["nvarchar"] = new HashSet<string> { "nvarchar", "ntext" },
            ["nchar"] = new HashSet<string> { "nchar", "nvarchar", "ntext" },
            ["text"] = new HashSet<string> { "text", "varchar(max)", "nvarchar(max)" },
            ["ntext"] = new HashSet<string> { "ntext", "nvarchar(max)" },
            ["binary"] = new HashSet<string> { "binary", "varbinary", "image" },
            ["varbinary"] = new HashSet<string> { "varbinary", "image" },
            ["image"] = new HashSet<string> { "image", "varbinary(max)" },
            ["tinyint"] = new HashSet<string> { "tinyint", "smallint", "int", "bigint" },
            ["smallint"] = new HashSet<string> { "smallint", "int", "bigint" },
            ["int"] = new HashSet<string> { "int", "bigint" },
            ["bigint"] = new HashSet<string> { "bigint" },
            ["float"] = new HashSet<string> { "float", "real" },
            ["real"] = new HashSet<string> { "real", "float" },
            ["decimal"] = new HashSet<string> { "decimal", "numeric", "float", "real" },
            ["numeric"] = new HashSet<string> { "numeric", "decimal", "float", "real" },
            ["money"] = new HashSet<string> { "money", "decimal" },
            ["smallmoney"] = new HashSet<string> { "smallmoney", "money", "decimal" },
            ["bit"] = new HashSet<string> { "bit" },
            ["date"] = new HashSet<string> { "date", "smalldatetime", "datetime", "datetime2" },
            ["time"] = new HashSet<string> { "time", "datetime", "datetime2" },
            ["smalldatetime"] = new HashSet<string> { "smalldatetime", "datetime", "datetime2" },
            ["datetime"] = new HashSet<string> { "datetime", "datetime2" },
            ["datetime2"] = new HashSet<string> { "datetime2" },
            ["datetimeoffset"] = new HashSet<string> { "datetimeoffset" },
            ["uniqueidentifier"] = new HashSet<string> { "uniqueidentifier" },
            ["xml"] = new HashSet<string> { "xml" },
        };

        public bool CanSafelyAlterColumn(SqlServerDataType currentType, SqlServerDataType newType)
        {
            // Check for identical types
            if (currentType.BaseType == newType.BaseType)
            {
                return IsCompatibleSizeChange(currentType, newType);
            }

            // Check for safe type conversions
            if (IsSafeTypeConversion(currentType, newType))
            {
                return IsCompatibleSizeChange(currentType, newType);
            }

            return false;
        }

        private bool IsSafeTypeConversion(SqlServerDataType currentType, SqlServerDataType newType)
        {
            return safeAlterations.TryGetValue(currentType.BaseType, out var safeTypes) &&
                   (safeTypes.Contains(newType.BaseType) || safeTypes.Contains(newType.ToString()));
        }

        private bool IsCompatibleSizeChange(SqlServerDataType currentType, SqlServerDataType newType)
        {
            // For types without size specifications
            if (currentType.MaxLength == null && newType.MaxLength == null &&
                currentType.Precision == null && newType.Precision == null)
            {
                return true;
            }

            // For variable-length types (string and binary)
            if (IsVariableLengthType(currentType.BaseType) && IsVariableLengthType(newType.BaseType))
            {
                // Allow conversion from fixed-length types to variable-length types
                if (IsFixedLengthType(currentType.BaseType) && !IsFixedLengthType(newType.BaseType))
                {
                    return true;
                }

                // Allow conversion from non-max to max
                if (currentType.MaxLength != "max" && newType.MaxLength == "max")
                {
                    return true;
                }

                // Allow increasing size for non-max types
                if (currentType.MaxLength != "max" && newType.MaxLength != "max")
                {
                    return IsIncreasingSizeOrMax(currentType.MaxLength, newType.MaxLength);
                }

                // Disallow conversion from max to non-max or decreasing size
                return false;
            }

            // Special cases for text, ntext, and image types
            if (IsLegacyLobType(currentType.BaseType))
            {
                return IsCompatibleLobTypeChange(currentType.BaseType, newType.BaseType, newType.MaxLength);
            }

            // For numeric types
            if (IsNumericType(currentType.BaseType) && IsNumericType(newType.BaseType))
            {
                return IsIncreasingPrecisionAndScale(currentType, newType);
            }

            // For datetime types
            if (IsDateTimeType(currentType.BaseType) && IsDateTimeType(newType.BaseType))
            {
                return IsIncreasingPrecision(currentType, newType);
            }

            return false;
        }

        private bool IsVariableLengthType(string baseType)
        {
            return baseType == "varchar" || baseType == "nvarchar" || baseType == "varbinary" ||
                   baseType == "char" || baseType == "nchar" || baseType == "binary";
        }

        private bool IsFixedLengthType(string baseType)
        {
            return baseType == "char" || baseType == "nchar" || baseType == "binary";
        }

        private bool IsLegacyLobType(string baseType)
        {
            return baseType == "text" || baseType == "ntext" || baseType == "image";
        }

        private bool IsCompatibleLobTypeChange(string currentBaseType, string newBaseType, string newMaxLength)
        {
            switch (currentBaseType)
            {
                case "text":
                    return (newBaseType == "varchar" || newBaseType == "nvarchar") && newMaxLength == "max";
                case "ntext":
                    return newBaseType == "nvarchar" && newMaxLength == "max";
                case "image":
                    return newBaseType == "varbinary" && newMaxLength == "max";
                default:
                    return false;
            }
        }

        private bool IsNumericType(string baseType)
        {
            return baseType == "tinyint" || baseType == "smallint" || baseType == "int" ||
                   baseType == "bigint" || baseType == "decimal" || baseType == "numeric" ||
                   baseType == "float" || baseType == "real";
        }

        private bool IsDateTimeType(string baseType)
        {
            return baseType == "datetime" || baseType == "datetime2" || baseType == "datetimeoffset";
        }

        private bool IsIncreasingSizeOrMax(string currentSize, string newSize)
        {
            if (newSize == "max") return true;
            if (currentSize == "max") return false;

            if (int.TryParse(currentSize, out int currentSizeInt) &&
                int.TryParse(newSize, out int newSizeInt))
            {
                return newSizeInt >= currentSizeInt;
            }

            return false;
        }

        private bool IsIncreasingPrecisionAndScale(SqlServerDataType currentType, SqlServerDataType newType)
        {
            if (!currentType.Precision.HasValue || !newType.Precision.HasValue) return false;

            if (newType.Precision.Value < currentType.Precision.Value) return false;

            if (!currentType.Scale.HasValue || !newType.Scale.HasValue) return true;

            return newType.Scale.Value >= currentType.Scale.Value;
        }

        private bool IsIncreasingPrecision(SqlServerDataType currentType, SqlServerDataType newType)
        {
            if (!currentType.Precision.HasValue || !newType.Precision.HasValue) return true;

            return newType.Precision.Value >= currentType.Precision.Value;
        }
    }
}