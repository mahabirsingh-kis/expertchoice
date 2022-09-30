/* Patch MasterDB up to version 0.94 (091024):
 + create tables 'StructureMeetings', 'StructureTokens'.  */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.93') BEGIN


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


 CREATE  INDEX [IX_StructureMeetingsPrjID] ON [dbo].[StructureMeetings]([ProjectID]) ON [PRIMARY]
 CREATE  INDEX [IX_StructureTokensMID] ON [dbo].[StructureTokens]([MeetingID]) ON [PRIMARY]


 UPDATE [Extra] SET PropertyValue='0.94' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
