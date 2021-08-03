# SQL Rules

## Performance

### SRP0001

Microsoft Rule:

Ignorable: No

#### Description

Views should not use other views as a data sourcb

#### Notes

Views that use other views in their from cause are extremely inefficient and will result in non-optimal execution plans.

### SRP0002

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using patterns that start with '%' with the LIKE keyword  (Sargeable)

#### Notes

This rule checks for usage of wildcard characters at the beginning of a word while searching using the LIKE keyword. Usage of wildcard characters at the beginning of a LIKE pattern results in an index scan, which defeats the purpose of an index.

### SRP0003

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using DISTINCT keyword inside of aggregate functions.

#### Notes

The rule checks all aggregate functions (except MIN and MAX) for using the DISTINCT keyword. The using DISTINCT in aggregate function can often cause significant performance degradation especially when used multiple times or with other aggregate functions in the same select.

### SRP0004

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid returning results in triggers.

#### Notes

This rule scans triggers to ensure they do not send data back to the caller. Applications that modify tables or views with triggers do not necessarily expect results to be returned as part of the modification operation. For this reason it is not recommended to return results from within triggers.

### SRP0005

Microsoft Rule:

Ignorable: Yes

#### Description

SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers

#### Notes

This rule scans triggers and stored procedures to ensure they SET NOCOUNT to ON at the beginning. Use SET NOCOUNT ON at the beginning of your SQL batches, stored procedures and triggers in production environments, as this prevents the sending of DONE_IN_PROC messages and suppresses messages like '(1 row(s) affected)' to the client for each statement in a stored procedure. For stored procedures that contain several statements that do not return much actual data, setting SET NOCOUNT to ON can provide a significant performance boost, because network traffic is greatly reduced.

### SRP0006

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using not equal operator (<>,!=) in the WHERE clause.  (Sargeable)

#### Notes

The rule checks for usage of the not equal operator in the WHERE clause as it result table and index scans. Consider replacing the not equal operator with equals (=) or inequality operators (>,>=,<,<=) if possible.

### SRP0007

Microsoft Rule:

Ignorable: No

#### Description

Local cursor not closed

#### Notes

The rule checks if any local cursor is closed until the end of the batch. Because when open, the cursor still holds locks on referred-to-tables or views, you should explicitly close it as soon as it is no longer needed.

### SRP0008

Microsoft Rule:

Ignorable: No

#### Description

Local cursor not explicitly de-allocated

#### Notes

### SRP0009

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid wrapping columns within a function in the WHERE clause. (Sargeable)

#### Notes

When a WHERE clause column is wrapped inside a function, the query optimizer does not see the column and if an index exists on the column, the index will not to be used.

### SRP0010

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid the use of functions with UPDATE / INSERT  / DELETE statements. (Halloween Protection)

#### Notes

When a user defined function that does not use SCHEMABINDING is used in an action query the data modifications have to be spooled to tempdb in a two step operation.

### SRP0011

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using NOT IN predicate in the WHERE clause.

#### Notes

Using NOT IN predicate in the WHERE clause generally performs badly, because the SQL Server optimizer has to use a TABLE SCAN instead of an INDEX SEEK even the filtering columns are covered by index.

### SRP0012

Microsoft Rule:

Ignorable: Yes

#### Description

Consider indexing the columns referenced by IN predicates in order to avoid table scans.

#### Notes

Consider indexing the columns referenced by IN predicates in order to avoid table scans

### SRP0013

Microsoft Rule:

Ignorable: Yes

#### Description

Consider replacing the OUTER JOIN with EXISTS

#### Notes

The rule checks for OUTER JOIN-s which could be replaced with EXISTS. Prefer use of EXISTS keyword for existence checks, unless performance issues are encountered. In these cases, it is better to resort to using a LEFT JOIN and null check. The traditional method of checking for row existence is to use a LEFT JOIN and checking the nullability of a LEFT JOIN'ed column in the WHERE clause. The problem with this method is that SQL Server needs to load all of the rows from the OUTER JOIN'ed table. In cases where the matched rows are significantly less than the total rows, it is unnecessary work for SQL Server. Another method of checking for existence is using the EXISTS predicate function. This is preferably to the LEFT JOIN method, since it allows SQL Server to find a row and quit (using a row count spool), avoiding unnecessary row loading.

