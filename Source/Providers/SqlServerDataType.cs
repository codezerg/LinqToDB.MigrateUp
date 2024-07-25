using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LinqToDB.MigrateUp.Providers
{
    internal class SqlServerDataType
    {
        public string BaseType { get; private set; }
        public int? Precision { get; private set; }
        public int? Scale { get; private set; }
        public string MaxLength { get; private set; }

        private static readonly Regex DataTypeRegex = new Regex(
            @"^(\w+)\s*(?:\(\s*((?:max|\d+)(?:\s*,\s*(\d+))?)?\s*\))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        private static readonly HashSet<string> TypesWithMaxLength = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "char", "varchar", "nchar", "nvarchar", "binary", "varbinary"
        };

        private static readonly HashSet<string> TypesWithPrecision = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "decimal", "numeric", "datetime2", "datetimeoffset", "time"
        };

        private static readonly HashSet<string> TypesWithScale = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "decimal", "numeric"
        };

        public SqlServerDataType(string dataTypeString)
        {
            Parse(dataTypeString.ToLower());
        }

        public SqlServerDataType(string baseType, string maxLength = null, int? precision = null, int? scale = null)
        {
            BaseType = baseType;
            if (TypesWithMaxLength.Contains(baseType))
                MaxLength = maxLength;
            if (TypesWithPrecision.Contains(baseType))
                Precision = precision;
            if (TypesWithScale.Contains(baseType))
                Scale = scale;
        }

        private void Parse(string dataTypeString)
        {
            var match = DataTypeRegex.Match(dataTypeString.Trim());
            if (!match.Success)
            {
                throw new ArgumentException("Invalid SQL Server data type string", nameof(dataTypeString));
            }

            BaseType = match.Groups[1].Value;

            if (match.Groups[2].Success)
            {
                if (match.Groups[3].Success && TypesWithScale.Contains(BaseType))
                {
                    if (TypesWithPrecision.Contains(BaseType))
                        Precision = int.Parse(match.Groups[2].Value);
                    Scale = int.Parse(match.Groups[3].Value);
                }
                else if (TypesWithMaxLength.Contains(BaseType))
                {
                    MaxLength = match.Groups[2].Value.Equals("max", StringComparison.OrdinalIgnoreCase)
                        ? "max"
                        : match.Groups[2].Value;
                }
                else if (TypesWithPrecision.Contains(BaseType))
                {
                    Precision = int.Parse(match.Groups[2].Value);
                }
            }
        }

        public override string ToString()
        {
            if (Precision.HasValue && Scale.HasValue)
            {
                return $"{BaseType}({Precision},{Scale})";
            }
            else if (!string.IsNullOrEmpty(MaxLength))
            {
                return $"{BaseType}({MaxLength})";
            }
            else
            {
                return BaseType;
            }
        }

        public static SqlServerDataType FromString(string dataTypeString)
        {
            return new SqlServerDataType(dataTypeString);
        }
    }
}