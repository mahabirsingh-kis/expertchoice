/* Patch MasterDB up to version 0.96 (101006):
 + add table "UserTemplates".  */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.95') BEGIN

CREATE TABLE [dbo].[UserTemplates] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[UserID] [int] NOT NULL ,
	[TemplateName] [nvarchar] (200) NOT NULL ,
	[Comment] [nvarchar] (255) NOT NULL ,
	[StructureType] [int] NOT NULL ,
	[StreamSize] [int] NULL ,
	[Stream] [image] NULL,
	[ModifyDate] [datetime] NULL 
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[UserTemplates] WITH NOCHECK ADD 
	CONSTRAINT [PK_UserTemplates] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 

 CREATE  INDEX [IX_UserTemplates_UID] ON [dbo].[UserTemplates]([UserID]) ON [PRIMARY]
 CREATE  INDEX [IX_UserTemplates_MD] ON [dbo].[UserTemplates]([ModifyDate]) ON [PRIMARY]

 UPDATE [Extra] SET PropertyValue='0.96' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
