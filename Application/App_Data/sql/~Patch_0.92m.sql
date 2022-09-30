/* Patch MasterDB up to version 0.93 (090806):
 + create table AuthToken */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.92') BEGIN

CREATE TABLE [dbo].[AuthTokens] (
	[MeetingID] [nvarchar] (50) NOT NULL ,
	[AuthToken] [int] NOT NULL ,
	[Master] [tinyint] NULL 
) ON [PRIMARY]

 CREATE  INDEX [IX_AuthTokensKey] ON [dbo].[AuthTokens]([MeetingID], [AuthToken]) ON [PRIMARY]


 UPDATE [Extra] SET PropertyValue='0.93' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
