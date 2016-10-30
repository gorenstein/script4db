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
    class OdbcReadTableStructure
    {
        private OdbcConnection _connection;
        private string _tableName;
        private string _skeletonCreate;
        private string _skeletonInsert;

        public OdbcReadTableStructure(OdbcConnection connection, string tableName)
        {
            _connection = connection;
            _tableName = tableName;
            //Test();
            ParceStructure();
        }

        public string SkeletonCreate
        {
            get { return _skeletonCreate; }
        }

        public string SkeletonInsert
        {
            get { return _skeletonInsert; }
        }

        private void ParceStructure()
        {
            List<TableColumn> tableColumns = new List<TableColumn>();
            StringBuilder buildCreateSeleton = new StringBuilder(string.Empty);
            DataTable table = _connection.GetSchema("Columns");

            foreach (DataRow row in table.Rows)
            {
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

            // CREATE TABLE Persons (PersonID int, LastName varchar(255));
            // Generate Skeletons "PersonID int, LastName varchar(255)"
            foreach (TableColumn column in tableColumns)
            {
                //Console.WriteLine(column.ToString());
                //Console.WriteLine(column.GetSqlDefinition());
                if (buildCreateSeleton.Length > 0) buildCreateSeleton.Append(", ");
                buildCreateSeleton.Append(column.GetSqlDefinition());
            }
            _skeletonCreate = buildCreateSeleton.ToString();
        }

        private void Test()
        {
                // Connect to the database then retrieve the schema information.
                DataTable table = _connection.GetSchema("Tables");

                // Display the contents of the table.
                DisplayData(table);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
        }

        private static void DisplayData(System.Data.DataTable table)
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
