/* Patch MasterDB up to version 0.991 (110319):
 + Add ProjectID, UserID, Created columns to PrivateURLs table. */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.99') BEGIN

 ALTER TABLE [dbo].[PrivateURLs] ADD [ProjectID] [int] NULL
 ALTER TABLE [dbo].[PrivateURLs] ADD [UserID] [int] NULL
 ALTER TABLE [dbo].[PrivateURLs] ADD [Created] [datetime] NULL
 
 CREATE  INDEX [IX_PrivateURL_PID] ON [dbo].[PrivateURLs]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_PrivateURL_UID] ON [dbo].[PrivateURLs]([UserID]) ON [PRIMARY]
 
 UPDATE [Extra] SET PropertyValue='0.991' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