### SRP0014

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid the use of table variables in join clauses

#### Notes

- Execution plan choices may not be optimal or stable when a table variable contains a large amount of data ( above 100 rows).
- Table variables are not supported in the SQL Server optimizer's cost-based reasoning model. Therefore, they should not be used when cost-based choices are required to achieve an efficient query plan. Temporary tables are preferred when cost-based choices are required. This typically includes queries with joins, parallelism decisions, and index selection choices.
- Queries that modify table variables do not generate parallel query execution plans. Performance can be affected when very large table variables, or table variables in complex queries, are modified. In these situations, consider using temporary tables instead. Queries that read table variables without modifying them can still be parallelized.
- Indexes cannot be created explicitly on table variables, and no statistics are kept on table variables. In some cases, performance may improve by using temporary tables instead, which support indexes and statistics.
- CHECK constraints, DEFAULT values and computed columns in the table type declaration cannot call user-defined functions.
- Assignment operation between table variables is not supported.
- Because table variables have limited scope and are not part of the persistent database, they are not affected by transaction rollbacks.
- Table variables cannot be altered after creation.2:21

### SRP0015

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid the use of calculations on columns in the where clause. (Sargeable)

#### Notes

When a filtering WHERE clause column used in a calculation, the query optimizer does not see the column and if an index exists on the column, the index most likely will not to be used.

### SRP0016

Microsoft Rule:

Ignorable: No

#### Description

Data types on both sides of an equality check should be the same in the where clause.  (Sargeable)

#### Notes

When fields of different data types are joined on or compared, if they are not the same data type, one type will be implicitly converted to the other type. Implicit conversion can lead to data truncation and to performance issues appears in query filter.

### SRP0017

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid updating columns that are part of the primary key.  (Halloween Protection)

#### Notes

- Halloween protection will be invoked causing the data being updated to be done in a two step operation that spools to tempdb.
- All the foreign keys that reference the updated key have to be also updated. If the foreign keys are indexed, it will cause their indexes to be also updated, which can be an expensive operation. Otherwise, if no index exists for foreign key columns, a table lock will be applied.
- When the table has Change Tracking enabled, the values of the primary key column identify the rows that have been changed and this is the only information from the tracked table that is recorded with the change information. If the synchronization of the changed data is implemented based on the Change Tracking, it will fail because of the modified primary key column values.
- If this key is referenced in any external system the reference will be broken upon update.
- The primary keys are usually clustered. Updating the table's clustered index will cause also update of the existing non-clustered indexes.

### SRP0018

Microsoft Rule:

Ignorable: No

#### Description

Query uses a high number of joins.

#### Notes

Queries that use a high number of joins will cause the compiler to time out trying to find a good execution plan. Consider re-arranging the tables in the joins from smallest to largest table and applying the FORCE ORDER query hint.

### SRP0019

Microsoft Rule: SR0009

Ignorable: No

#### Description

Use of very small variable length type (size 1 or 2)

#### Notes

This rule checks for use of very small variable length type (with size of 1 or 2). It is recommended to use fixed length data type when size is less than 3. Variable length type with size less than 3 consumes more space than the same fixed length type.

### SRP0020

Microsoft Rule:

Ignorable: No

#### Description

Table does not have a clustered index

#### Notes

### SRP0021

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid modification of parameters in a stored procedure prior to use in a select query

#### Notes

For best query performance, in some situations you'll need to avoid assigning a new value to a parameter of a stored procedure within the procedure body, and then using the parameter value in a query. The stored procedure and all queries in it are initially compiled with the parameter value first passed in as a parameter to the query.

### SRP0022

Microsoft Rule:

Ignorable: No

#### Description

Consider using RECOMPILE query hint instead of WITH RECOMPILE option

