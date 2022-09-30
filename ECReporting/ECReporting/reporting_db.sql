create database ReportingDb
go

use ReportingDb

create table ReportsQueries
  (ReportId int identity not null,
   ReportName nvarchar(200) not null,
   Comment nvarchar(1000),
   Modified datetime not null,
   ReportQuery ntext not null);

alter table ReportsQueries add constraint pk_ReportsQueries primary key (ReportId);
create unique index idx_ReportsQueries on ReportsQueries(ReportName);

create table Snapshots
  (
   SnapshotId int identity not null,
   ProjectId int not null,
   SnapshotTime datetime not null
  );

alter table Snapshots add constraint pk_snapshots
primary key (SnapshotId, ProjectId);

create unique index idx_snapshots_1 on Snapshots(ProjectId, SnapshotTime);

CREATE TABLE Workgroups
  (SnapshotID int not null,
   WorkgroupID int NOT NULL,
   OwnerID int NOT NULL,
   ECAMID int NOT NULL,
   Name nvarchar(250) NOT NULL,
   Status int default 0 NOT NULL,
   LicenseData image,
   LicenseKey nvarchar(100),
   Comment nvarchar(1000),
   Created datetime,
   LastModified datetime,
   LastVisited datetime);

alter table Workgroups add constraint pk_Workgroups primary key (SnapshotID, WorkgroupID);

create index idx_Workgroups_ECAMID on Workgroups(SnapshotID, ECAMID);
create index idx_Workgroups_OwnerID on Workgroups(SnapshotID, OwnerID);
create index idx_Workgroups_Name on Workgroups(SnapshotID, Name);

create table RoleGroups
  (SnapshotID int not null,
   RoleGroupID int NOT NULL,
   WorkgroupID int NOT NULL,
   RoleLevel int NOT NULL,
   GroupType int default 0 NOT NULL,
   Status int default 0 NOT NULL,
   Name nvarchar(250) NOT NULL,
   Comment nvarchar(1000),
   Created datetime,
   LastModified datetime);

alter table RoleGroups add constraint pk_RoleGroups primary key (SnapshotID, RoleGroupID);

create unique index idx_RoleGroups_1 on RoleGroups(SnapshotID, RoleGroupID, WorkgroupID);
create unique index idx_RoleGroups_2 on RoleGroups(SnapshotID, WorkgroupID, RoleGroupID);
create index idx_RoleGroups_RoleLevel on RoleGroups(SnapshotID, RoleLevel);
create index idx_RoleGroups_GroupType on RoleGroups(SnapshotID, GroupType);

alter table RoleGroups add constraint fk_RoleGroups_Workgroups
foreign key (SnapshotID, WorkgroupID)
references Workgroups(SnapshotID, WorkgroupID) on delete cascade;

CREATE TABLE RoleActions
  (
   SnapshotID int not null,
   RoleGroupID int NOT NULL,
   ActionID int NOT NULL,
   ActionType int NOT NULL,
   Status int NOT NULL,
   Comment nvarchar(1000));

alter table RoleActions add constraint pk_RoleActions
primary key(SnapshotID, RoleGroupID, ActionID);

create index idx_RoleActions_ActionType on RoleActions(SnapshotID, ActionType);

alter table RoleActions add constraint fk_RoleActions_RoleGroup
foreign key(SnapshotID, RoleGroupID) 
references RoleGroups(SnapshotID, RoleGroupID) on delete cascade;

CREATE TABLE Users
  (SnapshotID int NOT NULL,
   UserID int NOT NULL,
   OwnerID int,
   Email nvarchar(64) NOT NULL,
   Password nvarchar(64),
   Status int default 0,
   FullName nvarchar(80),
   Comment nvarchar(1000),
   DefaultWGID int,
   Created datetime,
   LastModified datetime,
   LastVisited datetime,
   LastPageID int,
   LastURL nvarchar(500),
   LastWorkgroupID int,
   IsOnline bit,
   EULAVersion nvarchar(10),
   SessionID nvarchar(80));

alter table Users add constraint pk_Users primary key(SnapshotID, UserID);

create index idx_Users_DefaultWGID on Users(SnapshotID, DefaultWGID);
create index idx_Users_OwnerID on Users(SnapshotID, OwnerID);
create unique index idx_Users_EMail on Users(SnapshotID, EMail);

/* alter table Users add constraint fk_Users_Users
foreign key(SnapshotID, OwnerID)
references Users(SnapshotID, UserID) on delete no action;

alter table Users add constraint fk_Users_Workgroups
foreign key(SnapshotID, DefaultWGID)
references Workgroups(SnapshotID, WorkgroupID) on delete no action; */

