/* Patch ProjectsDB up to version 1.0.3 (140210):
 + Add column IsTerminalNode to table 'CombinedJudgments'; */

IF EXISTS (SELECT * FROM Properties WHERE PropertyName='DatabaseVersion' AND PropertyValue='1.0.2') BEGIN
IF OBJECT_ID('DF_CombinedJudgments_IsTerminalNode') IS NULL BEGIN
  ALTER TABLE [dbo].[CombinedJudgments] ADD [IsTerminalNode] [bit] NOT NULL
  ALTER TABLE [dbo].[CombinedJudgments] ADD  CONSTRAINT [DF_CombinedJudgments_IsTerminalNode]  DEFAULT (('false')) FOR [IsTerminalNode]
END

  UPDATE Properties Set PropertyValue='1.0.3' WHERE PropertyName='DatabaseVersion'

END
