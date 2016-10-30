using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace script4db.Connections
{
    class TableColumn
    {
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
                    if (_type.Equals("varchar")) { _type = "nvarchar"; }
                }
            }
        }
        public string ColumnSize { get; set; }
        public string Nullable { get; set; }

        //override 
        public string GetSqlDefinition()
        {
            string sqlSyntax = string.Format("");

            if (Type.IndexOf("CHAR") > -1)
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