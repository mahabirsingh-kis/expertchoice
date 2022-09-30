/* Create Canvas MasterDB v 0.99992 (160815) */


CREATE TABLE [dbo].[AuthTokens] (
	[MeetingID] [nvarchar] (50) NOT NULL ,
	[AuthToken] [int] NOT NULL ,
	[Master] [tinyint] NULL 
) ON [PRIMARY]


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


CREATE TABLE [dbo].[PrivateURLs] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[Hash] [nvarchar] (32) NOT NULL ,
	[URL] [nvarchar] (1024) NOT NULL ,
	[ProjectID] [int] NULL ,
	[UserID] [int] NULL ,
	[Created] [datetime] NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[Projects] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[GUID] [char] (36) NULL ,
	[WorkgroupID] [int] NOT NULL ,
	[Passcode] [nvarchar] (64) NOT NULL ,
	[Passcode2] [nvarchar] (64) NULL ,
	[ReplacedID] [int] NULL ,
	[FileName] [nvarchar] (200) NULL ,
	[ProjectName] [nvarchar] (250) NULL ,
	[Status] [int] NOT NULL ,
	[Status2] [int] NULL DEFAULT (0),
	[Comment] [nvarchar] (1000) NULL ,
	[OwnerID] [int] NOT NULL ,
	[MeetingID] [varchar] (16) NULL ,
	[MeetingID2] [varchar] (16) NULL ,
	[MeetingOwnerID] [int] NULL ,
	[LockStatus] [int] NULL ,
	[LockedByUserID] [int] NULL ,
	[LockExpiration] [datetime] NULL ,
	[ProjectType] [int] NULL DEFAULT(0),
	[Created] [datetime] NULL ,
	[LastModify] [datetime] NULL ,
	[LastVisited] [datetime] NULL 
) ON [PRIMARY]


CREATE TABLE [dbo].[RoleActions] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[RoleGroupID] [int] NOT NULL ,
	[ActionType] [int] NOT NULL ,
	[Status] [int] NOT NULL ,
	[Comment] [nvarchar] (1000) NULL 
) ON [PRIMARY]


CREATE TABLE [dbo].[RoleGroups] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[WorkgroupID] [int] NOT NULL ,
	[RoleLevel] [int] NOT NULL ,
	[GroupType] [int] NOT NULL ,
	[Status] [int] NOT NULL ,
	[Name] [nvarchar] (250) NOT NULL ,
	[Comment] [nvarchar] (1000) NULL ,
	[Created] [datetime] NULL ,
	[LastModify] [datetime] NULL 
) ON [PRIMARY]


 CREATE TABLE [dbo].[Signup] (
	[SignupID] [int] NOT NULL ,
	[RegistrationID] [nvarchar] (255) NOT NULL ,
	[FirstName] [nvarchar] (255) NULL ,
	[LastName] [nvarchar] (255) NULL ,
	[email] [nvarchar] (255) NOT NULL ,
	[organization] [nvarchar] (255) NULL ,
	[phone] [nvarchar] (255) NULL ,
	[registerdate] [datetime] NULL ,
	[EmailConfirmed] [smallint] NOT NULL ,
	[ip] [nvarchar] (255) NULL ,
	[host] [nvarchar] (255) NULL ,
	[Referral] [nvarchar] (255) NULL ,
	[Workgroup] [nvarchar] (255) NULL ,
   	[OptIn] [smallint] ,
	[HubspotUserToken] [nvarchar] (255) NULL 
 ) ON [PRIMARY]


 CREATE TABLE [dbo].[Snapshots](
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[DT] [datetime] NOT NULL,
	[ProjectID] [int] NOT NULL,
	[SnapshotType] [smallint] NOT NULL,
   	[RestoredFrom] [int] NULL DEFAULT(0) ,
   	[SnapshotIdx] [int] NULL DEFAULT(0) ,
	[Stream] [varbinary](max) NULL,
	[StreamMD5] [nvarchar](64) NULL,
	[Workspace] [nvarchar](max) NULL,
	[WorkspaceMD5] [nvarchar](64) NULL,
	[Comment] [nvarchar](255) NULL ,
   	[Details] [nvarchar] (255) NULL
  ) ON [PRIMARY]


CREATE TABLE [dbo].[StructureMeetings] (
	[MeetingID] [int] NOT NULL ,
	[Password] [nvarchar] (100) NULL ,
	[OwnerEmail] [nvarchar] (100) NULL ,
	[OwnerName] [nvarchar] (100) NULL ,
	[ProjectID] [int] NOT NULL ,
	[State] [tinyint] NULL 
) ON [PRIMARY]

