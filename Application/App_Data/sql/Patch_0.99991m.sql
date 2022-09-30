/* Patch MasterDB up to version 0.99992 (160815)
 + Add columns to 'Snapshots' table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.99991' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE [dbo].[Snapshots] ADD [RestoredFrom] [int] NULL DEFAULT(0)
   ALTER TABLE [dbo].[Snapshots] ADD [SnapshotIdx] [int] NULL DEFAULT(0)
   ALTER TABLE [dbo].[Snapshots] ADD [Details] [nvarchar] (255) NULL

   UPDATE [Extra] SET PropertyValue='0.99992' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
