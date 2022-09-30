/* Patch MasterDB up to version 0.992 (110407):
 + Add EULAVersion column to UserWorkgroups table;
 + Add EULAFile, LifetimeProjects columns to Workgroups table. */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.991') BEGIN

 ALTER TABLE [dbo].[Workgroups] ADD [EULAFile] [nvarchar] (200) NULL
 ALTER TABLE [dbo].[Workgroups] ADD [LifetimeProjects] [int]  NULL
 ALTER TABLE [dbo].[UserWorkgroups] ADD [EULAVersion] [nvarchar] (200)  NULL

 UPDATE [Extra] SET PropertyValue='0.992' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END


GO


IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.992') BEGIN

 UPDATE Workgroups SET LifetimeProjects = (SELECT MAX(ID) FROM Projects) FROM Workgroups, Projects WHERE LifetimeProjects IS NULL AND Workgroups.Status=128
 UPDATE Workgroups SET LifetimeProjects = (SELECT COUNT(ID) FROM Projects WHERE WorkgroupID = Workgroups.ID) FROM Workgroups, Projects WHERE LifetimeProjects IS NULL

END
