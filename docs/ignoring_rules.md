# Ignoring Rules

Certain rules can be ignored within the T-SQL of the object itself. This is useful for whenever you want a rule to continue to run, but you have valid reasons for ignoring it in certain instances. Not all rules are ignorable. Refer to the documentation to determine which ones are.

In those cases you can use:

- IGNORE {RuleId}: Ignores a single occurrence of a rule within the file. Must be placed on, or above the line that violated the rule.
- GLOBAL IGNORE {RuleId}: Ignores all occurrences of that rule within the file.

Ignores must be done in comment syntax using either of these formats:

- -- IGNORE {RuleId}
- /* IGNORE {RuleId} */ - This is the preferred syntax as it will not cause sql to be malformed if the line breaks are removed.

## Examples

For example, for us to ignore the rule [SRD0032](Design/SRD0032.md) (Avoid use of OR in where clause) we can this several ways. This is a very common rule to have to ignore.

To ignore just one of the rules:

```sql
CREATE PROCEDURE dbo.Example AS 
BEGIN
    SELECT * 
    FROM dbo.table1 
    WHERE id1 = 1 OR id2 = 2 /* IGNORE SRD0032 */ 


    SELECT * 
    FROM dbo.table2 
    WHERE id1 = 1 OR id2 = 2 -- this will still flag as a violation of SRD0032
END
```

Now you could just add ignores for each occasion of this rule violation, but instead you could use a global ignore to ignore all violations of that rule within the stored procedure.

```sql
CREATE PROCEDURE dbo.Example AS 
BEGIN
    /* GLOBAL IGNORE SRD0032 */
    SELECT * 
    FROM dbo.table1 
    WHERE id1 = 1 OR id2 = 2 


    SELECT * 
    FROM dbo.table2 
    WHERE id1 = 1 OR id2 = 2 
END
```

## Notes

If you find that you do not want a rule to ever run, you can simply uncheck it from your list of rules in your project properties instead of adding global ignores everywhere.
