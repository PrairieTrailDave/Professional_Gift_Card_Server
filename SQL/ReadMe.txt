Thank you for purchasing the Gift and Loyalty database.

Before building the database, there is a manual step needed. 
There is a database role and a user that must be created first. See the start of the build.sql file for details.

Do not run all these scripts blindly. The Stubs and Tests in their respective directories are development files 
rather than part of the production environment.


The directories in this package include
Change Scripts - there is currently only one file in this directory. In previous versions, there were a lot more.
Create Scripts - where the build scripts are.
                 Build.sql - builds the gift and loyalty specific tables
                 BuildMsg.sql - this is no longer needed, but is included for documentation purposes
                                A lot of stored procedures return an error code. Here is a mapping of that code to a reason
                 MSUser.sql - this builds the tables compatible with the Microsoft user membership functions
                 NewMSUser.sql - this is an experimental development to see if we could work with the newer Microsoft sign on structure
Queries        - holds the stored procedures used in the transactions.
                 Note: there are several stored procedures that are subroutines to other procedures and not called from the application
                       IncrementVisitCount.sql
                       LogTransaction.sql
                       ValidateMerchant.sql
                       It helps to install them first
Reports        - holds the stored procedures to run some of the reports on the system
                 Not all reports need a stored procedure
Stubs          - Visual Studio does not always figure out what the stored procedure interface is. For some of the more complicated routines,
                 we needed to build a stub and install that stub, then run Visual Studio to build the EMDX, then install the real routine.
                 This directory holds the stubs that we used.
Tests          - These are the unit / regression tests to make sure that the stored procedures actually work. 
                 They are run from SQL Management Studio