#### Notes

The rule checks that stored procedures do not use WITH RECOMPILE procedure option. The OPTION(RECOMPILE) is the preferred method when a recompile is needed. The WITH RECOMPILE procedure option instructs the Database Engine does not cache a plan for this procedure and the procedure is compiled at run time.

### SRP0023

Microsoft Rule:

Ignorable: No

#### Description

When checking for existence use EXISTS instead of COUNT

#### Notes

COUNT will iterate through every row in the table before returning the result whereas EXISTS will stop as soon as records are found.

### SRP0024

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid the use of correlated subqueries except for very small tables

#### Notes

<https://en.wikipedia.org/wiki/Correlated_subquery>

## Design

### SRD0001

Microsoft Rule:

Ignorable: No

#### Description

Tables should have a natural key

#### Notes

### SRD0002

Microsoft Rule:

Ignorable: No

#### Description

Tables should have a primary key

#### Notes

### SRD0003

Microsoft Rule:

Ignorable: No

#### Description

Primary Keys should avoid using GUIDS or wide VARCHAR columns

#### Notes

Consider moving the clustered index to a different column or consider using NewSequentialId() system function for generating sequential unique identifiers. The native uniqueidentifier, and large varchar data types are not suitable for clustered indexing, because they cause terrible page splits because their values can be completely random.

### SRD0004

Microsoft Rule:

Ignorable: No

#### Description

Columns on both sides of a foreign key should be indexed.

#### Notes

The rule checks for not indexed foreign keys in the current database. Create an index on any foreign key as the foreign keys are used in joins almost always benefit from having an index. It is better to create indexes on all foreign keys, despite the possible overhead of maintaining unneeded indexes than not to have index when needed.

### SRD0005

Microsoft Rule:

Ignorable: No

#### Description

Avoid the char column type except for short static length data

#### Notes

Usage of the char column datatype for lengthy variable type data can cause extra storage to be needed, and for extra processing to remove trailing whitespace.

### SRD0006

Microsoft Rule: SR0001

Ignorable: Yes

#### Description

Avoid `select *`

#### Notes

This rule checks stored procedures, functions, views and triggers for use of '*' in column lists of SELECT statements. Though use of '*' is convenient, it may lead to less maintainable applications. Changes to table or view definitions may cause errors or performance decrease. Using the proper column names takes less load on the database, decreases network traffic and hence can greatly improve performance.

### SRD0007

Microsoft Rule: SR0008

Ignorable: No

#### Description

Use SCOPE_IDENITY instead of @@IDENITY

#### Notes

The rule checks for use of @@IDENTITY server variable. It is recommended to use SCOPE_IDENTITY() instead. @@IDENTITY is not limited to a specific scope and is not a reliable indicator of the most recent user-created identity.

### SRD0008

Microsoft Rule: SR0010

Ignorable: No

#### Description

Avoid using deprecated syntax for joins

#### Notes

This rule checks for the use of non-ANSI join syntax. It is recommended to use the more readable ANSI-Standard JOIN clauses instead of the old style joins. The WHERE clause is used only for filtering data with the ANSI joins, but with older style joins, the WHERE clause handles both the join condition and filtering data.

### SRD0009

Microsoft Rule:

Ignorable: No

#### Description

Wrap multiple action statements within a transaction

#### Notes

Not wrapping multiple action statements in a transaction inside a stored procedure can lead to malformed data if only some of the queries succeed.

### SRD0010

Microsoft Rule:

Ignorable: No

#### Description

Start identity columns used in a primary key with a seed of 1000

#### Notes

### SRD0011

Microsoft Rule:

Ignorable: No

#### Description

Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL.

#### Notes

