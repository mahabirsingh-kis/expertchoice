/* Patch MasterDB up to version 0.9992 (140212)
 ! Risk version
 + Add column 'Status2' to 'Projects' table 
 + Add columns 'Status2', 'TTStatus2', 'Step2' to 'Workspace' table */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9991' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[Projects] ADD [Status2] [int] NOT NULL DEFAULT (0)

 ALTER TABLE [dbo].[Workspace] ADD [Status2] [int] NULL DEFAULT (0)
 ALTER TABLE [dbo].[Workspace] ADD [TTStatus2] [int] NULL DEFAULT (0)
 ALTER TABLE [dbo].[Workspace] ADD [Step2] [int] NULL DEFAULT (-1)

 ALTER TABLE [dbo].[UserTemplates] ADD [XMLName] [nvarchar] (60) NULL

 CREATE  INDEX [IX_ProjectStatus2] ON [dbo].[Projects]([Status2]) ON [PRIMARY]
 CREATE  INDEX [IX_WSStatus2] ON [dbo].[Workspace]([Status2]) ON [PRIMARY]

END

 

 
GO


 
 
IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9991' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 UPDATE Projects SET Status2=Status
 UPDATE Workspace SET Status2=Status, TTStatus2=TTStatus, Step2=-1

 UPDATE [Extra] SET PropertyValue='0.9992' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END