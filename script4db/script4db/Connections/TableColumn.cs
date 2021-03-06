﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

namespace script4db.Connections
{
    class TableColumn
    {
        public TableColumn()
        {
            ColumnSize = 255;
            ProviderType = OdbcType.VarChar;
        }

        private string[] sqlFieldTypeWithLength = new string[]
        {
            //"INT",
            "VARCHAR",
            "CHAR",
            "BIT",
            "TINYINT",
            "SMALLINT",
            "MEDIUMINT",
            "INTEGER",
            "BIGINT",
            "BINARY",
            "VARBINARY"
        };

        private string[] sqlFieldTypeWithLengthAndDecimals = new string[]
        {
            "REAL",
            "DOUBLE",
            "FLOAT",
            "DECIMAL",
            "NUMERIC"
        };

        private string[] sqlFieldTypeWithoutLength = new string[]
        {
            "INT",
            "DATE",
            "TIME",
            "TIMESTAMP",
            "DATETIME",
            "YEAR",
            "TINYBLOB",
            "BLOB",
            "MEDIUMBLOB",
            "LONGBLOB",
            "TINYTEXT",
            "TEXT",
            "MEDIUMTEXT",
            "LONGTEXT",
            "JSON",
            "NVARCHAR(MAX)",
            "NUMBER"
        };

        /*
          ENUM(value1,value2,value3,...)
          SET(value1,value2,value3,...)
         */

        private OdbcType[] odbcTypeNumeric = new OdbcType[]
        {
            OdbcType.BigInt,
            OdbcType.Int,
            OdbcType.SmallInt,
            OdbcType.TinyInt,
            OdbcType.Bit,
            OdbcType.Decimal,
            OdbcType.Numeric,
            OdbcType.Double,
            OdbcType.Real
        };

        private OdbcType[] odbcTypeString = new OdbcType[]
        {
            OdbcType.Char,
            OdbcType.NChar,
            OdbcType.NVarChar,
            OdbcType.VarChar,
            OdbcType.NText,
            OdbcType.Text,
            OdbcType.Binary,
            OdbcType.Image,
            OdbcType.UniqueIdentifier,
            OdbcType.VarBinary
        };

        private OdbcType[] odbcTypeDatetime = new OdbcType[]
        {
            OdbcType.DateTime,
            OdbcType.Timestamp,
            OdbcType.Date,
            OdbcType.Time,
            OdbcType.SmallDateTime
        };

