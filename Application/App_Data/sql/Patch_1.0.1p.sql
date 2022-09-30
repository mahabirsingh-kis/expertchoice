/* Patch ProjectsDB up to version 1.0.2 (140210):
 + Add table 'CombinedJudgments';  */

IF EXISTS (SELECT * FROM Properties WHERE PropertyName='DatabaseVersion' AND PropertyValue='1.0.1') BEGIN
IF OBJECT_ID('dbo.CombinedJudgments') IS NULL BEGIN
CREATE TABLE [CombinedJudgments](
	[ProjectID] [int] NOT NULL,
	[CombinedUserID] [int] NOT NULL,
	[HierarchyID] [int] NOT NULL,
	[WRTNodeID] [int] NOT NULL,
	[Child1ID] [int] NULL,
	[Child2ID] [int] NULL,
	[CombinedValue] [float] NULL,
	[UsersCount] [int] NULL
) ON [PRIMARY]


  CREATE INDEX [IX_CJ_CUID] ON [dbo].[CombinedJudgments] ([CombinedUserID])  ON [PRIMARY]
  CREATE INDEX [IX_CJ_HID] ON [dbo].[CombinedJudgments] ([HierarchyID]) ON [PRIMARY]
  CREATE INDEX [IX_CJ_PrjID] ON [dbo].[CombinedJudgments] ([ProjectID]) ON [PRIMARY]
  CREATE INDEX [IX_CJ_WRT_NID] ON [dbo].[CombinedJudgments] ([WRTNodeID]) ON [PRIMARY]
END
  UPDATE Properties Set PropertyValue='1.0.2' WHERE PropertyName='DatabaseVersion'
END
