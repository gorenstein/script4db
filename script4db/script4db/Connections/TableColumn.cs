using System;
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
        private string[] stringFieldTypes =
            new string[]
            {
                "CHAR", "VARCHAR",
                "BINARY", "VARBINARY",
                "BLOB", "TINYBLOB", "MEDIUMBLOB", "LONGBLOB",
                "TEXT", "TINYTEXT", "MEDIUMTEXT", "LONGTEXT",
                "ENUM", "SET", "JSON"
            };
        private OdbcType _providerType;
        public string Type { get; set; }
        public string Name { get; set; }
        public int OrdinalPosition { get; set; }
        public int ColumnSize { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public OdbcType ProviderType
        {
            get { return _providerType; }
            set
            {
                _providerType = value;
                switch (_providerType)
                {
                    case OdbcType.BigInt:
                    case OdbcType.Int:
                    case OdbcType.SmallInt:
                    case OdbcType.TinyInt:
                    case OdbcType.Bit:
                        Type = "INT";
                        break;
                    case OdbcType.Char:
                    case OdbcType.NChar:
                    case OdbcType.NVarChar:
                    case OdbcType.VarChar:
                    case OdbcType.NText:
                    case OdbcType.Text:
                        Type = "VARCHAR";
                        break;
                    case OdbcType.DateTime:
                    case OdbcType.Timestamp:
                    case OdbcType.Date:
                    case OdbcType.Time:
                    case OdbcType.SmallDateTime:
                        Type = "DATETIME";
                        break;
                    case OdbcType.Decimal:
                    case OdbcType.Numeric:
                    case OdbcType.Double:
                    case OdbcType.Real:
                        Type = "DOUBLE";
                        break;
                    case OdbcType.Binary:
                    case OdbcType.Image:
                    case OdbcType.UniqueIdentifier:
                    case OdbcType.VarBinary:
                    default:
                        Type = "VARCHAR";
                        break;
                }
            }
        }
        public string DataType { get; set; }
        public string Nullable { get; set; }

        public string GetFieldSetValuePlaceholder()
        {
            // INSERT INTO TableName SET col_string=":col_string", col_num=:col_num 
            string sqlSyntax = string.Format("");

            if (!IsNumeric() || Type == "DATETIME") sqlSyntax = string.Format("':{0}:'", Name);
            else sqlSyntax = string.Format(":{0}:", Name);

            return sqlSyntax;
        }

        public bool IsNumeric()
        {
            return !stringFieldTypes.Contains(Type);
        }

        public string GetCreateFieldDefinition()
        {
            string sqlSyntax = string.Format("");
            string nulable = "NULL"; //override this.Nullable

            if (stringFieldTypes.Contains(Type))
            {
                int size = (ColumnSize > 255) ? 255 : ColumnSize;
                sqlSyntax = string.Format("{0} {1}({2}) {3}", Name, Type, size, nulable);
            }
            else
            {
                sqlSyntax = string.Format("{0} {1} {2}", Name, Type, nulable);
            }

            return sqlSyntax;
        }
        public override string ToString()
        {
            string tblColumn =
                string.Format("Pos: {0} | ColName:{1} | Type:{2}/{3}/{4}/{5} | Size:{6} | {7}",
                                OrdinalPosition, Name, DataType, Type, _providerType.ToString(), (int)_providerType, ColumnSize, Nullable
                             );

            return tblColumn;
        }
    }
}