/* Patch StreamProjectsDB up to version 1.0.4 (140210)
 + Replace 'image' for 'varbinary(max)' for few tables */


IF EXISTS (SELECT * FROM [Properties] WHERE PropertyName='DatabaseVersion' AND PropertyValue='1.0.3') BEGIN

 ALTER TABLE [dbo].[ModelStructure] ALTER COLUMN [Stream] VARBINARY (MAX) NOT NULL;
 ALTER TABLE [dbo].[SurveyStructure] ALTER COLUMN [Stream] VARBINARY (MAX) NOT NULL;
 ALTER TABLE [dbo].[UserData] ALTER COLUMN [Stream] VARBINARY (MAX) NOT NULL;

 UPDATE [Properties] SET PropertyValue='1.0.4' WHERE PropertyName='DatabaseVersion'

END
