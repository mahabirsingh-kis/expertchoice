/* Patch MasterDB up to version 0.92 (090520):
 + recreate Logs Table;
 - drop ExtraInfo table;
 + create Extra table;
 * modify Projects table;
 * modify RoleGroups table;
 * modify UserWorkgroups table;
 * modify Users table;
 * modify Workgroups table;
 * modify Workspace table; */

IF NOT OBJECT_ID('dbo.ExtraInfo') IS NULL BEGIN
IF EXISTS (SELECT * FROM ExtraInfo WHERE ObjectProperty='DatabaseVersion' AND ObjectValue LIKE '0.81') BEGIN

  DROP TABLE [dbo].[Logs]

  ALTER TABLE [dbo].[Projects] DROP COLUMN [DatabaseName]
  ALTER TABLE [dbo].[Projects] ADD [MeetingOwnerID] [int] NULL
  ALTER TABLE [dbo].[Projects] ADD [LockStatus] [int] NULL
  ALTER TABLE [dbo].[Projects] ADD [LockedByUserID] [int] NULL
  ALTER TABLE [dbo].[Projects] ADD [LockExpiration] [datetime] NULL
  ALTER TABLE [dbo].[Projects] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[Projects] ADD [LastModify] [datetime] NULL
  ALTER TABLE [dbo].[Projects] ADD [LastVisited] [datetime] NULL 

  IF EXISTS (SELECT * FROM sysindexes WHERE name='IX_PGID') DROP INDEX [dbo].[RoleGroups].[IX_PGID]
  ALTER TABLE [dbo].[RoleGroups] DROP COLUMN [ParentID]
  ALTER TABLE [dbo].[RoleGroups] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[RoleGroups] ADD [LastModify] [datetime] NULL

  ALTER TABLE [dbo].[UserWorkgroups] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[UserWorkgroups] ADD [LastVisited] [datetime] NULL
  ALTER TABLE [dbo].[UserWorkgroups] ADD [LastProjectID] [int] NULL 

  ALTER TABLE [dbo].[Users] DROP COLUMN [Phone]
  ALTER TABLE [dbo].[Users] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[Users] ADD [LastModify] [datetime] NULL
  ALTER TABLE [dbo].[Users] ADD [LastVisited] [datetime] NULL
  ALTER TABLE [dbo].[Users] ADD [LastPageID] [int] NULL
  ALTER TABLE [dbo].[Users] ADD [LastURL] [nvarchar] (500) NULL
  ALTER TABLE [dbo].[Users] ADD [LastWorkgroupID] [int] NULL
  ALTER TABLE [dbo].[Users] ADD [isOnline] [bit] NULL
  ALTER TABLE [dbo].[Users] ADD [EULAversion] [nvarchar] (10) NULL
  ALTER TABLE [dbo].[Users] ADD [SessionID] [nvarchar] (80) NULL

  ALTER TABLE [dbo].[Workgroups] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[Workgroups] ADD [LastModify] [datetime] NULL
  ALTER TABLE [dbo].[Workgroups] ADD [LastVisited] [datetime] NULL 

  ALTER TABLE [dbo].[Workspace] ADD [Created] [datetime] NULL
  ALTER TABLE [dbo].[Workspace] ADD [LastModify] [datetime] NULL 

END
END



GO



IF NOT OBJECT_ID('dbo.ExtraInfo') IS NULL BEGIN
IF EXISTS (SELECT * FROM ExtraInfo WHERE ObjectProperty='DatabaseVersion' AND ObjectValue LIKE '0.81') BEGIN

  CREATE TABLE [dbo].[Extra] (
  	[ID] [int] IDENTITY (1, 1) NOT NULL ,
  	[TypeID] [int] NOT NULL ,
  	[ObjectID] [int] NOT NULL ,
  	[PropertyID] [int] NOT NULL ,
  	[PropertyValue] [nvarchar] (2000) NULL 
  ) ON [PRIMARY]

  CREATE TABLE [dbo].[Logs] (
  	[ID] [bigint] IDENTITY (1, 1) NOT NULL ,
  	[DT] [datetime] NOT NULL ,
  	[ActionID] [char] (10) NOT NULL ,
  	[UserID] [int] NULL ,
  	[WorkgroupID] [int] NOT NULL ,
  	[TypeID] [int] NULL ,
  	[ObjectID] [int] NULL ,
  	[Comment] [nvarchar] (1000) NULL ,
  	[Result] [nvarchar] (1000) NULL 
  ) ON [PRIMARY]

END
END



GO



IF NOT OBJECT_ID('dbo.ExtraInfo') IS NULL BEGIN 
IF EXISTS (SELECT * FROM ExtraInfo WHERE ObjectProperty='DatabaseVersion' AND ObjectValue LIKE '0.81') BEGIN

 ALTER TABLE [dbo].[Extra] WITH NOCHECK ADD 
  	CONSTRAINT [PK_ExtraID] PRIMARY KEY  CLUSTERED 
  	(
  		[ID]
  	)  ON [PRIMARY] 


 ALTER TABLE [dbo].[Logs] WITH NOCHECK ADD 
  	CONSTRAINT [PK_LogsID] PRIMARY KEY  CLUSTERED 
  	(
  		[ID]
  	)  ON [PRIMARY] 


 ALTER TABLE [dbo].[Extra] ADD 
  	CONSTRAINT [IX_ExtraSearch] UNIQUE  NONCLUSTERED 
  	(
  		[TypeID],
  		[ObjectID],
  		[PropertyID]
  	)  ON [PRIMARY] 

 CREATE  INDEX [IX_LogsDT] ON [dbo].[Logs]([DT]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsAID] ON [dbo].[Logs]([ActionID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsUID] ON [dbo].[Logs]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsWID] ON [dbo].[Logs]([WorkgroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsTID] ON [dbo].[Logs]([TypeID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsOID] ON [dbo].[Logs]([ObjectID]) ON [PRIMARY]

 CREATE  INDEX [IX_ProjectName] ON [dbo].[Projects]([ProjectName]) ON [PRIMARY]

 CREATE  INDEX [IX_WorkgroupsName] ON [dbo].[Workgroups]([Name]) ON [PRIMARY]

 DROP TABLE [dbo].[ExtraInfo]

 INSERT INTO [Extra] (TypeID, ObjectID, PropertyID, PropertyValue) VALUES (1,1,1,'0.92')

END
END
