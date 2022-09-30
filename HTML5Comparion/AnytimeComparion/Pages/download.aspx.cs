using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using ECCore;
using GenericDBAccess;
using Canvas;
using ExpertChoice;
using ExpertChoice.Data;
using ExpertChoice.Service;
using ECWeb = ExpertChoice.Web;
using ExpertChoice.Database;
using System.Web.Services;
using System.Web.Script.Serialization;
using AnytimeComparion.Pages.external_classes;
using GenericDBAccess.ECGenericDatabaseAccess;
using OfficeOpenXml;
using System.Net;

namespace AnytimeComparion.Pages
{
    public partial class WebForm3 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var projectid = Request.QueryString["projectid"];
            var ext = Request.QueryString["ext"];



            DownloadDecision(Convert.ToInt32(projectid), ext);
        }

        const string _MailMerge_MDB = "ComparionInvitations.mdb";
        const string _MailMerge_DOC_AT = "MailMergeAT.docx";
        const string _MailMerge_DOC_TT = "MailMergeTT.docx";
        const string _MailMerge_ReadMe = "README.docx";
        const string _MailMerge_Folder = "MailMerge";
        const string _MailMerge_TableName = "Office_Address_list";

        private const string dInvitations = "Invitations";
        private const string dStyles = "Styles";
        private const string sStyleHyperLink = "styleHyperlink";

        public const bool DoCompactJet = true;

        private static Chilkat.StringArray _Files2Archive = null;

        const string _CSV_DELIM = ";";

        const int BuffSize = 4 * 1024 * 1024;

        public bool CheckVar(string sVarName, bool DefValue)
        {
            HttpContext context = HttpContext.Current;
            string sValue = DefValue.ToString().ToLower();
            if (sVarName != null && (context.Request[sVarName] != null))
                sValue = Convert.ToString(context.Request[sVarName]).ToLower();
            return (sValue == true.ToString().ToLower() | sValue == "true" | sValue == "1" | sValue == "yes");
        }


        public string CheckVar(string sVarName, string DefValue)
        {
            HttpContext context = HttpContext.Current;
            string Res = DefValue;
            var idd = context.Request.QueryString["id"];
            if (sVarName != null && context.Request.QueryString[sVarName] != null)
                Res = Convert.ToString(context.Request.QueryString[sVarName]);
            return Res;
        }

        public int CheckVar(string sVarName, int DefValue)
        {
            HttpContext context = HttpContext.Current;
            int Res = DefValue;
            try
            {
                if (sVarName != null && context.Request[sVarName] != null)
                    Res = Convert.ToInt32(context.Request[sVarName]);
            }
            catch (Exception ex)
            {
                Res = DefValue;
            }
            return Res;
        }


        internal string[] _MailMerge_ExtraColumns
        {
            get
            {
                var App = (clsComparionCore)HttpContext.Current.Session["App"];
                return ECWeb.Options._TEMPL_LIST_ALL(App.isRiskEnabled);
            }
        }

        private string PrepareDownloadFile(int ProjectID, bool fIgnoreZip, ref string sTempFolder, ref string sError)
        {
            // D0856
            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];
            string sFilename = "";
            sError = "";

            clsProject Project = null;
            if (ProjectID == App.ProjectID | (ProjectID == -1 && App.HasActiveProject()))
                Project = App.ActiveProject;
            else
                Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList);
            if (Project == null)
                return "";
            ProjectID = Project.ID;
            // D1629
            if (!App.CanUserDoProjectAction(ecActionType.at_mlDownloadModel, App.ActiveUser.UserID, ProjectID, App.ActiveUserWorkgroup))
                return "";
            // D0857

            sFilename = FileService.File_CreateTempName();
            // D0132
            // D0127 ===
            bool fMasterDB = App.get_CanUserDoSystemWorkgroupAction(ecActionType.at_slManageAnyWorkgroup, App.ActiveUser.UserID) && CheckVar("db", "") == "master";
            // D0289 + D0368 + D0423 + D0479
            bool fAsZIP = CheckVar("zip", fMasterDB) & !fIgnoreZip;
            // D0240 + D023 + D1169

            // D0378 ===
            ECTypes.ECModelStorageType fType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase;
            // D0387
            string sExt = "";
            bool isPipeParams = false;
            // D0832
            bool isInvitations = false;
            // D1629
            bool isMailMerge = false;
            // D1828
            //Dim sTextContent As String = "" ' D0423 -D0425
            var ss = "ahps";
            switch (ss)
            {
                case "ahp":
                    fType = ECTypes.ECModelStorageType.mstAHPFile;
                    // D0387
                    sExt = FileService._FILE_EXT_AHP;
                    break;
                case "ahpx":
                    fType = ECTypes.ECModelStorageType.mstCanvasDatabase;
                    // D0387
                    sExt = FileService._FILE_EXT_AHPX;
                    break;
                case "ahps":
                    fType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase;
                    // D0387
                    sExt = FileService._FILE_EXT_AHPS;
                    break;
                // D0423 ===
                case "gridxml":
                    fType = ECTypes.ECModelStorageType.mstXMLFile;
                    sExt = FileService._FILE_EXT_XML;
                    fAsZIP = false;
                    break;
                // D0423 ==
                //  D0832 ===
                case "pipexml":
                    fType = ECTypes.ECModelStorageType.mstXMLFile;
                    sExt = FileService._FILE_EXT_XML;
                    fAsZIP = false;
                    isPipeParams = true;
                    break;
                // D0832 ==
                // D1629 ===
                case "invitations":
                case "tt_invitations":
                    // D3351
                    fType = ECTypes.ECModelStorageType.mstXMLFile;
                    sExt = ".xlsx";
                    fAsZIP = false;
                    isInvitations = true;
                    break;
                // D1629 ==
                // D1828 ===
                case "mailmerge":
                    fType = ECTypes.ECModelStorageType.mstXMLFile;
                    sExt = ".temp";
                    fAsZIP = true;
                    isMailMerge = true;
                    break;
                    // D1828 ==
            }
            // D0378 ==

            // D0368
            if (!fMasterDB)
            {

                // D2914 ===
                //If fType <> ECModelStorageType.mstCanvasStreamDatabase AndAlso Not Project.isValidDBVersion AndAlso Not Project.isDBVersionCanBeUpdated Then
                // D3645
                if (!Project.isValidDBVersion && !Project.isDBVersionCanBeUpdated)
                {
                    sError = string.Format("Error on download project '{2}': " + TeamTimeClass.ResString("msgWrongProjectDBVersion"), Project.DBVersion.GetVersionString(), ECTypes.GetCurrentDBVersion().GetVersionString(), StringFuncs.ShortString(Project.ProjectName, 65));
                    return "";
                }
                // D2914 ==

                if (Project.CheckGUID())
                    App.DBProjectUpdate(ref Project, false, "Init Project GUID");
                // D0892

                // D0419
                if (fType == ECTypes.ECModelStorageType.mstAHPFile | fType == ECTypes.ECModelStorageType.mstCanvasDatabase)
                {
                    Project = Project.Clone();
                    // D0151
                    // D0183 ===
                    if (Project.isLoadOnDemand)
                    {
                        Project.isLoadOnDemand = false;
                        Project.ResetProject();
                        Project.ProjectManager.ResourceAligner.Load(Project.ProjectManager.StorageManager.StorageType, Project.ProjectManager.StorageManager.ProjectLocation, Project.ProjectManager.StorageManager.ProviderType, Project.ID);  // D4857
                    }
                    // D0183 ==
                    if (fType == ECTypes.ECModelStorageType.mstAHPFile)
                        InfodocService.ParseInfodocsMHT2RTF(ref Project);
                    // D0151 + D0419 + D0459
                }
                var sFileConnString = "";
                switch (fType)
                {

                    case ECTypes.ECModelStorageType.mstAHPFile:
                    case ECTypes.ECModelStorageType.mstCanvasDatabase:
                        // D0378 + D0387
                        File.Copy((fType == ECTypes.ECModelStorageType.mstAHPFile ? Consts._FILE_PROJECTDB_AHP : Consts._FILE_PROJECTDB_AHPX), sFilename, true);
                        // D0368 + D0378 + D0387

                        Common.DebugInfo("Create mdb file and start save decision data", Common._TRACE_INFO);
                        // D0412

                        //Project.ProjectManager.StorageManager.Writer.WriteOnlyAllowedJudgmentsToAHP = CheckVar("skipunused", false);
                        // D3156

                        // Create ahp file
                        //Project.ProjectManager.StorageManager.GoalDefaultInfoDoc = Project.PipeParameters.ProjectPurpose;
                        // D0133 + D0174

                        //Dim tFileProvType As DBProviderType = DBProviderType.dbptODBC 'C0830
                        GenericDB.DBProviderType tFileProvType = GenericDB.DBProviderType.dbptOLEDB;
                        sFileConnString = clsConnectionDefinition.BuildJetConnectionDefinition(sFilename, tFileProvType).ConnectionString;
                        // D0368 + D0459 'C0830


                        string oldLocation = Project.ProjectManager.StorageManager.ProjectLocation;
                        //C0019
                        GenericDB.DBProviderType oldProviderType = Project.ProjectManager.StorageManager.ProviderType;
                        // D0346

                        Project.ProjectManager.StorageManager.StorageType = fType;
                        //C0028 + D0368 + D0378 + D0387

                        //Project.ProjectManager.StorageManager.ProjectLocation = IIf(fType = ECModelStorageType.mstAHPFile, sFilename, sFileConnString)   'C0028 + D0368 + D0378 + D0387 'C0830
                        Project.ProjectManager.StorageManager.ProjectLocation = sFileConnString;
                        //C0830

                        Project.ProjectManager.StorageManager.ProviderType = tFileProvType;
                        // D0346 + D0368

                        //C20070822 + D0130 'C0028
                        if (!Project.ProjectManager.StorageManager.Writer.SaveProject())
                        {
                            sError = "Can't save project. Please contact with administrator.";
                            //FetchAccess()
                        }

                        //C0051===
                        //Dim cnstr As String
                        //cnstr = "Driver={Microsoft Access Driver (*.mdb)};Dbq=" + sFilename + ";Uid=Admin;Pwd=;"
                        //Project.PipeParameters.Write(PipeStorageType.pstDatabase, sFileConnString, tFileProvType)    ' D0174 + D0329 + D0368 'C0390
                        switch (fType)
                        {
                            case ECTypes.ECModelStorageType.mstAHPFile:
                            case ECTypes.ECModelStorageType.mstAHPDatabase:
                                Project.PipeParameters.TableName = "MProperties";
                                Project.PipeParameters.PropertyNameColumnName = "PropertyName";
                                Project.PipeParameters.PropertyValueColumnName = "PValue";
                                break;
                            case ECTypes.ECModelStorageType.mstCanvasDatabase:
                                Project.PipeParameters.TableName = PipeParameters.PROPERTIES_DEFAULT_TABLE_NAME;
                                Project.PipeParameters.PropertyNameColumnName = PipeParameters.PROPERTY_NAME_DEFAULT_DB_COLUMN_NAME;
                                Project.PipeParameters.PropertyValueColumnName = PipeParameters.PROPERTY_VALUE_DEFAULT_DB_COLUMN_NAME;
                                break;
                        }

                        Project.PipeParameters.Write(Canvas.PipeParameters.PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID);
                        //C0390
                        Project.PipeParameters.PipeMessages.Save(Canvas.PipeParameters.PipeStorageType.pstDatabase, sFileConnString, tFileProvType, Project.ID);
                        //C0052 + D0174 + D0329 + D0368 'C0420
                        //C0051==

                        Project.ProjectManager.Attributes.ReadAttributes(Attributes.AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID);
                        Project.ProjectManager.Attributes.WriteAttributes(Attributes.AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1);

                        Project.ProjectManager.Attributes.ReadAttributeValues(Attributes.AttributesStorageType.astStreamsDatabase, oldLocation, oldProviderType, Project.ID, -1);
                        Project.ProjectManager.Attributes.WriteAttributeValues(Attributes.AttributesStorageType.astDatabase, sFileConnString, tFileProvType, -1, -1);

                        //C0783===
                        Project.ProjectManager.AntiguaDashboard.LoadPanel(ECTypes.ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID);
                        Project.ProjectManager.AntiguaRecycleBin.LoadPanel(ECTypes.ECModelStorageType.mstCanvasStreamDatabase, oldLocation, oldProviderType, Project.ID);

                        Project.ProjectManager.AntiguaDashboard.SavePanel(fType, sFileConnString, tFileProvType, -1);
                        Project.ProjectManager.AntiguaRecycleBin.SavePanel(fType, sFileConnString, tFileProvType, -1);
                        //C0783==

                        ECCore.MiscFuncs.ECMiscFuncs.WriteSurveysToAHPFile(oldLocation, oldProviderType, Project.ID, sFileConnString, tFileProvType);

                        Project.ProjectManager.StorageManager.ProviderType = oldProviderType;
                        // D0346
                        Project.ProjectManager.StorageManager.ProjectLocation = oldLocation;
                        //C0019
                        //Project.ProjectManager.StorageManager.StorageType = ECModelStorageType.mstCanvasDatabase 'C0028 'C0783
                        Project.ProjectManager.StorageManager.StorageType = ECTypes.ECModelStorageType.mstCanvasStreamDatabase;
                        //C0783
                        // D0127 ==

                        //clsDatabaseAdvanced.CopyCustomTablesFromDatabaseToFile(App.CanvasProjectsConnectionDefinition.DBName, sFilename, _AHP_EXTRATABLES_LIST, _AHP_EXTRATABLES_PREFIX)  ' D0166 + D0329 + D0330 + D0479 'C0617
                        Project.ResetProject();
                        // D0151
                        JetDatabasesService.FixRAcontraintsTableAfterDownload(sFilename);
                        //C0427

                        // D0950 ===
                        if (fType == ECTypes.ECModelStorageType.mstAHPFile && CheckVar("ra", false))
                        {
                            ECCore.MiscFuncs.ECMiscFuncs.UpdateMPropertiesWithRAFlag(sFileConnString, tFileProvType);
                        }
                        // D0950 ==

                        //Project.ProjectManager.StorageManager.Writer.WriteOnlyAllowedJudgmentsToAHP = false;
                        // D3156
                        break;

                    // D0378 ===
                    case ECTypes.ECModelStorageType.mstCanvasStreamDatabase:
                        // D0387

                        Common.DebugInfo("Start saving stream to the file...", Common._TRACE_INFO);
                        // D0412

                        //C0742===
                        //Dim FS As New IO.FileStream(sFilename, FileMode.OpenOrCreate, FileAccess.Write)
                        //Project.ProjectManager.StorageManager.Reader.LoadFullProjectStream_CanvasStreamDatabase(FS)
                        //FS.Close()
                        //C0742==

                        //C0742===
                        //Dim sm As clsStorageManager2 = Project.ProjectManager.StorageManager
                        //MiscFuncs.DownloadProject_CanvasStreamDatabase(sm.ProjectLocation, sm.ProviderType, sm.ModelID, sFilename)
                        ECCore.MiscFuncs.ECMiscFuncs.DownloadProject_CanvasStreamDatabase(Project.ConnectionString, Project.ProviderType, Project.ID, sFilename, App.isRiskEnabled);
                        // D2256
                        break;
                    //C0742==

                    // D0423 ===
                    case ECTypes.ECModelStorageType.mstXMLFile:
                        // D1828 ===

                        if (isMailMerge)
                        {
                            Common.DebugInfo("Prepare MailMerge ...", Common._TRACE_INFO);

                            string sTmpFolder = FileService.File_CreateTempName();
                            sTempFolder = sTmpFolder;
                            FileService.File_CreateFolder(ref sTmpFolder);

                            string SrcDOC = (CheckVar("tt", false) ? _MailMerge_DOC_TT : _MailMerge_DOC_AT);
                            // D1835
                            string ProjectName = FileService.GetProjectFileName(Project.ProjectName, SrcDOC, "MailMerge", ".docx");
                            // D1835
                            try
                            {
                                File.Copy(Consts._FILE_DATA + _MailMerge_Folder + "\\" + _MailMerge_MDB, sTmpFolder + "\\" + _MailMerge_MDB);
                                File.Copy(Consts._FILE_DATA + _MailMerge_Folder + "\\" + SrcDOC, sTmpFolder + "\\" + ProjectName);
                                File.Copy(Consts._FILE_DATA + _MailMerge_Folder + "\\" + _MailMerge_ReadMe, sTmpFolder + "\\" + _MailMerge_ReadMe);

                                _Files2Archive = new Chilkat.StringArray();
                                _Files2Archive.Append(sTmpFolder + "\\" + _MailMerge_MDB);
                                _Files2Archive.Append(sTmpFolder + "\\" + ProjectName);
                                _Files2Archive.Append(sTmpFolder + "\\" + _MailMerge_ReadMe);
                                // D1835
                            }
                            catch (Exception ex)
                            {
                                sError = "Can't copy required templates.";
                            }

                            if (string.IsNullOrEmpty(sError))
                            {
                                Common.DebugInfo("Save data to MDB file...", Common._TRACE_INFO);

                                GenericDB.DBProviderType ProviderType = GenericDB.DBProviderType.dbptODBC;
                                sFileConnString = clsConnectionDefinition.BuildJetConnectionDefinition(sTmpFolder + "\\" + _MailMerge_MDB, ProviderType).ConnectionString;

                                if (GenericDB.CheckDBConnection(ProviderType, sFileConnString))
                                {
                                    using (System.Data.Common.DbConnection dbConnection = GenericDB.GetDBConnection(ProviderType, sFileConnString))
                                    {
                                        // D2232
                                        dbConnection.Open();

                                        System.Data.Common.DbCommand oCommand = GenericDB.GetDBCommand(ProviderType);
                                        oCommand.Connection = dbConnection;

                                        string sExtraParams = "";
                                        string sTemplates = "";
                                        foreach (string sTempl in _MailMerge_ExtraColumns)
                                        {
                                            oCommand.CommandText = string.Format("ALTER TABLE {0} ADD COLUMN {1} TEXT", _MailMerge_TableName, sTempl.Replace("%%", ""));
                                            GenericDB.DBExecuteNonQuery(ProviderType, oCommand);
                                            sExtraParams += ", ?";
                                            sTemplates += sTempl + System.Environment.NewLine;
                                        }

                                        oCommand.CommandText = string.Format("INSERT INTO {0} VALUES (?{1})", _MailMerge_TableName, sExtraParams);

                                        // D1835 ===
                                        List<int> tUsers = new List<int>();
                                        foreach (string sID in CheckVar("uid_list", "").Split(','))
                                        {
                                            int ID = 0;
                                            if (int.TryParse(sID, out ID))
                                                tUsers.Add(ID);
                                        }
                                        // D1835 ==

                                        foreach (clsApplicationUser tUser in App.DBUsersByProjectID(ProjectID))
                                        {
                                            // D1835
                                            if (!tUser.CannotBeDeleted && (tUsers.Count == 0 || tUsers.Contains(tUser.UserID)))
                                            {
                                                oCommand.Parameters.Clear();
                                                oCommand.Parameters.Add(new System.Data.Odbc.OdbcParameter("ID", tUser.UserID));
                                                string sData = TeamTimeClass.ParseAllTemplates(sTemplates, tUser, Project);
                                                int idx = 0;
                                                var s = System.Environment.NewLine.ToString();
                                                foreach (string sLine in sData.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries))
                                                {
                                                    oCommand.Parameters.Add(new System.Data.Odbc.OdbcParameter("Param" + idx.ToString(), sLine.Trim()));
                                                    idx += 1;
                                                }
                                                int affected = GenericDB.DBExecuteNonQuery(ProviderType, oCommand);
                                            }
                                        }

                                        oCommand = null;
                                        dbConnection.Close();
                                    }
                                    Common.DebugInfo("Mail merge DB is done.", Common._TRACE_INFO);

                                }
                                else
                                {
                                    sError = "Can't open template DB for saving data";
                                }
                            }

                        }
                        else
                        {
                            // D1828 ==
                            // D1629 ===
                            if (isInvitations)
                            {
                                Common.DebugInfo("Save invitations...", Common._TRACE_INFO);

                                //Dim sInvites As String = String.Format("{1}{0}{2}{0}{3}" + vbCrLf, _CSV_DELIM, ResString("tblHyperlink"), ResString("tblUserEmail"), ResString("tblUsername"))
                                WriteInvitationXLSX(sFilename, Project.ID, CheckVar("type", "").ToLower() == "tt_invitations");
                                // D3351
                                List<clsApplicationUser> tUsers = App.DBUsersByProjectID(Project.ID);
                                if (tUsers != null)
                                {
                                    foreach (clsApplicationUser tUser in tUsers)
                                    {
                                        if (!tUser.CannotBeDeleted)
                                        {
                                            string sName = tUser.UserName;
                                            if (sName.Contains(_CSV_DELIM) || sName.Contains("\""))
                                                sName = string.Format("\"{0}\"", sName.Replace("\"", "\"\""));
                                            // D1814
                                            //           sInvites += String.Format("{1}{0}{2}{0}{3}" + vbCrLf, _CSV_DELIM, ParseAllTemplates(_TEMPL_URL_EVALUATE, tUser, Project), tUser.UserEmail, sName)
                                        }
                                    }
                                }
                                //My.Computer.FileSystem.WriteAllText(sFilename, sInvites, False)

                            }
                            else
                            {
                                // D1629 ==
                                // D0832 ===
                                if (isPipeParams)
                                {
                                    Common.DebugInfo("Save pipe params...", Common._TRACE_INFO);
                                    Project.PipeParameters.Write(Canvas.PipeParameters.PipeStorageType.pstXMLFile, sFilename, App.DefaultProvider, Project.ID);
                                }
                                else
                                {
                                    // D0832 ==
                                    Common.DebugInfo("Start getting Grid XML...", Common._TRACE_INFO);
                                    // D0425 ===
                                    string sXML = Project.ProjectManager.mXML.GetDataGrid1XML(Project.ProjectManager, Project.ProjectManager.UserID);
                                    //C0453
                                    StreamWriter f = new StreamWriter(sFilename, false, System.Text.Encoding.Unicode);
                                    f.Write(sXML);
                                    f.Close();
                                    f = null;
                                    // D0425 ==
                                }
                                // D0423 ==
                            }
                        }
                        break;
                }
                // D0378 ==

                Common.DebugInfo("File is saved", Common._TRACE_INFO);
                // D0412


            }
            else
            {
                // Create MasterDB file
                // D0108 ===
                File.Copy(Consts._FILE_SQL_EMPTY_MDB, sFilename, true);
                // D0792
                string sConnString = clsDatabaseAdvanced.GetConnectionString(App.CanvasMasterConnectionDefinition.DBName, GenericDB.DBProviderType.dbptODBC);
                // D0330 + D0412 + D0459
                // D0479
                if (!clsDatabaseAdvanced.CopyDatabaseToJet(sConnString, sFilename, ref sError))
                {
                    sError = string.Format("<span title='{0}'>Can't create project copy</span>", StringFuncs.SafeFormString(sError));
                    // D0792
                    //FetchAccess() ' D0130
                }
                // D0108 ==

            }

            // D0412 ===
            if (string.IsNullOrEmpty(sError) && DoCompactJet && (fType == ECTypes.ECModelStorageType.mstAHPFile | fType == ECTypes.ECModelStorageType.mstCanvasDatabase))
            {
                JetDatabasesService.CompactJetDatabase(sFilename, ref sError);
            }
            // D0412 ==

            //Dim sFName As String = IIf(fMasterDB, String.Format("{0}.mdb", App.Options.CanvasMasterDBName), Project.FileName)   ' D0108 + D0315
            string sFName = (fMasterDB ? string.Format("{0}.mdb", App.Options.CanvasMasterDBName) : "");
            // D0108 + D0315 + D1193
            // D0180 ===
            if (!fMasterDB)
            {
                // D1629 ===
                string sExtraName = "";
                if (isPipeParams)
                    sExtraName = " (settings)";
                if (isInvitations)
                    sExtraName = " (links)";
                if (isMailMerge)
                    sExtraName = " (MailMerge)";
                // D1828
                sFName = FileService.GetProjectFileName(sFName, Project.ProjectName + sExtraName, Project.Passcode, sExt);
                // D0346 + D0378 + D1629
                // D1629 ==
            }
            // D0180 ==

            // D0856 ===
            if (string.IsNullOrEmpty(sTempFolder))
            {
                sTempFolder = FileService.File_CreateTempName();
                FileService.File_Erase(sTempFolder);
                if (!FileService.File_CreateFolder(ref sTempFolder))
                    sTempFolder = "";
            }

            if (!string.IsNullOrEmpty(sTempFolder))
            {
                try
                {
                    if (File.Exists(sTempFolder + "\\" + sFName))
                        sFName = Project.ID.ToString() + " _" + sFName;
                    sFName = sTempFolder + "\\" + sFName;
                    File.Copy(sFilename, sFName);
                    FileService.File_Erase(sFilename);
                    sFilename = sFName;
                }
                catch (Exception ex)
                {
                    sFName = sFilename;
                }
            }
            // D0856 ==

            // D0240 ===
            if (fAsZIP && string.IsNullOrEmpty(sError))
            {
                string sZIPFile = Path.ChangeExtension(sFilename, (CheckVar("ext", "").ToLower() == "ahpz" ? FileService._FILE_EXT_AHPZ : FileService._FILE_EXT_ZIP));
                Chilkat.StringArray FilesList = new Chilkat.StringArray();
                // D1828 ===
                if (_Files2Archive == null)
                {
                    FilesList.Append(sFilename);
                }
                else
                {
                    FilesList = _Files2Archive;
                }
                // D1828 ==
                if (ArchivesService.PackZipFiles(FilesList, sZIPFile, ref sError))
                {
                    sError = "";
                }
                else
                {
                    fAsZIP = false;
                }

                if (File.Exists(sZIPFile))
                {
                    FileService.File_Erase(sFilename);
                    sFilename = sZIPFile;
                }
            }
            // D0240 ==

            if (!string.IsNullOrEmpty(sError))
            {
                if (!string.IsNullOrEmpty(sFilename))
                    FileService.File_Erase(sFilename);
                if (!string.IsNullOrEmpty(sTempFolder))
                    FileService.File_DeleteFolder(sTempFolder);
                sFilename = "";
            }

            return sFilename;
        }

        public void WriteInvitationXLSX(string Filename, int ProjectID, bool fIsTeamTime)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            clsProject Project = null;
            if (ProjectID == App.ProjectID | (ProjectID == -1 && App.HasActiveProject()))
            {
                Project = App.ActiveProject;
            }
            else
            {
                Project = clsProject.ProjectByID(ProjectID, App.ActiveProjectsList);
            }

            if (Project != null)
            {
                List<clsApplicationUser> Users = App.DBUsersByProjectID(ProjectID);
                try
                {
                    //erase the file if it already exists
                    try
                    {
                        if (File.Exists(Filename))
                        {
                            File.Delete(Filename);
                        }
                    }
                    catch
                    {
                    }
                    dynamic MasterXLSX = System.AppDomain.CurrentDomain.BaseDirectory + "App_Data/Invitations/Master.xlsx";
                    System.IO.File.Copy(MasterXLSX, Filename);
                    dynamic fi = new FileInfo(Filename);
                    using (ExcelPackage package = new ExcelPackage(fi))
                    {
                        ExcelWorksheet Invitations = package.Workbook.Worksheets.Where(x => x.Name == dInvitations).First();
                        ExcelWorksheet Styles = package.Workbook.Worksheets.Where(x => x.Name == dStyles).First();
                        int row = 1;
                        if (Users != null)
                        {
                            foreach (clsApplicationUser tUser in Users)
                            {
                                if (!tUser.CannotBeDeleted)
                                {
                                    row += 1;
                                    string sName = tUser.UserName;
                                    if (sName.Contains(_CSV_DELIM) || sName.Contains("\""))
                                        sName = string.Format("\"{0}\"", sName.Replace("\"", "\"\""));
                                    string URL = TeamTimeClass.ParseAllTemplates((fIsTeamTime ? ECWeb.Options._TEMPL_URL_EVALUATE_TT : ECWeb.Options._TEMPL_URL_EVALUATE), tUser, Project);
                                    // D3351
                                    Invitations.Cells[row, 1].Value = sName;
                                    Invitations.Cells[row, 2].Value = tUser.UserEMail;
                                    Invitations.Cells[row, 3].Value = URL;
                                    Invitations.Cells[row, 3].StyleName = sStyleHyperLink;
                                }
                            }
                        }
                        //==============cleanup========================
                        Styles.Hidden = eWorkSheetHidden.VeryHidden;
                        package.Workbook.Worksheets[dInvitations].View.TabSelected = true;
                        package.Save();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error creating Invitation XLSX");
                }

            }
        }



        public void RawResponseEnd()
        {
            if (Response.IsClientConnected)
            {
                Response.Flush();
                Response.Close();
                Response.End();
            }
        }

        public void RawResponseStart()
        {
            HttpContext context = HttpContext.Current;
            Common.DebugInfo("Clear headers for SSL raw data");
            context.Response.Buffer = true;
            context.Response.Clear();
            context.Response.ClearHeaders();
            context.Response.ClearContent();
            context.Response.Cache.SetCacheability(HttpCacheability.Private);
            context.Response.CacheControl = "private";
            context.Response.Cache.SetLastModified(DateTime.Now);
        }


        public string GetLink(int ProjectID)
        {
            var App = (clsComparionCore)HttpContext.Current.Session["App"];
            var Passcode = App.DBProjectByID(ProjectID).Passcode.ToString();
            return Passcode;
        }

        public void DownloadDecision(int ProjectID, string ext)
        {
            //var file = new FileInfo("C:\\Users\\Day Shift\\AppData\\Local\\Temp\\tmp8D10.tmp\\Eileen 2016 Presidential Election III abbreviated Single (7_18_2016 12_37 PM).ahps");
            //Response.ClearContent();

            //// LINE1: Add the file name and attachment, which will force the open/cance/save dialog to show, to the header
            //Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name));

            //// Add the file size into the response header
            //Response.AddHeader("Content-Length", file.Length.ToString());

            //// Set the ContentType
            //Response.ContentType = "application/octet-stream";

            //// Write the file into the response (TransmitFile is for ASP.NET 2.0. In ASP.NET 1.1 you have to use WriteFile instead)
            //Response.TransmitFile(file.FullName);

            //// End the response
            //Response.End();


            HttpContext context = HttpContext.Current;
            var App = (clsComparionCore)context.Session["App"];


            // D0856 ===
            string sError = "";
            string sTempFolder = "";
            string sFilename = "";

            if (CheckVar("mode", "").ToLower() == "multi")
            {
                string[] sIDs = CheckVar("list", "").Trim().ToLower().TrimEnd(Convert.ToChar(",")).Split(Convert.ToChar(","));
                Chilkat.StringArray sFileList = new Chilkat.StringArray();
                string sArcName = "";
                foreach (string sID in sIDs)
                {
                    int ID = -1;
                    if (int.TryParse(sID, out ID))
                    {
                        sFilename = PrepareDownloadFile(ID, false, ref sTempFolder, ref sError);
                        if (string.IsNullOrEmpty(sError) && !string.IsNullOrEmpty(sFilename) && File.Exists(sFilename))
                        {
                            sFileList.Append(sFilename);
                            if (sArcName.Length < 50)
                                sArcName += (string.IsNullOrEmpty(sArcName) ? "" : "_") + Path.GetFileNameWithoutExtension(sFilename);
                        }
                    }
                }
                sFilename = "";
                if (sFileList.Count > 0)
                {
                    if (string.IsNullOrEmpty(sArcName))
                        sArcName = "Comparion_Projects";
                    sArcName += FileService._FILE_EXT_ZIP;
                    if (string.IsNullOrEmpty(sFilename))
                        sFilename = Path.GetTempPath();
                    else
                        sFilename = sTempFolder + "\\";
                    if (File.Exists(sFilename + sArcName))
                        sArcName = DateTime.Now.ToString("yyMMddHHmmss") + "_" + sArcName;
                    sFilename += sArcName;
                    if (ArchivesService.PackZipFiles(sFileList, sFilename, ref sError))
                        sError = "";
                    else
                        sFilename = "";
                }


            }
            else
            {
                //string sID = CheckVar("id", "-1")
                int ID = ProjectID;
                //if (!int.TryParse(sID, out ID))
                //    ID = -1;
                if (ID == -1 && !App.HasActiveProject())
                    RawResponseEnd();
                sFilename = PrepareDownloadFile(ID, false, ref sTempFolder, ref sError);

            }

            bool FResult = (string.IsNullOrEmpty(sError)) && !string.IsNullOrEmpty(sFilename) && File.Exists(sFilename);
            // D0856 ==

            if (FResult)
            {
                // Generate downloadable content
                //RawResponseStart();
                //// D0041

                ////Dim sDownloadName As String = HttpUtility.UrlPathEncode(Path.GetFileName(sFilename))    ' D1191
                //string sDownloadName = Path.GetFileName(sFilename);
                //// D1191 + D3221

                //context.Response.AppendHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", sDownloadName));
                //// D0856 + D1191

                //// D1629 ===
                //string sContentType = "application/octet-stream";
                //if (CheckVar("type", "").ToLower() == "invitations")
                //    sContentType = "application/vnd.ms-excel";
                //if (Path.GetExtension(sFilename).ToLower() == FileService._FILE_EXT_ZIP)
                //    sContentType = "application/zip";
                //context.Response.ContentType = sContentType;
                // D0240 + D0470 + D0856
                // D1629 ==

                // D0010 enforced operation with file copy
                //Dim sFilenameTemp As String = sFilename + ".tmp"
                //DebugInfo("Create copy of file")
                //My.Computer.FileSystem.CopyFile(sFilename, sFilenameTemp, True)
                long fileLen = new FileInfo(sFilename).Length;
                // D0425
                //If sTextContent <> "" Then fileLen = sTextContent.Length Else fileLen = My.Computer.FileSystem.GetFileInfo(sFilenameTemp).Length ' D0423

                Response.AddHeader("Content-Length", Convert.ToString(fileLen));

                Common.DebugInfo(string.Format("Start transferring for {0} bytes", fileLen));
                if (fileLen > 0)
                {
                    // D0423 -D0425 ===
                    //If sTextContent <> "" Then
                    //    Response.Write(sTextContent)
                    //Else
                    // -D0425 ==

                    // D1169 ===
                    FileStream fs = new FileStream(sFilename, FileMode.Open, FileAccess.Read);
                    BinaryReader r = new BinaryReader(fs);
                    int total = 0;

                    while (total < fileLen)
                    {
                        byte[] Buff = r.ReadBytes(BuffSize);
                        if (Buff.GetUpperBound(0) >= 0)
                        {
                            total += Buff.GetUpperBound(0) + 1;
                            context.Response.BinaryWrite(Buff);
                        }
                    }

                    r.Close();
                    fs.Close();

                    //Response.BinaryWrite(My.Computer.FileSystem.ReadAllBytes(sFilename)) ' D0378

                    // D1169 ==


                    //End If '-D0425
                    // D0423 ==
                }

                var file = new FileInfo(sFilename);
                Response.ClearContent();

                // LINE1: Add the file name and attachment, which will force the open/cance/save dialog to show, to the header
                Response.AddHeader("Content-Disposition", String.Format("attachment; filename={0}", file.Name));

                // Add the file size into the response header
                Response.AddHeader("Content-Length", file.Length.ToString());

                // Set the ContentType
                Response.ContentType = "application/octet-stream";

                // Write the file into the response (TransmitFile is for ASP.NET 2.0. In ASP.NET 1.1 you have to use WriteFile instead)
                Response.TransmitFile(file.FullName);

                // End the response

                App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "", string.Format("Filename: {0}; Size: {1}", Path.GetFileName(sFilename), fileLen));
                // D0496 + D0856
                Response.End();
                //File_Erase(sFilenameTemp)
                FResult = true;
            }

            //FileService.File_Erase(sFilename);
            //if (!string.IsNullOrEmpty(sTempFolder))
            //    FileService.File_DeleteFolder(sTempFolder);
            // D0857

            // D1296 ===
            if (!FResult)
            {
                App.DBSaveLog(dbActionType.actDownload, dbObjectType.einfProject, App.ProjectID, "Error on download", sError);
                if (sError.ToLower().StartsWith("chilkat"))
                    sError = "Error on zip file. Contact with system administrator";
                // D1297
                //lblError.Text = sError; =======================> This is the error here
            }
            // D1296 ==
            // Flush and close stream instead IIS sending HTML-page
            // D0363 ===
            //WebClient myWebClient = new WebClient();
            //myWebClient.DownloadFile()

            if (context.Response.IsClientConnected)
            {
                //if (FResult)
                //    RawResponseEnd();
                // D0275 + D0363
            }
            // D0363 ==
            //return FResult;
        }
    }
}