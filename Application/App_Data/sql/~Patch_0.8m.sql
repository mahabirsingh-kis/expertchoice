/* Patch MasterDB up to version 0.81 (081222):
 + Add column 'ExpirationDate' to 'UserWorkgroups' table; 
 + Add column 'Step' to 'Workspace' table; 
 + Add column 'MeetingID' to 'Projects' table; */

IF EXISTS (SELECT * FROM ExtraInfo WHERE ObjectProperty='DatabaseVersion' AND ObjectValue LIKE '0.8') BEGIN

 ALTER TABLE [dbo].[UserWorkgroups] ADD [ExpirationDate] [datetime] NULL
 ALTER TABLE [dbo].[Workspace] ADD [Step] [int] NOT NULL DEFAULT ((0))
 ALTER TABLE [dbo].[Projects] ADD [MeetingID] [varchar](16) NULL

END


GO


IF EXISTS (SELECT * FROM ExtraInfo WHERE ObjectProperty='DatabaseVersion' AND ObjectValue LIKE '0.8') BEGIN

 UPDATE Workspace SET Step = ObjectValue FROM ExtraInfo WHERE ExtraType = 54 AND ObjectProperty = 'Step' AND ObjectID = Workspace.ID
 UPDATE Projects SET MeetingID = ObjectValue FROM ExtraInfo WHERE ExtraType = 20 AND ObjectProperty = 'SynchronousMeetingID' AND ObjectID = Projects.ID

 UPDATE ExtraInfo SET ObjectValue = '0.81' WHERE ObjectProperty='DatabaseVersion'

END