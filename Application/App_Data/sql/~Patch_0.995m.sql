/* Patch MasterDB up to version 0.996 (120223):
 + Add TeamTimeData table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.995' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

  CREATE TABLE [dbo].[TeamTimeData] (
  	[ID] [int] IDENTITY (1, 1) NOT NULL ,
  	[DT] [datetime] NOT NULL ,
  	[ProjectID] [int] NOT NULL ,
  	[UserID] [int] NOT NULL ,
  	[ObjectID] [smallint] NOT NULL ,
  	[Data] [nvarchar] (2000) NOT NULL 
  ) ON [PRIMARY]

  ALTER TABLE [dbo].[TeamTimeData] WITH NOCHECK ADD 
  	CONSTRAINT [PK_TeamTimeID] PRIMARY KEY  CLUSTERED 
  	(
 		[ID]
  	)  ON [PRIMARY] 

   CREATE  INDEX [IX_TT_PID] ON [dbo].[TeamTimeData]([ProjectID]) ON [PRIMARY]
   CREATE  INDEX [IX_TT_UID] ON [dbo].[TeamTimeData]([UserID]) ON [PRIMARY]

   UPDATE [Extra] SET PropertyValue='0.996' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
