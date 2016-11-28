script4db
=========

About
-----
Script interpreter for database manipulation from PC.
Can be used for running batch commands on PC to make routine jobs with different databases.

Features include:
- Run non query SQL command (INSERT, UPDATE, DELETE, CREATE, DROP, RENAME, ALTER)

- Copy tables between two different databases *(fields types mapping matrix see [mapping.md](https://github.com/gorenstein/script4db/blob/master/mapping.md))*

- Support ODBC as preconfigured by name or as connection string *(tested on Access & MySQL connection)*

- For each command you can define behavior on error: stop script or skip/continue

- script constants

Documentation
-------------
For details see [example.script4db](https://github.com/gorenstein/script4db/blob/master/example.script4db) file.

Installation
------------
No need for any installation.
Use [script4db.exe](https://github.com/gorenstein/script4db/blob/master/script4db.exe) to run (compiled for any processor).

Drivers for used ODBC connections must be installed.
Required .NET Framework 4.5.2 or above.

License
-------
Apache License Version 2.0, January 2004
