Mapping notes by export table
=============================

All fields as nullable
----------------------
In target table overrided all fields as nullable

Mapped ODBC fields types
------------------------
In target database will created table with fields types:
- OdbcType.BigInt, OdbcType.Int, OdbcType.SmallInt, OdbcType.TinyInt, OdbcType.Bit
	- as 'INT'
- OdbcType.Char, OdbcType.NChar, OdbcType.NVarChar, OdbcType.VarChar, OdbcType.NText, OdbcType.Text
	- if string is =< 255 chars as 'VARCHAR' 
	- if string is > 255 chars as 'LONGTEXT' for Access or as 'TEXT' for MySQL and others
- OdbcType.DateTime, OdbcType.Timestamp; OdbcType.Date, OdbcType.Time, OdbcType.SmallDateTime:
	- as 'DATETIME'
- OdbcType.Decimal, OdbcType.Numeric, OdbcType.Double, OdbcType.Real:
	- as 'NUMBER' for Access; 'DECIMAL' for others
- OdbcType.Binary, OdbcType.Image, OdbcType.UniqueIdentifier, OdbcType.VarBinary:
	- as 'LONGTEXT' for Access; 'BLOB' for MySQL; 'NVARCHAR(MAX)' for MSSQL and as 'TEXT' for Oracle