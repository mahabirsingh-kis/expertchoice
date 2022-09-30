/* Patch MasterDB up to version 0.9998 (151211)
 + Add column 'OpportunityID' to table 'Workgroups'; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9997' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE [dbo].[Workgroups] ADD [OpportunityID] [nvarchar] (250) NULL

   UPDATE [Extra] SET PropertyValue='0.9998' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