This rule scans stored procedures, views, functions and triggers to flag use of equality and inequality comparisons involving a NULL constant. These comparisons are undefined when ANSI_NULLS option is set to ON. It is recommended to set ANSI_NULLS to ON and use the IS keyword to compare against NULL constants. Care must be taken when comparing null values. The behavior of the comparison depends on the setting of the SET ANSI_NULLS option. When SET ANSI_NULLS is ON, a comparison in which one or more of the expressions is NULL does not yield either TRUE or FALSE; it yields UNKNOWN. This is because a value that is unknown cannot be compared logically against any other value. This occurs if either an expression is compared to the literal NULL, or if two expressions are compared and one of them evaluates to NULL.

### SRD0012

Microsoft Rule:

Ignorable: Yes

#### Description

Variable declared but never referenced or assigned.|

#### Notes

### SRD0013

Microsoft Rule:

Ignorable: No

#### Description

Wrap TRY..CATCH  around multiple data manipulation statements.|The rule checks for SELECT INTO,INSERT,DELETE and UPDATE statements which are neither inside TRY..CATCH block.

#### Notes

This check is important, because, by default, SQL Server will not rollback all the previous changes within a transaction if a particular statement fails and setting XACT_ABORT is not ON.

### SRD0014

Microsoft Rule:

Ignorable: No

#### Description

TOP clause used in a query without an ORDER BY clause.|This rule checks for usages of TOP in queries without an ORDER BY clause.

#### Notes

It is generally recommended to specify sort criteria when using TOP clause. Otherwise, the results produced will be plan dependent and may lead to undesired behavior.

### SRD0015

Microsoft Rule:

Ignorable: No

#### Description

Always use a column list in INSERT statements.|When inserting into a table or view

#### Notes

 it is recommended that the target column list be explicitly specified. This results in more maintainable code and helps in avoiding problems when the table structure changes (like adding or dropping a column).

### SRD0016

Microsoft Rule:

Ignorable: Yes

#### Description

Input parameter never used.|This rule checks for not used stored procedure or function input parameters.

#### Notes

Unused parameters not necessarily negatively affect the performance, but they just add bloat to your stored procedures and functions.

### SRD0017

Microsoft Rule:

Ignorable: Yes

#### Description

DELETE statement without row limiting conditions.

#### Notes

The rule looks for DELETE statements not having a WHERE clause. Consider reviewing your code to avoid unintentionally losing all the rows in the table.

### SRD0018

Microsoft Rule:

Ignorable: Yes

#### Description

UPDATE statement without row limiting conditions.

#### Notes

The rule looks for UPDATE statements not having a WHERE clause. Consider reviewing your code to avoid unintentionally updating all the records in the table.

### SRD0019

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid joining with views.|The rule checks for joining with views as this may have performance implication when used without having good knowledge of the underlying tables and may lead to unnecessary joins.

#### Notes

Although views are useful for many reasons, they hide the underlying sources and may mislead unacquainted developers and produce redundant joins.

### SRD0020

Microsoft Rule:

Ignorable: No

#### Description

The query has issues with the join clause. |It is either missing a backing foreign key or the join is missing one or more columns.

#### Notes

The rule checks the T-SQL code for queries having joined tables and missing join a predicate for one of the tables. It identifies the joined table sources which do not have any column referenced neither in the join conditions nor in the WHERE clause. Without a join predicate, the query result will include the Cartesian product of all rows.

### SRD0021

Microsoft Rule:

Ignorable: Yes

#### Description

Consider using EXISTS predicate instead of IN predicate.

#### Notes

The rule check T-SQL code for IN predicate using a sub-query as they can be replaced by EXISTS predicate. Using EXISTS predicate is often considered better than IN predicate, especially when NOT IN predicate is used.

### SRD0022

Microsoft Rule:

Ignorable: Yes

#### Description

Not going to Implement, Avoid converting dates to string during date comparison.

#### Notes

The rule checks T-SQL script for date comparison made with using conversion to string (the CONVERT function). Consider using DATEADD and DATEDIFF functions as the to string conversion can lead to incorrect results.

### SRD0024

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid executing dynamic code using EXECUTE statement.

#### Notes

Use of dynamic code with EXEC can fall prey to sql injection attacks, and also does not save the execution plan in the cache which causes a query recompile every time.

