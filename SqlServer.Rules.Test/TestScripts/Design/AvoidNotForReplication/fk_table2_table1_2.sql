﻿ALTER TABLE dbo.table2 ADD
CONSTRAINT [fk_table2_table1_2]
FOREIGN KEY ([table1_id]) REFERENCES [dbo].[table1]([table1_id]) 

GO