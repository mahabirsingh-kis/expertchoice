/* Patch MasterDB up to version 0.97 (101206):
 * make clean-up for duplicates in UserWorkgroups table;
 + add combined indexes for UserWorkgroups, Workspaces tables. */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.96') BEGIN

 DELETE FROM UserWorkgroups WHERE NOT ID IN (SELECT MIN(id) as id From UserWorkgroups GROUP BY UserID, WorkgroupID)
 DELETE FROM Workspace WHERE NOT ID IN (SELECT MIN(id) as id From Workspace GROUP BY UserID, ProjectID) 

 ALTER TABLE [dbo].[Workspace] ADD 
	CONSTRAINT [IX_UID_PID] UNIQUE  NONCLUSTERED 
	(
		[UserID],
		[ProjectID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[UserWorkgroups] ADD 
	CONSTRAINT [IX_UID_WID] UNIQUE  NONCLUSTERED 
	(
		[UserID],
		[WorkgroupID]
	)  ON [PRIMARY] 
 
 UPDATE [Extra] SET PropertyValue='0.97' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
