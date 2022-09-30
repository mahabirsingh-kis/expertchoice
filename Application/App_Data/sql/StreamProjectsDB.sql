/* Model Stream tables. Version 1.0.5  */

CREATE TABLE [dbo].[ModelStructure] (
	[ProjectID] [int] NOT NULL ,
	[StructureType] [int] NOT NULL ,
	[StreamSize] [int] NOT NULL ,
	[Stream] [VARBINARY] (MAX) NOT NULL ,
	[ModifyDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[UserData] (
	[ProjectID] [int] NOT NULL ,
	[UserID] [int] NOT NULL ,
	[DataType] [int] NOT NULL ,
	[StreamSize] [int] NOT NULL ,
	[Stream] [VARBINARY] (MAX) NOT NULL ,
	[ModifyDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

CREATE TABLE [dbo].[Properties] (
        [PropertyName] [nvarchar] (255) NOT NULL ,
        [PropertyValue] [nvarchar] (1000) NOT NULL 
) ON [PRIMARY]

CREATE TABLE [CombinedJudgments](
	[ProjectID] [int] NOT NULL,
	[CombinedUserID] [int] NOT NULL,
	[HierarchyID] [int] NOT NULL,
	[WRTNodeID] [int] NOT NULL,
	[Child1ID] [int] NULL,
	[Child2ID] [int] NULL,
	[CombinedValue] [float] NULL,
	[UsersCount] [int] NULL,
	[IsTerminalNode] [bit] NOT NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[Reports_Users](
	[ProjectID] [bigint] NOT NULL,
	[UserID] [bigint] NOT NULL,
	[UserEmail] [nvarchar](80) NOT NULL,
	[UserName] [nvarchar](255) NULL,
 CONSTRAINT [IX_Reports_Idx] UNIQUE 
(
	[ProjectID] ASC,
	[UserID] ASC
) ON [PRIMARY])

CREATE TABLE [dbo].[Reports_Judgments](
	[ProjectID] [bigint] NOT NULL,
	[UserID] [bigint] NOT NULL,
	[WRTNodeID] [bigint] NOT NULL,
	[Child1ID] [bigint] NOT NULL,
	[Child2ID] [bigint] NOT NULL,
	[Value] [nvarchar](max) NULL,
	[Comment] [nvarchar](max) NULL
) ON [PRIMARY]

CREATE TABLE [dbo].[Reports_HierarchyStructure](
	[ProjectID] [bigint] NOT NULL,
	[NodeID] [bigint] NOT NULL,
	[HierarchyID] [bigint] NOT NULL,
	[NodePath] [nvarchar](max) NULL,
	[NodeName] [nvarchar](max) NULL,
	[ParentNodeID] [bigint] NULL,
	[Comment] [nvarchar](max) NULL,
	[MeasurementType] [nvarchar](50) NULL,
	[IsCoveringObjective] [bit] NOT NULL,
 CONSTRAINT [IX_Reports_H_Idx] UNIQUE
(
	[ProjectID] ASC,
	[NodeID] ASC,
	[HierarchyID] ASC
) ON [PRIMARY])




ALTER TABLE [dbo].[ModelStructure] ADD 
	CONSTRAINT [IX_ModelStructure_PID_ST] UNIQUE  NONCLUSTERED 
	(
		[ProjectID],
		[StructureType]
	)  ON [PRIMARY] 

 CREATE  INDEX [IX_ModelStructure_PID] ON [dbo].[ModelStructure]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_ModelStructure_ST] ON [dbo].[ModelStructure]([StructureType]) ON [PRIMARY]

 ALTER TABLE [dbo].[CombinedJudgments] ADD  CONSTRAINT [DF_CombinedJudgments_IsTerminalNode]  DEFAULT (('false')) FOR [IsTerminalNode]

ALTER TABLE [dbo].[UserData] ADD 
	CONSTRAINT [IX_UserData_PID_UID_DT] UNIQUE  NONCLUSTERED 
	(
		[ProjectID],
		[UserID],
		[DataType]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[Properties] WITH NOCHECK ADD 
        CONSTRAINT [PK_Properties] PRIMARY KEY  CLUSTERED (
         [PropertyName]
) ON [PRIMARY] 



 CREATE  INDEX [IX_UserData_PID] ON [dbo].[UserData]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_UserData_UID] ON [dbo].[UserData]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_UserData_DT] ON [dbo].[UserData]([DataType]) ON [PRIMARY]
 CREATE INDEX [IX_ModelStructure_MD] ON [dbo].[ModelStructure]([ModifyDate]) ON [PRIMARY]
 CREATE INDEX [IX_UserData_MD] ON [dbo].[UserData]([ModifyDate]) ON [PRIMARY]
 CREATE INDEX [IX_CJ_CUID] ON [dbo].[CombinedJudgments] ([CombinedUserID])  ON [PRIMARY]
 CREATE INDEX [IX_CJ_HID] ON [dbo].[CombinedJudgments] ([HierarchyID]) ON [PRIMARY]
 CREATE INDEX [IX_CJ_PrjID] ON [dbo].[CombinedJudgments] ([ProjectID]) ON [PRIMARY]
 CREATE INDEX [IX_CJ_WRT_NID] ON [dbo].[CombinedJudgments] ([WRTNodeID]) ON [PRIMARY]
 CREATE INDEX [IX_Reports_PID] ON [dbo].[Reports_Judgments] ([ProjectID] ASC) ON [PRIMARY]
 CREATE INDEX [IX_Reports_UID] ON [dbo].[Reports_Judgments] ([UserID] ASC) ON [PRIMARY]
 CREATE INDEX [IX_Reports_WRT] ON [dbo].[Reports_Judgments] ([WRTNodeID] ASC) ON [PRIMARY]
 CREATE INDEX [IX_Reports_H_PID] ON [dbo].[Reports_HierarchyStructure] ([ParentNodeID] ASC) ON [PRIMARY]
 ALTER TABLE [dbo].[Reports_HierarchyStructure] ADD CONSTRAINT [DF_Reports_HierarchyStructure_ParentNodeID] DEFAULT ((-1)) FOR [ParentNodeID]
 ALTER TABLE [dbo].[Reports_HierarchyStructure] ADD CONSTRAINT [DF_Reports_HierarchyStructure_IsCoveringObjective] DEFAULT ((0)) FOR [IsCoveringObjective]
 ALTER TABLE [dbo].[Reports_Judgments] ADD  CONSTRAINT [DF_Reports_J_Child1ID]  DEFAULT ((-1)) FOR [Child1ID]
 ALTER TABLE [dbo].[Reports_Judgments] ADD  CONSTRAINT [DF_Reports_J_Child2ID]  DEFAULT ((-1)) FOR [Child2ID]

 GO



 INSERT INTO Properties (PropertyName, PropertyValue) VALUES ('DatabaseVersion', '1.0.5')
