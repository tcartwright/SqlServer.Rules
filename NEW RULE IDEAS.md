# NEW RULE IDEAS

- FKS: check for nocheck, not for replication
- Constraints: check for:
  - udf usage
  - nocheck
- ~~@@IDENTITY check~~ DONE IN MS RULE SR0008
- Suggest SCHEMABINDING for functions that do not touch tables
- Detect sql injection possibilities???
  - Would like to only detect injection opportunies where sql is being concatenated in from a variable.
  - Would also like to build a scanner for .Net code
