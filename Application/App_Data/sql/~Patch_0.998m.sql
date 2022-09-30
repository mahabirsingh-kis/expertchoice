/* Patch MasterDB up to version 0.999 (120606)
 + Add column 'HubspotUserToken' to 'Signup' table; */


IF NOT OBJECT_ID('dbo.Signup') IS NULL BEGIN
IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.998' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE SignUp ADD HubspotUserToken [nvarchar] (255)

   UPDATE [Extra] SET PropertyValue='0.999' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
END