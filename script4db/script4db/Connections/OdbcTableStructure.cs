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
        private DbType _targetDbType;
        private List<TableColumn> tableColumns;
        private string _skeletonCreateTable;
        private string _fieldNames;
        private string _fieldValues;

        public OdbcTableStructure(OdbcConnection connection, string tableName, DbType targetDbType)
        {
            _connection = connection;
            _tableName = tableName;
            _targetDbType = targetDbType;
            //Test();
            ParceStructure();
        }

        public string SkeletonCreateTable
        {
            get { return _skeletonCreateTable; }
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
            tableColumns = new List<TableColumn>();
            StringBuilder buildCreateSeleton = new StringBuilder(string.Empty);
            StringBuilder buildFieldNames = new StringBuilder(string.Empty);
            StringBuilder buildFieldValues = new StringBuilder(string.Empty);

            //Retrieve column schema into a DataTable.
            DataTable schemaTable = GetSchemaTable();

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
                            if (int.TryParse(field[column].ToString(), out result))
                                tableColumn.ColumnSize = result;
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
                                tableColumn.ProviderType = (OdbcType)result;
                            break;
                        case "DataType":
                            tableColumn.DataType = field[column].ToString();
                            break;
                        case "AllowDBNull":
                            tableColumn.Nullable = (field[column].ToString().Equals("True")) ? "NULL" : "NOT NULL";
                            break;
                        default:
                            break;
                    }
                }
                Console.WriteLine("Table:{0} | {1}", _tableName, tableColumn.ToString());
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
                Console.WriteLine(column.ToString());
                //Console.WriteLine(column.GetCreateFieldDefinition());
                //Console.WriteLine(column.GetFieldSetValueDefinition());
                buildCreateSeleton.Append(delimiter + column.GetCreateFieldDefinition(_targetDbType));
                buildFieldNames.Append(delimiter + column.Name);
                buildFieldValues.Append(delimiter + column.GetFieldSetValuePlaceholder(_targetDbType));
                delimiter = ",";
            }
            _skeletonCreateTable = buildCreateSeleton.ToString();
            _fieldNames = buildFieldNames.ToString();
            _fieldValues = buildFieldValues.ToString();
        }

        public bool IsColumnNumeric(int fieldNum)
        {
            foreach (TableColumn column in tableColumns)
            {
                if (column.OrdinalPosition == fieldNum)
                {
                    return column.IsNumeric();
                }
            }
            throw new System.ArgumentException("This part of code must be never reachable in IsColumnNumeric.", this.GetType().Name);
        }

        public bool IsColumnDatetime(int fieldNum)
        {
            foreach (TableColumn column in tableColumns)
            {
                if (column.OrdinalPosition == fieldNum)
                {
                    return column.IsDatetime();
                }
            }
            throw new System.ArgumentException("This part of code must be never reachable in IsColumnNumeric.", this.GetType().Name);
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

            //Console.WriteLine("Table:{0}", _tableName);
            //Console.WriteLine(string.Format("ServerVersion:{0}", _connection.ServerVersion));
            //Console.WriteLine(string.Format("Database:{0}", _connection.Database));
            Console.WriteLine(string.Format("DataSource:{0}", _connection.DataSource));
            return;

            ////Retrieve column schema into a DataTable.
            //DataTable schemaTable = GetSchemaTable();

            ////For each field in the table...
            //foreach (DataRow field in schemaTable.Rows)
            //{
            //    //For each property of the field...
            //    foreach (DataColumn property in schemaTable.Columns)
            //    {
            //        //Display the field name and value.
            //        Console.WriteLine(property.ColumnName + " = " + field[property].ToString());
            //    }
            //    Console.WriteLine("------------------");
            //}
        }
    }
}