create table UsersWorkgroups
  (SnapshotID int not null,
   UserID int NOT NULL,
   WorkgroupID int NOT NULL,
   RoleGroupID int NOT NULL,
   Status int default 0 NOT NULL,
   Comment nvarchar(1000),
   ExpirationDate datetime,
   Created datetime,
   LastVisited datetime,
   LastProjectID int);

alter table UsersWorkgroups add constraint pk_UsersWorkgroups primary key(SnapshotID, UserID, WorkgroupID);

create index idx_UsersWorkgroups_RoleGroupID on UsersWorkgroups(SnapshotID, RoleGroupID);
create index idx_UsersWorkgroups_Status on UsersWorkgroups(SnapshotID, Status);
create unique index idx_UsersWorkgroups_WGID_UserID on UsersWorkgroups(SnapshotID, WorkgroupID, UserID);

alter table UsersWorkgroups add constraint fk_UsersWorkgroups_Users
foreign key(SnapshotID, UserID)
references Users(SnapshotID, UserID) on delete cascade;

alter table UsersWorkgroups add constraint fk_UsersWorkgroups_Workgroups
foreign key(SnapshotID, WorkgroupID)
references Workgroups(SnapshotID, WorkgroupID) on delete cascade;

alter table UsersWorkgroups  add constraint fk_UsersWorkgroups_RoleGroups
foreign key(SnapshotID, RoleGroupID)
references RoleGroups(SnapshotID, RoleGroupID) on delete no action;

CREATE TABLE Projects
  (SnapshotID int not null,
   WorkgroupID int NOT NULL,
   ProjectID int NOT NULL,
   OwnerID int NOT NULL,
   Passcode nvarchar(64) NOT NULL,
   FileName nvarchar(200) NOT NULL,
   ProjectName nvarchar(250),
   Status int default 0 NOT NULL,
   Comment nvarchar(1000),
   MeetingID varchar(16),
   MeetingOwnerID int,
   LockStatus int,
   LockedByUserID int,
   LockExpiration datetime,
   Created datetime,
   LastModified datetime,
   LastVisited datetime);

alter table Projects add constraint pk_Projects
primary key(SnapshotID, ProjectID);

create unique index idx_Projects_1 on Projects(SnapshotID, ProjectID, WorkgroupID);
create unique index idx_Projects_2 on Projects(SnapshotID, WorkgroupID, ProjectID);
create unique index idx_Projects_Passcode on Projects(SnapshotID, Passcode);
create unique index idx_Projects_ProjectName on Projects(SnapshotID, ProjectName);
create index idx_Projects_OwnerID on Projects(SnapshotID, OwnerID);
create index idx_Projects_Status on Projects(SnapshotID, Status);

/* alter table Projects add constraint fk_projects
foreign key(SnapshotId, ProjectId)
references Snapshots(SnapshotId, ProjectId) on delete cascade; */

CREATE TABLE Workspace
  (SnapshotID int not null,
   UserID int NOT NULL,
   ProjectID int NOT NULL,
   GroupID int NOT NULL,
   Status int default 0 NOT NULL,
   Comment nvarchar(1000),
   Step int,
   Created datetime,
   LastModified datetime);

alter table Workspace add constraint pk_Workspace primary key(SnapshotID, UserID, ProjectID);

create unique index idx_Workspace_1 on Workspace(SnapshotID, ProjectID, UserID);
create index idx_Workspace_GroupID on Workspace(SnapshotID, GroupID);
create index idx_Workspace_Status on Workspace(SnapshotID, Status);

alter table Workspace add constraint fk_Workspace_Users
foreign key(SnapshotID, UserID)
references Users(SnapshotID, UserID) on delete cascade;

alter table Workspace add constraint fk_Workspace_Projects
foreign key(SnapshotID, ProjectID)
references Projects(SnapshotID, ProjectID) on delete cascade;

alter table Workspace add constraint fk_Workspace_RoleGroups
foreign key(SnapshotID, GroupID)
references RoleGroups(SnapshotID, RoleGroupID)
on delete no action;

/* Nodes */
create table Nodes
  (
   SnapshotId int not null,
   ProjectId int not null,
   NodeId int not null,
   OriginalNodeId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   ParentID int,
   IsEnabled int,
   IsAlternative int not null,
   MType int default 0,
   MMode int default 0,
   Comment nvarchar(1000),
   RUCurveID int,
   AUCurveID int,
   RScaleID int,
   SFuncID int,
   DefaultDataInstanceID int
  );

alter table Nodes add constraint pk_nodes
primary key (SnapshotId, ProjectId, NodeId);

create index idx_Nodes_Parent
on Nodes(SnapshotId, ProjectId, ParentId);

