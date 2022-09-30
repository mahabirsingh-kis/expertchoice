/* Patch MasterDB up to version 0.9993 (140210)
 + Replace 'image' for 'varbinary(max)' for few tables */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9992' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[UserTemplates] ALTER COLUMN [Stream] VARBINARY (MAX) NULL;
 ALTER TABLE [dbo].[Workgroups] ALTER COLUMN [LicenseData] VARBINARY (MAX) NULL;

 UPDATE [Extra] SET PropertyValue='0.9993' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
