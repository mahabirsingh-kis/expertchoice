CREATE TABLE [dbo].[Surveys](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[WorkgroupID] [int] NOT NULL DEFAULT ((0)),
	[OwnerID] [int] NULL,
	[Title] [varchar](250) NULL,
	[Comments] [text] NULL,
	[State] [int] NOT NULL DEFAULT ((0)),
	[DBName] [varchar](50) NOT NULL,
	[GUID] [varchar](50) NULL
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
ALTER TABLE [dbo].[Surveys] WITH NOCHECK ADD 
	CONSTRAINT [PK_Surveys] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 



CREATE  INDEX [Surveys_WID] ON [dbo].[Surveys]([WorkgroupID]) ON [PRIMARY]
CREATE  INDEX [Surveys_OID] ON [dbo].[Surveys]([OwnerID]) ON [PRIMARY]
CREATE  INDEX [Surveys_GUID] ON [dbo].[Surveys]([GUID]) ON [PRIMARY]