CREATE TABLE [dbo].[StructureTokens] (
	[TokenID] [int] NOT NULL ,
	[MeetingID] [int] NOT NULL ,
	[EMail] [nvarchar] (100) NOT NULL ,
	[Username] [nvarchar] (100) NULL ,
	[ClientType] [tinyint] NULL 
) ON [PRIMARY]

CREATE TABLE [dbo].[TeamTimeData] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[DT] [datetime] NOT NULL ,
	[ProjectID] [int] NOT NULL ,
	[UserID] [int] NOT NULL ,
	[ObjectID] [smallint] NOT NULL ,
	[Data] [ntext] NOT NULL 
) ON [PRIMARY]

CREATE TABLE [dbo].[UserTemplates] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[UserID] [int] NOT NULL ,
	[TemplateName] [nvarchar] (200) NOT NULL ,
	[Comment] [nvarchar] (255) NOT NULL ,
	[StructureType] [int] NOT NULL ,
	[StreamSize] [int] NULL ,
	[Stream] [VARBINARY] (MAX) NULL ,
	[StreamHash] [nvarchar] (32) NULL ,
	[ModifyDate] [datetime] NULL ,
	[IsCustom] [bit] NOT NULL DEFAULT 0
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[UserWorkgroups] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[UserID] [int] NOT NULL ,
	[WorkgroupID] [int] NOT NULL ,
	[RoleGroupID] [int] NOT NULL ,
	[Status] [int] NOT NULL ,
	[Comment] [nvarchar] (1000) NULL ,
	[ExpirationDate] [datetime] NULL ,
	[EULAVersion] [nvarchar] (200) NULL ,
	[Created] [datetime] NULL ,
	[LastVisited] [datetime] NULL ,
	[LastProjectID] [int] NULL 
) ON [PRIMARY]


CREATE TABLE [dbo].[Users] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[Email] [nvarchar] (64) NOT NULL ,
	[Password] [nvarchar] (64) NULL ,
	[PasswordStatus] [int] NULL DEFAULT(0) ,
	[Status] [int] NULL ,
	[FullName] [nvarchar] (80) NULL ,
	[Comment] [nvarchar] (1000) NULL ,
	[DefaultWGID] [int] NULL ,
	[OwnerID] [int] NULL ,
	[Created] [datetime] NULL ,
	[LastModify] [datetime] NULL ,
	[LastVisited] [datetime] NULL ,
	[LastPageID] [int] NULL ,
	[LastURL] [nvarchar] (500) NULL ,
	[LastWorkgroupID] [int] NULL ,
	[isOnline] [bit] NULL ,
	[EULAversion] [nvarchar] (10) NULL ,
	[SessionID] [nvarchar] (80) NULL 
) ON [PRIMARY]


CREATE TABLE [dbo].[Workgroups] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[Name] [nvarchar] (250) NOT NULL ,
	[Status] [int] NOT NULL ,
	[Comment] [nvarchar] (1000) NULL ,
	[OwnerID] [int] NOT NULL ,
	[ECAMID] [int] NOT NULL ,
	[LicenseData] [VARBINARY] (MAX) NULL ,
	[LicenseKey] [nvarchar] (100) NULL ,
	[EULAFile] [nvarchar] (200) NULL ,
	[LifetimeProjects] [int] NULL ,
	[Created] [datetime] NULL ,
	[LastModify] [datetime] NULL ,
	[LastVisited] [datetime] NULL ,
	[OpportunityID] [nvarchar] (250) NULL ,
        [WordingTemplates] [VARBINARY] (MAX) NULL
) ON [PRIMARY]


CREATE TABLE WorkgroupParams (
	WorkgroupID int NOT NULL,
	ParameterID int NOT NULL,
	ParameterValue float NULL DEFAULT(-1)
)  ON [PRIMARY]


CREATE TABLE [dbo].[Workspace] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[UserID] [int] NOT NULL ,
	[ProjectID] [int] NOT NULL ,
	[GroupID] [int] NOT NULL ,
	[Status] [int] NOT NULL ,
	[Status2] [int] NULL DEFAULT (0),
	[TTStatus] [int] NULL DEFAULT (0),
	[TTStatus2] [int] NULL DEFAULT (0),
	[Comment] [nvarchar] (1000) NULL ,
	[Step] [int] NULL ,
	[Step2] [int] NULL DEFAULT (-1),
	[Created] [datetime] NULL ,
	[LastModify] [datetime] NULL 
) ON [PRIMARY]



