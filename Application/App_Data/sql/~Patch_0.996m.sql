/* Patch MasterDB up to version 0.997 (120306):
 + Recreate column 'Data' in TeamTimeData table; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.996' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE TeamTimeData DROP COLUMN Data
   ALTER TABLE TeamTimeData ADD Data [ntext]

   UPDATE [Extra] SET PropertyValue='0.997' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
