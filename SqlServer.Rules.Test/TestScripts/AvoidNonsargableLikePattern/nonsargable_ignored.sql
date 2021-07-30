CREATE VIEW [dbo].[nonsargable_view_ignored] AS
	-- IGNORE SRP0002
	SELECT table1_id FROM [dbo].[table1] WHERE [data] LIKE '%what am I doing?'

GO