GO



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


ALTER TABLE [dbo].[PrivateURLs] WITH NOCHECK ADD 
	CONSTRAINT [PK_PrivateURLs] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Projects] WITH NOCHECK ADD 
	CONSTRAINT [PK_Projects] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[RoleActions] WITH NOCHECK ADD 
	CONSTRAINT [PK_RoleActions] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Signup] WITH NOCHECK ADD 
	CONSTRAINT [PK_Signup] PRIMARY KEY  CLUSTERED 
	(
		[SignupID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Signup] ADD 
	CONSTRAINT [DF_Signup_EmailConf] DEFAULT (0) FOR [EmailConfirmed],
	CONSTRAINT [IX_Signup_RegID] UNIQUE  NONCLUSTERED 
	(
		[RegistrationID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Snapshots] WITH NOCHECK ADD 
	CONSTRAINT [PK_Snapshots] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[StructureMeetings] WITH NOCHECK ADD 
	CONSTRAINT [PK_StructureMeetings] PRIMARY KEY  CLUSTERED 
	(
		[MeetingID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[StructureTokens] WITH NOCHECK ADD 
	CONSTRAINT [PK_StructureTokens] PRIMARY KEY  CLUSTERED 
	(
		[TokenID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[TeamTimeData] WITH NOCHECK ADD 
	CONSTRAINT [PK_TeamTimeID] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[UserWorkgroups] WITH NOCHECK ADD 
	CONSTRAINT [PK_UW] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[UserWorkgroups] ADD 
	CONSTRAINT [IX_UID_WID] UNIQUE  NONCLUSTERED 
	(
		[UserID],
		[WorkgroupID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[Users] WITH NOCHECK ADD 
	CONSTRAINT [PK_Users] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[UserTemplates] WITH NOCHECK ADD 
	CONSTRAINT [PK_UserTemplates] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[Workgroups] WITH NOCHECK ADD 
	CONSTRAINT [PK_Workgroups] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Workspace] WITH NOCHECK ADD 
	CONSTRAINT [PK_Workspace] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


ALTER TABLE [dbo].[Workspace] ADD 
	CONSTRAINT [IX_UID_PID] UNIQUE  NONCLUSTERED 
	(
		[UserID],
		[ProjectID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[Extra] ADD 
	CONSTRAINT [IX_ExtraSearch] UNIQUE  NONCLUSTERED 
	(
		[TypeID],
		[ObjectID],
		[PropertyID]
	)  ON [PRIMARY] 




 CREATE  INDEX [IX_AuthTokensKey] ON [dbo].[AuthTokens]([MeetingID], [AuthToken]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsDT] ON [dbo].[Logs]([DT]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsAID] ON [dbo].[Logs]([ActionID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsUID] ON [dbo].[Logs]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsWID] ON [dbo].[Logs]([WorkgroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsTID] ON [dbo].[Logs]([TypeID]) ON [PRIMARY]
 CREATE  INDEX [IX_LogsOID] ON [dbo].[Logs]([ObjectID]) ON [PRIMARY]
ALTER TABLE [dbo].[PrivateURLs] ADD 
	CONSTRAINT [IX_Hash] UNIQUE  NONCLUSTERED 
	(
		[Hash]
	)  ON [PRIMARY] 
 CREATE  INDEX [IX_PrivateURL_PID] ON [dbo].[PrivateURLs]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_PrivateURL_UID] ON [dbo].[PrivateURLs]([UserID]) ON [PRIMARY]
ALTER TABLE [dbo].[Projects] ADD 
	CONSTRAINT [DF_Projects_Status] DEFAULT (0) FOR [Status],
	CONSTRAINT [IX_Passcode] UNIQUE  NONCLUSTERED 
	(
		[Passcode]
	)  ON [PRIMARY] 
 CREATE  INDEX [IX_Passcode2] ON [dbo].[Projects]([Passcode2]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectStatus] ON [dbo].[Projects]([Status]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectStatus2] ON [dbo].[Projects]([Status2]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectName] ON [dbo].[Projects]([ProjectName]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectsWID] ON [dbo].[Projects]([WorkgroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectMID] ON [dbo].[Projects]([MeetingID]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectMID2] ON [dbo].[Projects]([MeetingID2]) ON [PRIMARY]
 CREATE  INDEX [IX_ProjectGUID] ON [dbo].[Projects]([GUID]) ON [PRIMARY]
 CREATE  INDEX [IX_GID] ON [dbo].[RoleActions]([RoleGroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_AType] ON [dbo].[RoleActions]([ActionType]) ON [PRIMARY]
ALTER TABLE [dbo].[RoleGroups] ADD 
	CONSTRAINT [DF_RoleGroups_GroupType] DEFAULT (0) FOR [GroupType],
	CONSTRAINT [DF_RoleGroups_Status] DEFAULT (0) FOR [Status]
 CREATE  INDEX [IX_GroupType] ON [dbo].[RoleGroups]([RoleLevel]) ON [PRIMARY]
 CREATE  INDEX [IX_RGWorkgroupID] ON [dbo].[RoleGroups]([WorkgroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_RGGT] ON [dbo].[RoleGroups]([GroupType]) ON [PRIMARY]
 CREATE  INDEX [IX_SignupEmail] ON [dbo].[Signup]([SignupID]) ON [PRIMARY]
 CREATE INDEX [IX_Snapshots_DT] ON [dbo].[Snapshots] ([DT]) ON [PRIMARY]
 CREATE INDEX [IX_Snapshots_PID] ON [dbo].[Snapshots] ([ProjectID]) ON [PRIMARY]
 CREATE INDEX [IX_Snapshots_Type] ON [dbo].[Snapshots] ([SnapshotType])  ON [PRIMARY]
 CREATE  INDEX [IX_StructureMeetingsPrjID] ON [dbo].[StructureMeetings]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_StructureTokensMID] ON [dbo].[StructureTokens]([MeetingID]) ON [PRIMARY]
 CREATE  INDEX [IX_TT_PID] ON [dbo].[TeamTimeData]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_TT_UID] ON [dbo].[TeamTimeData]([UserID]) ON [PRIMARY]
ALTER TABLE [dbo].[UserWorkgroups] ADD 
	CONSTRAINT [DF_UserWorkgroups_Status] DEFAULT (0) FOR [Status]
 CREATE  INDEX [IX_UWUID] ON [dbo].[UserWorkgroups]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_UWWGID] ON [dbo].[UserWorkgroups]([WorkgroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_UWStatus] ON [dbo].[UserWorkgroups]([Status]) ON [PRIMARY]
 CREATE  INDEX [IX_UWGID] ON [dbo].[UserWorkgroups]([RoleGroupID]) ON [PRIMARY]
ALTER TABLE [dbo].[Users] ADD 
	CONSTRAINT [DF_Users_Status] DEFAULT (0) FOR [Status],
	CONSTRAINT [IX_Email] UNIQUE  NONCLUSTERED 
	(
		[Email]
	)  ON [PRIMARY] 
 CREATE  INDEX [IX_UsersOWID] ON [dbo].[Users]([OwnerID]) ON [PRIMARY]
 CREATE  INDEX [IX_DefWGID] ON [dbo].[Users]([DefaultWGID]) ON [PRIMARY]
 CREATE  INDEX [IX_UserTemplates_UID] ON [dbo].[UserTemplates]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_UserTemplates_MD] ON [dbo].[UserTemplates]([ModifyDate]) ON [PRIMARY]
ALTER TABLE [dbo].[Workgroups] ADD 
	CONSTRAINT [DF_Workgroups_Status] DEFAULT (0) FOR [Status]
 CREATE  INDEX [IX_WorkgroupsName] ON [dbo].[Workgroups]([Name]) ON [PRIMARY]
 CREATE  INDEX [IX_WorkgroupsOWID] ON [dbo].[Workgroups]([OwnerID]) ON [PRIMARY]
 CREATE  INDEX [IX_WorkgroupsECAMID] ON [dbo].[Workgroups]([ECAMID]) ON [PRIMARY]
 CREATE UNIQUE INDEX WkgParam_Idx ON WorkgroupParams
	(
	WorkgroupID,
	ParameterID
	) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]

ALTER TABLE [dbo].[Workspace] ADD 
	CONSTRAINT [DF_Workspace_Status] DEFAULT (0) FOR [Status]
 CREATE  INDEX [IX_WUID] ON [dbo].[Workspace]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_WPID] ON [dbo].[Workspace]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_WGID] ON [dbo].[Workspace]([GroupID]) ON [PRIMARY]
 CREATE  INDEX [IX_WStatus] ON [dbo].[Workspace]([Status]) ON [PRIMARY]
 CREATE  INDEX [IX_WStatus2] ON [dbo].[Workspace]([Status2]) ON [PRIMARY]




GO



 INSERT INTO [Extra] (TypeID, ObjectID, PropertyID, PropertyValue) VALUES (1, 1, 1, '0.99992')
