using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Odbc;

namespace script4db.Connections
{
    class OdbcTableStructure
    {
        private OdbcConnection _connection;
        private string _tableName;
        private string _tableType;
        private string _skeletonCreate;
        private string _fieldNames;
        private string _fieldValues;

        public OdbcTableStructure(OdbcConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
            //Test();
            _tableType = GetTableType();
            ParceStructure();
        }

        public string SkeletonCreate
        {
            get { return _skeletonCreate; }
        }
        public string FieldNames
        {
            get { return _fieldNames; }
        }
        public string FieldValues
        {
            get { return _fieldValues; }
        }
        private void ParceStructure()
        {
            List<TableColumn> tableColumns = new List<TableColumn>();
            StringBuilder buildCreateSeleton = new StringBuilder(string.Empty);
            StringBuilder buildFieldNames = new StringBuilder(string.Empty);
            StringBuilder buildFieldValues = new StringBuilder(string.Empty);
            DataTable table;
            switch (_tableType)
            {
                case "TABLE":
                    table = _connection.GetSchema("Columns");
                    break;
                case "VIEW":
                    table = _connection.GetSchema("Columns", new[] { null, null, null, "VIEW" });
                    //table = _connection.GetSchema("ViewColumns");
                    break;
                default:
                    throw new System.ArgumentException("Table or View not exist.", "TableStructure");
            }

            Console.WriteLine("{0}: {1}", _tableType, _tableName);

            foreach (DataRow row in table.Rows)
            {
                Console.WriteLine("Table:{0} | ColName:{1} | Type:{2} | Size:{3}", row["TABLE_NAME"].ToString(), row["COLUMN_NAME"].ToString(), row["TYPE_NAME"].ToString(), row["COLUMN_SIZE"].ToString());
                if (row["TABLE_NAME"].ToString().Equals(_tableName))
                {
                    tableColumns.Add(new TableColumn
                    {
                        Name = row["COLUMN_NAME"].ToString(),
                        OrdinalPosition = (int)row["ORDINAL_POSITION"],
                        ColumnSize = row["COLUMN_SIZE"].ToString(),
                        Type = row["TYPE_NAME"].ToString(),
                        Nullable = (row["NULLABLE"].ToString().Equals("1")) ? "NULL" : "NOT NULL"
                    });
                }
            }

            var sortedScriptColumns =
                from sc in tableColumns
                orderby sc.OrdinalPosition
                select sc;
            tableColumns = sortedScriptColumns.ToList<TableColumn>();

            string delimiter = "";

            // CREATE TABLE Persons (PersonID int, LastName varchar(255));
            // Generate Skeletons "PersonID int, LastName varchar(255)"
            foreach (TableColumn column in tableColumns)
            {
                //Console.WriteLine(column.ToString());
                //Console.WriteLine(column.GetCreateFieldDefinition());
                //Console.WriteLine(column.GetFieldSetValueDefinition());
                buildCreateSeleton.Append(delimiter + column.GetCreateFieldDefinition());
                buildFieldNames.Append(delimiter + column.Name);
                buildFieldValues.Append(delimiter + column.GetFieldSetValuePlaceholder());
                delimiter = ",";
            }
            _skeletonCreate = buildCreateSeleton.ToString();
            _fieldNames = buildFieldNames.ToString();
            _fieldValues = buildFieldValues.ToString();
        }

        private string GetTableType()
        {
            DataTable table;
            table = _connection.GetSchema("Tables");
            if (IsExistName(table)) return "TABLE";

            table = _connection.GetSchema("Views");
            if (IsExistName(table)) return "VIEW";

            return "";
        }

        private bool IsExistName(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (col.ColumnName == "TABLE_NAME" && (string)row[col] == _tableName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void Test()
        {
            // Connect to the database then retrieve the schema information.
            DataTable table = _connection.GetSchema("Tables");

            // Display the contents of the table.
            DisplayData(table);

            table = _connection.GetSchema("Views");
            DisplayData(table);

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void DisplayData(DataTable table)
        {
            foreach (System.Data.DataRow row in table.Rows)
            {
                foreach (System.Data.DataColumn col in table.Columns)
                {
                    Console.WriteLine("{0} = {1}", col.ColumnName, row[col]);
                }
                Console.WriteLine("============================");
            }
        }

    }
}
