/* Patch StreamProjectsDB up to version 1.0.5 (140620)
 + Add tables 'Reports_HierarchyStructure', 'Reports_Users', 'Reports_Judgments' */

IF EXISTS (SELECT * FROM [Properties] WHERE PropertyName='DatabaseVersion' AND PropertyValue='1.0.4') BEGIN

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

CREATE INDEX [IX_Reports_PID] ON [dbo].[Reports_Judgments] ([ProjectID] ASC) ON [PRIMARY]
CREATE INDEX [IX_Reports_UID] ON [dbo].[Reports_Judgments] ([UserID] ASC) ON [PRIMARY]
CREATE INDEX [IX_Reports_WRT] ON [dbo].[Reports_Judgments] ([WRTNodeID] ASC) ON [PRIMARY]
CREATE INDEX [IX_Reports_H_PID] ON [dbo].[Reports_HierarchyStructure] ([ParentNodeID] ASC) ON [PRIMARY]
ALTER TABLE [dbo].[Reports_HierarchyStructure] ADD CONSTRAINT [DF_Reports_HierarchyStructure_ParentNodeID] DEFAULT ((-1)) FOR [ParentNodeID]
ALTER TABLE [dbo].[Reports_HierarchyStructure] ADD CONSTRAINT [DF_Reports_HierarchyStructure_IsCoveringObjective] DEFAULT ((0)) FOR [IsCoveringObjective]
ALTER TABLE [dbo].[Reports_Judgments] ADD  CONSTRAINT [DF_Reports_J_Child1ID]  DEFAULT ((-1)) FOR [Child1ID]
ALTER TABLE [dbo].[Reports_Judgments] ADD  CONSTRAINT [DF_Reports_J_Child2ID]  DEFAULT ((-1)) FOR [Child2ID]
  
 UPDATE [Properties] SET PropertyValue='1.0.5' WHERE PropertyName='DatabaseVersion'

END