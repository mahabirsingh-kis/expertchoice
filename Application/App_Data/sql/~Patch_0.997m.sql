/* Patch MasterDB up to version 0.998 (120516):
 + Add column 'OptIn' to 'Signup' table; */


IF NOT OBJECT_ID('dbo.Signup') IS NULL BEGIN
IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.997' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE SignUp ADD OptIn [smallint]

   UPDATE [Extra] SET PropertyValue='0.998' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
END