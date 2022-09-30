/* Patch MasterDB up to version 0.9991 (130213)
 ! Risk version
 + Add columns 'Passcode2', 'Meeting2' and their indexes to 'Projects' table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.999' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[Projects] ADD [Passcode2] [nvarchar] (64) NULL
 ALTER TABLE [dbo].[Projects] ADD [MeetingID2] [nvarchar] (16) NULL

 CREATE  INDEX [IX_Passcode2] ON [dbo].[Projects]([Passcode2]) ON [PRIMARY]
 CREATE  INDEX [IX_MeetingID2] ON [dbo].[Projects]([MeetingID2]) ON [PRIMARY]

 UPDATE [Extra] SET PropertyValue='0.9991' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END