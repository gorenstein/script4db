using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Threading.Tasks;
using System.Reflection;

namespace script4db.Connections
{
    class OdbcUtil
    {
        // https://msdn.microsoft.com/en-us/library/system.data.idbconnection(v=vs.110).aspx
        // http://stackoverflow.com/questions/30343269/sqlgetinfo-how-to-use-this-function
        public static DbType GetDbType(OdbcConnection connection)
        {
            // Remember connection state
            ConnectionState originalState = connection.State;

            // Ensure that the connection is opened (otherwise executing the command will fail)
            if (originalState != ConnectionState.Open)
                connection.Open();

            //foreach (SqlInfo item in Enum.GetValues(typeof(SqlInfo)))
            //{
            //    System.Console.WriteLine("{0}: {1}", item.ToString(), GetInfoStringUnhandled( connection, item));
            //}

            DbType dbType;

            switch (GetInfoStringUnhandled(connection, SqlInfo.DBMS_NAME))
            {
                case "ACCESS":
                    dbType = DbType.Access;
                    break;
                case "Microsoft SQL Server":
                    dbType = DbType.MSSQL;
                    break;
                case "MySQL":
                    dbType = DbType.MySQL;
                    break;
                case "Oracle":
                    dbType = DbType.Oracle;
                    break;
                default:
                    dbType = DbType.unknow;
                    break;
            }

            // Restory connection state
            if (originalState == ConnectionState.Closed)
                connection.Close();

            return dbType;
        }

        public enum SqlInfo : ushort
        {
            DATA_SOURCE_NAME = (ushort)2,
            DRIVER_NAME = (ushort)6,
            DRIVER_VER = (ushort)7,
            ODBC_VER = (ushort)10,
            SERVER_NAME = (ushort)13,
            SEARCH_PATTERN_ESCAPE = (ushort)14,
            DBMS_NAME = (ushort)17,
            DBMS_VER = (ushort)18,
            IDENTIFIER_CASE = (ushort)28,
            IDENTIFIER_QUOTE_CHAR = (ushort)29,
            CATALOG_NAME_SEPARATOR = (ushort)41,
            DRIVER_ODBC_VER = (ushort)77,
            GROUP_BY = (ushort)88,
            KEYWORDS = (ushort)89,
            ORDER_BY_COLUMNS_IN_SELECT = (ushort)90,
            QUOTED_IDENTIFIER_CASE = (ushort)93,
            SQL_OJ_CAPABILITIES_30 = (ushort)115,
            SQL_SQL92_RELATIONAL_JOIN_OPERATORS = (ushort)161,
            SQL_OJ_CAPABILITIES_20 = (ushort)65003
        }

        public static string GetInfoStringUnhandled(OdbcConnection ocn, SqlInfo info)
        {
            MethodInfo GetInfoStringUnhandled = ocn.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance).First(c => c.Name == "GetInfoStringUnhandled");
            return Convert.ToString(GetInfoStringUnhandled.Invoke(ocn, new object[] { (ushort)info }));
        }
    }
}
