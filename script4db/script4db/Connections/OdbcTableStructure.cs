using System;
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
        private string _skeletonCreate;
        private string _fieldNames;
        private string _fieldValues;

        public OdbcTableStructure(OdbcConnection connection, string tableName)
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

            //Retrieve column schema into a DataTable.
            DataTable schemaTable = GetSchemaTable();

            //Console.WriteLine("{0}:", _tableName);

            TableColumn tableColumn;
            int result;

            //For each field in the table...
            foreach (DataRow field in schemaTable.Rows)
            {
                tableColumn = new TableColumn();

                //For each property of the field...
                foreach (DataColumn column in schemaTable.Columns)
                {
                    switch (column.ColumnName)
                    {
                        case "ColumnName":
                            tableColumn.Name = field[column].ToString();
                            break;
                        case "ColumnOrdinal":
                            tableColumn.OrdinalPosition = (int)field[column];
                            break;
                        case "ColumnSize":
                            tableColumn.ColumnSize = field[column].ToString();
                            break;
                        case "NumericPrecision":
                            if (int.TryParse(field[column].ToString(), out result))
                                tableColumn.NumericPrecision = result;
                            break;
                        case "NumericScale":
                            if (int.TryParse(field[column].ToString(), out result))
                                tableColumn.NumericScale = result;
                            break;
                        case "ProviderType":
                            if (int.TryParse(field[column].ToString(), out result))
                                tableColumn.ProviderType = result;
                            break;
                        case "DataType":
                            tableColumn.Type = field[column].ToString();
                            break;
                        case "AllowDBNull":
                            tableColumn.Nullable = (field[column].ToString().Equals("1")) ? "NULL" : "NOT NULL";
                            break;
                        default:
                            break;
                    }
                }
                //Console.WriteLine("Table:{0} | {1}", _tableName, tableColumn.ToString());
                tableColumns.Add(tableColumn);
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

        private DataTable GetSchemaTable()
        {
            //Retrieve records from the table into a DataReader.
            string cmdText = string.Format("SELECT * FROM {0}", _tableName);
            OdbcCommand command = new OdbcCommand(cmdText, _connection);
            OdbcDataReader dataReader = command.ExecuteReader();

            //Retrieve column schema into a DataTable.
            DataTable schemaTable = dataReader.GetSchemaTable();

            return schemaTable;
        }

        private void Test()
        {
            //Retrieve column schema into a DataTable.
            DataTable schemaTable = GetSchemaTable();

            //For each field in the table...
            foreach (DataRow field in schemaTable.Rows)
            {
                //For each property of the field...
                foreach (DataColumn property in schemaTable.Columns)
                {
                    //Display the field name and value.
                    Console.WriteLine(property.ColumnName + " = " + field[property].ToString());
                }
                Console.WriteLine("------------------");
            }
        }
    }
}
