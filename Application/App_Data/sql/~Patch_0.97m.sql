/* Patch MasterDB up to version 0.98 (110314):
 * add "GUID", "ReplacedID" columns to "Projects" table;
 + add index for Projects.GUID. */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.97') BEGIN

 ALTER TABLE [dbo].[Projects] ADD [GUID] [char] (36) NULL
 ALTER TABLE [dbo].[Projects] ADD [ReplacedID] [int] NULL

 CREATE  INDEX [IX_ProjectGUID] ON [dbo].[Projects]([GUID]) ON [PRIMARY]
 
 UPDATE [Extra] SET PropertyValue='0.98' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