### SRD0025

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using column numbers in ORDER BY clause.

#### Notes

The rule checks for ORDER BY clauses which reference select list column using the column number instead of the column name. The column numbers in the ORDER BY clause as it impairs the readability of the SQL statement. Further, changing the order of columns in the SELECT list has no impact on the ORDER BY when the columns are referred by names instead of numbers.

### SRD0026

Microsoft Rule:

Ignorable: No

#### Description

Do not use VARCHAR or NVARCHAR data types without specifying length.

#### Notes

Without specifying the length SQL Server will either assign a default length or determine the length for you (if casting a variable).

### SRD0027

Microsoft Rule:

Ignorable: No

#### Description

Do not use DECIMAL or NUMERIC data types without specifying precision and scale.

#### Notes

The rule checks the T-SQL code for use DECIMAL or NUMERIC data types without specifying length. Avoid defining columns, variables and parameters using DECIMAL or NUMERIC data types without specifying precision, and scale. If no precision and scale are provided, SQL Server will use its own default values - (18, 0).

### SRD0028

Microsoft Rule:

Ignorable: Yes

#### Description

Consider prefixing column names with table name or table alias.

#### Notes

The rule checks SELECT,UPDATE and DELETE statements which use more than one table source and reference columns which are not prefixed with table name or table alias. Consider prefixing column names with table name or alias in order to improve readability and avoid ambiguity.

### SRD0030

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using Hints to force a particular behavior.|The rule checks for usage of query hints, table hints or join hints in the SELECT, UPDATE, DELETE, MERGE and INSERT statements.

#### Notes

Because the SQL Server query optimizer typically selects the best execution plan for a query, it is recommended to be use hints only as a last resort by experienced developers and database administrators.

### SRD0031

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using CHARINDEX function.

#### Notes

Removed.

### SRD0032

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid OR operator in queries.|The rule checks SELECT, UPDATE and DELETE statements for use of the OR operator in their filtering clauses.

#### Notes

Often, the OR operator can confuse SQL Server and prevent it from coming up with a good query plan. Check the Query Plan and look for undesirable behavior such as index scans or table spools. If a seek is performed, check to make sure that is it seeking on all of the intended columns, rather than performing a WHERE filter on columns that should otherwise be seekable. This is the pattern: (@param is null or column=@param) or slightly different: (@param=0 or column=@param)

### SRD0033

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using cursors.

#### Notes

The rule identifies CURSOR usage inside the code and notifies for cursor declarations. Review your code and consider using a set-based solution instead of the cursor/iterative solution for the given task.

### SRD0034

Microsoft Rule:

Ignorable: No

#### Description

Do not use the NOLOCK clause

#### Notes

- **Dirty read** - this is the one most people are aware of; you can read data that has not been committed, and could be rolled back some time after you've read it - meaning you've read data that never technically existed.
- Missing rows - because of the way an allocation scan works, other transactions could move data you haven't read yet to an earlier location in the chain that you've already read, or add a new page behind the scan, meaning you won't see it at all.
- Reading rows twice - bimilarly, data that you've already read could be moved to a later location in the chain, meaning you will read it twice.
- Reading multiple versions of the same row - when using READ UNCOMMITTED, you can get a version of a row that never existed; for example, where you see some columns that have been changed by concurrent users, but you don't see their changes reflected in all columns. This can even happen within a single column (see a great example from Paul White).
- Index corruption - surely you are not using NOLOCK in INSERT/UPDATE/DELETE statements, but if you are, you should be aware that this syntax is deprecated and that it can cause corruption, even in SQL Server 2014 RTM - see this tip for more information.Note that you should check for the hint in any views that you are trying to update, too.

### SRD0035

Microsoft Rule:

Ignorable: No

#### Description

Do not use WAITFOR DELAY/TIME statement in stored procedures, functions, and triggers.

#### Notes

The rule checks for WAITFOR statement with DELAY or TIME being used inside stored procedure, function or trigger. The WAITFOR statement blocks the execution of the batch, stored procedure, or transaction until a specified time or time interval is reached. This is not typically wanted in a OLTP system unless for a very specific reason.

