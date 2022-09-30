/* This is a MS SQL script that perform a tables clean-up as removing all projects,
   last time accessed/modify/creation is earlier than 3 years ago.
   Also removed all related/dependent objects in the DB.

   (!) Please be very careful with a real "DELETE" queries since you can't rollback these queries.
   
   ver 190117 // Alexander Domanov <adomanov@expertchoice.com> */

IF OBJECT_ID(N'tempdb..#old', N'U') IS NOT NULL   
  DROP TABLE #old;  
GO

DECLARE @old TABLE (id int);
SELECT ID INTO #old FROM Projects d WHERE COALESCE(LastVisited, LastModify, Created) < DATEADD(year, -3, GETDATE());

/* (!) Need to replace "SELECT *" to "DELETE" below this line for a real rows delete */

SELECT * FROM ModelStructure WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM UserData WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM Workspace WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM Extra WHERE ObjectID IN (SELECT * FROM #old) AND TypeID IN (2, 3);
SELECT * FROM PrivateURLs WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM StructureTokens WHERE MeetingID IN (SELECT MeetingID FROM StructureMeetings WHERE ProjectID IN (SELECT * FROM #old));
SELECT * FROM StructureMeetings WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM Snapshots WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM TeamTimeData WHERE ProjectID IN (SELECT * FROM #old);
SELECT * FROM Projects WHERE ID IN (SELECT * FROM #old);
/* Clean-up Logs if required: */
/* SELECT * FROM Logs WHERE DT < DATEADD(year, -3, GETDATE()); */
