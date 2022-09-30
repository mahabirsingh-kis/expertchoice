/* Patch ProjectsDB up to version 1.0.1 (080917):
 + Add field 'ModifyDate' field for tables 'ModelStructure', 'UserData'; 
 + Add 'Properties' table and set DatabaseVersion as 1.0.1; */

   IF NOT EXISTS (SELECT * FROM [sysindexes] WHERE name='IX_ModelStructure_MD') BEGIN
     ALTER TABLE [dbo].[ModelStructure] ADD [ModifyDate] [datetime] NULL
     CREATE INDEX [IX_ModelStructure_MD] ON [dbo].[ModelStructure]([ModifyDate]) ON [PRIMARY]
   END

   IF NOT EXISTS (SELECT * FROM [sysindexes] WHERE name='IX_UserData_MD') BEGIN
     ALTER TABLE [dbo].[UserData] ADD [ModifyDate] [datetime] NULL
     CREATE INDEX [IX_UserData_MD] ON [dbo].[UserData]([ModifyDate]) ON [PRIMARY]
   END

   IF OBJECT_ID('dbo.Properties') IS NULL BEGIN
     CREATE TABLE [dbo].[Properties] (
	[PropertyName] [nvarchar] (250) NOT NULL ,
	[PropertyValue] [nvarchar] (1500) NOT NULL 
     ) ON [PRIMARY]
     ALTER TABLE [dbo].[Properties] WITH NOCHECK ADD 
	CONSTRAINT [PK_Properties] PRIMARY KEY  CLUSTERED (
	 [PropertyName]
     ) ON [PRIMARY] 
    INSERT INTO Properties (PropertyName, PropertyValue) VALUES ('DatabaseVersion', '1.0.1')
   END