### SRD0036

Microsoft Rule:

Ignorable: No

#### Description

Do not use SET ROWCOUNT to restrict the number of rows.

#### Notes

The rule checks for usage of the SET ROWCOUNT setting. It is recommended to use the TOP clause or the new in SQL 2012 FETCH keyword instead of SET ROWCOUNT as it will not be supported in the future versions of SQL Server for INSERT,UPDATE and DELETE statements. In addition to that is being phased out, the SET ROWCOUNT has another problem - when a ROWCOUNT is set and there is INSERT, UPDATE, DELETE or MERGE statements which fire a trigger, all the statements in the trigger will have the same row limit applied.

### SRD0037

Microsoft Rule:

Ignorable: No

#### Description

Not going to implement: Ensure variable assignment from SELECT with no rows.

#### Notes

A common mistake that's made is that a SELECT statement will always assign to the variables in the SELECT. This isn't the case if the SELECT statement doesn't return a row. In this case, safeguards need to be created.

### SRD0038

Microsoft Rule:

Ignorable: Yes

#### Description

Consider aliasing all table sources in the query.

#### Notes

### SRD0039

Microsoft Rule:

Ignorable: No

#### Description

Use fully qualified object names in SELECT, UPDATE, DELETE, MERGE and EXECUTE statements. [schema].[name]

#### Notes

There is a minor performance cost with not using two part names. Each time sql server runs across a one part name it has to look up the associated schema to the object.

### SRD0040

Microsoft Rule: SA0126

Ignorable: No

#### Description

Operator combines two different types will cause implicit conversion.

#### Notes

Removed.

### SRD0041

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid use of the SELECT INTO syntax.

#### Notes

The rule checks for SELECT INTO statement being used. Consider replacing the SELECT INTO statement with explicit table creation and then use the INSERT INTO statement.

### SRD0043

Microsoft Rule:

Ignorable: No

#### Description

The arguments of the function 'COALESCE' are not of the same datatype.

#### Notes

The rule checks and warns if COALESCE function arguments do not have same data type. Consider reviewing your code as unexpected results can occur if arguments with different datatypes are feed through a COALESCE.

### SRD0043

Microsoft Rule:

Ignorable: No

#### Description

The arguments of the function 'ISNULL' are not of the same datatype.

#### Notes

The rule checks and warns if ISNULL function arguments do not have same data type. Consider the possible truncation which may result when the second parameter of the function is implicitly converted to the type of the first parameter.

### SRD0044

Microsoft Rule:

Ignorable: No

#### Description

The RAISERROR statement with severity above 18 and requires WITH LOG clause.

#### Notes

The rule checks RAISERROR statements for having severity above 18 and not having a WITH LOG clause. Error severity levels greater than 18 can only be specified by members of the sysadmin role, using the WITH LOG option. Severity levels from 0 through 18 can be specified by any user. Severity levels from 19 through 25 can only be specified by members of the sysadmin fixed server role or users with ALTER TRACE permissions. For severity levels from 19 through 25, the WITH LOG option is required.

### SRD0045

Microsoft Rule:

Ignorable: No

#### Description

Excessive number of indexes on table found

#### Notes

Having too many indexes on a table can cause performance issues with action queries as each time an action query is run all associated indexes need to be updated as well.

### SRD0046

Microsoft Rule:

Ignorable: No

#### Description

Do not use the real or float data types as they are approximate value data types.

#### Notes

REAL and FLOAT do not store accurate values. They store **Approximate** values.

### SRD0047

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid using columns that match other columns by name, but are different in type or size

#### Notes

Columns are found in multiple tables that match by name but differ by either type or size. If the columns truly have different meanings, they should differ by name as well or they should match in datatype and size.

### SRD0048

Microsoft Rule:

Ignorable: No

#### Description

Not going to implement: Deprecated string aliasing

#### Notes

This rule checks T-SQL script for use of column aliasing where the name of the expression uses a string value. The syntax is deprecated and it is recommended to use quoted identifiers instead.

