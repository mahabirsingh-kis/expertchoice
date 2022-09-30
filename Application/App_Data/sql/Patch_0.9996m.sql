/* Patch MasterDB up to version 0.9997 (140628)
 + Add column 'WordingTemplates' to table 'Workgroups'; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9996' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE [dbo].[Workgroups] ADD [WordingTemplates] [VARBINARY] (MAX) NULL

   UPDATE [Extra] SET PropertyValue='0.9997' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
