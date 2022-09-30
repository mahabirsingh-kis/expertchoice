/* Patch MasterDB up to version 0.995 (111031):
 + Add Workgroup column to Signup table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.994' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[Signup] ADD [Workgroup] [nvarchar] (255) NULL 

 UPDATE [Extra] SET PropertyValue='0.995' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
