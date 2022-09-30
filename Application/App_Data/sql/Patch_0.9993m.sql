/* Patch MasterDB up to version 0.9994 (140210)
 + Add "StreamHash' column to 'UserTemplates' table */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9993' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[UserTemplates] DROP COLUMN [XMLName]
 ALTER TABLE [dbo].[UserTemplates] ADD [StreamHash] [nvarchar] (32) NULL
 ALTER TABLE [dbo].[UserTemplates] ADD [IsCustom] [bit] NOT NULL DEFAULT 0

 UPDATE [Extra] SET PropertyValue='0.9994' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