### SRD0049

Microsoft Rule: SRD0013

Ignorable: No

#### Description

Wrap multiple action statements within a try catch

#### Notes

The rule checks for SELECT INTO,INSERT,DELETE and UPDATE statements which are not inside a TRY..CATCH block. This check is important, because, by default, SQL Server will not rollback all the previous changes within a transaction if a particular statement fails and setting XACT_ABORT is not ON.

### SRD0050

Microsoft Rule:

Ignorable: Yes

#### Description

The comparison expression always evaluates to TRUE or FALSE

#### Notes

Checks join and where clause predicates for predicates that will always evaluate to true/false.

### SRD0051

Microsoft Rule:

Ignorable: No

#### Description

Don't use deprecated TEXT,NTEXT and IMAGE data types.

#### Notes

The data types ntext, text, and image will be removed in a future version of Microsoft SQL Server. Avoid using these data types in new development work, and plan to modify applications that currently use them. Use nvarchar(max), varchar(max), and varbinary(max) instead.

### SRD0052

Microsoft Rule:

Ignorable: No

#### Description

Index has exact duplicate or overlapping index

#### Notes

The rule matches exact duplicating or partially duplicating indexes. The exact duplicating indexes must have the same key columns in the same order, and the same included columns but in any order. These indexes are sure targets for elimination. The overlapping indexes share the same leading key columns, but the included columns are ignored. These types of indexes are probable dead indexes walking.

### SRD0053

Microsoft Rule:

Ignorable: Yes

#### Description

Object has different collation than the rest of the database.

#### Notes

### SRD0054

Microsoft Rule:

Ignorable: Yes

#### Description

Not going to implement: Statement is not terminated with semicolon

#### Notes

The rule checks for statements not terminated with semicolon. Consider terminating all statements with semicolon to improve readability and compatibility with future versions of SQL Server. As more and more keywords, and statements are added to Transact-SQL, there will appear more cases where the semicolon terminator will be required.

### SRD0055

Microsoft Rule:

Ignorable: No

#### Description

The SQL module was created with ANSI_NULLS and/or QUOTED_IDENTIFIER options set to OFF

#### Notes

The rule checks existing SQL modules which have ANSI_NULLS and/or QUOTED_IDENTIFIER settings saved with value OFF. Consider reviewing the need for these options settings, and in case they are not required, you should recreate the SQL module using a session that has both these options set to ON. Even these settings may not currently relate performance problems, they may prevent further performance optimizations, such as filtered indexes, indexes on computed columns or indexed views.

### SRD0056

Microsoft Rule:

Ignorable: Yes

#### Description

Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY

#### Notes

The rule checks the code for using any of the @@IDENTITY function. When the queries use parallel execution plans, the identity functions may return incorrect results.

### SRD0057

Microsoft Rule:

Ignorable: No

#### Description

Do not mix DML with DDL statements. Group DDL statements at the beginning of procedures followed by DML statements

#### Notes

The rule checks stored procedures, triggers and functions for having a DDL statements mixed between DML statements. If DDL operations are performed within a procedure or batch, the procedure or batch is recompiled when it encounters the first subsequent DML operation affecting the table involved in the DDL.

### SRD0058

Microsoft Rule:

Ignorable: No

#### Description

Always use parameter names when calling stored procedures

#### Notes

The rule checks EXECUTE statements for stored procedure not being called with named parameters, but with parameters by position.

### SRD0059

Microsoft Rule:

Ignorable: No

#### Description

Only checkable in the DB: Column created with option ANSI_PADDING set to OFF

#### Notes

ANSI_PADDING set to OFF can cause unintended data alterations that can leave the data in an unusable state. When padded, char columns are padded with blanks, and binary columns are padded with zeros. When trimmed, char columns have the trailing blanks trimmed, and binary columns have the trailing zeros trimmed.

### SRD0060

Microsoft Rule:

Ignorable: No

#### Description

The procedure grants itself permissions. Possible missing GO command

