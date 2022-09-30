/* Patch MasterDB up to version 0.9999 (160208)
 + Add column 'ProjectType' to table 'Projects'; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9998' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

   ALTER TABLE [dbo].[Projects] ADD [ProjectType] [int] NULL DEFAULT(0)

   UPDATE [Extra] SET PropertyValue='0.9999' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
