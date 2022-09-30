/* Patch MasterDB up to version 0.9996 (140323)
 + Add column 'PasswordStatus' to table 'Users'; 
 + Add column 'Changed' to 'Extra' table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9995' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE [dbo].[Users] ADD [PasswordStatus] [int] NULL DEFAULT(0)

   UPDATE [Extra] SET PropertyValue='0.9996' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
