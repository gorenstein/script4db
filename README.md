script4db
=========

About
-----
Script interpreter for database manipulation from PC.
Can be used for running batch commands on PC to make routine jobs with different databases.

Features include:
- Run non query SQL command (INSERT, UPDATE, DELETE, CREATE, DROP, RENAME, ALTER)

- Copy tables between two different databases *(fields types mapping matrix see [mapping.md](https://github.com/gorenstein/script4db/blob/master/mapping.md))*

- Support ODBC as preconfigured by name or as connection string *(tested on Access 2016 / MySQL 5.7 / MS SQL Server 2014)*

- For each command you can define behavior on error: stop script or skip/continue

- script constants

Installation
------------
No need for any installation.
Use [script4db.exe](https://github.com/gorenstein/script4db/blob/master/script4db.exe) to run _(compiler settings: any processor, prefer 32-bit)_.

Drivers (32bit/x86) for used ODBC connections must be installed.
Required .NET Framework 4.5.2 or above.

Using
-----
For interactive work simply start script4db.exe

Command line syntax to auto run script:
~~~
script4db.exe /script=you_script_filename /AutoClose=onSuccess
~~~

* If script file used connection to db file over network path, please run console as user with sufficient access right to network share
  (for example current user can having it, but local-admin on default have not it).

* Arguments names are case insensitive.

* Supported auto close mode values _(case insensitive)_ : onSuccess, onError, always, disable.

* If autoclose command line argument is not defined, will be set 'disable' mode on default.

Script Syntax
-------------
For details see [example.script4db](https://github.com/gorenstein/script4db/blob/master/example.script4db) file.


License
-------
Apache License Version 2.0, January 2004
