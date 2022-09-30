/* Patch MasterDB up to version 0.99 (110316):
 + Add PrivateURLs table. */

IF EXISTS (SELECT * FROM Extra WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1 AND PropertyValue='0.98') BEGIN

CREATE TABLE [dbo].[PrivateURLs] (
	[ID] [int] IDENTITY (1, 1) NOT NULL ,
	[Hash] [nvarchar] (32) NOT NULL ,
	[URL] [nvarchar] (1024) NOT NULL 
) ON [PRIMARY]

ALTER TABLE [dbo].[PrivateURLs] WITH NOCHECK ADD 
	CONSTRAINT [PK_PrivateURLs] PRIMARY KEY  CLUSTERED 
	(
		[ID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[PrivateURLs] ADD 
	CONSTRAINT [IX_Hash] UNIQUE  NONCLUSTERED 
	(
		[Hash]
	)  ON [PRIMARY] 
 
 UPDATE [Extra] SET PropertyValue='0.99' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
