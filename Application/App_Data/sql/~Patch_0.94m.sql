/* Patch MasterDB up to version 0.95 (100322):
 + add column "TTStatus" to tables "Workspace" with default value -2 (Unspecified).  */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.94') BEGIN

 ALTER TABLE [dbo].[Workspace] ADD [TTStatus] [int] NOT NULL DEFAULT ((-2))

 UPDATE [Extra] SET PropertyValue='0.95' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
