/* Patch MasterDB up to version 0.994 (111027):
 + Add Refferal column to Signup table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.993' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

 ALTER TABLE [dbo].[Signup] ADD [Referral] [nvarchar] (255) NULL 

 UPDATE [Extra] SET PropertyValue='0.994' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
