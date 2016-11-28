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
	- if string is > 255 chars as 'MEMO' for Access or as 'TEXT' for MySQL and others
- OdbcType.DateTime, OdbcType.Timestamp; OdbcType.Date, OdbcType.Time, OdbcType.SmallDateTime:
	- as 'DATETIME' (NULL or empty will as DateTime.MinValue / 0001-01-01 00:00:00 exported)
- OdbcType.Decimal, OdbcType.Numeric, OdbcType.Double, OdbcType.Real:
	- as 'DOUBLE'
- OdbcType.Binary, OdbcType.Image, OdbcType.UniqueIdentifier, OdbcType.VarBinary:
	- as 'MEMO' for Access or as 'TEXT' for MySQL and others