#### Notes

The rule checks for stored procedures, changing its own permissions. It is possible that a GO end of batch signaling command is missing and the statements in the script following the procedure are included in the procedure body.

### SRD0061

Microsoft Rule:

Ignorable: No

#### Description

The database is configured with invalid options.

#### Notes

The database is configured with invalid options. Many of these options can have an adverse affect as well as affect performance.

### SRD0062

Microsoft Rule:

Ignorable: No

#### Description

Temp table not configured with COLLATION options

#### Notes

Create SQL Server temporary tables with the correct collation or use database default as the tempdb having a different collation than the database can cause issues and or data instability.

### SRD0063

Microsoft Rule:

Ignorable: No

#### Description

Avoid the use of IF statements in stored procedures.

#### Notes

IF statements that are wrapped around queries can change the execution plan and can cause issues with a plan being cached that is not optimal for all code branches.

### SRD0064

Microsoft Rule:

Ignorable: Yes

#### Description

Cache multiple calls to GETDATE or SYSDATETIME into a variable

#### Notes

The value of GETDATE or SYSDATETIME can alter during the course of executing a series of queries providing unexpected results.

## Naming

### SRN0001

Microsoft Rule:

Ignorable: Yes

#### Description

Avoid 'fn_' prefix when naming functions.

#### Notes

This rule checks for user defined scalar functions with `fn_`. Though this practice is supported, it is recommended that the prefixes not be used to avoid name clashes with Microsoft shipped objects.

### SRN0002

Microsoft Rule: SR0016

Ignorable: Yes

#### Description

Avoid `sp_` prefix when naming stored procedures.

#### Notes

This rule checks for creation of stored procedure with names starting with `sp_`.The prefix `sp_` is reserved for system stored procedure that ship with SQL Server. Whenever SQL Server encounters a procedure name starting with sp_, it first tries to locate the procedure in the master database, then it looks for any qualifiers (database, owner) provided, then it tries dbo as the owner. So you can really save time in locating the stored procedure by avoiding the `sp_` prefix.

### SRN0003

Microsoft Rule: SR0011

Ignorable: No

#### Description

Avoid using special characters in object names.

#### Notes

The rule checks the current context for database objects with special characters in the name. Naming a database object by using any special character (space, tab, newline, left square bracket, right square bracket, single quotation mark and double quotation mark) will make it more difficult not only to reference that object, but also to read code that contains the name of that object.

### SRN0004

Microsoft Rule: SR0012

Ignorable: No

#### Description

Avoid using reserved words for type names.

#### Notes

The rule checks the current context for user defined types using a reserved word as type name. You should avoid using a reserved word as the name of a user-defined type because readers will have a harder time understanding your database code. You can use reserved words in SQL Server as identifiers and object names only if you use delimited identifiers.

### SRN0005

Microsoft Rule:       |SRN0007

Ignorable: No

#### Description

Avoid constraints created with system generated name.

#### Notes

The rule checks all the constraints in the current database for having a system generated name.

### SRN0006

Microsoft Rule:

Ignorable: No

#### Description

Using two part naming on objects [Schema].[Name] is recommended

#### Notes

Without specifying the schema in the CREATE script will cause SQL Server to try to assign the correct schema which will default to the current users default schema and may or may not be dbo.

### SRN0007

Microsoft Rule:

Ignorable: No

#### Description

General naming violation.

#### Notes

Multiple possible rule violations:

- Name '{name}' starts with a number.
- Name '{name}' contains invalid characters. Please only use alphanumerics and underscores.
- Primary Key '{name}' does not follow the company naming standard. Please use the name PK_{tableName}.
- Index '{name}' does not follow the company naming standard. Please use the name IX##_{tableName}.
- Foreign Key '{name}' does not follow the company naming standard. Please use the format FK##_{tableName}.
- Check Constraint '{name}' does not follow the company naming standard. Please use the format CK_*.
- Constraint '{name}' does not follow the company naming standard. Please use the name DF_{tableName}_{columnName}.
