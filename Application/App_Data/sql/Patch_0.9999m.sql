/* Patch MasterDB up to version 0.99991 (160318)
 + Add column 'Snapshots' table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9999' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

CREATE TABLE [dbo].[Snapshots](
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[DT] [datetime] NOT NULL,
	[ProjectID] [int] NOT NULL,
	[SnapshotType] [smallint] NOT NULL,
	[Stream] [varbinary](max) NULL,
	[StreamMD5] [nvarchar](64) NULL,
	[Workspace] [nvarchar](max) NULL,
	[WorkspaceMD5] [nvarchar](64) NULL,
	[Comment] [nvarchar](255) NULL,
   ) ON [PRIMARY]


ALTER TABLE [dbo].[Snapshots] WITH NOCHECK ADD 
	CONSTRAINT [PK_Snapshots] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 


   CREATE INDEX [IX_Snapshots_DT] ON [dbo].[Snapshots] ([DT]) ON [PRIMARY]
   CREATE INDEX [IX_Snapshots_PID] ON [dbo].[Snapshots] ([ProjectID]) ON [PRIMARY]
   CREATE INDEX [IX_Snapshots_Type] ON [dbo].[Snapshots] ([SnapshotType])  ON [PRIMARY]

   UPDATE [Extra] SET PropertyValue='0.99991' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
