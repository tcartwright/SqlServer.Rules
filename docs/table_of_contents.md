[This document is automatically generated. All changed made to it WILL be lost]: <>  
  
# Table of Contents  
  
  
## Design  
  
| Rule Id | Friendly Name | Ignorable | Description |
|----|----|----|----|
| [SRD0001](Design/SRD0001.md) | Missing natural key | false | Table does not have a natural key. |
| [SRD0002](Design/SRD0002.md) | Missing primary key | false | Table does not have a primary key. |
| [SRD0003](Design/SRD0003.md) | Avoid wide primary keys | false | Primary Keys should avoid using GUIDS or wide VARCHAR columns. |
| [SRD0004](Design/SRD0004.md) | Index on Foreign Key | false | Columns on both sides of a foreign key should be indexed. |
| [SRD0005](Design/SRD0005.md) | Avoid long CHAR types | true | Avoid the (n)char column type except for short static length data. |
| [SRD0006](Design/SRD0006.md) | Avoid SELECT * | true | Avoid using SELECT *. |
| [SRD0009](Design/SRD0009.md) | Non-transactional body | false | Wrap multiple action statements within a transaction. |
| [SRD0010](Design/SRD0010.md) | Low identity seed value | false | Start identity column used in a primary key with a seed of 1000 or higher. |
| [SRD0011](Design/SRD0011.md) | Equality Compare With NULL Rule | false | Equality and inequality comparisons involving a NULL constant found. Use IS NULL or IS NOT NULL. |
| [SRD0012](Design/SRD0012.md) | Unused variable | false | Variable declared but never referenced or assigned. |
| [SRD0013](Design/SRD0013.md) | Expected error handeling | false | Wrap multiple action statements within a try catch. |
| [SRD0014](Design/SRD0014.md) | TOP without an ORDER BY | false | TOP clause used in a query without an ORDER BY clause. |
| [SRD0015](Design/SRD0015.md) | Implicit column list | false | Always use a column list in INSERT statements. |
| [SRD0016](Design/SRD0016.md) | Unused input parameter | true | Input parameter never used. Consider removing the parameter or using it. |
| [SRD0017](Design/SRD0017.md) | Avoid Deletes Without Where Rule | true | DELETE statement without row limiting conditions. |
| [SRD0018](Design/SRD0018.md) | Unbounded UPDATE | true | UPDATE statement without row limiting conditions. |
| [SRD0019](Design/SRD0019.md) | Avoid joining tables with views | true | Avoid joining tables with views. |
| [SRD0020](Design/SRD0020.md) | Incomplete or missing JOIN predicate | false | The query has issues with the join clause. It is either missing a backing foreign key or the join is missing one or more columns. |
| [SRD0021](Design/SRD0021.md) | Consider EXISTS Instead Of In Rule | true | Consider using EXISTS instead of IN when used with a subquery. |
| [SRD0024](Design/SRD0024.md) | Avoid EXEC or EXECUTE | true | Avoid EXEC and EXECUTE with string literals. Use parameterized sp_executesql instead. |
| [SRD0025](Design/SRD0025.md) | Avoid ORDER BY with numbers | true | Avoid using column numbers in ORDER BY clause. |
| [SRD0026](Design/SRD0026.md) | Unspecified type length | false | Do not use these data types (VARCHAR, NVARCHAR, CHAR, NCHAR) without specifying length. |
| [SRD0027](Design/SRD0027.md) | Unspecified precision or scale | false | Do not use DECIMAL or NUMERIC data types without specifying precision and scale. |
| [SRD0028](Design/SRD0028.md) | Consider Column Prefix Rule | true | Consider prefixing column names with table name or table alias. |
| [SRD0030](Design/SRD0030.md) | Avoid Use of HINTS | true | Avoid using Hints to force a particular behavior. |
| [SRD0031](Design/SRD0031.md) | Avoid using CHARINDEX | true | Avoid using CHARINDEX function in WHERE clauses. |
| [SRD0032](Design/SRD0032.md) | Avoid use of OR in where clause | true | Try to avoid the OR operator in query where clauses if possible.  (Sargable) |
| [SRD0033](Design/SRD0033.md) | Avoid Cursors | true | Avoid using cursors. |
| [SRD0034](Design/SRD0034.md) | Use of NOLOCK | false | Do not use the NOLOCK clause. |
| [SRD0035](Design/SRD0035.md) | Forced delay | false | Do not use WAITFOR DELAY/TIME statement in stored procedures, functions, and triggers. |
| [SRD0036](Design/SRD0036.md) | Do not use SET ROWCOUNT | true | Do not use SET ROWCOUNT to restrict the number of rows. |
| [SRD0038](Design/SRD0038.md) | Alias Tables Rule | true | Consider aliasing all table sources in the query. |
| [SRD0039](Design/SRD0039.md) | Object not schema qualified | false | Use fully qualified object names in SELECT, UPDATE, DELETE, MERGE and EXECUTE statements. [schema].[name]. |
| [SRD0041](Design/SRD0041.md) | Avoid SELECT INTO temp or table variables | true | Avoid use of the SELECT INTO syntax. |
| [SRD0043](Design/SRD0043.md) | Possible side-effects implicit cast | false | The arguments of the function '{0}' are not of the same datatype. |
| [SRD0044](Design/SRD0044.md) | Error handling requires SA permissions | false | The RAISERROR statement with severity above 18 requires the WITH LOG clause. |
| [SRD0045](Design/SRD0045.md) | Excessive indexes on table | false | Excessive number of indexes on table found on table. |
| [SRD0046](Design/SRD0046.md) | Use of approximate data type | false | Do not use the real or float data types for parameters or columns as they are approximate value data types. |
| [SRD0047](Design/SRD0047.md) | Ambiguous column name across design | true | Avoid using columns that match other columns by name, but are different in type or size. |
| [SRD0050](Design/SRD0050.md) | Expression reducible to constaint | true | The comparison expression always evaluates to TRUE or FALSE. |
| [SRD0051](Design/SRD0051.md) | Do Not Use Deprecated Types Rule | false | Don?t use deprecated TEXT, NTEXT and IMAGE data types. |
| [SRD0052](Design/SRD0052.md) | Duplicate/Overlapping Index | false | Index has exact duplicate or borderline overlapping index. |
| [SRD0053](Design/SRD0053.md) | Explicit collation other | true | Object has different collation than the rest of the database. Try to avoid using a different collation unless by design. |
| [SRD0055](Design/SRD0055.md) | Object level option override | false | The object was created with invalid options. |
| [SRD0056](Design/SRD0056.md) | Unsafe identity retrieval | true | Use OUTPUT or SCOPE_IDENTITY() instead of @@IDENTITY. |
| [SRD0057](Design/SRD0057.md) | Do Not Mix DML With DDL Rule | false | Do not mix DML with DDL statements. Group DDL statements at the beginning of procedures followed by DML statements. |
| [SRD0058](Design/SRD0058.md) | Ordinal parameters used | false | Always use parameter names when calling stored procedures. |
| [SRD0060](Design/SRD0060.md) | Permission change in stored procedure | false | The procedure grants itself permissions. Possible missing GO command. |
| [SRD0061](Design/SRD0061.md) | Invalid database configured options | false | The database is configured with invalid options. |
| [SRD0062](Design/SRD0062.md) | Implicit collation | false | Create SQL Server temporary tables with the correct collation or use database default as the tempdb having a different collation than the database can cause issues and or data instability. |
| [SRD0063](Design/SRD0063.md) | Avoid wrapping SQL in IF statement | true | Do not use IF statements containing queries in stored procedures. |
| [SRD0064](Design/SRD0064.md) | Consider Caching Get Date To Variable | true | Cache multiple calls to GETDATE or SYSDATETIME into a variable. |
| [SRD0065](Design/SRD0065.md) | Avoid NOT FOR REPLICATION | false | Avoid 'NOT FOR REPLICATION' unless this is the desired behavior and replication is in use. |
  