        public string Name { get; set; }
        public int OrdinalPosition { get; set; }
        public int ColumnSize { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public OdbcType ProviderType { get; set; }
        public string DataType { get; set; }
        public string Nullable { get; set; }

        public string GetFieldSetValuePlaceholder()
        {
            return string.Format(":{0}:", Name);
        }

        public bool IsNumeric()
        {
            return odbcTypeNumeric.Contains(this.ProviderType);
        }
        public bool IsString()
        {
            return odbcTypeString.Contains(this.ProviderType);
        }
        public bool IsStringShort()
        {
            return ColumnSize <= 255 && IsString();
        }
        public bool IsStringLong()
        {
            return ColumnSize > 255 && IsString();
        }
        public bool IsDatetime()
        {
            return odbcTypeDatetime.Contains(this.ProviderType);
        }
        public bool IsWithLenght(string sqlFieldTypeName)
        {
            return sqlFieldTypeWithLength.Contains(sqlFieldTypeName);
        }
        public bool IsWithoutLenght(string sqlFieldTypeName)
        {
            return sqlFieldTypeWithoutLength.Contains(sqlFieldTypeName);
        }
        public bool IsWithLenghtAndDecimals(string sqlFieldTypeName)
        {
            return sqlFieldTypeWithLengthAndDecimals.Contains(sqlFieldTypeName);
        }
        public string GetCreateFieldDefinition(DbType targetDbType)
        {
            string sqlSyntax = string.Format("");
            string nulable = "NULL"; //override this.Nullable
            string sqlFieldTypeName = GetSqlSyntaxName(targetDbType);
            string escName = GetEscapedName(Name, targetDbType);

            if (IsWithLenght(sqlFieldTypeName))
            {
                sqlSyntax = string.Format("{0} {1}({2}) {3}", escName, sqlFieldTypeName, ColumnSize, nulable);
            }
            else if (IsWithLenghtAndDecimals(sqlFieldTypeName))
            {
                int decimalPlace = (NumericScale == 0) ? 2 : NumericScale;
                sqlSyntax = string.Format("{0} {1}({2},{3}) {4}", escName, sqlFieldTypeName, NumericPrecision, decimalPlace, nulable);
            }
            else if (IsWithoutLenght(sqlFieldTypeName))
            {
                sqlSyntax = string.Format("{0} {1} {2}", escName, sqlFieldTypeName, nulable);
            }
            else throw new System.ArgumentException("Can't define Sql Syntax for Field Definition.", this.GetType().Name);

            return sqlSyntax;
        }

        private string GetSqlSyntaxName(DbType targetDbType)
        {
            string sqlName;
            switch (ProviderType)
            {
                case OdbcType.BigInt:
                case OdbcType.Int:
                case OdbcType.SmallInt:
                case OdbcType.TinyInt:
                case OdbcType.Bit:
                    sqlName = "INT";
                    break;
                case OdbcType.Char:
                case OdbcType.NChar:
                case OdbcType.NVarChar:
                case OdbcType.VarChar:
                case OdbcType.NText:
                case OdbcType.Text:
                    if (IsStringShort()) sqlName = "VARCHAR";
                    else if (IsStringLong()) sqlName = GetSqlSyntaxNameForLongString(targetDbType);
                    else throw new System.ArgumentException("Can't define SqlSyntaxName for string field.", this.GetType().Name);
                    break;
                case OdbcType.DateTime:
                case OdbcType.Timestamp:
                case OdbcType.Date:
                case OdbcType.Time:
                case OdbcType.SmallDateTime:
                    sqlName = "DATETIME";
                    break;
                case OdbcType.Decimal:
                case OdbcType.Numeric:
                case OdbcType.Double:
                case OdbcType.Real:
                    switch (targetDbType)
                    {
                        case DbType.Access:
                            sqlName = "NUMBER";
                            break;
                        case DbType.MySQL:
                        case DbType.MSSQL:
                        case DbType.Oracle:
                            sqlName = "DECIMAL";
                            break;
                        case DbType.undefined:
                        case DbType.unknow:
                            sqlName = "-=ERROR=-";
                            break;
                        default:
                            sqlName = "DOUBLE";
                            break;
                    }
                    break;
                case OdbcType.Binary:
                case OdbcType.Image:
                case OdbcType.UniqueIdentifier:
                case OdbcType.VarBinary:
                default:
                    sqlName = GetSqlSyntaxNameForBinary(targetDbType);
                    break;
            }

            return sqlName;
        }

        private string GetSqlSyntaxNameForLongString(DbType targetDbType)
        {
            string sqlName;
            switch (targetDbType)
            {
                case DbType.Access:
                    sqlName = "LONGTEXT";
                    break;
                case DbType.MySQL:
                    sqlName = "TEXT";
                    break;
                case DbType.MSSQL:
                    sqlName = "TEXT";
                    break;
                case DbType.Oracle:
                    sqlName = "TEXT";
                    break;
                case DbType.undefined:
                case DbType.unknow:
                default:
                    throw new System.ArgumentException(string.Format("Not allowed targetDbType: '{0}'.", targetDbType.ToString()), this.GetType().Name);
            }

            return sqlName;
        }

        private string GetSqlSyntaxNameForBinary(DbType targetDbType)
        {
            string sqlName;
            switch (targetDbType)
            {
                case DbType.Access:
                    sqlName = "LONGTEXT";
                    break;
                case DbType.MySQL:
                    sqlName = "BLOB";
                    break;
                case DbType.MSSQL:
                    sqlName = "NVARCHAR(MAX)";
                    break;
                case DbType.Oracle:
                    sqlName = "TEXT";
                    break;
                case DbType.undefined:
                case DbType.unknow:
                default:
                    throw new System.ArgumentException(string.Format("Not allowed targetDbType: '{0}'.", targetDbType.ToString()), this.GetType().Name);
            }

            return sqlName;
        }

        public string GetEscapedName(string nameToEscape, DbType targetDbType)
        {
            string escName;
            switch (targetDbType)
            {
                case DbType.Access:
                case DbType.MySQL:
                    escName = string.Format("`{0}`", nameToEscape);
                    break;
                case DbType.MSSQL:
                    escName = string.Format("[{0}]", nameToEscape);
                    break;
                case DbType.Oracle:
                    escName = nameToEscape;
                    break;
                case DbType.undefined:
                case DbType.unknow:
                default:
                    throw new System.ArgumentException(string.Format("Not allowed targetDbType: '{0}'.", targetDbType.ToString()), this.GetType().Name);
            }

            return escName;
        }

        public override string ToString()
        {
            string tblColumn =
                string.Format("Pos: {0} | ColName:{1} | Type:{2}/{3}/{4} | Size:{5} | Precision:{6} | Scale:{7} | {8}",
                                OrdinalPosition, Name, DataType, ProviderType.ToString(), (int)ProviderType, ColumnSize, NumericPrecision, NumericScale, Nullable
                             );

            return tblColumn;
        }
    }
}