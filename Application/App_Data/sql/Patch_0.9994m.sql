/* Patch MasterDB up to version 0.9995 (140303)
 + Add table 'WorkgroupParams'; */


IF EXISTS (SELECT * FROM [Extra] WHERE PropertyValue='0.9994' AND TypeID=1 AND ObjectID=1 AND PropertyID=1) BEGIN

CREATE TABLE WorkgroupParams (
	WorkgroupID int NOT NULL,
	ParameterID int NOT NULL,
	ParameterValue float NULL DEFAULT(-1)
)  ON [PRIMARY]

CREATE UNIQUE INDEX WkgParam_Idx ON WorkgroupParams (
	WorkgroupID,
	ParameterID
) WITH (IGNORE_DUP_KEY = OFF) ON [PRIMARY]

   UPDATE [Extra] SET PropertyValue='0.9995' WHERE TypeID=1 AND ObjectID=1 AND PropertyID=1

END
