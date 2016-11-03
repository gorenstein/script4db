using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    class TableColumn
    {
        private string[] stringFieldTypes =
            new string[]
            {
                "CHAR", "VARCHAR",
                "BINARY", "VARBINARY",
                "BLOB", "TINYBLOB", "MEDIUMBLOB", "LONGBLOB",
                "TEXT", "TINYTEXT", "MEDIUMTEXT", "LONGTEXT",
                "ENUM", "SET", "JSON"
            };

        private string _type;
        public string Name { get; set; }
        public int OrdinalPosition { get; set; }
        public string Type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (!string.IsNullOrEmpty(_type))
                {
                    if (_type.Equals("integer")) { _type = "int"; }
                    if (_type.Equals("date")) { _type = "datetime"; }
                    //if (_type.Equals("varchar")) { _type = "nvarchar"; }
                }
            }
        }
        public string ColumnSize { get; set; }
        public string Nullable { get; set; }

        public string GetFieldSetValuePlaceholder()
        {
            // INSERT INTO TableName SET col_string=":col_string", col_num=:col_num 
            string sqlSyntax = string.Format("");

            if (stringFieldTypes.Contains(Type))
            {
                sqlSyntax = string.Format("':{0}'",Name);
            }
            else
            {
                sqlSyntax = string.Format(":{0}", Name);
            }

            return sqlSyntax;
        }
         
        public string GetCreateFieldDefinition()
        {
            string sqlSyntax = string.Format("");

            if (stringFieldTypes.Contains(Type))
            {
                sqlSyntax = string.Format("{0} {1}({2}) {3}", Name, Type, ColumnSize, Nullable);
            }
            else
            {
                sqlSyntax = string.Format("{0} {1} {2}", Name, Type, Nullable);
            }

            return sqlSyntax;
        }
        public override string ToString()
        {
            string tblColumn = string.Format("{0} {1} {2} {3} {4}", OrdinalPosition, Name, _type, ColumnSize, Nullable);

            return tblColumn;
        }
    }
}