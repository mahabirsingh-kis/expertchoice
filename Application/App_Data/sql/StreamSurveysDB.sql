CREATE TABLE [dbo].[SurveyStructure](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SurveyID] [int] NOT NULL,
	[DataType] [int] NOT NULL,
	[ObjectGUID] [varchar](50) NULL CONSTRAINT [DF_SurveyStructure_RespondentID]  DEFAULT (''),
	[StreamSize] [int] NOT NULL CONSTRAINT [DF_SurveyStructure_StreamSize]  DEFAULT ((0)),
	[Stream] [VARBINARY] (MAX) NULL,
	[ModifyDate] [datetime] NOT NULL CONSTRAINT [DF_SurveyStructure_ModifyDate]  DEFAULT (getdate())
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
ALTER TABLE [dbo].[SurveyStructure] WITH NOCHECK ADD 
	CONSTRAINT [PK_SurveyStructure] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 



CREATE  INDEX [SurveyStructure_WID] ON [dbo].[SurveyStructure]([SurveyID]) ON [PRIMARY]
CREATE  INDEX [SurveyStructure_OID] ON [dbo].[SurveyStructure]([ObjectGUID]) ON [PRIMARY]
