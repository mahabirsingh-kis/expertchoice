/* Patch MasterDB up to version 0.993 (111013):
 + Add Signup table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.992' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 CREATE TABLE [dbo].[Signup] (
	[SignupID] [int] NOT NULL ,
	[RegistrationID] [nvarchar] (255) NOT NULL ,
	[FirstName] [nvarchar] (255) NULL ,
	[LastName] [nvarchar] (255) NULL ,
	[email] [nvarchar] (255) NOT NULL ,
	[organization] [nvarchar] (255) NULL ,
	[phone] [nvarchar] (255) NULL ,
	[registerdate] [datetime] NULL ,
	[EmailConfirmed] [smallint] NOT NULL ,
	[ip] [nvarchar] (255) NULL ,
	[host] [nvarchar] (255) NULL 
 ) ON [PRIMARY]

 ALTER TABLE [dbo].[Signup] WITH NOCHECK ADD 
	CONSTRAINT [PK_Signup] PRIMARY KEY  CLUSTERED 
	(
		[SignupID]
	)  ON [PRIMARY] 

ALTER TABLE [dbo].[Signup] ADD 
	CONSTRAINT [DF_Signup_EmailConf] DEFAULT (0) FOR [EmailConfirmed],
	CONSTRAINT [IX_Signup_RegID] UNIQUE  NONCLUSTERED 
	(
		[RegistrationID]
	)  ON [PRIMARY] 

 CREATE  INDEX [IX_Signup_Email] ON [dbo].[Signup]([SignupID]) ON [PRIMARY]

 UPDATE [Extra] SET PropertyValue='0.993' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