create index idx_Nodes_DataInstance
on Nodes(SnapshotId, ProjectId, DefaultDataInstanceID);

alter table Nodes add constraint fk_nodes_projects
foreign key(SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* Alternatives contributions */
create table AltsContributions
  (
   SnapshotId int not null,
   ProjectId int not null,
   AltNodeID int not null,
   CovObjNodeID int not null 
  );

alter table AltsContributions add constraint pk_AltsContribution
primary key(SnapshotId, ProjectId, AltNodeId, CovObjNodeId);

create unique index idx_AltsContributions 
on AltsContributions(SnapshotId, ProjectId, CovObjNodeId, AltNodeId);

alter table AltsContributions add constraint fk_AltsContribution_AltNode
foreign key(SnapshotId, ProjectId, AltNodeId)
references Nodes(SnapshotId, ProjectId, NodeId) on delete cascade;

alter table AltsContributions add constraint fk_AltsContribution_CovObjNode
foreign key(SnapshotId, ProjectId, CovObjNodeId)
references Nodes(SnapshotId, ProjectId, NodeId) on delete no action;

/* Rating scales */
create table RatingScales
  (
   SnapshotId int not null,
   ProjectId int not null,
   RScaleId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   IsExplicitlySet int default 0,
   Comment nvarchar(1000)
  );

alter table RatingScales add constraint pk_RatingScales
primary key (SnapshotId, ProjectId, RScaleId);

alter table RatingScales add constraint fk_RatingScales_Projects
foreign key(SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* Rating intensities */
create table RatingIntensities 
  (
   SnapshotId int not null,
   ProjectId int not null,
   RScaleId int not null,
   IntensityId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   IntensityValue float not null,
   Comment nvarchar(1000),
  );

alter table RatingIntensities add constraint pk_RatingIntensities
primary key (SnapshotId, ProjectId, RScaleId, IntensityId);

alter table RatingIntensities add constraint fk_RatingIntensities_Scale
foreign key(SnapshotId, ProjectId, RScaleId)
references RatingScales(SnapshotId, ProjectId, RScaleId) on delete cascade;

/* Utility curves */
create table UtilityCurves
  (
   SnapshotId int not null,
   ProjectId int not null,
   CurveId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   Low float not null,
   High float not null,
   Curvature float not null,
   Comment nvarchar(1000)
  );

alter table UtilityCurves add constraint pk_UtilityCurves
primary key (SnapshotId, ProjectId, CurveId);

alter table UtilityCurves add constraint fk_UtilityCurves_Projects
foreign key(SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* Advanced utility curves */
create table AdvancedUtilityCurves
  (
   SnapshotId int not null,
   ProjectId int not null,
   CurveId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   Low float not null,
   High float not null,
   InterpolationMethod int not null,
   Comment nvarchar(1000)
  );

alter table AdvancedUtilityCurves add constraint pk_AdvUtilityCurves
primary key (SnapshotId, ProjectId, CurveId);

alter table AdvancedUtilityCurves add constraint fk_AdvUtilityCurves_Projects
foreign key(SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* Curves points */
create table CurvesPoints  
  (
   SnapshotId int not null,
   ProjectId int not null,
   CurveId int not null,
   XValue float not null,
   YValue float not null 
  );

create index idx_CurvesPoints on CurvesPoints(SnapshotId, ProjectId, CurveId);

alter table CurvesPoints add constraint fk_CurvesPoints_Curves
foreign key(SnapshotId, ProjectId, CurveId)
references AdvancedUtilityCurves(SnapshotId, ProjectId, CurveId) on delete cascade;

/* Step functions */
create table StepFunctions
  (
   SnapshotId int not null,
   ProjectId int not null,
   FunctionId int not null,
   Guid nvarchar(100) not null,
   Name nvarchar(1000) not null,
   Comment nvarchar(1000),
   IsPiecewiseLinear int 
  );

alter table StepFunctions add constraint pk_StepFunctions
primary key (SnapshotId, ProjectId, FunctionId);

alter table StepFunctions add constraint fk_StepFunctions_Projects
foreign key (SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* Step intervals */
create table StepIntervals
  (
   SnapshotId int not null,
   ProjectId int not null,
   FunctionId int not null,
   IntervalId int not null,
   Name nvarchar(1000),
   LowX float not null,
   HighX float not null,
   IntervalValue float not null,
   Comment nvarchar(1000) NULL
  );

alter table StepIntervals add constraint pk_StepIntervals
primary key (SnapshotId, ProjectId, FunctionId, IntervalId);

alter table StepIntervals add constraint fk_StepIntervals_Functions
foreign key (SnapshotId, ProjectId, FunctionId)
references StepFunctions(SnapshotId, ProjectId, FunctionId) on delete cascade;


CREATE TABLE ProjectUsers
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserId int not null,
   Guid nvarchar(100) not null,
   Email nvarchar(200) not null,
   FullName nvarchar(1000),
   Active int,
   Comment nvarchar(1000),
   LastJudgmentTime datetime,
   DataInstanceID int,
   GroupID int,
   VotingBoxID int,
   IncludedInSynchronous int,
   SyncEvalMode int
  );

alter table ProjectUsers add constraint pk_ProjectUsers 
primary key (SnapshotId, ProjectId, UserId);

alter table ProjectUsers add constraint fk_ProjectUsers_Projects
foreign key (SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

CREATE TABLE DataInstances
  (
   SnapshotId int not null,
   ProjectId int not null,
   Id int not null,
   UserId int,
   Name ntext,
   Comment ntext 
  );

alter table DataInstances add constraint pk_DataInstances
primary key (SnapshotId, ProjectId, Id);

alter table DataInstances add constraint fk_DataInstances
foreign key (SnapshotId, ProjectId)
references Projects(SnapshotId, ProjectId) on delete cascade;

/* --- Users data -- */

CREATE TABLE UserDisabledNodes
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserID int not null,
   NodeID int not null
  );

alter table UserDisabledNodes add constraint pk_UserDisabledNodes
primary key (SnapshotId, ProjectId, UserId, NodeId);

create unique index idx_UserDisabledNodes1
on UserDisabledNodes(SnapshotId, ProjectId, NodeId, UserId);

/* alter table UserDisabledNodes add constraint fk_UserDisabledNodes_Nodes
foreign key (SnapshotId, ProjectId, NodeId)
references Nodes (SnapshotId, ProjectId, NodeId) on delete cascade;

alter table UserDisabledNodes add constraint fk_UserDisabledNodes_Users
foreign key (SnapshotId, ProjectId, UserId)
references ProjectUsers(SnapshotId, ProjectId, UserId) on delete no action; */

CREATE TABLE PairwiseData
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserID int not null,
   WRTNodeID int not null,
   FirstNodeID int not null,
   SecondNodeID int not null,
   Advantage int not null,
   PWValue float not null,
   Comment nvarchar(1000),
   ModifyTime datetime 
  );

alter table PairwiseData add constraint pk_PairwiseData
primary key (SnapshotId, ProjectId, UserId, WRTNodeID, FirstNodeID, SecondNodeID);

create unique index idx_PairwiseData1
on PairwiseData (SnapshotId, ProjectId, WRTNodeID, UserId, FirstNodeID, SecondNodeID);

/* create unique index idx_PairwiseData1
on PairwiseData (SnapshotId, ProjectId, WRTNodeID, FirstNodeID, SecondNodeID, UserId); */

CREATE TABLE NonPairwiseData
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserID int not null,
   ParentNodeID int not null,
   NodeID int not null,
   InputValue float,
   Value float,
   Comment nvarchar(1000),
   ModifyTime datetime 
  );

alter table NonPairwiseData add constraint pk_NonPairwiseData
primary key (SnapshotId, ProjectId, UserId, ParentNodeID, NodeID);

create unique index idx_NonPairwiseData1
on NonPairwiseData (SnapshotId, ProjectId, ParentNodeID, UserId, NodeID);

CREATE TABLE NodesRestrictions
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserID int not null,
   NodeID int not null,
   NRType int
  );

alter table NodesRestrictions add constraint pk_NodesRestrictions
primary key (SnapshotId, ProjectId, UserId, NodeId);

create unique index idx_NodesRestrictions1
on NodesRestrictions (SnapshotId, ProjectId, NodeId, UserId);

CREATE TABLE AltsRestrictions
  (
   SnapshotId int not null,
   ProjectId int not null,
   UserID int not null,
   NodeID int not null,
   AltNodeID int not null,
   NRType int 
  );

alter table AltsRestrictions add constraint pk_AltsRestrictions
primary key (SnapshotId, ProjectId, UserId, NodeId, AltNodeId);

create unique index idx_AltsRestrictions1
on AltsRestrictions (SnapshotId, ProjectId, NodeId, UserId, AltNodeId);

go

create function GetSnapshotId(@ProjectId int, @SnapshotTime datetime)
returns int
as
begin
  declare @SnapId int
  set @SnapId = (select a.SnapshotId 
				   from Snapshots a 
                  where a.ProjectId = @ProjectId
                    and a.SnapshotTime = (select max(SnapshotTime) 
                                            from Snapshots b 
                                           where b.ProjectId = a.ProjectId 
                                             and b.SnapshotTime <= isnull(@SnapshotTime, GetDate())))
  return @SnapId
end;