## Performance  
  
| Rule Id | Friendly Name | Ignorable | Description |
|----|----|----|----|
| [SRP0001](Performance/SRP0001.md) | Nested Views | false | Views should not use other views as a data source. |
| [SRP0002](Performance/SRP0002.md) | Unanchored string pattern | true | Try to avoid using patterns that start with '%' when using the LIKE keyword if possible.  (Sargable) |
| [SRP0003](Performance/SRP0003.md) | Aggregate of unique set | true | Avoid using DISTINCT keyword inside of aggregate functions. |
| [SRP0004](Performance/SRP0004.md) | Noisy trigger | true | Avoid returning results in triggers. |
| [SRP0005](Performance/SRP0005.md) | Noisy trigger | true | SET NOCOUNT ON is recommended to be enabled in stored procedures and triggers. |
| [SRP0006](Performance/SRP0006.md) | Use of inequality | true | Try to avoid using not equal operator (<>,!=) in the WHERE clause if possible. (Sargable) |
| [SRP0007](Performance/SRP0007.md) | Dangling cursor | false | Local cursor not closed. |
| [SRP0008](Performance/SRP0008.md) | Unfreed cursor | false | Local cursor not explicitly deallocated. |
| [SRP0009](Performance/SRP0009.md) | Filtering on calculated value | true | Avoid wrapping columns within a function in the WHERE clause. (Sargable) |
| [SRP0010](Performance/SRP0010.md) | Function in data modification | true | Avoid the use of user defined functions with UPDATE/INSERT/DELETE statements. (Halloween Protection) |
| [SRP0011](Performance/SRP0011.md) | Non-member test in predicate | true | Avoid using the NOT IN predicate in a WHERE clause. (Sargable) |
| [SRP0012](Performance/SRP0012.md) | Un-indexed membership test | true | Consider indexing the columns referenced by IN predicates in order to avoid table scans. |
| [SRP0013](Performance/SRP0013.md) | Existence tested with JOIN | true | Consider replacing the OUTER JOIN with EXISTS. |
| [SRP0014](Performance/SRP0014.md) | Table variable in JOIN | true | Avoid the use of table variables in join clauses. |
| [SRP0015](Performance/SRP0015.md) | Avoid Column Calculations | true | Avoid the use of calculations on columns in the where clause. (Sargable) |
| [SRP0016](Performance/SRP0016.md) | Equality test with mismatched types | false | Data types on both sides of an equality check should be the same in the where clause. (Sargable) |
| [SRP0017](Performance/SRP0017.md) | Update of Primary key | true | Avoid updating columns that are part of the primary key.  (Halloween Protection) |
| [SRP0018](Performance/SRP0018.md) | High join count | false | Query uses a high number of joins.  |
| [SRP0020](Performance/SRP0020.md) | Missing Clustered index | false | Table does not have a clustered index. |
| [SRP0021](Performance/SRP0021.md) | Manipulated parameter value | true | Avoid modification of parameters in a stored procedure prior to use in a select query. |
| [SRP0022](Performance/SRP0022.md) | Procedure level recompile option | true | Consider using RECOMPILE query hint instead of the WITH RECOMPILE option. |
| [SRP0023](Performance/SRP0023.md) | Enumerating for existence check | true | When checking for existence use EXISTS instead of COUNT |
| [SRP0024](Performance/SRP0024.md) | Correlated subquery | true | Avoid the use of correlated subqueries except for very small tables. |
  
## Naming  
  
| Rule Id | Friendly Name | Ignorable | Description |
|----|----|----|----|
| [SRN0001](Naming/SRN0001.md) | UDF with System prefix | true | Avoid 'fn_' prefix when naming functions. |
| [SRN0002](Naming/SRN0002.md) | Procedure name may conflict system name | true | Avoid 'sp_' prefix when naming stored procedures. |
| [SRN0006](Naming/SRN0006.md) | Use of default schema | false | Two part naming on objects is required. |
| [SRN0007](Naming/SRN0007.md) | Name standard | false | General naming rules. |

