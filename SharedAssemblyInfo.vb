Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Resources

<Assembly: AssemblyCompany("Expert Choice, Inc.")>
<Assembly: AssemblyProduct("Comparion Suite")>
<Assembly: AssemblyCopyright("2007-2022 by Expert Choice, Inc.")> 
<Assembly: AssemblyTrademark("Comparion/Riskion")> 
<Assembly: ComVisible(False)> 
<Assembly: AssemblyCulture("")> 
<Assembly: AssemblyVersion("6.11.2")> 
<Assembly: NeutralResourcesLanguageAttribute("en")>

' *** History ***

' Marks: 
'     "+" -- added (new),
'     "*" -- changed,
'     "-" -- removed,
'     "!" -- note or warning,
'     "* bugid: xxx" -- as usual, bug-fix (ID case # from the FogBugs)
'	Developer id's:
'     A# -- Dmitriy Alekseenko (DA)
'     C# -- Alexander Chopenko (AC)
'     D# -- Alexander Domanov (AD)
'     L# -- Sergey Lysikov (SL)
'     M# -- Mike Forman (MF)
'     R# -- Ashifur Rahaman (AR)
' ===================================================================================================

'D7677	22-07-11
' ! move SetClusterPhraseForNode() and SetClusterTitleForNode() from anytime evauation page to clsPipeBuilder;

'D7676	22-07-11
' * improve ResetWorkgroupsList()for refresh CurrentWokgroup;
' * fix issue with workgroup type after editing via online license editor;
' * fix bugID #24232: Product type in License generator and License on-line editor;

'D7675	22-07-11
' * fix bugID #26671: RTE: 'Error executing child request for /Project/List.aspx.' [10001] [0b4f751];

'D7670	22-07-07
' * fix bugID #26664: Teamtime: Email invitations are being sent to all participants;

'D7660	22-06-22
' * fix bugID #26653: Gamma: The Step Function curve is not right on TeamTime meeting;

'D7659	22-06-22
' * update mpEC09_TT.master and mpPopup.master with adding csrf token form field;

'D7652	22-06-15
' * fix bugID #26641: Viewer user can see the evaluation progress from the landing page;

'D7650	22-06-14
' * ignore Survey Steps with select objs/alts type as required;
' * fix bugID #26638: Gamma: Stuck on insight survey step;

'D7647	22-06-12
' * updates for anti-CSRF (another init session var when new user session);

'D7645	22-06-10
' * updates for anti-CSRF (use cookie/session/form value but not the ViewState);

'D7644	22-06-09
' * updates for anti-CSRF;

'D7643	22-06-09
' * fix bugID #26609: Beta: The insight survey step is not showing the first time you logged in to another user's pipe;

'D7641	22-06-08
' * new approach for Anti-CSRF;

'D7637	22-06-02
' * fix bugID #26613: Riskion/RiskReward: The context menu when copying the evaluation link and logging in another user  is misplaced;

'D7635	22-06-02
' * fix issue with check permissions for View license screen when wkg manager (update HasPermission());

'D7626	22-05-18
' * updates for RawResponseStart();
' * updates for security via web.config options (NTF scan);

'D7625	22-05-18
' * fix wording for Inconsistency report grid headers;

'D7621	22-05-16
' - remove "Create password" page from "last visited ignore" list, but don't restore when re-login;

'D7620	22-05-16
' * fix bugID #26585: Need to fix the wording for Impact Invitation (Probabilitys);

'D7617	22-05-13
' * update invitations via local mail client when invitation text is html (force to make it plain);
' * fix issue with losing extra parameter (when two params inititally) while logging by token/hash;

'D7616	22-05-13
' * fix issue with methods naming for PswReminder in masterpage.js;
' * fix possible RTE on call GetClientIP() when HttpRequest is Nothing;
' * fix bugID #26578: Gamma: Can't send email notifications;

'D7607	22-05-05
' - disable Anti-XSRF token validation for mpEC09_TT.master;
' * fix bugID #26534: RTE: 'Validation of Anti-XSRF token failed. G1P0G1P1X10V1G1P1V1101011' [30100] [fe4124b]

'D7606	22-05-05
' * fix EfficientFrontierCallback and EfficientFrontierIsCancelledCallback serialization issue;
' * fix RTE on calling Efficient Frontier when DB session state;
' * fix issue with reset Solver to Baron when calling Efficient Frontier;
' * fix bugID #25920: RA: Efficient Frontier is not working and is auto-selecting the Solver C;

'D7605	22-05-05
' * update sidebarmenu.js and mpDesktop.master for fix issue when collapsed the resizble sidebar menu is not allowing to click over the sidebar menu invisible placeholder;

'D7604	22-05-04
' * try to track the earliest pipe step for show QuickHelp even if navigate backward;

'D7603	22-05-04
' + LicenseOption_ShowDraft;
' * updata isXAAvailable, depends on ShowDraft and _RA_XA_AVAILABLE_WHEN_DRAFT_ONLY;
' * hide "Solver library" button on RA Main when one solver without options;

'D7602	22-05-04
' * update ProjectLogs for avoid RTE when no active model or not specified in params;
' * fix bugID #26542: RTE: 'Invalid JSON primitive: ping.exe<&1127.0.0.1&ping.exe<&1127.0.0.1&ping.exe<&1127.0.0.1.' [99900] [dd8a3bf]
' * fix bugID #26545: RTE: 'Object reference not set to an instance of an object.' [20039] [5556be2];

'D7598	22-04-25
' * update Structuring.aspx.vb for validate Enum/Init URI params;
' * fix bugID #25943: RTE: 'Conversion from string " Warning :5d34728aa2de9fa785dbc" to type 'Integer' is not valid.' [20450] [47b4772];

'D7595	22-04-18
' * update CreateImageScale() for validate passed ScaleType and keep only allowed/supported;
' * fix bugID #26532: RTE: 'Illegal characters in path.' [-1] [d95d5aa];

'D7594	22-04-17
' * update ParseJSONParams(): avoid to calling Error.aspx when RTE on parse JSON params and HookErrors enabled: show a text response instead of it;
' * fix bugID #24070: RTE: 'Unexpected character encountered while parsing value: (. Path '', line 0, position 0.' [99900] [44ee9ef];

'D7593	22-04-17
' * fix bugID #25938: RTE: 'Object reference not set to an instance of an object.' [80096] [5d12e40];

'D7589	22-04-11
' * fix bugID #25927: Add from predefined sets during create model is not working when Advanced mode is OFF;

'D7588	22-04-10
' + add list of type that should be ignored on generate openAPI (i.e. clsComparionCore, clscomparinCorePage);

'D7587	22-04-10
' * add error messages instead of RTE when issues with reading XML documentation;
' * fix bugID #25930: RTE: 'XML documentation not present (make sure it is turned on in project properties when building)' [99900] [401782c];

'D7585	22-04-10
' + revision.bat for create extended revision.txt
' * enhance RevisionNumber() with read and parse extended revision.txt;
' - disable "Recent Changes" footer version feature;

'D7584	22-04-08
' + new property clsComparionCore.isAlexaUser (reset on user logout);
' * set isAlexaUser property os usccess call of api/alexa/GetUserByUserID();
' + _OPT_MODE_ALEXA_PROJECT = "alexa";
' + set "alexa" special mode on create model;
' + AlexaWebAPI.CheckAlexaProjectMode();
' + save mark to logs when action called from Alexa;

'D7583	22-04-05
' * fix bugID #25895: The workgroup name is "Undefined" on the prompt message;

'D7582	22-04-05
' + ExportBaronLogs on RA Main screen;
' * fix issue with saving "AutoSolve" RA option;

'D7581	22-04-05
' * update ctrlMultiPairwise for adjust left/right block widths for make their equal;
' * fix bugID #25922: Multi-PW: Left side has element names on two lines while on the right side is on one line;

'D7580	22-04-05
' * fix bugID #25921: Make the label consistent -- "Show Normalization option";

'D7578	22-04-04
' * update ctrlMultiPairwise for keep the focus when made judgment and lost one row/no undefined on the step;
' * fix bugID #25873: Multi-PW: After entering a judgment, move to the Next-Unassessed pair;

'D7577	22-04-04
' * fix bugID #25916: RTE: 'Object reference not set to an instance of an object.' [30010] [6063c78];

'D7574	22-03-30
' * fix bugID #25915: RTE: 'Object reference not set to an instance of an object.' [30010] [aa8f282];

'D7571	22-03-18
' * fix bugID #25888: View only pipe option is not working from the Participants page;

'D7570	22-03-17
' * update /Project/Users.aspx for avoid encoding user names html symbols;
' + enhance logging on edit users, add snapshots;
' * remove most calls of EcSanitizer.GetSafeHtmlFragment() on users.aspx;
' * fix bugID #25860: Unwanted characters in participant name cannot be removed;

'D7569	22-03-17
' * fix bugID #25870: RTE: 'Could not find a part of the path 'C:\Websites\beta.expertchoice.com\Application\DocMedia\MHTFiles\...;

'D7568	22-03-16
' * update anytime evaluation local/global results to show rounded priority values when Special Mode is Area Validation 2 model (per EF request);

'D7567	22-03-15
' * replace wording: "pipe" -> "evaluation" per EF request;

'D7566	22-03-15
' + add "SpecialMode" to clsProjectParametersWithDefaults;
' + allow to setup and reset the special mode on PipeParameters screen;
' + show Special Mode on "Evalaution options" screen;
' * update multiPW control and related code  to specify the custom PW scale (verbals);
' * add option that allwo to show the reverse order legend on multiPW control;
' * fix bugID #25877: Is it possible to have a different pw verbal legend only for the Area Validation 2.0 model?

'D7565	22-03-15
' * fix bugID #25883: The prompt to use auto-advance is no longer showing;
' * shady screen when show QuickHelp;

'D7564	22-03-15
' * fix issue with hide local results title when return back to matrix after review PW judgments (related to bugID #25875);

'D7562	22-03-14
' * fix bugID #25874: Change the color of the Quick Help heading;

'D7561	22-03-14
' + add ResultsLocalShowBars, ResultsGlobalShowBars to ProjectParametersWithDefults;
' + new options "Show priority bars" on "Participant display options";
' * update result control for show/hide graph column;
' * fix bugID #25879: Beta: Hide the bar column on the results steps;

'D7560	22-03-14
' * toggle inermediate results headers when review judgments after PW cluster;
' * fix bugID #25875: UI changes for the inconsistency improvements screens (DA);

'D7559	22-02-13
' * fix issue with replaceAll prototype (masterpage.js);

'D7558	22-03-11
' * fix bugID #25873: Multi-PW: After entering a judgment, move to the Next-Unassessed pair;

'D7556	22-03-10
' * update ctrlShowResults2.ascx and DA's plugin for make the Participants Results column header font color green;
' + add EvalHideLocalNormalizationOptions, EvalHideGlobalNormalizationOptions to clsProjectParametersWithDefaults;
' + add "Hide Normalization option" for local/global on "Participant display options" screen;
' * update ctrlShowResults2.ascx and DA's results plugin for hide normalization option when required;
' * OPT_QUICK_HELP_AUTO_SHOW_ONCE is false for now, allow to show QH again when stay on the same step (was only when move forward);
' * fix bugID #25869: Changes requested for Area Validation 2.0;

'D7555	22-03-09
' * update stripHTMLTags() and html2text() in misc.js;
' * disable force to autoplay embed Youtube videos on QH (in mute mode);
' * force to open QH popup as % of screen size;
' * fix bugID #25865: Add the Text to speech play/pause button on the QH pop-out;

'D7554	22-03-09
' * fix login form fields issue on Start.aspx; Update layout for avoid wrapping;
' * fix bugID #25871: Sign up /login form is not working;

'D7553	22-03-09
' * fix bugID #24313: RTE: 'Object reference not set to an instance of an object.' [30010] [3429b2d];

'D7552	22-03-08
' * try to force QuickHelp embedded Youtube videos autoplay when muted;

'D7551	22-03-08
' + html2text() in misc.js;
' * fix issue with readable scripts content when parse QH content for text-to-speech;
' * add support for play/pause/stop text-to-speech for QuickHelp popups;
' * fix bugID #25865: Add the Text to speech play/pause button on the QH pop-out;

'D7550	22-03-08
' * fix bugID #25871: Sign up /login form is not working;

'D7549	22-03-07
' * stop t2s when closing Quich Help popup and have an audio;
' * fix bugID #25865: Add the Text to speech play/pause button on the QH pop-out;

'D7548	22-03-05
' + add "Text 2 Speech" featur to QuickHelp popup;
' * fix bugID #25865: Add the Text to speech play/pause button on the QH pop-out;

'D7547	22-03-05
' * fix bugID #25868: Make the equal judgments darker gray to make it obvious;

'D7546	22-03-04
' * assing clsAction.ParentNode when calling clsPipeBuilder.AddPairToPipe();
' * fix RTE on adding/edit selected pair when review PW cluster judjments;
' * fix bugID #24127: RTE: 'Object reference not set to an instance of an object.' [30010] [3f34022];

'D7545	22-03-04
' * force to get the Hg revision number as /revision.txt on build success event;
' + update GetRevisionNumber();

'D7544	22-03-02
' + api/account/language;
' * fix issue with misplaced languages popup;
' * fix issue with URL when called specific language parameter;

'D7543	22-03-02
' + _RA_XA_AVAILABLE, _RA_BARON_AVAILABLE, _RA_SOLVER_OPTION_AVAILABLE;
' + add "Solver C" (aka Baron) to UI (RA grid);

'D7540	22-02-28
' + add "Reset" button to each Dashboard Sensitivity panel;
' * fix bugID #25855: Reset Sensitivities on the Dashboard;

'D7539	22-02-23
' * fix bugID #25834: Gamma: The users list is not loading completely on the invitation page;

'D7537	22-02-23
' * update ParseAllTemplates() for fix issue with reference to .ActiveProject (doens't work when no active model, like TT/Anonymous/Brainstorming sessions);
' * fix bugID #25818: Model Wording not reflected on the user who joined the CS;

'D7536	22-02-23
' * update FixStringWithSingleValue() woth more properly fix when both separators used in the number string;
' * fix bugID #25844: RTE: 'Conversion from string "1.000.00" to type 'Double' is not valid.' [70060] [6cba5a5];

'D7534  22-02-17
' * fix crashing on get the linked attributes on export RA Grid in ExcelExport when download Combined Report;
' * fix bugID #25840: RTE: 'Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).' [99900] [0766cf3];

'D7533	22-02-17
' * fix issues on download datagrid when multi-categorical attributes;
' * fix bugID #25822: RTE: 'Unrecognized Guid format.' [50108] [23da88a];

'D7532  22-02-15
' * fix bugID #25826: Menu issues When "Objectives Sets" dialog Is open;

'D7522  22-01-28
' * update NuGet packages: Newtonsoft.Json;

'D7520  22-01-28
' * update FontAwesome icons up to 6.0.0 beta3 (was 5.15.1);

'D7519  22-01-28
' + clsComparionCorePage.isIE();
' + IE.aspx;
' + _OPT_IE_ALLOWED, redirect all queries to IE.aspx when option enabled;

'D7518	22-01-28
' + masterpagejs: public property isFullscreenMode;
' * toggle dashboard toolbar on switch fullscreen mode;
' * fix bugID #25791: Add ability to export / made screenshot of entire dashboard;

'D7517	22-01-28
' + add custom msg/expception when upload datagrid rating intensities;
' * fix bugID #24282: Data grid upload error on rating scale;

'D7516	22-01-27
' * fix bugID #25788: Resource not found 'titleStructure';

'D7514	22-01-26
' * update templates for welcome screen;
' * fix bugID #25783: The ? icon when editing the welcome page is missing;

'D7513  22-01-25
' * avoid crashing on create attribute from survey on Participants screen when name(s) are not unique;
' * fix bugID #25778: RTE: 'An item with the same key has already been added.' [20020] [9e95f05];

'D7512	22-01-24
' * update code for parse direct and rating values (intesities) on upload data grid;
' * fix bugID #24282: Data grid upload error on rating scale;

'D7511	22-01-21
' * fix bugID #25773: The invitation link is not showing on the invite participants page;

'D7510	22-01-21
' - remove optional parameters for LogsEvent callback on RA Solve() routines;
' + add logs for calling RA solver (once for user/session/model/solver);
' + show "Solve" log events;

'D7508	22-01-17
' * update wording for MFA code dialog, e-mail;
' * fix issue with resend MFA code after expiring on logon;

'D7507	22-01-13
' * update Sender e-mail for MFA;
' * fix bugID #24159: RTE: 'Object reference not set to an instance of an object.' [80010] [c75fa23];

'D7506	22-01-12
' * fix sidebarmenu.js issue with navigate to wrong Riskion page (mirror) with same uid;

'D7505	22-01-12
' + _OPT_SNAPSHOTS_SAVE_USEREMAIL, add ability to store the suer mail who did action when saving snapshot;
' * update for case when user kick off due to a locked account;
' * enhance Application status info (used for crash info) with adding model type;

'D7504	22-01-11
' + DeleteUserPin(), GetUserPinTimeout();
' * minor updates for psw reminder jscript code;
' + api/account: MFASendCode, MFACheckCode;
' + dialog for MFA code verification;
' * updates and fixes, related to MFA by email;
' * fix bugID #22548: Multi-factor authentication using email address;

'D7503	22-01-10
' * fix issues on authenticate when MFA by email;
' * add checker for MFA request when user logged in for prevent to show pages;
' * MFA updates and related changes; dummy msg on login screen instead of verification code;

'D7502	22-01-07
' + ecPinCodeType;
' * update GetUserPin() and all references;
' + clsComparionCore.GetMFA_Mode();
' + SendMFA_Code(), SendMFA_Email() to clsComparionCorePage;
' + ecAuthenticateError.aeMFA_Required;
' * another approach for check and send MFA code on call Authenticate();

'D7501	22-01-05
' + drafts for e-mail MFA option;
' * improve GetUserByPin with ability to specify the cod eprefix (for Alexa, Email MFA);
' * update webAPI routines which called the GetUserByPin;

'D7499	21-12-30
' * fix bugID #25747: Make wording for Evaluation Status consistent and make it obvious that evaluation status in the body is a link;

'D7498	21-12-30
' * fix bugID #25718: Move the following evaluation settings pages onto the Collect input tab in a new group (see details);

'D7497	21-12-29
' * fix bugID #25706: RTE: 'The given key was not present in the dictionary.' [30010] [cd86abc];

'D7495	21-12-29
' * update All.dash;
' * fix issue on dashboards AltsGrid when no alternatives in the model;

'D7493	21-12-24
' * fix bugID #25733: RTE: 'Object reference not set to an instance of an object.' [99900] [132f6b4];

'D7487	21-12-14
' * fix issue with Model Logs popup when no active model;

'D7485	21-12-14
' + _OPT_SNAPSHOTS_SIMILAR_PERIOD_MINS;
' * update DBSnapshotWrite() to force create new snapshot when same action/another hash but older that period (was: update for last one snapshot);

'D7479	21-12-13
' * update jSessionStatus.CreateFromBaseObject() now calling jProject.GetProjectByID() (was jProject.CreateFromBaseObject);
' * fix bugID #25683: Model Creator of the currently open model is getting blank after few seconds;

'D7474	21-12-06
' * fix bugID #24646: Gamma: Evaluator can download the model;

'D7472	21-12-03
' * fix bugID #24610: Add new participants - Paste from clipboard - group to assign is selected, but it assigns all to another group as well;

'D7466	21-11-30
' * fix bugID #24592: Gamma: Dashboard panel size labels are misplaced;

'D7464	21-11-29
' * force to save DefaultWGID on call DBUpdateDateTime for _TABLE_USERS;
' * minor updates for store last visited page (project details);
' * minor updates on user authenticate;
' * fix issue with restoring last visited workgroup/projet was previously it was opened from another workgrpup by hash link;

'D7462	21-11-29
' * fix bugID #24592: Gamma: Dashboard panel size labels are misplaced;

'D7459	21-11-26
' * update SAML/Assertion.aspx for try to log PM to TT session (was: show msg about using the regular logon form);
' * fix bugID #24447: SSO: Joining CS and TeamTime Meeting is not working;

'D7457	21-11-26
' * update pageslist for template/archive projects;
' * fix bugID #24594: Riskion: Events page is missing on Templates model;

'D7456	21-11-25
' + context menus for Riskion models on P{articipants screen: get link,. open link, view pipe;
' * fix bugID #24575: Participant link changing depending on the page you came from;

'D7454	21-11-22
' * upgrade TinyMCE up to version 5.10.2 (was 5.8.2);

'D7453	21-11-22
' + _OPT_INFODOC_PARSE_PLAIN_LINKS (true);
' * pre-parse plain text links on show/edit infodoc;

'D7449	21-11-15
' * fix issue with logging by MeetingID via SAML assertion;
' * update invitations for TT when SSO;
' * fix bugID #24447: SSO: Joining CS and TeamTime Meeting is not working;
' * fix bugID #24449: SSO: Hide/Revise inapplicable UI;

'D7446	21-11-08
' + SSO_Logout() draft;
' * call SSO_Logout() on UserLogout() with log events;
' * fix bugID #24493: SSO: Be able to perform SSO logout;

'D7445	21-11-08
' * fix issue with login form validation when calling SSO;
' * optional way to log into TT/CS meeting without login form;
' * fix bugID #24447: SSO: Joining CS and TeamTime Meeting is not working;

'D7444	21-11-08
' * update Infodocs preview for SSO_only mode when display URLs in the invitation e-mails;
' * update RishEditor for hide URLs when SSO_Only and don't show URL templates;
' * fix bugID #24449: SSO: Hide/Revise inapplicable UI;

'D7443	21-11-04
' * fix issue with logging SessID on user authorize;
' * fix issue with save data about sending anytime invitations;
' * show "SSO logs" events in the logs; add to Workgroup Statistic;

'D7442	21-11-04
' * /api/service/action=version return data as plain text (was incorrect json);

'D7440	21-11-04
' * fix issue with reset destination URL on authenticate from SAML assertion page;
' * more logging on SAML Assertion page;
' * fix issue with overriding SSO_* params on call Authenticate();
' * fix bugID #24446: SSO: Prompt – Invalid SAML response: The message is not an HTTP Post when logging in using the Model link;

'D7439	21-11-03
' * allow to override user credentials when calling Authenticate() and used SSO credentials;
' * another approaches for redirect from SAML Assertion screen;
' * fix bugID #24448: SSO: User is redirected to the models’ list page when accessing a model link (instead of inside the model);

'D7438	21-11-03
' * update welcome screen for show meeting login form when MeetingID is specified (TT/CS);
' * fix bugID #24447: SSO: Joining CS and TeamTime Meeting is not working;

'D7433	21-11-01
' * fix bugID #24449: SSO: Hide/Revise inapplicable UI;

'D7432	21-11-01
' + SSO resources for e-mails with a common link instead of hashes;
' * update CreateLogonURL() and CreateEvaluateSignupURL() for avoid to generate hash links when SSO_Only;
' * fix bugID #24445: SSO: Email Registration issues;

'D7427	21-10-27
' * fix bugID #24423: RTE: 'Object reference not set to an instance of an object.' [10100] [03b9baa];

'D7425	21-10-25
' * another approach for compose e-mail via creating MailAddress() for SendMail() function;

'D7424	21-10-25
' * updates for SAML/Assertion for keep debug flag and show extra info, check new attributes for original email, user name; update wording;

'D7422	21-10-25
' * fix bugID #24412: RTE: 'Cannot find ContentPlaceHolder 'head_JSFiles' in the master page '/mpEC09_TT.master'...

'D7420	21-10-18
' * another approach for call SSO with specify retURL as base64 encoded original query params;
' * update SAML/Assertion.aspx for check original parameters and use Authenticate() instead of Logon();
' + _OPT_SHOW_LINKS_WHEN_SSO_ONLY;
' * update Invitation screens (AT, TT), Evaluation progress, Participants list (prj, wkg) for show/hide user links;
' * update welcome screen and hide eregular login form when SSO_Only enabled;
' * show welcome screen with SSO login button and don;'t auto-redirect to IdP when SSO_Only and not a first page loading (avoid loop after logout);
' * minor updates, related to using SSO/SSO_Only;

'D7419	21-10-14
' * new custom msg when new user logged via SSO;
' + import user name for logged via SSO when it's not specified in the DB;

'D7416	21-10-12
' * fix bugID #24320: RTE: 'Object reference not set to an instance of an object.' [20020] [6c6deb6];

'D7414	21-10-12
' * fix bugID #24354: Gamma: Goal hierarchy dropdown is showing fo the Alternatives List panel;

'D7413	21-10-12
' * improve SAML/Assertion with estra debug info;
' * update way to get the target URL when calling SAML/Assertion, extra checks for options and current state;
' * fix issue with passing ApplicationError.Message to other pager (like Projects List) when came from outside or user is authorized;
' * fix issues with using Server.TransferRequest() when should be Server.Transfer();
' * add SSO_Def* options to Install/Settings.aspx;

'D7412	21-10-08
' * improve SAML/Assertion page with pass error msg/auth result to welcome screen when something goes wrong;
' * detect the default wkg/role on SSO call for just signed user for attach to wkg;
' * minor updates for login screen when SSO;

'D7410	21-10-06
' * fix dialog issue when 1 interval for SP on anytime evaluation pipe;
' * fix bugID #24329: RTE: 'Unable to cast object of type 'ECCore.clsDirectMeasureData' to type 'ECCore.clsUtilityCurveMeasureData'.' [30016]

'D7409	21-10-05
' * fix bugID #24330: RTE: 'Object reference not set to an instance of an object.' [99900] [6971719];

'D7408	21-10-02
' * fix bugID #24326: RTE: 'The given key was not present in the dictionary.' [20020] [b1a4c96];

'D7406	21-10-01
' * fix issue with serialization for ProjectWordingTemplate;
' + "PPM" comparion default option sets;

'D7405	21-10-01
' * fix bugID #24318: RTE: 'Object reference not set to an instance of an object.' [99900] [6d11c9f];
' * fix bugID #24319: RTE: 'Object reference not set to an instance of an object.' [20020] [4420820];

'D7403	21-09-29
' * fix merge issues on manage participants screen;
' * update SAML library and configs;

'D7402	21-09-29
' * fix RTE when calling /SAML/Assertin.aspx just a page;
' * fix bugID #24306: RTE: 'Object reference not set to an instance of an object.' [10008] [048df1e];

'D7401	21-09-29
' * add code for close and delete infodocs on close/delete clsReportItem, clsReport with ecReportItemType.Infodoc;
' * fix bugID #24302: Dashboard: When cloning an information panel, the information content is being reset;

'D7400	21-09-29
' * update ctrlMultiRatings for avoid to select intensity in case of the same name for direct value;
' * fix bugID #24282: Data grid upload error on rating scale;

'D7394	21-09-27
' * updates for Model Description floating button;
' * fix bugID #21299: Minor issues on Model Description/Details page;

'D7393	21-09-23
' * update DataGridUpload for hide Loading panel when upload ends with an error;

'D7391	21-09-23
' + default content for dashbord information block ("Your custom text");
' * fix bugID #24283: Flatten dashboard 3 dot menu;

'D7390	21-09-23
' - remove confirmation on save/reset wkg wording template;
' + show information block on wkg wording templates page about appliying wording;
' * fix bugID #24275: Use Workgroup Wording is not working?

'D7389	21-09-23
' * track last snapshot datetime for cached evaluation progress on the Participants list;
' * fix bugID #21327: RiskionNS: Participants > Evaluation Progress is not updating;

'D7388	21-09-22
' * fix issue with reload models list on restore default option sets;
' + allow to download default option set from the title row;

'D7387	21-09-22
' * fix bugID #24275: Use Workgroup Wording is not working?

'D7386	21-09-22
' * another approach for tracking the cached evaluation progress on the Participants list, based on last modify time and last judgment time;
' * fix bugID #21327: RiskionNS: Participants > Evaluation Progress is not updating;

'D7385	21-09-21
' * force to reset cached eval progress on reload model and on change participant roles;
' * always show "refresh" for participants eval progress (was only for a big  models);
' * fix bugID #24264: Participant judgments count on Participants page does not update when you update the role;

'D7384	21-09-21
' * add expand/collapse/pick current buttons to Page Lookup dialog;
' * enhance Page lookup popup with extra colors and ability to export with URLs;

'D7383	21-09-20
' * move KO Help routines from mpDesktop.aspx to masterpage.js;
' + add icon for online help to mpEmpty.master;
' * fix bugID #24242: The Help icon is missing on the Judgment Matrix opened from Inconsistency Report;

'D7381	21-09-10
' * fix bugID #24226: Refresh issue when making a model online;

'D7378	21-09-09
' * fix bugID #22597: Gamma: Ratings direct entry is not saving;

'D7376	21-09-08
' + add /Account/Forgotten.aspx that raise psw reminder dlg (for keep an old links/references);

'D7375	21-09-08
' * update wrong calls of getProjectLinkDialog();
' * update api/project/set_online for check prj when locked  for mtg only when online;
' * update client-side code for check the real model Online status when calling copy to clipboard and ask to set model on-line;
' * fix bugID #24226: Refresh issue when making a model online;

'D7370	21-09-03
' * always show "Use workgroup wording..." option (was only when Advanced mode);
' * fix bugID #24215: RTE: 'Object reference not set to an instance of an object.' [30010] [f1ed4fe];

'D7369	21-09-01
' * switch KO to widget 2.0 for Comparion (was only for Riskion);

'D7368	21-09-01
' * temporary disable "Send Mail" from Participants list due to UI issues (FB18000);

'D7366	21-08-31
' * fix issue with check isUndefined for survey evaluation steps that in fact are required/missing;
' * fix bugID #24197: Insight Survey required questions are easily skipped;

'D7365	21-08-30
' * fix issue with clsSurvey.AllowToSkipPage;
' + clsSurvey.HasUndefined;
' * update Anytime evaluation for check Survey requireed, undefined, etc;
' * block pipe steps when Survey has required questions not depend on AllowMissingJudgments option;
' * fix issue with "Tree View" on evaluation when steps not allowed due to missing judgments;
' * fix issue with missing survey step titles for step hints;
' * minor fixes for Rich Editor / Save for clusters when QH;
' * fix bugID #24197: Insight Survey required questions are easily skipped;

'D7364	21-08-27
' * fix bugID #24204: RTE: 'Object reference not set to an instance of an object.' [80080] [4d1eb73];

'D7363	21-08-24
' + clsComparionCore.OriginalSessionUser;
' * track the session-in-session and save the original user e-mail to the logs (mostly when calling DBSaveLog);

'D7361	21-08-23
' * fix bugID #24187: Monetary Value not saving;

'D7357	21-08-19
' * fix bugID #24106: CyberSecurity: Invalidate any session tokens associated with user when account password reset is performed;

'D7356	21-08-18
' * fix bugID #24106: CyberSecurity: Invalidate any session tokens associated with user when account password reset is performed;

'D7355	21-08-17
' * fix bugID #24159: RTE: 'Object reference not set to an instance of an object.' [80010] [c75fa23];

'D7354	21-08-13
' - /api/alexa/ask;
' * update code /api/workgroup/UpdateUserWorkgroup for a valid wkg disable with put more info to the log and select wkg when disable active;

'D7353	21-08-12
' * fix bugID #24160: RTE when trying to load Custom Constraints page;

'D7352	21-08-12
' * update /Styles/ for using common jQuery/js libs/Styles;
' * try to remove server headers (Server, X-AspNet-Version, X-Powered-By);
' * fix bugID #24117: CyberSecurity: Possible server-side template injection;

'D7351	21-08-11
' * fix bugID #24156: View only user has no menu;

'D7350	21-08-11
' * fix bugID #24108: CyberSecurity: Cross-site scripting;

'D7349	21-08-11
' * fix issue with select the right workflow element when active page is another Riskion hierarchy submenu;
' * fix label on RiskRegister (when joined Riskion pipe and Impact is active hid);
' * fix bugID #24102: Riskion Flipped Menus;

'D7348	21-08-09
' * update .aspx and js file for replace enclosed < /> tagw with full pair < ></ > due to breaking changes in jQuery 3.5;

'D7347	21-08-09
' * update  sidebarmenu.js and clsMasterPageBase for detect/mark current page for Riskion when different hierarchies on the same level;
' * fix bugID #24102: Riskion Flipped Menus;

'D7345	21-08-06
' * update jQuery library up to 3.6.0 (was 3.3.1);

'D7344	21-08-05
' + api/workgroup/UpdateUserWorkgroup;

'D7343	21-08-04
' * updates for Riskion navigation; try to open "mirror" link, fix issue with wrong HID links, etc;
' * fix bugID #24102: Riskion Flipped Menus;

'D7339	21-07-30
' * fix issues with missing "Manage Participants" button on CS screen for session Owners;

'D7335	21-07-29
' * fix bugID #24119: Gamma: Charts not working on Edge browser;

'D7334	21-07-28
' * fix bugID #24107: CyberSecurity: Block access to project details by participants with no access;

'D7333	21-07-26
' * add loadSVGLib() to masterpage.js that allows to load SVG library depend on browser;

'D7332	21-07-26
' + api/alexa/ask for run any "cmd" API queries but try to user authenticate bafore that if new session;
' * fix bugID #24099: Test and Submit Choose Now Skill for Certification;

'D7331	21-07-23
' * fix bugID #22685: When an existing user logs in and their password no longer satisfies site requirements, force password change;

'D7329	21-07-22
' * update CS for disanle PM functionality for evaluators;
' * fix bugID #22692: Evaluator who open model from models list can view DEFINE MODEL > TT Brainstroming;

'D7327	21-07-22
' ! update Logon() for make all params required, add extra option for know is blank psw allowed;
' - LogonAndGetCredentials() deprecated;
' * check user psw when no complexity option and blank is not allowed;
' * update ValidatePswComplexity() and ignore most requirement when no complexity option;
' * update passfield.js for accept/validate poasswords when blank is not allowed and no complexity;
' * enhance GetPswReminderBodyText() and use for send the same e-mail content.
' * send a special e-mail when set psw for System Manager
' * fix bugID #22685: When an existing user logs in and their password no longer satisfies site requirements, force password change;

'D7326	21-07-21
' * fix bugID #22547: Risk datagrid double scrolls;

'D7325	21-07-21
' * hide CS page when scan sitemap for non-PM users;

'D7324	21-07-21
' * fix issue with showing the psw len limits on Password.aspx;

'D7323	21-07-21
' * upgrade devExtreme up to 21.1.4 (was 20.2.5);

'D7322	21-07-21
' * fix bugID #22547: Risk datagrid double scrolls;

'D7321	21-07-21
' * update Import_Items() for api/pm/report/ for avoid crashing when parse simple JSON;
' * fix bugID #22645: Dashboard issues;

'D7320	21-07-21
' * fix bugID #24093: Teamtime: when rating scale item is very long, truncate it with ...;

'D7319	21-07-21
' + add "Model Lookup" icon to workflow row (right side);

'D7318	21-07-20
' * don't show attributes by default when add Alternatives grid to dashboards;

'D7317	21-07-20
' * fix select user(s) dialog on Dashboards screen;

'D7316	21-07-20
' * try to fix bugID #24086: RTE: 'Object reference not set to an instance of an object.' [80010] [457ccd5];

'D7315	21-07-19
' + add hook error on parse JSON (clsComparionCorePage.ParseJSONParams);

'D7314	21-07-19
' * allow to set psw for other System Managers on Users.aspx;

'D7313	21-07-14
' * fix bugID #22712: Riskion: Hide the "Use responsive UI links" checkbox;

'D7311	21-07-13
' * fixes for Users.aspx for avoid encoding list of passed e-mails;
' * update DetachWorkgroup() for avoid crashing when passed user is not found;
' * fix bugID #22696: RTE: 'Object reference not set to an instance of an object.' [80010] [8199446];
' * fix bugID #22697: RTE: 'Object reference not set to an instance of an object.' [80010] [0ef7f21];

'D7309	21-07-13
' * new "Workgroup statistic" page, based on ajax loading and dxDataGrid;
' * fix bugID #22698: Issues on "Workgroup Statistics" page;

'D7308	21-07-12
' * revert back reference to Structure/Hierarchy for dashboard Alternatives List;
' * update api/pm/dashboard/Synthesize for always load the attributes (was only for Alts grid);
' * update dashboards UI: relink plain list of alternatives to AlternativesGrid code with a limited functionality;
' * ignore "All participants" group for api/pm/user/list by default;
' * fix issue with missing "filter" button on dashboard coomon toolbar when alts list;
' * hide made and total count columns for dashboard Participants list panel by default;
' * few minor updates for dashaboards;
' * fix bugID #22645: Dashboard issues;

'D7307	21-07-12
' * fix issue with restore last visted URL when no active project;
' * fix bugID #22674: The previosuly accessed model is being reopened automatically;

'D7306	21-07-11
' * update api/project/Close for reset last opene model on close;
' * fix bugID #22674: The previosuly accessed model is being reopened automatically;

'D7305	21-07-09
' * fix bugID #22685: When an existing user logs in and their password no longer satisfies site requirements, force password change;

'D7304	21-07-09
' - remove unused options for ols CWSw, services, Cassini port, etc;

'D7303	21-07-08
' * fix bugID #22678: Hide the Intermediate Results row lines when changing the normalization;

'D7302	21-07-08
' * force to enable filtering when adding panel to dashboard from Charts/Synthesise;
' * fix issue with an empty dashboard "Participants" panel EditURL;
' * fix issue with clsReportItem.EditURL when not started from the "/";
' * fix bugID #22645: Dashboard issues;

'D7301	21-07-07
' * updates for dashboard Participants List panel;

'D7300	21-07-07
' * fix setting NormalizationMode in ctrlShowResults2 for overall results;
' * fix bugID #22677: Gamma: Incorrect Overall Results in the pipe;

'D7299	21-07-06
' * fix bugID #22626: Riskion: Add from predefined sets of sources/objectives from Create Model is not working properly;

'D7298	21-07-01
' * upgrade TinyMCE up to 5.8.2 (was 5.8.1);
' * fix bugID #22661: Tiny MCE: Can no longer resize image by dragging the corner;

'D7297	21-07-01
' * fix bugID #22655: Gamma: The normalization dropdown is missing on the pipe;
' * fix bugID #21276: Pipe: Objectives Results has Normalzation dropdown which is not applicable;

'D7295	21-07-01
' ! move webAPI /project/user to /pm/user;
' * update params for /pm/user/?list;

'D7294	21-06-29
' * updates for dashboard charts; refresh toolbat on change chart type, minor fix on init just added chart panel;

'D7294	21-06-30
' + add json classes for clsUser, clsAttribute (for users), clsGroup, etc.;
' * new webAPI pm/user/;

'D7291	21-06-23
' * force to add "Aternatives Grid" dashabord panel (was "Alternatives list") when adding from "Alternatives" screen;
' + add ability to add "Participants" panel fto dashboards;

'D7290	21-06-21
' * another approach for showing "Add from predefined sets..." dialog when option set on create new model;
' * fix bugID #22644: The add predefined objectives modal is open when going back to objectives page;
' * fix bugID #22626: Riskion: Add from predefined sets of sources/objectives from Create Model is not working properly;

'D7289	21-06-21
' * fix bugID #22622: TeamTime: The evaluator's evaluation progress displayed is for the selected user of the PM;

'D7288	21-06-21
' * fix bugID #22637: RTE: 'Object reference not set to an instance of an object.' [80010] [eb50e3b];

'D7287	21-06-21
' * fix bugID #22633: Riskion: Overall Results -- Duplicate colon on a Category tooltip;

'D7286	21-06-21
' * fix bugID #22637: RTE: 'Object reference not set to an instance of an object.' [80010] [eb50e3b];

'D7285	21-06-07
' * update clsPipeBuilder: GetPipeActionNodes() and GetFirstEvalPipeStepForNode() for getting th eirhg tone node when AllPWOutcomes (PW of Probabilites);
' * fix bugID #22609: Jump to cluster with PW of Probabilites method is not working;

'D7284	21-06-07
' * fix bugID #22613: Inconsistency report - clicking the icon for login as participant goes to the wrong step;

'D7283	21-06-02
' * hide normalization options for Riskion when local results for PW steps;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7282	21-06-02
' * update scale intensities header on ctrlMultiRatings when show path;

'D7281	21-05-31
' * fix bugID #22590: Disabled user in a workgroup can be enabled on model level but not saving;

'D7280	21-05-28
' * update View License: hide "Edit workgroup" and "Workgroup Managers" buttons when no user permissions;
' * fix issue with getting the list of WM fron View License screen for System workgroup manager when not active workgroup;
' * fix bugID #22591: SWM: Issues when the workgroup's "Manage by" is just edited;

'D7279	21-05-28
' * enable Feedback option and fix issue with pipe build when option has been changed;

'D7279	21-05-28
' * fix bugID #22597: Gamma: Ratings direct entry is not saving;

'D7278	21-05-28
' * fix bugID #18674: Workgroup Managers count is not working;

'D7277	21-05-27
' - disable custom tooltip for multi PW evaluation control;

'D7276	21-05-26
' - disable checking for user permissions on call webAPI/sessionState (due to kick off when administrative oage for another workgroup):
' * force to set the OnwerID the same as Managed By (ECAM) on edit workgroup;
' * fix bugID #22591: SWM: Issues when the workgroup's "Manage by" is just edited;

'D7275	21-05-26
' * fix bugID #22588: The search icon is missing;

'D7274	21-05-26
' * fix bugID #22550: Predefined sets not loading on every first try to open the modal;

'D7273	21-05-26
' * fix bugID #22591: SWM: Issues when the workgroup's "Manage by" is just edited;

'D7272	21-05-26
' * fix issue with reloading page on add new workgroup and confirmation another different license type;

'D7271	21-05-26
' * clsComparionCorePage.isJustEvaluator() now calculates only once and depend on wkg role group (was: workspace wkg);
' * fix issues with visible "Starred", "Models history" for evaluators;
' * fix bugID #22576: Workgroup Member has "Favorites" model from Page Lookup;

'D7270	21-05-25
' + clsComparionCorePage.CustomWorkgroupPermissions;
' * update HasPermission() for optional check when specific workgroup (when create a page);
' + use CustomWorkgroupPermissions for all admin pages;
' * significant changes for check permissions, especially when have Own* roles;
' * fix alignment issue on View License screen;
' * fix bugID #22572: System Workgroup Manager -- Create Workgroup button is missing;

'D7269	21-05-25
' * fix check user permissions on HasPermission() when Own* role;
' * fix bugID #22577: SMW that is a Workgroup Manager has the Statistic link disabled;

'D7268	21-05-24
' * fix issue with check user permissions when system workgroup attached but have "Own*" roles only;
' * fix bugID #22573: SMW can access restricted pages for a wokgroup that he is only a member;

'D7267	21-05-20
' - jProject.GetProjectByIDAsJSON;
' + clsJsonObject.doInherit;
' * replace .doInherit() method in jProject and jUserTeamTime with calling clsJsonObject.doInherit();

'D7266	21-05-20
' * upgrade TinyMCE up to 5.8.1 (was 5.7.0):

'D7265	21-05-20
' * another approach for TinyMCE adding images: auto-upload via API instead of inline blob;
' + api/pm/infodoc/upload method for create model infodoc image;
' * fix bugID #22574: RTE: 'External component has thrown an exception.' [30019] [c8feff0];

'D7264	21-05-18
' * always show option "Add objective from predefined set..." on "New model" dialog (was only when Advanced);

'D7263	21-05-18
' + add option "Add objective from predefined set..." on "New model" dialog;
' * update api/project/create and hierarchy editor for support new option;

'D7262	21-05-18
' * enhanced logging with judgments data for anytime evaluation;

'D7261	21-05-17
' + new fullscreen mode (when supporting);

'D7260	21-05-13
' * fix bugID #22550: Predefined sets not loading on every first try to open the modal;

'D7259	21-05-13
' * force to use the Abolute normalization for Riskion TT results;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7258	21-05-13
' + HelpAuthWebAPI.GetHelpPath;
' * fix bugID #18001: NS: Use the KO help page on rich text editor;

'D7256	21-05-12
' * fix issue with normalization when TT Overall results for Riskion models;
' * fix issue with TT Overall hierarchy tree section resize and alignment priority values;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7255	21-05-12
' * fix bugID #18001: NS: Use the KO help page on rich text editor;

'D7253	21-05-07
' - disable scanning for Wireframes on load page;

'D7252	21-05-06
' * fix bugID #22523: Dynamic Sensitivity: The "Add page to" dashboard option is missing;

'D7251	21-05-05
' * update api/alexa/UnregisterUser for avoid to delete the account, but remove the link by AlexaID;

'D7250	21-05-05
' * redo approach for link user by PIN when Alexa skill;

'D7249	21-05-04
' + webconfig option PINAllowed, available on System settings screen;
' + allow to show/hide menu "Get PIN code";
' + api/alexa/LoginByPIN;

'D7248	21-05-03
' * force to use XA solver for RA;
' - disable ability to use Gurobi solver;

'D7247	21-04-30
' * improve api/alexa/GetUserByUserID() for return message and error code when have login issues but user exists;
' * ignore models, deleted as marked when logging as Alexa;

'D7246	21-04-29
' * fix bugID #21509: Dashboard: Can add "Datagrid" to dashboard even it is not supported;

'D7245	21-04-27
' + create openapi.json next to .yaml on generate documentation for webAPI;

'D7244	12-04-25
' * disable editing for Panel Type on clone dashboard item;
' * fix bugID #21447: Gamma: Dashboard RTE when cloning a panel;

'D7243	21-04-24
' * fix bugID #21276: Pipe: Objectives Results has Normalzation dropdown which is not applicable;

'D7242	21-04-24
' + SESSION_PIPE_PGID, keep last visited anytime evaluation pageID;
' * use last used pipe as link for "Evaluation" header icon link when simple evaluator;
' * fix bugID #21492: Controls Invite Link just redirects to Likelihood pipe ;

'D7241	21-04-22
' * fix bugID #21492: Controls Invite Link just redirects to Likelihood pipe

'D7240	21-04-22
' * fix bugID #21481: DataMapping: button "Import" shown when should be "Export";

'D7238	21-04-14
' + ctrlShowResults2.Show_Normalization;
' * hide normalization option for Riskion anytime evaluation result steps;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7237	21-04-14
' * fix bugID #21476: RTE: 'Object reference not set to an instance of an object.' [60209] [9cd17bd];

'D7236	21-04-13
' * fix bugID #20724: Dashboard Export JSON no scroll bar;

'D7235	21-04-13
' * enhance "Create Model" dialog for Riskion with adding required "Timeframe" field;
' * add optional "timeframe" parameter to api/project Create() method;

'D7234	21-04-12
' * update ctrlSensitivityAnalysis.ascx for add opt_ShowNormalization;
' * hide normalization options for Riskion pipe sA steps;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7234	21-04-12
' * update ctrlSensitivityAnalysis.ascx for add opt_ShowNormalization;
' * hide normalization options for Riskion pipe SA steps;
' * fix bugID #21468: Riskion: In the Synthesize screens we removed the Normalization pulldown but we do have in the pipe;

'D7233	21-04-09
' * fix issue with repaint/scroll Details form when edit data;

'D7231	21-04-09
' * hide "Get PIN code" when Brainstorming session for anonymous;

'D7230	21-04-09
' * update Install/Settings.aspx for mark the option which are specified as custom value, show confirmation on reset custom value;

'D7228	21-04-11
' * fix bugID #21245: Scale description not displaying on TeamTime for Step Function / Utility Curve;

'D7227	21-04-07
' * update clone dashboard item dialog for replace default name on change type;
' * fix bugID #21447: Gamma: Dashboard RTE when cloning a panel;

'D7226	21-04-06
' * fix bugID #21447: Gamma: Dashboard RTE when cloning a panel;

'D7225	21-04-06
' * fix bugID #21449: CR Dashboard: RTE when dragging the Sensitivity Objective Bars;

'D7224	21-04-06
' * openAPI generator improvements;

'D7223	21-04-05
' * enhance api/account/Logon for specify a most used parameters;

'D7222	21-03-31
' * reorganize webapi for help (KO, openAPI);
' + _PGID_WEBAPI_HELP;
' * move openAPI generator to api/help/openapi/;
' * reorganize statis Swagger UI help page and now this is server-based _PGID_WEBAPI_HELP;

'D7221	21-03-31
' * enhance openAPI generator with adding comments from XML docuemntation, add more summary info, few fixes and updates;

'D7220	21-03-30
' + Microsoft.OpenApi, Microsoft.OpenApi.Readers;
' + enhance OpenAPI generator with using MS library;

'D7219	21-03-29
' * new approach for openAPI generator;

'D7218	21-03-24
' * minor update for GetNameByFilename() for decode URL_based names;
' * call GetNameByFilename() on upload models, create from file.

'D7217	21-03-23
' * fix bugID #21293: Risk model type definition won't update when changing the model type ;

'D7216	21-03-23
' * enhanced cpation for model type in the master page header;
' * update dynamic styles for master page headers;

'D7215	21-03-22
' * draft for openAPI parser/generator;

'D7214	21-03-17
' * update Riskion.sitemap for move Identify/Structure one level top (make as workflow);

'D7213	21-03-17
' * fix bugID #21391: Can't stop TT meeting from the models list;

'D7212	21-03-17
' * fix bugID #21393: Changes on Copying Settings to another hierarchy;

'D7211	21-03-17
' * change notification on update TT participants list;
' * fix bugID #21396: TeamTime Select Participants -- Included no. of users is incorrect;

'D7210	21-03-17
' * fix issue on call "Map" for DataMapping dialog;

'D7209	21-03-16
' * another approach for Alexa webapi user authorize/check: using only userId passed from Alexa;
' * updates for Alexa webapi signup method, add "unregisteruser";
' - getSessionStateJSON();
' + jSessionState, update all references for getSessionStateJSON();
' * fix issue when unable to delete user via calling DBUserDelete();

'D7208	21-03-15
' + enhance jWorkgroup with extra data;

'D7207	21-03-15
' * redo alexa webapi for accept the userID by create a username-based email;

'D7206	21-03-15
' * improve api/alexa with support userId passed from Alexa devices;'
' + add "Refresh" button to workgroups list;
' + api/workgroup/list;

'D7205	21-03-12
' * improve clsComparionCore.Logon() for allow to call as special logon;
' - disable DB logs for RoleActions;
' * enhance api/alexa for return expected data when register or login Alexa user;

'D7204	21-03-12
' * disable auto-load recommended on change tab when KO widget 2.0;

'D7203	21-03-12
' * update webapi for Alexa: prevent calling Authenticate since user can be just registere and no wrk/projects;

'D7202	21-03-11
' + force to load "Recommended" for KO widget 2.0 and try to keep the irignal article;
' * fix bugID #21333: Install Riskion Help widget and context-sensitive;

'D7201	21-03-11
' + add support for KO widget 2.0;
' * fix bugID #21333: Install Riskion Help widget and context-sensitive;

'D7200	21-03-11
' * fix bugID #21389: Riskion Teamtime is crashing; not model dependent;

'D7199	21-03-10
' * updates for avoid to crash when DB exists but empty or wrong;

'D7198	21-03-10
' * update api/alexa Register, Login methods for return sessionState and change code as .Data (was .ObjectID);

'D7197	21-03-10
' * enhance getSessionStateJSON() with extra details;
' * pass getSessionStateJSON as jActionResult.Tag on call /api/account/ logon;

'D7196	21-03-09
' * enhance /api/project/ create with auto-select default option set when Src_ID is not specified;

'D7195	21-03-09
' * fix calling psw reminder from Password.aspx;
' * fix layout issue on Password.aspx when form is not available (link expired, set psw is not allowed, etc);
' * fix bugID #21254: RTE: 'Object reference not set to an instance of an object.' [40051] [11e700e];
' * fix bugID #21375: Password link is still usable even after already creating a password;

'D7194	21-03-08
' * fix bugID #21380: Information Document Settings;

'D7193	21-03-08
' * fix bugID #21375: Password link is still usable even after already creating a password;

'D7192	21-03-08
' * fix bugID #21377: "Priority" should be "Likelihood" or "Impact";

'D7191	21-03-07
' * improve api/alexa methods with return error code for track different cases;
' * force to create alexa userID always as combination of name + random number;

'D7190	21-03-05
' * fix bugID #21254: RTE: 'Object reference not set to an instance of an object.' [40051] [11e700e];

'D7189	21-03-04
' * updates for api/alexa CreateGroup when fill workgroup data and attach user as wkg manager by default;
' * minor updates for Logs api (auto-parse templates);

'D7188	21-03-04
' + enhance api/alexa with methods RegisterNewUser, CreateGroup, LoginByUserID;

'D7187	21-03-03
' + draft Pin code page (_PGID_PINCODE, 40099)
' * force to login and show oin page when asked on start (?pin or start via /pin/);
' * _DEF_PINCODE_TIMEOUT (5 mins for now);
' + GetUserPin(),  GetUserByPin();
' + /api/account/ new method ConnectWithPin;

'D7186	21-03-03
' * fix bugID #21367: Scale Assessment - Has Previous button with the hint text saying "Evaluate Likelihood";

'D7185	21-03-03
' * update DBParse_ProjectLockInfo for write to DB unlocked projects (due to timeout);
' * update "ViewLogs" workgrpoup selector options;
' * fix bugID #21363: View Logs -- Search from workgroup dropdown;

'D7184	21-03-02
' * fix bugID #21365: Navigation Box option question;

'D7182	21-03-01
' * minor updates for popup on psw reminder;
' * hook hit for  "Enter" key on set psw screen;
' * fix bugID #21362: Minor changes for password reset/create;

'D7180	21-02-26
' * fix build issue for DataMapping;
' * fix bugID #21354: Data Mapping: Not working at all (Feb 2021) - RTE when accessing the page/options;

'D7178	21-02-26
' * fix bugID #21254: RTE: 'Object reference not set to an instance of an object.' [40051] [11e700e];

'D7177	21-02-23
' + add custom serialization for RadTreeView on anytime evaluation screen;
' * fix RTE xxxx "...is not a valid value for Int32.' [30010][....]";

'D7176	21-02-23
' * fix bugID #21349: Gamma: Creating a new model no longer updates the models list and open the model -- need to refresh to show the model;

'D7175	21-02-22
' * enhance api/project/copy for submit just created project data even when saved in older version (but keep current ProjectID);
' * update onOpenProject() in masterpage.js for add just copied model to data set and refresh the grid content;
' * fix bugID #21346: Downgraded model won't show up on the list until page is refreshed manually;

'D7174	21-02-22
' * fix bugID #21345: %%Models%% not parsed;

'D7173	21-02-22
' * fix bugID #21326: RiskionNS: View only pipe for controls highlights the Likelihood workflow;

'D7172	21-02-22
' + show (?) Help icon on mpDesktop.master for Riskion, add abuility to load help pages from https://riskion.knowledgeowl.com;
' * fix bugID #21333: Install Riskion Help widget and context-sensitive;

'D7171	21-02-22
' * fix bugID #21335: Pipe link going to wrong step;

'D7170	21-02-19
' * fix bugID #21337: Unending Please Wait on Riskion's Reports pages;

'D7169	21-02-19
' * update styles for devExtreme disable input elements color (make more contrast);

'D7168	21-02-19
' * fix bugID #21338: Left-clicking the button to the left of the model is not showing the context menu;

'D7167	21-02-18
' * update api/account/logon for avoid pushing all requaet parameters as part of URL;

'D7166	21-02-18
' * enhance api/service/ping when checking projects list for get the list of modified/created workspaces for user;
' * optimization checks and update ActiveProjectsList when it was changes while calling api/service/ping;
' * update api/workgroup/open for check userworkgroup on switch workgroup;
' * enhance mpDesktop.master for analyze the response on call api/workgrpoup/open;
' * update ProjectsList for try to refresh the whole row when project item has been changed and "Push" called for datasource;
' * fix bugID #21289: Pentest: Session Management;

'D7165	21-02-18
' * fix issue with adding just updates/edited models on call "ping" when user doesn't have permissions for these projects;
' * fix bugID #21319: Newly created model is showing on a user's models list even the user is not associated with the model;

'D7164	21-02-18
' * upgrade TinyMCE Nuget (5.6.2 -> 5.7.0);

'D7163	21-02-15
' * fix bugID #21311: RTE: 'Object reference not set to an instance of an object.' [40051] [7646d42];

'D7162	21-02-11
' * fix issue with active project reload on every call (fix clsComparionCorePage.Preinit());

'D7160	21-02-09
' * update api/service/Session_State for track user permissions for current page when active model;
' * force to reset dependent data structures on reset active user workspaces;
' * check changes Group for userWorkgroup and Workspace on clsComparionCorePage.PreInit in additional to disabled/deleted;
' * update clsComparionCore.Logon() for avoid issue when user can't login when he was disabled in the last visited workgroup;
' * fix bugID #21289: Pentest: Session Management;

'D7159	21-02-09
' * updte jProject.FillProjetData() for check project state and workspace for disabled;
' * update clsComparionCorePage.PreInit() when re-read active session (User, UwerWorkgroup, Workspace) and check for disabled/missing for terminate session;
' * update clsComparionCore.Init() for avoid to set active project specified as URI parameter when it's not allowed;
' * fix bugID #21289: Pentest: Session Management;

'D7157	21-02-08
' * update RichEditor.aspx for prevent using URI injections on call ajax;
' * fix bugID #21286: Pentest: Server-side template injections in RichEditor

'D7156	21-02-08
' * fix RTE on open Riskion model when specify Impact hierarchy;
' * fix bugID #21298: RiskionNS: Jump to Impact screens from models list goes to Likelihood screens instead;

'D7155	21-02-05
' * fix issue with Projects History popup layout;

'D7154	21-02-05
' * enhance /api/project/Open with an extra parameter "passcode";
' * fix issue with urlWithParams() in masterpage.js;
' * fix bugID #21298: RiskionNS: Jump to Impact screens from models list goes to Likelihood screens instead;

'D7153	21-02-05
' * upgrade devExtreme up to latest version (20.2.3 -> 20.2.5);

'D7152	21-02-05
'* fix issue with select panel type on add/edit dashboard panel;

'D7150	21-02-01
' * upgrade Newtonsoft.Json NuGet package up to the latest versions (12.0.1 -> 12.0.3);
' * upgrade BuildWebCompiler NuGet package up to the latest versions (1.12.394 -> 1.12.405);
' * upgrade TinyMCE NuGet package up to the latest versions (5.0.1 -> 5.6.2);

'D7149	21-02-01
' * fix bugID #21288: Pentest: configuration issues

'D7148	21-02-01
' * fix bugID #21287: Pentest: issues with cookies;

'D7147	21-01-31
' * fix bugID #21291: Make the creation of password for new users a setting;

'D7146	21-01-29
' + show icon for users who locked due to wrong attempts on Participants screen;
' + allow to unlock user from Participants screen;
' * fix bugID #21235: Lock user -- The icon to unlock a locked out user due to wrong password is missing;

'D7145	21-01-29
' * update Authenticate() for avoid to ask user set a new psw when came by link to anytime/teamtime evaluation only for evaluators;
' * fix bugID #20773: TeamTime: Invited Evaluators shouldn't be asked to create password and existing links became invalid;

'D7144	21-01-28
' * update Authenticate() for avoid to ask user set a new psw when PasswordStatus=-1 and user came by link to anytime/teamtime evaluation;
' * fix bugID #20773: TeamTime: Invited Evaluators shouldn't be asked to create password and existing links became invalid;

'D7143	21-01-28
' * updates and fixes for Add/Edit dashboard item dialog;
' * minor dashboard updates for Objectives panel;

'D7142	21-01-27
' * rename "Dashboards" to "DDM" with a tooltip;
' * new "Add/Edit dashboard item" dialog with list of panel types;
' - disable sync WRTNode with Objective panels;
' * optimization for call DSA data init when no SA/Charts and changes node on Objectives;
' * fix RTE on call webapi "dsa_init" when "risk" is not passed;
' + add ability to edit node name directly from Obejctves panel;

'D7141	21-01-26
' * update Infodoc.aspx and ParseVideoLinks() for inject jscript code for auto-start youtube video on load infodoc;
' * fix bugID #21236: Quick help issues;

'D7140	21-01-26
' * Dashboards upload definitions menus changes;

'D7139	21-01-26
' * force to resize dxDataGrid when resize column by default for all instances (masterpage.js inits);

'D7138	21-01-22
' * fix bugID #20773: TeamTime: Invited Evaluators shouldn't be asked to create password and existing links became invalid;

'D7137	21-01-22
' * fix issue with clean-up user hashes on set new password;
' * avoid to lock/increase attempts counter when try to login by expired/invalid hash link;
' * disable warning on set the same psw value (when no complexity);
' * fix bugID #17427: Participant specific links are expiring when create new password;

'D7136	21-01-21
' + SwaggerUI for show API documentation (client-side js-version );
' + Documentation module with extenstions .GetDocumentation() and .GetSummary();
' + openapi.json draft template;

'D7135	21-01-21
' * updates for ParseVideoLinks() and ParseTextHyperlinks();
' * fix bugID #21236: Quick help issues;

'D7134	21-01-20
' * update ParseVideoLinks for another option to force autoplay for youtube videos;
' * fix issue with switching youtube videos (QH) to fullscreen mode (as adding extra params to popup iframe);
' * fix bugID #21236: Quick help issues;

'D7133	21-01-20
' * minor updates for wording on show psw requirement;
' * minor update when disable "Allow balnk psw" whne complexity enabled on Settings.aspx;
' * fix issue with set new psw as blank when blank psw allowed and the old one was blank;
' * fix bugID #21208: Password Complexity Issues;

'D7132	21-01-19
' * using Levenshtein distance for check the different characters on set new password (ValidatePswComplexity);

'D7131	21-01-19
' * another approach for Password.aspx when psw complexity: now no tips for input fields but checkmarks below;
' * extra validation on submit new pas on Password.aspx;
' * fix validation when missing something from requirement(s) but the common "streight" is pretty enough to pass;
' * fix RTE when reset/restore password (no ActiveUser) on Password.aspx;
' * fix bugID #21208: Password Complexity Issues;

'D7130	21-01-12
' * update code for detect the selected TT participants who can receive invitations;
' * fix bugID #21177: Can't send a test TT invite email to the login PM;

'D7129	21-01-12
' * update button for manage groups on Anytime Send Invitations;
' * add "Manage Groups" button to TT Select participants screen;
' * improve selection on "Select by group" dialog on TT Select participant;
' * fix bugID #21174: Add an icon to open Manage Groups from TT Select Participants screen;

'D7128	21-01-12
' * totally new Password.aspx with using new psw validation control;
' * fix some issues on check user psw complexity;
' * fix bugID #21208: Password Complexity Issues;

'D7127	21-01-11
' * new dialog on set user psw when confirm about keep hash links;
' * fix bugID #17427: Participant specific links are expiring when create new password;

'D7126	21-01-09
' * fix bugID #21207: RTE: 'An entry with the same key already exists.' [99900] [019e330];

'D7121	21-01-08
' + StringService.Levenshtein_Distance();

'D7120	21-01-08
' * fix bugID #21206: RTE: 'Index was outside the bounds of the array.' [99900] [6230d76];

'D7119	21-01-07
' * minor fix for validation password when no complexity option and blank allowed;
' + show confirmation about removing or keep hashes on change user psw from Account page or "Manage Participants" screens;
' * fix bugID #17427: Participant specific links are expiring when create new password;

'D7118	21-01-07
' * fix bugID #21198: Can't save password in user settings even the password complexity is disabled;

'D7117	21-01-07
' * redo error messages as popups instead of notify on Users.aspx;
' * fix bugID #21196: Error message shows HTML tags;

'D7115	21-01-05
' * minor updates for Error.aspx that tries to avoid client-size issues and fix passing user IP in crash info;

'D7114	21-01-04
' + clsComparionCore.HashCodesReset;
' + clsComparionCorePage.TinyURLUpdateUserPsw();
' * update Password.aspx for call TinyURLUpdateUserPsw and don't remove exiasting TokenURLs for user;
' * fix bugID #17427: Participant specific links are expiring when create new password;

'D7113	21-01-04
' * fixes for actve use when calling api/project/user/TeamTime_UsersList;
' * updates for TT invitations screen selected users popup;
' * fix bugID #21177: Can't send a test TT invite email to the login PM;

'D7112	21-01-04
' * fix bugID #21188: Filter on TT Select Participants is taking effect on other models

'D7111	20-12-29
' * TT Invitation screen: fix issue with selected users (ignore who can't evaluate, fix dialog with list of selected);
' * fix bugID #21177: Can't send a test TT invite email to the login PM;

'D7110	20-12-29
' * fix bugID #20772: TeamTIme: Check/Uncheck all applies to all the participants even there is a Has Data Filter;

'D7109	20-12-28
' * fix issue with saving values for cost and custom constraints on RA Funding Pools screen;

'D7107	20-12-25
' * fix RTE on getting ResourcePool on RA Funding Pools screen;

'D7106	20-12-25
' * enhance RA Fundiong Pools with support ResourcePools and ability to switch/specify pools for Cost and any custom constraint;

'D7103	20-12-23
' * upgrade NuGet packages: Newtonsoft.Json (12.0.1 -> 12.0.3), BuildWebCompiler (1.12.394 -> 1.12.405);
' * upgrade devExtreme controls (20.2.3 -> 20.2.4);

'D7102	20-12-22
' * fix bugID #21158: Funded Alternatives on Time Periods screen are different with what's on Portfolio view when SOLVE was done from Time Periods;

'D7101	20-12-22
' * add button for show selected participants on TT "Invite"/"Send invitation" tab;
' * count the active user as selected (in case when links allowed) for send TT invitations;
' * fix bugID #21177: Can't send a test TT invite email to the login PM;

'D7100	20-12-22
' * show "Starred" column on Projects List only for PMs (who can create a model);
' * fix bugID #21176: Favorite column is hidden when a model is open?

'D7099	20-12-22
' * updates pagination for ViewLogs;
' * fix bugID #21170: 404 Page not found error when trying to view logs;

'D7098	20-12-21
' + Parameters.RAFundingPoolsUserPrty();
' * init RASolver.UseFundingPoolsPriorities based on RAFundingPoolsUserPrty on load RA;
' * now RA Funding Pools using FP Priorities on solve and display these colums when enabled only;
' * fix RA FP grid layout issues when no prty used;
' * fix issue with resize RA PF screen for avoid scrollers;

'D7097	20-12-21
' * fix bugID #21170: 404 Page not found error when trying to view logs;

'D7096	20-12-17
' * fix bugID #21158: Funded Alternatives on Time Periods screen are different with what's on Portfolio view when SOLVE was done from Time Periods;

'D7095	20-12-17
' * fix bugID #21160: Add page to Dashboard on screens where it is not applicable;

'D7094	20-12-17
' * fix bugID #21165: Dynamic Groups shouldn't be on groups list on General Link invitation;

'D7093	20-12-14
' * fix issues with headers on RA Funding Pools; minor updates for FP page;
' + "Show Benefits" option on RA FP screen;

'D7092	20-12-14
' * improve Funding Pools with "Benefit" columns;

'D7091	20-12-11
' [!] check commit at 20-12-22
' * improve sections selector on Combined Reports;
' * fix bugID #21150: Combined Reports: Before Downloading the xlsx file, add a prompt where user can select which "sheets" he wants to download;

'D7090	20-12-10
' * new CombinedReports screen with export options for download XLSX;
' * fix bugID #21150: Combined Reports: Before Downloading the xlsx file, add a prompt where user can select which "sheets" he wants to download;

'D7088	20-12-09
' * fix bugID #21145: Are we supposedly showing the email for CS user on this tooltip?

'D7087	20-12-07
' * avoid to remember the Combined report downloading page as last visited;

'D7086	20-12-07
' * improve RA infeasibility  analysis dialog with extended selection, other formatting, remember options. etc;
' * another UI for RA infeasibility analysis results block;

'D7085	20-12-07
' * fix bugID #21126: CR CS: Edit and Apply buttons on information documents editor are disabled;

'D7084	20-12-07
' * revert back the adding extra "dt" code to .css for avoid caching, but expect Custom.aspx reports;
' * fix bugID #21033: RiskonNS: The workflow tabs change when generating report;

'D7083	20-12-05
' * Solver B by default;

'D7082	20-12-05
' * dashboards release notes;

'D7081	20-12-04
' - disable auto-switching to Gurobi solver when RA model is infeasible;
' * fix bugID #21138: RA Portfolio View - wrong error message is shown;

'D7080	20-12-04
' * fix bugID #19608: Gamma: Gray bar at the bottom of pages;

'D7079	20-12-02
' ! rename _PGID_RA_TIMEPERIODS to _PGID_RA_TIMEPERIODS_SETTINGS;
' + add RA Timeperiods Overview separate page (based on RA Main "Timeperiods" grid);
' * hide RA Main "Time periods" buttons;

'D7078	20-12-01
' * fix and improve RA Solver Logs for Gurobi;

'D7077	20-11-30
' * fix crashing Gurobi Solver when calling for session-state instances (remove relaxList);

'D7076	20-11-30
' * fix cloning alternatives on RA Main when create/sync risk associated model;
' * fix bugID #21110: CR: RA Associate Risk Model is not working;

'D7075	20-11-30
' * minor UI updates for RA Main screen;

'D7074	20-11-30
' * fix bugID #21118: Type error "Graphica";
' * fix bugID #21119: Should be "four" (Phone number was added);

'D7073	20-11-27
' * fix bugID #21108: Funding Pool Alternaitves Sorting is inconsistent;

'D7072	20-11-27
' * fix bugID #21112: RA Tooltip for ID is incorrect;
' * fix bugID #21113: CR RA Multiple tooltips getting stuck on RA Time Periods mode;

'D7071	20-11-26
' * fix bugID #19608: Gamma: Gray bar at the bottom of pages;

'D7070	20-11-26
' * fix bugID #21102: CR: RTE when assigning alternatives attributes when there is no default attribute (broken);

'D7069	20-11-24
' * fix RTE on edit alternative attribute value;
' * fix bugID #20860: Firefox: The text box is not aligned correctly;

'D7068	20-11-23
' * disable some confirmations on anytime evaluation per EF request;
' * no paging for participants list on Anytime invitations screen;

'D7067	20-11-23
' + _OPT_MY_RISK_REWARD_ALLOWED (false for CR);
' + add ability to hide MyRiskReward model type on create/edit Riskion model propertied;

'D7066	20-11-23
' * enable t2s for anytime evaluation result screen;

'D7065	20-11-21
' * try to allow resize the dashboard panel over the window bound (at the bottom line);
' * fix bugID #20788: Dashboard -- can't adjust the height of the bottom panels by dragging;

'D7064	20-11-20
' * fix bugID #21075: CR Dashboard: Multiple Users selected in Charts are not selected when added to the dashboard;

'D7063	20-11-20
' * dirty fix for scale assessment "exclude list" for step function: use comment with guid instead of ID;
' * fix bvugID #21088: CR: Scale assessment -- Selecting an intensity that will have 0% priority is not working;

'D7062	20-11-19
' * fix bugID #21061: Move the sentence about reviewing judgments to the bottom (inermediate results);

'D7061	20-11-19
' * fix header framed infodocs alignment for ctrlMaultiPairwise (related to 21067);

'D7060	20-11-19
' * fix bugID #21067: Pipe no longer update the no. of pairs per step without manually reloading the page;

'D7059	20-11-19
' * fix bugID #21084: CR: Added Portfolio view panel has no funded Alternatives;

'D7058	20-11-19
' * fix bugID #21076: CR Dashboard: Add Alternatives Grid to Dashboard is adding the Dynamic Sensitivity instead;

'D7057	20-11-19
' * fix bugID #21061: Move the sentence about reviewing judgments to the bottom (inermediate results);

'D7056	20-11-17
' * Upload DataGrid popup now as a in-line dialog;

'D7055	20-11-17
' * update devExtreme styles for fix issue with dxFileUpload control layout;

'D7054	20-11-12
' * upgrade devExtreme from 20.1.7 up to 20.2.3;
' * upgrade fontAwesome from 5.13.0 up to 5.15.1;

'D7053	20-11-11
' * fix bugID #21058: After evaluating controls, is it correct that it redirects to Risk Results page?

'D7052	20-11-11
' * update resources for RA screen;
' * fix issues for RA infeasibility dialogs;
' * fix bugID #21052: RA: Infeasibility Analysis Solutions;

'D7051	20-11-10
' * try to perform the "clear" project restore from the snapshot on call SnapshotRestoreProject();
' * check model users after the calling SnapshotRestoreProject() and save "backup" snapshot;
' * show "Restore" action in Project Logs;
' * fix bugID #21050: Restore Snapshot is not working on some models (student models);

'D7050	20-11-10
' * update styles and resourecs for dashboards;
' * fix bugID #21051: Is it correct that the dashboard toolbar is also color gray when Show Panel Borders is ON?

'D7049	20-11-09
' * fix bugID #21038: RiskionNS: Clicking on Synthesize > Consensus View goes to the Reports page;

'D7046	20-11-05
' * fix bugID #21037: RA: Make the Alternative Names header consistent on the RA screens (Model Terminology);

'D7045	20-11-05
' * fix bugID #21020: Consensus View step is -1 when the page doesn't exists;

'D7044	20-11-05
' * fix bugID #21033: RiskonNS: The workflow tabs change when generating report;

'D7043	20-11-04
' * force to show short name on Landing pages (full as tooltip);
' * fix issue with _PGID_REPORT_OVERALLRESULTS page (fix detection on ReportView.aspx);
' * fix bugID #21028: Riskion Reports - when you choose the Likelihood of Events report, you get the Sources and Events report;

'D7040	20-11-03
' * fix bugID #21021: Message when Insight Survey is not included on the license;

'D7039	20-11-03
' * update dashboard panels with custom state savers (widget setting) for avoid to save encoded state right after the first loading;
' * fix bugID #21001: CR: Dashboard -- NO OBJECT FOUND error when deleting a dashboard;

'D7038	20-11-03
' * fix bugID #20734: When using rich text for insight survey question a line feed is inserted in front of the question;

'D7036	20-11-02
' * fix bugID #21009: Dashboard -- Model Description: Unnecessary scroll;

'D7033	20-10-29
' * optimize loading RA data for dashboards;
' * reset SolveState when call ResetFunded() for RA scenario;
' * fix bugID #20963: Dashboard: Portfolio View panel is updating based on the last solved scenario;

'D7032	20-10-29
' * dashboards: disable common toolbar till all data will be loaded;
' * replace FetchWrongObject reply on api/pm/report/item_edit with a jActionResult with erro state/message;
' * fix bugID #21001: CR: Dashboard -- NO OBJECT FOUND error when deleting a dashboard;

'D7031	20-10-29
' * fix crashing on enable Alternatives filtering for dashboards when list of alternatives;
' * fix possible RTE on api/pm/dashboard when filter alternatives
' * fix bugID #21002: RTE: 'Conversion from string "" to type 'Integer' is not valid.' [99900] [65b8902];

'D7030	20-10-28
' * fix bugID #21003: Gamma: TT : The "You are done" message is no longer centered;

'D7028	20-10-27
' * fix scrolling issues on TT;
' * fix issue with UC graph pamars when TT session;
' * fix bugID #20998: Teamtime: decreasing utility curve chart displays as if increasing (all branches);

'D7027	20-10-27
' + jRAScenario, jRAALternative, jRAGRid;
' + api/pm/ra/get_portfolio_grid;
' * updates for RA webAPI methods for using just added jRA* structures;
' * updates dashboadrs screen for use RA webAPI;

'D7026	20-10-27
' + api/pm/ra/;
' * add "Scenarios" to dashboards common panel when Portfolio View panel used;

'D7025	20-10-26
' * fix bugID #20989: CR: Participants List > Send Email Error;

'D7023	20-10-23
' * fixes for Page lookup;

'D7022	20-10-22
' * fix styles for chat button icon;

'D7021	20-10-22
' * dynamic page title and URL for dashboard for make each dashboard with unique paghe URL;

'D7020	20-10-22
' + update clsReportItem.ItemType as String (was ecReportItemType ENUM);
' * update reports code for allow to read/parse JSON with unknown ecReportTypeItem but not crashing on deserialize;
' * update dashboards UI for show msg when unsupported panel type;

'D7019	20-10-13
' * allow to use Install/System.aspx when np license psw hash and when SelfHost license;
' * update Install/System.aspx wording;

'D7018	20-10-12
' * Show Timeline field on "Project descriptions" tab;

'D7017	20-10-12
' * update dashboards for new references to Alts/Obj Grids;
' * auto-replace an old PageID for Alts/Obj Grids with a new pages from sitemap;

'D7016	20-10-12
' + "download" icon next to model name in the page header (mpDesktop.master);
' * update Project Details screen and show floating button for edit project description;

'D7014	20-10-12
' + add common loader panels to all dxDataGrid, dxTreeList on dashboards;
' * fix issue with "Show Local" checkbox for dashboard Objective grid;
' * fix bugID #20783: Dashboard - No global priorities for Objectives Grids and Charts;

'D7013	20-10-09
' * fix bugID #20896: CR: Add a tooltip on Use Responsive UI links checkbox;

'D7012	20-10-09
' * fix bugID #20897: CR: Error in AnyTime invitation page;

'D7011	20-10-09
' * update code for Page and Frame on dashboards and make it more consistemt to API;
' + _rep_item_Infodoc, reportItemType.Infodoc;
' + reObjectType.DashboardInfodoc;
' + add support for just added reObjectType.DashboardInfodoc to RichEditor and Infodoc.aspx (based on PM.InfoDocs.GetCustomInfoDoc);
' + add Custom infodoc to dashboard;

'D7010	20-10-09
' + add new dashboard samples (All);
' + allow to crearte a dashboard from sample by double click on the item from the list;

'D7009	20-10-08
' * fix bugID #20879: CR: "Use Responsive UI link" checkbox gets uncheck everytime page is refresh;

'D7008	20-10-08
' + Dashboards "space" panel;

'D7007	20-10-07
' * force to reset check flag for default option sets on edit workgroup or license;
' * update License.aspx for hide treatments parameters when not a Riskion license;

'D7006	20-10-06
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D7005	20-10-05
' * move code for T2S from alpha to CR;
' * allow T2S only for evaluation  steps (ignore results and so on);

'D7004	20-10-05
' * fix bugID #20854: Shortcuts link not showing for Controls for Sources page;

'D7003	20-10-05
' * update Project Details: Timeframe field now is open text

'D7001	20-10-01
' + clsComparionCore.CheckProjectManagerUserAsActive();
' * call CheckProjectManagerUserAsActive() on clsComparionCorePage.Init();
' * fix bugID #20735: RTE in Synthesize on the very first attempt to open Consensus view;

'D7000	20-10-01
' * minor optimization on calling CheckProjectManagerUsers() for avoid saving the same data;
' * check ProjectManager.User is Nothing on clsComparionCorePage.Init();
' * fix bugID #20735: RTE in Synthesize on the very first attempt to open Consensus view;

'D6999	20-10-01
' * fix possible RTE on call "set_alt_contributions" (Structuring.aspx);
' * fix bugID #18756: RTE: 'Object reference not set to an instance of an object.' [20450] [121250d];

'D6998	20-10-01
' * fix bugID #20842: Do not allow the PM to disable himself in the Participants page;

'D6997	20-10-01
' * update Riskion Project Details: show Assumption and timeframe on Project Description tab;

'D6996	20-09-30
' * disable auto-play for t2s on anytime evaluation when page was reloaded due to a session timeout;

'D6995	20-09-30
' + _PGHOTKEY_PREFIX as "shortcut";
' * improve clsMasterPageBase GetHotKeysList() for check custom title for shortcuts;
' * minor updates for shortcuts popup (make scrollable on small screen resolutions);

'D6994	20-09-29
' * improve webAPI/projects when Create() and default option sets can't be read;
' * show extended message when unable to create model and promt to restore default option sets;
' * auto-parse jAction.Message on send response when contains %%;
' * fix bugID #20836: CR: Can't create a model on my workgroup and resource not parsed %%model%%;

'D6993	20-09-29
' * fix bugID #20796: Dashboard: Bug when deleting a panel while in maximized view;

'D6992	20-09-29
' * fix bugID #20824: RTE: 'Object reference not set to an instance of an object.' [10001] [19eb4fa];

'D6991	20-09-27
' + _OPT_EVAL_T2S_ALLOWED option (disabled for CR/beta for now);

'D6987	20-09-23
' * fix bugID #20735: RTE in Synthesize on the very first attempt to open Consensus view;

'D6986	20-09-23
' * update Dashboards: force to reload charts data when adding a new portfolio view;
' * fix bugID #20732: Create Portfolio view dashboard panel;

'D6985	20-09-23
' * fix bugID #20784: Firefox: Freezed columns have white coulmn headers bg instead of gray;

'D6984	20-09-23
' * fix issue with missing Task on anytime evaluation screens;
' * updates for T2S auto-play;
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D6983	20-09-23
' * update TT structuring for avoid crashing when check permissions for anonymous user;
' * fix bugID #18756: RTE: 'Object reference not set to an instance of an object.' [20450] [121250d];

'D6981	20-09-21
' * ctrlMultiRatings.ascx: another view for prty values on Intensities list;
' * ctrlShowResults2: don't show nornalization when RiskReward model;
' * fix issue for Risk Reward cluster pharse when saving redundant event type "Risk, Reward";
' * fix issue with wrong URL (XSS/encoded) on save custom cluster phrase;

'D6980	20-09-18
' * disable notifications for anytime evaluation T2S (not supported, no auto-play);

'D6979	20-09-15
' * don't rememeber the last used panel type when adding to dashboard, placeholder by default;

'D6977	20-09-14
' * fix issue with T2S auto-play user option on anytime evaluation pipe;
' * minor fixes for tTS;
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D6976	20-09-14
' * try fix bugID #20754: Unnecessary scroll on Measurement Method screen;

'D6975	20-09-14
' * fix bugID #20753: QH in Scale Assessment;

'D6974	20-09-14
' * avoid crashing on CS strcturing when check user permissions while anonymous;
' * fix bugID #18756: RTE: 'Object reference not set to an instance of an object.' [20450] [121250d];

'D6973	20-09-14
' * enhance CheckProjectManagerUsers() for case when PM users list was changed and PM.User is nothing then try to re-init it;
' * fix bugID #20735: RTE in Synthesize on the very first attempt to open Consensus view;

'D6972	20-09-14
' * update Users.aspx: check each user for Admin role and disable his "login/view" links;
' * show msg when lin is empty on calling OpenLinkWithReturnUser() in mpDesktop.master;
' * fix bugID #20740: RTE when logging in another user;

'D6971	20-09-10
' * fix bugID #19579: RiskionNS: add new fields for Riskion Description (Time Frame and Assumptions);

'D6970	20-09-10
' * fix bugID #20719: CR Dashboard: Unending Loading when selecting All Participants on SA User;

'D6969	20-09-10
' * update ctrlMultiRatings for apply changes on hit Enter key and go to the next row;

'D6968	20-09-10
' * fix bugID #19579: RiskionNS: add new fields for Riskion Description (Time Frame and Assumptions);

'D6967	20-09-09
' * fix bugID #20733: RTE: 'Object reference not set to an instance of an object.' [20199] [28efc53];

'D6966	20-09-09
' * updates for t2s fucntionality (based on rr2);
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D6965	20-09-08
' * fix bugID #20723: RiskionNS: Hide the dashboard icon and page;
' * fix bugID #20724: Dashboard Export JSON no scroll bar;
' * fix bugID #20725: Typo error: "me" should be "be";
' * fix bugID #20726: Archives -- Is it correct that Dashboard is available but not the Reports workflow?

'D6964	20-09-08
' * fix bugID #20716: Reports: No letter "N" on left menu (for New feature);

'D6963	20-09-08
' * fix bugID #20720: CR Dashboard -- Toolbar gets disabled when clicked upload dashboard from file then cancel;

'D6962	20-09-08
' + ecText2Speech;
' + PM.Parameters.EvalTextToSpeech;
' + add "Text-to_speech" option on PipeParams;
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D6958	20-09-07
' * fix bugID #20721: Name the dashboard create options consistently;

'D6956	20-09-07
' + _FILE_KNOWN_ISSUES_RISKION (/knownissues_riskion.htm);
' * show "Known issues" depend on Comparion/Riskion;

'D6955	20-09-04
' * anytime evaluation draft text2speech feature;
' * fix bugID #20712: Implement text to speech for evaluation pipe;

'D6954	20-09-04
' * fix bugID #20709: Non-Responsive pipe -- The Cancel button is missing;

'D6952	20-09-03
' * some code optimization on creating xlsx report (combined generator);
' * fix issues on calling AddAlternativesWRTObjectivesInfodocs();

'D6951	20-09-02
' * keep wrtNodeID when re-load SA data on dashboards;
' * fix bugID #19668: Dashboard: WRT no longer working for Sensitivities (broken);

'D6950	20-09-02
' * fix bugID #19637: CR Dashboard: WRT node dropdown is not working;

'D6949	20-09-02
' * fix bugID #20705: Custom Column title is not reverting to default;

'D6948	20-09-02
' * avoid to sort PSA on dashboard (for to sort by prty);
' * save PM.Parameters on call pi/pm/params/?set_param_option;
' * fix bugID #19667: Dashboard: Sorting issues on Dynamic and Performance Sensitivity (broken);

'D6947	20-09-02
' * add ability to show plain text for psw fields (disabled for now);

'D6946	20-09-02
' * update all Riskion default option sets: add "ModifiedNationalIntelligenceCouncilScale" likelihood rating scale and make it default;

'D6945	20-09-02
' * update styles for avoid navigation messing/overlapping when narrow screens;

'D6944	20-09-01
' * show "use workgroup wording templates" on create model dialog;

'D6943	20-09-01
' * fix issue with disabled toolbar on dashboards when adding nwe dashboard/panel;
' * fix bugID #19677: CR Dashboard: "Obj not defined" error when adding Charts Panels / Toolbar getting disabled;

'D6942	20-09-01
' * fix issue with display "Disabled" on Wkg Manage Participants list;

'D6941	20-09-01
' + add ability to show "New" sticker for sidebar menu and Landing page items;

'D6940	20-09-01
' * rename Riskion default option sets;
' * enable storing the last used default option set on create a new model;
' * fix bugID #20698: CR: No default options set selected by default;

'D6939	20-08-31
' * fix bugID #20695: Dashboard: Change panel color still persists even after cancelling;

'D6937	20-08-31
' * update Comparion.sitemap for download Combined reports (XLSX) immediately;
' * fix bugID #19673: ExcelEport: Simplified UI to Generate Combined Report for Working Version;

'D6936	20-08-31
' * fix bugID #19692: Add a "(AnyTime Evaluation only)" note;

'D6935	20-08-31
' * fix bugID #20696: Rename to "Hide Combined Results";
' * fix bugID #19681: Can't create a new model when a Template is open;

'D6934	20-08-28
' * update CombinedReport.aspx: remove non-working links and some UI changes;
' * fix bugID #19673: ExcelEport: Simplified UI to Generate Combined Report for Working Version;

'D6933	20-08-28
' * fix bugID #19681: Can't create a new model when a Template is open;

'D6932	20-08-28
' * Projects List: enable context menu with a limited action for models, created with a newer code version;

'D6931	20-08-28
' * enhancements for "Create project" dialog on Projects List when don't need to provide the default value (previously used);
' * fix bugID #19681: Can't create a new model when a Template is open;

'D6930	20-08-28
' * update "Create project" dialog on Projects List when show "Default option sets" or "Templates" list and don't provide the default value (previously used);

'D6929	20-08-28
' + "Dashboards" tab;
' + add ability to show "New" icon next to workflow menu item;

'D6928	20-08-28
' + SASortMode enum, PM.Parameters.SensitivitySorting;
' * remember SA sorting when changing on SA dashboard panels;
' * fix bugID #19667: Dashboard: Sorting issues on Dynamic and Performance Sensitivity (broken);

'D6927	20-08-26
' * upgrade DevExtreme controls up to 20.1.7 (was 20.1.4);

'D6925	20-08-25
' * fix bugID #19667: Dashboard: Sorting issues on Dynamic and Performance Sensitivity;

'D6924	20-08-25
' * fix bugID #19677: CR Dashboard: "Obj not defined" error when adding Charts Panels / Toolbar getting disabled;

'D6923	20-08-24
' * force to reload all SA panels on dashboard when change normalization option;
' * force to reload data when ASA panel added o dashboard and had a different normalization in data_sa;
' * avoid crashing when try to change options while SA/Chart widgets are not loaded/render fully;
' * disable main toolbar while loading data on Dashboards;
' * update styles for dxToolbar when disabled and dxButtonGroup, just labels, dxList;
' * fix bugID #19636: CR Dashboard: Normalization Issues on Grid and Sensitivity Delta Alternatives;
' * fix bugID #19564: CR: Bug - Alternative priorities - not consistent across charts and grids;

'D6921	20-08-21
' + _OPT_RR_CUSTOM_WORDING, don't use a special wording for MyRiskReward models on call PrepareTaskCommon();
' * update MyRiskReward default option set for using "Sources" instead of "Strategies";

'D6920	20-08-20
' ! hide "Normalized for Selected" for all synthesize screens;
' + show "% of Maximum" normalization for dashboards;
' * fix bugID #19636: CR Dashboard: Normalization Issues on Grid and Sensitivity Delta Alternatives;

'D6916	20-08-20
' * reset Dashboard ASA panels and reload data_sa on change normalization;
' * fix bugID #19636: CR Dashboard: Normalization Issues on Grid and Sensitivity Delta Alternatives;

'D6915	20-08-19
' * add global Normalization option to dashboard toolbar;
' * fix bugID #19636: CR Dashboard: Normalization Issues on Grid and Sensitivity Delta Alternatives;

'D6914	20-08-18
' * hide "Add page to..." for non-PMs;
' * fix bugID #19645: Dashboards - Sharing;

'D6913	20-08-18
' * update dashboards "Add from sample" diloag: use dxTileView instead of dxList and make consistent view as for create new dashboard dialog;
' * fix bugID #19541: Dashboard: Add Sample Dashboard and Upload Dashboard (not completed);

'D6912	20-08-18
' * update GetPipeStepTask() and GetPipeStepHint() for detect the original Impact hierarchy;
' * fix bugID #19654: Rating scale assessment phrase says "likelihood" should read "consequence";

'D6911	20-08-18
' * improve webapi/pm/report with check user permissions for edit model, add logs/snapshots for new methods;
' * fix issue with dashboard "ViewOnly" mode for PMs;
' * update issues in ViewOnly mode for dashboards on client side;
' * allow most common options for viewers on dashboards;
' * fix bugID #19645: Dashboards - Sharing;

'D6910	20-08-18
' + viewOnly on Dashboard now available;
' * udpate dashboards screen for support ViewOnly mode;
' * fix bugID #19645: Dashboards - Sharing;

'D6909	20-08-17
' * fix bugID #19348: Efficient Frontier -- Chrome Unnecassary Horizontal scroll;

'D6908	20-08-17
' * improve jProject.GetActionResultByProject() for return for just Viewers the Dashboard screen by default on login;

'D6907	20-08-14
' * enhance webAPI with ability to import report items to current report/dashboard;
' * improve dialog with samples and now user can select the sample and after that create a dashboard by clicking a dialog button;
' * fix bugID #19541: Dashboard: Add Sample Dashboard and Upload Dashboard (not completed);

'D6906	20-08-17
' * improve Anytime invitation screen with abiliy tto specify role for user that doesn't have evaluation permission (i.e. Viewer) but with getting the right one URL (responsive or regular);

'D6905	20-08-14
' * fix bugID #19644: Dashboard download options changes;

'D6904	20-08-12
' * fix minor issues on download reports/dashboard;
' * fix issue with webAPI/pm/report/add_sample when the same sample more than once and was reloaded;

'D6903	20-08-12
' * fix issue with filtering alternatives for SA dashboard panels;
' * fix bugID #19565: Dashboards: Incorrect Alternatives List;

'D6902	20-08-12
' * add ability to switch alternatives filtering for dashboards;
' * fix bugID #19565: Dashboards: Incorrect Alternatives List;

'D6901	20-08-12
' + wepAPI/pm/report/add_sample;
' * minor updates and fixes for dashboard samples;
' * caching GetReportSamples();
' * fix bugID #19541: Dashboard: Add Sample Dashboard and Upload Dashboard (not completed);

'D6900	20-08-12
' * move getReport*Name from webAPI/pm/report to clsComparionCorePage and make Public;
' * use ".dash" and ".rep" extensions for downloaded dashboard and report jsons;
' * minor updates for download/upload dashboards;
' + clsComparionCorePage.GetReportSamples();
' + draft "Add Sample" dialog;

'D6899	20-08-11
' * fix styles for toolbar collpased buttongroup menu items;
' ! now ReportsListJSON() getting thr clsProjectsCollection (was clsProjectManager);
' * move code for parse JSON to clsReportsMeta/clsReportsCollection from webAPI to clsRepots.vb;
' * redo approach for upload dashboards/repots;

'D6898	20-08-11
' * enhance webAPI repotrs for allow to download specific report/dashboard or all;
' * updates for some menu styles;
' * fix issue with unable to upload dashboard when no dashboards in the model;

'D6897	20-08-10
' * update icon and tooltip for dashboard panel infodoc icon when expanded;
' * fix bugID #19477: Dashboard: Infodoc icon color doesn't update on maximized view;

'D6896	20-08-10
' + add draft "Alts filtering" button to Dashboards common toolbar;

'D6895	20-08-10
' * fix bugID #19623: The column choose heading/close button is not showing after reset;

'D6894	20-08-10
' * fix bugID #19477: Dashboard: Infodoc icon color doesn't update on maximized view;

'D6893:	20-08-10
' * enhance and refresh SA User button on dashboard common toolbar;
' * fix bugID #19345: Dashboard: Implement toolbar for sensitivities;

'D6892	20-08-07
' * fix bugID #19583: Dashboard: The check boxes are not visible on Grid's Select Participant/Groups;

'D6891	20-08-07
' * add dashboard common option "SA user";
' * update panels for normalization options and switch to normalized when distributive;
' * fix bugID #19345: Dashboard: Implement toolbar for sensitivities;

'D6890	20-08-07
' * fix bugID #19617: Dashboard: The WRT node is not being remembered;

'D6889	20-08-06
' * improve getProjectLinkDialog() in masterpage.js for allow to show Likelihood and Impact links for Riskion projects;
' * fix bugID #19605: RiskionNS: Get Links only has Likelihood links;

'D6888	20-08-06
' * fix bugID #19607: CR: The multi-pw sliders are right aligned on Scale Assessment pipe;

'D6887	20-08-06
' * fix issue with missing role groups for System Workgroup users list (related to #19610);

'D6886	20-08-04
' * allow to import reports from uploaded json;
' * fix bugID #19541: Dashboard: Add Sample Dashboard and Upload Dashboard (not completed);

'D6885	20-08-04
' * enhance Project List for don't allow to open non-supported models with newer than expected versions;

'D6884	20-08-04
' * improve clsReports: make all Deserialize() methods as shared, update all references;
' * enhance api/pm/report/upload function for avoid issues on parse data, add supporting for jsons with plain data and when meta info;

'D6883	20-08-03
' * hide "Cost" and "Risk" columns for AltsGrid by default;
' + add "Reset grid settings" button next to "Columns..." for dashboard panels;
' * fix bugID #19559: Dashboard: Cost and Risk are being shown by default on Alternatives Grid panel and the column chooser won't work;

'D6882	20-08-03
' * force to reset ContentOptions on change ItemType when calling api/pm/report/edit_item;
' * auto-update panel name on properties dialog when change type and was a default title;
' * updat resetElement() on dashboards;
' * fix bugID #19593: Dashboard: Can't resize panels after editing an option;

'D6881	20-08-03
' * fix bugID #19589: The Models History list is confusing due to different time zone;

'D6880	20-08-03
' * fix bugID #19590: Model Look up issues;

'D6879	20-08-03
' * update mpDesktop.master and don't use "no_scroll" by default, so need to call toggleScrollMain() when required;
' * fix bugID #19582: CR: Unnecessary scrolls and new unknown options for Grid Results;

'D6878	20-08-02
' * fix bugID #19559: Dashboard: Cost and Risk are being shown by default on Alternatives Grid panel and the column chooser won't work;

'D6877	20-07-31
' * force to use the general page name for Grids on add to dashboard;
' + store last chosen panel type, added to dashboard;
' * fix bugID #19585: Dashboard: Grid results panel is blank when it is first time to be created;

'D6876	20-07-31
' + ProjectManager.Parameters.Synthesis_WRTNodeID;
' * replace local SAWRTNodeID and GoalWRTNodeID with new Synthesis_WRTNodeID property;
' * fix bugID #19184: Dashboards: Options selected on Grids pages are not applied when added to the dashboard;

'D6875	20-07-30
' * fix bugID #19582: CR: Unnecessary scrolls and new unknown options for Grid Results;

'D6874	20-07-30
' * fix bugID #19584: Dashboard: Remove the light blue background on the panel tool bar;
' * fix bugID #19586: Dashboard: Panel toolbar is auto-collapsing when changing an option;

'D6873	20-07-29
' * new approach for prepareURLWithParams() in ec.reports.js;
' * fix wrong param anmes in onGetReportParams();
' * add missing default URLs for Alt/Obj Grid on dashboards;
' * fix bugID #19572: Dashboard: Open original page is not showing for Alternatives/Objectives Grid;

'D6872	20-07-29
' * rename noScroll() to toggleScrollMain() and remove all references for now;
' * force to avoid main scrolling for all pages based on mpDesktop.master;
' * fix bugID #19575: CR: Can't scroll the contributions horizontally (Chrome);

'D6871	20-07-29
' * updates for dashboards webAPI download/upload;

'D6870	20-07-28
' + ecReportsStreamType, clsReportsMeta;
' * redo pm/report/download for save clsReportsMeta instead on clsReportsCollection;
' * drafts for upload dashboard dialog and webAPI;

'D6869	20-07-28
' + add custom scrolling on drag dashboard panels;
' * fix bugID #19571: Dashboard is not auto-scrolling when trying to move a panel;

'D6868	20-07-28
' * add noScroll() to masterpage.js that toggle scrolling for main page area (mpDesktop.master);
' * use noScroll() for pages when was avoid scrolling via direct styles;

'D6867	20-07-28
' * fix bugID #19544: Unnecessary horizontal scroll on multi pw evaluation which makes the boxes cut-off;

'D6866	20-07-27
' * show dashboard descriprion as block below the title instead of the tooltip;

'D6865	20-07-27
' * fix bugID #19545: Evaluators can access the get model link and CS links;

'D6864	20-07-27
' * draft "Printing" for dashboards;

'D6863	20-07-24
' * another approach for reset dashboard panels (total dispose and generate new content for each panel);
' * enhance dashboards: show message on panels when covering objective selected as WRT for some panel types (SA's, objs charts/grid);

'D6862	20-07-24
' * fix issue with init sidebar menu items when check "no_aip" (was unexpectedly disabled when AIP enabled);

'D6861	20-07-24
' + add dashboard when dbl click on layout when Add new dashboard dialog;
' * fix bugID #19560: Dashboard: Is it necessary to hide the panel size for layout with predefined sizes?
' * fix bugID #19540: Dashboard: Resizing of Panels gets reset?

'D6860	20-07-24
' * fix bugID #19552: Dashboard: when browser is resized we force to "flow view" but icon is disabled and can still resize the panels;

'D6859	20-07-23
' * fix bugID #19545: Evaluators can access the get model link and CS links;

'D6858	20-07-23
' * fix bugID #19558: TT: "Show by pages" showing on Rating judgments distribution;

'D6857	20-07-20
' + add OptionsList() and GetOptionValue() to dashboard server side code;
' * restore WRTNodeID for dashboards;
' * fix some issues with reset/refresh panles on dashboards;
' * optimization for reload only specific panels when change some options that can take affect to data/priorities on dashboard;
' * fix issues with sync WRT node on dashboards;
' * allow to choose dashboards wrt node from objectives panels;

'D6856	20-07-20
' * new approach for common WRT node option on dashboards;

'D6855	20-07-17
' * fix RTE in ctrlSensitivityAnalysis.ascx (GetDecimalsValue);

'D6854	20-07-16
' - temporary hide WRT option on Dashboards toolbar;
' * fix bugID #19533: CR: Extra line on tool bar;

'D6853	20-07-16
' * fix bugID #19531: Gamma: Downloaded Alternatives/Objectives Sets has unknown file type;

'D6852	20-07-15
' * update Dashboards for hide SA's, Objs Grid/Chart content when AIP enabled;
' * updates for some styles on dashboards;
' * fix issue with edit items from "Panels list";

'D6851	20-07-15
' + add "Use CIS", "Combined Mode", "Distirbitive mode" to dashboards common toolbar;

'D6850	20-07-15
' + clsProjectManager.Parameters.DecimalDigits that replaces the local params;
' + add "decimals" on Dashboards common toolbar;

'D6849	20-07-15
' * fix bugID #19528: Gamma: Message when exporting RA's grid to excel file has HTML tags;

'D6848	20-07-15
' * updates on expand/collapse dashboard panels;
' * fix bugID #19524: CR - Dashboard: Panel color resets when maximizing;

'D6847	20-07-14
' * store view mode for dashboards in sessionState;
' * fix bugID #19523: CR: Should we disable the resizing of panels when Flow View is selected?

'D6846	20-07-13
' * update Anytime invitations screen for Riskion: show notification about "joined" pipes;

'D6845	20-07-13
' * another approach for case when no dashboard items;

'D6844	20-07-13
' * update dashboard layouts;
' * new layout for "empty" dashboard;
' * fix bugID #19476: Dashboard: Default Layout seems not to be working properly and Object not found error;

'D6843	20-07-10
' * allow to choose WRT node from different panels on Dashboards (in progress);

'D6842	20-07-10
' * code refactoring for dashboards screen;

'D6841	20-07-09
' * code refactoring for dashboards screen;

'D6840	20-07-09
' * draft "WRT node" on dashboards common toolbar;

'D6839	20-07-06
' * fix bugID #19487: RiskionNS: Monetary value in the pipe is rounded off and not in sync with Risk Results;

'D6838	20-07-06
' * fix bugID #19488: RiskionNS: Focus on the Monetary value goes to direct input box;

'D6837	20-07-03
' + dashboards view mode: regular and flow;

'D6836	20-07-03
' * don't add the virtual placehodler when no items in the dashboard;
' * replace "empty" layouts on create dashboard with a single placeholder versions;
' * fix bugID #19476: Dashboard: Default Layout seems not to be working properly and Object not found error;

'D6835	20-07-02
' * fix bugID #19479: Dashboard: Add page to button appears in non-advanced mode inconsistently;
' * fix bugID #19476: Dashboard: Default Layout seems not to be working properly and Object not found error;

'D6834	20-07-02
' * update wording for Dashboards;
' * update layout for dashboard header: show description as icon/tooltip;
' * redo dashboard panel header layout, fix issues with toolbars, update padding, mergins, etc;
' * dashboard panel properties dialog: don't show size for placeholder, add "background color" option;
' * updates for dashboard "Panels" popup;
' * fix bugID #19475: Dashboard: Resource not found error;
' * fix bugID #19478: Dashboard: The Dashboard notes remain on the screen even after deleting the dashboard;
' * fix bugID #19480: Dashboard: Be able to delete a placeholder;

'D6833	20-07-01
' * another approach for dashboard panel headers (comments);

'D6832	20-06-30
' + resizable panels on Dashboards screen;

'D6831	20-06-30
' * fix issue with load grids on Dashboards;
' + allow to reorder items on Dashboards "Jump to..." popup list (reorder dashboard items);
' + add quick commands for edit/remove item on dashboards popup list;

'D6830	20-06-30
' * minor fix for multi-projects downloading: fix splitting and set the limit to 100 models as one archive;
' * fix bugID #19449: Beta: Download multiple models is being downloaded into multiple separate zip files;

'D6829	20-06-30
' * add "hidelookup" tag for "roles" in sitemap that allows to hide some sitemap items when Page Lookup tree;
' * fix bugID #18584: Navigation improvement: create search field somewhere on every page to search through all available menu items (site pages);

'D6828	20-06-29
' * dashboard styles updates;

'D6827	20-06-24
' * improve PrepareTaskCommon() and use set of custom wording for MyRiskReward models for get the 'single' wording based on the plural;
' * update MyRiskreward master project for use "Strategies" as default wording;

'D6826	20-06-24
' * improve "Jump to..." feature: now it can be always-on-top dialog with list of panels;

'D6825	20-06-24
' * fix issue on SyncCheckRows() when compare values;
' * fix bugID #19373: TeamTime: Problem with showing the Steps List;

'D6824	20-06-24
' * fix issue with re-open Page Lookup dialog;
' * try to highlight the search on PageLookup tree;
' * fix bugID #18584: Navigation improvement: create search field somewhere on every page to search through all available menu items (site pages);

'D6823	20-06-24
' * update TeamTime SyncCheckRows() for a valid compare direct values when ratings;
' * fix bugID #19373: TeamTime: Problem with showing the Steps List;

'D6822	20-06-23
' + clsRatingLisne.Scenario, .clsPairwiseLine.ScenarioNameLeft, .ScenarioNameRight;
' + HierarchyTerminalNodes(), RR_GetScenario() on Anytime evaluation page;
' * update multiRatings and multiPW controls for show Scensrio names when Mixed/MyRiskReward models;

'D6821	20-06-23
' * fix issue with parse project lock info on call DBParse_Project (lock data was ignored in most cases);
' * fix issue with lock project as TT (due to missing data in LockInfo);
' * fix issue with check project state and TT/AT evaluations at the same time;
' * fix bugID #19373: TeamTime: Problem with showing the Steps List;

'D6820	20-06-22
' * fix bugID #19432: "Show by pages" is showing on results screens;

'D6819	20-06-22
' * updates for dashboard tile styles;
' * force to always show "Add page to..." as project action;
' * allow to add Alts/Objs Grid to dashboards;

'D6818	20-06-19
' * minor updates for jDialog;
' * fix bugID #19404: Clicking the number of participants evaluated with a specific intensity doesn't show the list of participants;

'D6817	20-06-18
' * fix bugID #19403: TeamTime Legend colors doesn't show;

'D6816	20-06-18
' * fix bugID #19396: CR/Beta: TeamTime Access Mode not working properly;

'D6815	20-06-18
' * fix bugID #19405: Hide "Show PW when using keypads and judgments are hidden" and "Sort by Keypad number";

'D6814	20-06-18
' * replace all dxDialog in TeamTime client code to jDialog since the devextreme library is not using for mpEC09_TT.master but jDialog loaded;
' * fix some dxDialog calls for Anytime evaluation;
' * fix bugID #19404: Clicking the number of participants evaluated with a specific intensity doesn't show the list of participants;

'D6813	20-06-16
' + clsComparionCore.Options.RiskionRiskRewardMode;
' + add /rr/ page for toggle RiskionRiskRewardMode setting;
' * update webAPI project/Open for ignore project descrip[tion when open MyRiskRewardModel;
' * show simplified "New MyRiskReward model" dialog;
' * don't show navigation on RiskReward page when RiskionRiskRewardMode enabled;
' * ignore UseWorkgroupWording cooki setting on create new model;
' * enhance ParseAllTemplates when MyRiskReward model and wording is "scenarios" to use "Scenario" for plural that overrides the workgroup settings;

'D6812	20-06-16
' * enhance Projects list "Create new project" dialog with feature that choose and block the default option set depend on choosen project type;

'D6811	20-06-16
' * dashboards fix issue with "jump to..." refresh on change current dashboard;
' * move "Dashboards" under the Reports menu tab;
' + "Reset SA" on common toolbar;

'D6810	20-06-16
' * fix issue with edit Monetary Value on evaluation screen;

'D6809	20-06-16
' * fix bugID #19387: Favorites column disappears when you go to Deleted tab and back to Active models tab;

'D6808	20-06-15
' * disable online help for TeamTime;
' * fix bugID #19391: TeamTime Help still trying to open old pages and has error;

'D6807	20-06-15
' * fix crashing on run TT due to call DevExtreme;
' * fix issue with redraw on change TT infodoc mode;
' * fix bugID #19389: TeamTime: Switch for Infodoc mode Tooltip/Frame is not working;

'D6806	20-06-12
' + add "Get Project Links" icon near to "star" for project title in the header (mpDesktop.master);

'D6805	20-06-12
' * don't show models marked as deleted when show starred;
' * fix bugID #19383: Should we remove Deleted Models from the Favorites list?

'D6804	20-06-12
' - disable showing msg when upload Riskion model to Comparion workgroup (webAPI/project/Upload);

'D6803	20-06-12
' * another approach for copy text to clipboard with ability to do it and don't use devExtreme library;
' * fix bugID #19377: Copy TeamTime link doesn't work in Chrome/FF;

'D6802	20-06-11
' * add code for QH edit/view on RiskRegister.aspx;
' * fix issue with crashing on call QH editor from embed Risk Results/Risk Map steps;

'D6801	20-06-11
' * fix bugiD #19375: RTE: 'The given key was not present in the dictionary.' [77100] [889cffa];

'D6800	20-06-11
' * fix possible RTE on call AddEmptyMissingJudgments when Edges is Nothing;
' * update layout for Erro page when short stack trace and long session details;
' * another approach for check unsaved data on Evaluation screen;
' * fix bugID #19347: Unending Please Wait when press Cancel on the prompt when trying to leave pipe with unsaved data;

'D6799	20-06-10
' + _PGID_STRUCTURE_MYRISKREWARD (20130);
' + _DEF_PGID_ONSELECTPROJECT_MYRISKREWAR, _DEF_PGID_ONNEW_MYRISKREWARD;
' + create dummy MyRiskReward page;
' * update jProject.GetActionResultByProject() for open new page for MyRiskReward models;

'D6798	20-06-10
' + [CanvasTypes] ProjectType.ptMyRiskReward (8);
' + enhance clsProject with .isMixedModel() and .isMyRiskRewardModel();
' + add ability to choose MyRiskReward on create new Riskion model;
' + update Project details for support new MyRiskReward model type;
' * update all calls for check Mixed model and add the same for MyRiskReward;
' * fix bugID #19365: RiskionNS: add new model type - MyRiskReward;

'D6797	20-06-10
' * fix bugID #19347: Unending Please Wait when press Cancel on the prompt when trying to leave pipe with unsaved data;

'D6796	20-06-08
' * fix issue with message on reset search on Projects List
' * fix bugID #19186: Reset search message from projects list is not working properly;

'D6795	20-06-08
' * update webAPI/project/Create for set riskion project type;
' * fix bugID #19353: RiskionNS: Menu Names for Opportunity Model is incorrect for newly created model;

'D6794	20-06-08
' * fix issue with infinite loop on parse screencast links when calling ParseVideoLinks();
' * fix bugID #19341: Model is hanging up on Model Description screen;

'D6793	20-06-08
' * fix bugID #19344: Riskion NS: no collapse/expand group buttons on risk results screens (also on other screens with grouping - Events Grid);

'D6792	20-06-08
' * upgrade devExtreme up to 20.1.4 (was 20.1.3);

'D6791	20-06-04
' * fix bugID #19186: Reset search message from projects list is not working properly;

'D6790	20-06-04
' * fix bugID #19324: RiskionNS: QH icon is missing;

'D6789	20-06-04
' * add ability to store the passcode for built TT pipe and reset it on change hierarchy;
' * fix bugID #19328: RTE when start TT in Impact and then Likeilihood;

'D6788	20-06-04
' + GetDefaultPath();
' * fix bugID #19338: Can't go back to Models list from Workgroup page;

'D6787	20-06-04
' * update WebPageUtils.GetNodesBelowForPipe() for avoid to ignore categories;
' * fix bugID #19327: Message that there are pipe steps that will be skipped;

'D6786	20-06-04
' * enhance Infodoc_GetInlineImages() for return the file format as well;

'D6785	20-06-03
' * fix bugID #19321: CR: Confirmation on Wording Template uses the old page name "Evaluation What" and Unending Please Wait;
' * avoid to reset last visited project for user when close model/logout (fix issue with restore last visited model after logout);
' * another approach for restore last visited page when autorized user session and opening the welcome screen;

'D6784	20-06-03
' * fix issue with load data for charts when no userIDs selected;
' * update default layouts on create new dashboard (add more layouts for common used 16:9 screens);
' * dashboard general toolbar updates;
' * fix issue with refresh "jump to" on edit dashboard data/item(s);

'D6783	20-06-03
' * new approach for select users dialog on dashboards;
' * fix issue with refresh charts on add/remove users;
' + "Jump to..." on dashboards;
' * fix issue with widget data on change dashboard;
' + add quick link to Dashboards screen when context menu on Projects List;

'D6782	20-06-02
' * updates for select users dialog on dashboards;

'D6781	20-06-02
' * fix bugID #19318: CR: Pages goes behind the left menu (broken);

'D6780	20-06-02
' + add new Riskion default option sets: MyRiskRewards;
' * fix bugID #19322: Riskion NS: Add MyRiskReward option set;

'D6779	20-06-01
' * update Error.aspx for change layout when long StackTrace; avoid to use mpEC09_TT.master for error pages;

'D6778	20-06-01
' * new approach for show layouts on add new dashboard;

'D6777	20-06-01
' * fix bugID #19310: RiskionNS: Help is showing when pressing F1;

'D6776	20-05-30
' + Infodoc_GetInlineImages();

'D6775	20-05-29
' * fix issue with lose RiskionProjectType on call webAPI project/copy;
' * fix for bugID #19296: Riskion - project type is lost when "Save As" or upload from a file;

'D6774	20-05-29
' * updates for dashboard widget icons;
' * imprive code for create custom dashboard widget elements;
' * new approach for add/edit Dashboard dialog;
' * one layout chooser on create dashboard without "cols" option;
' + "column chooser" for participants lsit dashboard widgets;
' * few dashboard fixes and updates;

'D6773	20-05-28
' * minor update for login screen when autocomplete for password is off for every case;
' * fix issue with default page on open Opprtunity Riskion default option set;
' * minor fix for layout issue on download section (Details.aspx);

'D6772	20-05-27
' * use _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS_4PIPE (RiskResults.aspx) as old _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS;
' * use _OPT_RISK_RESULTS_PGID as _PGID_ANALYSIS_RISK_RESULTS_ALL_EVENTS_4PIPE on anytime evaluation screen;
' * fix bugID #19286: RiskionNS: Evaluator is redirected to Models List instead of Risk Results;

'D6770	20-05-26
' * improve approach for navigate from Project Lookup screen;
' + add exporting to tabbed plain text from Project Lookup;

'D6769	20-05-25
' * fix issue with passing filesnames with dots on WorkgroupTemplates;

'D6768	20-05-22
' * fix crashing on read old Riskion models when alt.DirectJudgmentsForNoCause is Nothing;
' * fix bugID #19149: RTE: 'Object reference not set to an instance of an object.' [80031] [879d58a];
' * fix bugID #19268: RTE: 'Object reference not set to an instance of an object.' [99900] [2cfd19e];

'D6767	20-05-21
' * bunch of updates for fix some possible high impact issues from scanner report;
' * fix bugID #19207: Story: Resolve high impact static code vulnerabilities;

'D6766	20-05-21
' * updates for WrkgList, WorkgroupEdit, RichEditor, Evaluate and clsComparionCorePage: fix some possible high impact issues from scanner report;

'D6765	20-05-20
' * update Users.aspx for avoid to pass "all";
' * update Users.aspx for remove participants from the list after send the command to server;
' * fix bugID #18437: Remove participants removes the wrong participants;

'D6764	20-05-19
' - temporary disable the editing the project name on Projects List by double click on cell space;
' * updates for Project Lookup: add "Export" button, auto-update on switch navigation layout;

'D6763	20-05-18
' * fix bugID #19231: RiskioNS: Menus messed up when going to Structure screen;

'D6762	20-05-18
' * update ctrlDirectData.ascx for avoid set isChanged on load page and when no data really has been changed;

'D6761	20-05-15
' * update ctrlMultiRatings: show comments in other place with an extended header and "Save", "Cancel" buttons;

'D6760	20-05-14
' * Page Lookup draft popup;

'D6759	20-05-14
' * update ctrlMultiPairwise for fix issue when try to leave stgep with a few missing GPW judgments;

'D6758	20-05-14
' * updates for Snapshots: set focus on add/edit;

'D6757	20-05-13
' + restore functionality for switch navigation layout;
' + independent navigation layout setting for Riskion and Comparion;
' + Riskion2.sitemap;

'D6756	20-05-12
' * update sidebarmenu.js, prepareSidebarItems() for disable sidebar menu items when no_aip and alts specified;

'D6755	20-05-12
' * update sidebarmenu.js for show/hide workflow elements/row when single navigation element (no workflows under the current tab when Riskion);

'D6754	20-05-11
' * fix issue with "Non-Commercial Use Only" header text for TT (mpEC09_TT.master);
' * fix issue with missing extension for "Dowload as Excel" from RA - Allocate;

'D6753	20-05-11
' * updates for dahsboard widget toolbars;
' * fix bugID #19163: beta-dashboards: The page is scrolled down by default;

'D6752	20-05-11
' + add "Ctrl+#" shortcut for jump to pipe step on anytime evaluation;

'D6751	20-05-11
' * fix bugID #19205: RTE: 'Arithmetic operation resulted in an overflow.' [77102] [75ebef7];

'D6750	20-05-08
' * fix issue with reset/overide some dashboard item options on load;
' * fix issue with resize widget toolbar on dashboards;
' * another approach for code that return the custom widget toolbar elements;

'D6749	20-05-08
' * another approach for widget toolbars on dashboards;
' * fix bugID #19163: beta-dashboards: The page is scrolled down by default;

'D6748	20-05-08
' - disable Alternatives/Obejctives Grids for dashboards;
' + draft version of dashboard widgets toolbar;
' * fix issue with resize content on collapse expanded widget;

'D6747	20-05-07
' * upgrade TinyMCE up to 5.2.2 (was 5.0.1);

'D6746	20-05-07
' * upgrade devExtreme up to 20.1.3 (was 20.1.2 beta);

'D6745	20-05-07
' + web.config option "Google-UA";
' + add system setting for specify the Google UA;
' + call Google analytics for welcome screen;

'D6744	20-05-07
' * add shortcuts for list of snapshots and add snapshot;
' * redo Riskion Identify/Structure menu;

'D6743	20-05-04
' * update ways how to cache content (clsComparionCorePage.PreInit and web.config <location> sections);

'D6742	20-04-30
' * fix bugID #19164: beta-dashboards: "Add page to" is missing on Synthesize Grids;

'D6741	20-04-30
' * fix bugID #19168: beta-dashboards: Clicking "Add page to" for Sensitivities just reloads the page;

'D6740	20-04-30
' * fix bugID #19168: beta-dashboards: Clicking "Add page to" for Sensitivities just reloads the page;

'D6739	20-04-30
' * fix bugID #19162: beta-dashoboards: Cloned dashboard won't show up until page is refreshed;

'D6738	20-04-30
' * fix bugID #19161: beta-dashboads: Use a consistent default Dashboard name;

'D6737	20-04-30
' * fix issue with user menu tooltip when Antigua;

'D6736	20-04-30
' * fix issue with parse resources when Antigua under the Anonymous;

'D6735	20-04-29
' * fix issue with download models (fixed doDownload() in masterpage.js);
' * fix issues with dialog on "Add page to..." when adding to dashboard;
' * fix bugID #18933: beta-dashboards: Add page to dashboard is not existing (only add reports but has error);

'D6734	20-04-28
' * update Users.aspx for generate "read only" link on server side;
' * fix issue with open Gecko "View pipe" from Participants screen;
' * fix bugID #19151: The user name/email is incorrect on View Only pipe;

'D6733	20-04-23
' * fix issue with parse JObject in clsComparionCorePage.ParseJSONParams();
' * fix issue with layour/render ajax error popup when stack trace/scrollable content;
' * fix wording with wrong template on copy model when license limit;

'D6732	20-04-22
' + add extra details about used settings on Anytime Invitations: "Group Specific Links";

'D6731	20-04-22
' * update Gecko anytime evaluation for show/hide navigation buttons, evaluation progress;
' * fix bugID #19134: Gamma: Hide navigation box for Responsive;

'D6730	20-04-22
' * fix bugID #19137: Gamma: Rich text editor not loading in responsive pipe;

'D6729	20-04-22
' * fix bugID #19139: Hide the "navigation box" and legend when navigation buttons are hidden;

'D6728	20-04-21
' * update fontawesome up to 5.13.0 (was 5.12.1);
' * update devextreme controls up to 20.1.2 beta (was 19.2.6);

'D6727	20-04-21
' + StringFuncs.CutHTMLHeaders();
' * call cutting-off the html headers for some situations when infodoc will be included inline as part of page (evaluations, surveys, etc.);
' * fix wrong css rule for .sidebar-menu ul li a in main.scss;
' * fix bugID #19131: The left menu spacing is too big;

'D6726	20-04-21
' * don't add step with rank alternatives to anytime evaluation pipe;
' * fix bugID #19133: Gamma: Unknown step being added on the responsive pipe when there is Alternatives Checklist insight question;

'D6725	20-04-20
' * enhance CheckProjectManagerUsers() for check if ProjectManager.UsersList has users with blank names;
' * update ImportProjectUsers() code for init user name for current user when his account is missing in Project Manager;
' * fix bugID #18927: No participant name when uploading or opening a model the user is not previosuly included;

'D6724	20-04-20
' * fix issue with RASolverPriority.Clone() that cause RTE on copy data;
' * enhance CopyScenario() on RA Main screen;
' * fix bugID #19126: Infeasilblity Analysis: Modify the original scenario is not working;

'D6723	20-04-20
' * fix bugID #19128: Rich Text editor tool bar in Comparion and Responsive are different although they are same TinyMCE;

'D6720	20-04-17
' + add "Reset" icon next to slider on ctrlDirectInput.ascx;
' * fix issue with code when not allowed missing judgments for Utility Curve anytime evaluation steps;
' * fix bugID #19112: The next button remains disabled even after entering judgment (data);

'D6719	20-04-17
' * enhance ResourceAligner.SetAlternativeCost() for check CC value and update vaklue with a cost when non-empty and equal to old cost value;
' * update all SetAlternativeCost() calls for proper work with synced CC$;
' * fix bugID #19113: RA: Be able to format Costs (dollar, Euro, etc.);

'D6718	20-04-17
' * fix bugID #19122: Duplicate "Not Rated" in the pipe;

'D6717	20-04-16
' * fix issue with read "Select Alternatives" survey question;

'D6715	20-04-16
' * update Start.aspx and clsComparionCorePage.Authenticate() for set user name as "Anonymous";
' * update user menu icon when Anonymous (as we had for CS);
' * don't show idle popup about help on anytime evaluation when Riskion model;

'D6714	20-04-15
' + _OPT_PARSE_YOUTUBE_LINKS (true), _OPT_PARSE_SCREENCAST_EMBED (false);
' * update ParseVideoLinks() (was ReplaceYoutubeLinks) with check new options;

'D6713	20-04-15
' * update anytime evaluation controls (DI, SF/UC) for hide framed infodoc(s) when no data;
' * fix bugID #19111: Unnecessary infodocs for events with no source;

'D6712	20-04-15
' * enhance ctrlUtilityCurve.ascx for support onChangeAction and add HasUndefined() property;
' * update anytime evaluation for support "Not allow missing judgments" for SF amd UC steps;
' * fix bugID #19112: THe next button remains disabled even after entering judgment (data);

'D6711	20-04-14
' * enhance anytime invitation group selector and add ability to open the Participants screen/Manage Groups dialog;

'D6710	20-04-14
' * fix bugID #18927: No participant name when uploading or opening a model the user is not previosuly included;

'D6709	20-04-14
' * fix issue with manual project lock in webAPI/project Set_Lock();
' * update mpDesktop.master for check and refresh project status icons (lock, on-line);
' * fix bugID #18053: Responsive pipe -- Evaluators can still access the pipe even model is Locked;

'D6708	20-04-14
' * remember "Don't show" QH option only for a session;
' * fix issue with enabling "Next" button for direct input whn no missing judgments allowed;
' * fix issue with adjust popup sizes when no resize_custom specified;
' * fix bugID #19108: RTE in the pipe when changed the measurement method;

'D6707	20-04-13
' * fix issue with update project user name when account is missing in the Project manager;
' * fix issue with setup a right role group when attach Wokrgroup Manager/Admin via call AttachProject();
' * fix bugID #19103: Participant name blank in model and cannot be updated;

'D6706	20-04-13
' * update ctrlMultiRatings for keep scroll on mobile devices, more narrow intensities list;

'D6705	20-04-10
' * extra fix for redirect to TT when asking about Geklo anytime while TT session in progress;
' * fix bugID #19034: Error when tried to open anytime pipe when TT is ongoing;

'D6704	20-04-10
' * fix bugID #19096: RTE: 'Unrecognized Guid format.' [30019] [eac68d9];

'D6703	20-04-10
' * update anytime evaluation for hide "*" footnote when hidden navigation in the joined Riskion pipe;

'D6702	20-04-10
' * enhance Risk Results with showing user score;

'D6701	20-04-09
' * fix bugID #18917: Be able to search model using access code even with dash sign;

'D6700	20-04-09
' * fix bugID #18941: Opening the view only pipe of the admin opens the logged in PM's pipe instead;

'D6699	20-04-09
' * fix issue in Project_HID_Move() in webAPI/project/;
' * fix issue with copy hierarchy on upload Comparion model to Riskion and choose "both";
' * 18844: Riskion NS: wrong hierarchies detection on model upload;

'D6698	20-04-09
' * fix bugID #19034: Error when tried to open anytime pipe when TT is ongoing;

'D6697	20-04-08
' * update TeamTime pipe for proper resize PW list on resize infodoc frames;
' * fix bugID: #19072: Infodocs overflow boundaries in Teamtime in Edge browser - also has unnecessary scrollbars;

'D6696	20-04-08
' * improve clsComparionCore.ProjectCreateFromFile() with an extra code for check the .ahps file for Impact hierarchy (is it Riskion model or Comparion);
' * update code for check hiearrchy on upload model via webAPI;
' * fix bugID #18844: Riskion NS: wrong hierarchies detection on model upload;

'D6695	20-04-07
' * fix bugID #19084: The Intuitive ranking grid wastes white spaces to the left (mobile);

'D6694	20-04-06
' * fix issue with "Don't automatically show" option on QH dialog for evaluators;

'D6693	20-04-06
' * disable devextreme scroll inits in masterpage.js due to a render issues;
' * fix bugID #19080: Make the scrolling of unfreezed column obvious;

'D6692	20-04-06
' * fix bugID #19072: Infodocs overflow boundaries in Teamtime in Edge browser - also has unnecessary scrollbars;

'D6691	20-04-06
' * enhance masterpage.js with block of devextreme inits;
' * fix bugID #19080: Make the scrolling of unfreezed column obvious;

'D6690	20-04-05
' * special patch for devExtreme "dots" icon when collpased toolbar items: replace with fontawesome dots icon;

'D6689	20-04-05
' * CtyptFuncs: remove #Const and add FIPS_MODE (False by default);
' * update Encrypt()/Dectypt() for use .Net or BouncyCastle depend on FIPS_MODE;
' + _OPT_FIPS_MODE, add webConfig option "FIPSMode";
' ! rename CanvasWebBase.EcSecurity to .XSS due to conflict with namespace ECSecurity;
' * update Install/Settings.aspx for show real config value fo FIPS_MODE;
' + show "FIPS mode" on /Install page;
' + enhance welcome screen for check FIPS mode (is it possible to use .Net methods) and show warning;
' + ?action=fips for temporary (user session) swith FIPS mode to ON;
' * fix bugID #19075: Add switch for "FIPS" DLL on install page;

'D6688	20-04-04
' * disabled animation on show/hide some popups (masterpage.js);
' * fix for avoid crashing on load multiratings when can't find assigned rating scales;
' * try to auto-reset created pipe for non-evaluation pages (clsComparionCorePage.LoadComplete());
' * fix bugID #19074: RTE on Likelihood pipe (model dependent);

'D6687	20-04-02
' * change flashing the word on multi ratings;
' * fix bugID #19070: Worrries with no judgments disappeared when going back to the impact judgment step;

'D6686	20-04-02
' * update PipeParams options: "Hide navigation box/Evaluation progress" and dependent "Hide navigation step buttons";
' * update anytime evaluation screen for show/hide navigation box/buttons depend on changed options;

'D6685	20-04-02
' * fix bugID #19065: The numbers in judgments steps are changing, can we just hide them?

'D6684	20-04-02
' + add "First" button before the "Previous" on the evluation screen when navigation box is disabled;

'D6683	20-04-01
' * redo for showing the Quick Help popups at the center of page;
' * another approach for highlight the word in the "Rate the ..." for ctrlMultiRatings;

'D6682	20-04-01
' * update isHTMLEmpty() with check <iframe> as well;
' * enhance Infodoc_Pack() with using isHTMLEmpty();
' * fix bugID #19013: Can't save the embedded media when the editor is blank (workaround is to enter text, etc.);

'D6681	20-04-01
' * fix bugID #18396: RTE: 'Object reference not set to an instance of an object.' [30010] [aa18ed3];

'D6680	20-04-01
' * flash wording on ctrlMultiRatings;

'D6679	20-04-01
' * don't show Quick Help for RiskResutls screen when just a regular page;
' * update masterpage.js showQuickHelp for show popup at the bottom and try to get the % of screen size;
' * enable option for show Quick help once;

'D6678	20-04-01
' * update ctrlMultiRatings.ascx LoadInfodocFrame() for prevent to use editURL when user can't edit infodocs;
' * fix bugID #19061: Permission error on Scale Description;

'D6677	20-04-01
' * fix bugID #19056: The Signup/login forms move to the left when changing the font size;

'D6676	20-04-01
' * minor updates for Rank alts grid when no data message;
' * redo approach for track previous pipe step;
' * remove some unsafe plugins for timyMCS (insert code, etc);
' * fix bugID #19068: Clicking Save should not auto-show the QH;
' * fix bugID #19042: Always show the quick help until user selected to not show automatically;

'D6675	20-03-31
' * enhance ReplaceYoutubeLinks() with a trying to parse screencast.com iframe and try to adjust the width;
' * fix bugID #19045: Auto-resize the embedded video in QH?

'D6674	20-03-30
' - hide Help icon for Riskion;
' * fix issues with edit QH directly from QH popup;
' * minor updates for QH icons on Risk Results; fix Heat Map resize issue when options in isWidget mode;
' * fix bugID #19059: Alternatives intuitive ranking feature;

'D6673	20-03-30
' * enhance GetPipeActionNodes() with adding EmbeddedContent/AlternativesRank;
' * updates for jHTMLArea (fix icons, add commands, fix z-index for color picker);
' + allow to edit task for Alternatives Rank pipe step;

'D6672	20-03-30
' * update masterpage.js: OpenRichEditor(), showQuickHelp();
' * enhance showIFrame with ability to use custom buttons;
' * allow to scroll frame content when loaded via showIFrame();
' * improve showing QH and edit infodoc with calling for parent/opener when possible;
' * another approach for show and edit QH on Evaluation screen and for RiskResults;
' * improved option "Don't automatically show" for QH as show popup[ option instead of overflow infodoc block;
' * enhance ReplaceYoutubeLinks() with a different approach with new embed code and auto-resize to fit on page;
' * enhance reize_custom on sidebarmenu.js for auto-resize opened dxPopup when resize window;
' * fix bugID #19041: Quick Help new modal bugs;
' * fix bugID #19045: Auto-resize the embedded video in QH?

'D6671	20-03-30
' + EmbeddedContent new type AlternativesRank;
' * update clsPipeBuilder.CreatePipe for analyze welcome survey questions and add an extra pipe step for rank alternatives;
' * update evaluation page for show draft step with grid for rank alternatives;

'D6670	20-03-30
' * fix bigID #19042: Always show the quick help until user selected to not show automatically;

'D6669	20-03-26
' * fix issue with showing "Embed Risk Map" option on PipeParams;
' * fix issue with preview Quick Help on embedded RiskResults pages;
' ! move onShowIFrame() from mpDesktop.master to masterpage.js;
' * preloader with change opacity when RiskResults as Riskion pipe step;

'D6668	20-03-26
' * another approach for showing Quick Help as popup dialog but not browser window popup;

'D6667	20-03-26
' * force to show the current user data on Heat Map when riskion pipe (isWidget);
' * fix bugID #19015: Riskion: add Heat map as the last one step in the joined pipe;

'D6666	20-03-26
' * fix bugID #19039: Riskion-Gamma: Risk Map grid sorting issue for Evaluators (COVID-19 model);

'D6665	20-03-26
' * fix issue with Quick Help icons on Riskion embedded screens (RiskResutls);
' * fix bugID #19016: Riskion: add Quick Help to Risk results and Heat map pages;

'D6664	20-03-26
' + EmbeddedContentType.HeatMap;
' + add option for Heat Map (Embed Risk Map) to PipeParams;
' + show HeatMap as pipe step in Impact evaluation;
' * update RiskResults for HeatMap fow hide controls and adapted resize;
' * redo approach for Prev/Next buttons on RiskResults, placing QuickHelp icons depend on the mode (isWidget);
' * update some properties on RiskResults for avoid saving to DB when isWidget mode;
' * fix bugID #19015: Riskion: add Heat map as the last one step in the joined pipe;

'D6663	20-03-25
' * update RiskResults as show buttons for HEatMap when came from Riskion pipe;
' * navigate between the Riskion pipes, Risk Results and Heat Map;

'D6662	20-03-25
' * fix issue with hover on disabled buttons (.button class);
' + ecEvaluationStepType: RiskResults, HeatMap;
' + GetEvaluationQuickHelpForCustom() and SetEvaluationQuickHelpForCustom();
' * update RiskResults for show Quick Help icons when came from pipe; add all dependent code for call rich editor, infodoc preview, etc.;
' * enhance RichEditor.aspx and Infodoc.aspx for support new QuickHelp types (Risk Results, Heat Map);
' * fix bugID #19016: Riskion: add Quick Help to Risk results and Heat map pages;

'D6661	20-03-24
' * update layout and some styles for ctrlMultiRatings.ascx;
' * replace most window.onresize with resize_custom;
' * auto-hide sidebar menu on Risk Results when came from Riskion pipe;

'D6660	20-03-24
' * update ctrlMultiratings: show prefix for Riskion scales and blink on change scale;

'D6659	20-03-24
' * mode code for set user psw from Users.aspx to clsComparionCore.SetUserPassword();
' * fix some issues on set psw and check complexity;
' + add a limit for hashes history (since we can store max 35 hashes in the DB field);
' * use SetUserPassword on Account/Edit page;

'D6658	20-03-23
' * replace dialog fpor reset user password on Users.aspx;
' * redo user psw hashes for using another structure;
' * fixes and updates for passfiled.js;
' * fix issue with validate psw complexity;

'D6657	20-03-22
' * minor updates for passfield.js;
' - _DEF_PASSWORD_MIN_LIFETIME, _OPT_PSW_MIN_LIFETIME;
' + clsComparionCore: GetPswComplexityPattern(), GetUserPswHashes()*, SetUserPswHashes()*, ValidatePswComplexity();
' + ecExtraProperty.UserPswHashesJsonData, ecExtraType.User;

'D6656	20-03-20
' * update Projects List and View Logs for show dates as other approach;
' * fix bugID #19008: Problem on Last Access Data sorting;

'D6655	20-03-20
' * enhance passfield.js with extra code/events;
' + add strength bar for psw input field; move most code for init to masterpage.js

'D6654	20-03-19
' * enhance passfield.js with extra code/events;
' * redo password field on Account/Edit;

'D6653	20-03-19
' * show "Save snapshot" icon on workflow row when Structuring meeting screen;

'D6652	20-03-18
' * fix bugID #18996: Issues with Models' list Advanced Mode;

'D6651	20-03-18
' * update CS for set user as PM;
' * fix bugID #18994: CS - allow to make those participants who have accounts on the site PM's for the model;

'D6650	20-03-18
' * update filtering for datagrid datetime fields on Projects List;
' * fix bugID #18978: Improve how Date Filter should work;

'D6649	20-03-17
' * update ViewLogs for allow serach by User and Object;

'D6648	20-03-16
' * fix bugID #18973: Riskion: wrong pipe step on evaluate Control Effectiveness from the bow-tie;

'D6647	20-03-16
' * fix bugID #18982: Snapshot name not parsed;

'D6646	20-03-16
' + add Password complexity settings: "PswHighComplexity", "PswMinLen", "PswMaxLen", "PswMinChanges", "PswMinLifetime", "PswMaxLifetime", "PswPrevHashes";
' * enhance code for process ajax callbacks and check some values on Settings.aspx;

'D6645	20-03-11
' * minor fix for _PGID_ERROR_403 url (was ?code=403 that fill passcode on login form);
' * don't show details on callback error 503;

'D6644	20-03-10
' * fix issue with user logout via webAPI and reset FedRAMP notification when session expired;
' * fix bugID #17094: After session time out, conceal information previously visible on the display with the system use notification (AC-11);

'D6643	20-03-10
' * update clsMasterPageBase.DTCode() for check the last modify date of /bin/ folder as well;
' ! remove Fetch*() functions from clsMasterPageBase to clsComparionCorePage;
' * update all calls for Fetch*() functions (webAPI);
' * update Error page: hide the msg about pushing button on submit data;
' ! update FetchAccess() and close connection via Fetch* when callback/ajax;
' * update Synthesis page for move code from .Init() to PreLoad(), add check in PreInit() section;
' * update all RA pages for move RA.Load() from PreInit to Init event;
' * update masterpage.js, _call_error() method: ignore tech details for some codes where need to just show error title, minor error popup updates;
' * fix bugID #18427: RTE: 'Object reference not set to an instance of an object.' [60202] [afebe52];
' * fix bugID #18953: RTE: 'Object reference not set to an instance of an object.' [77100] [03f4c82];

'D6642	20-03-09
' * add reset FedRAMP notification to UserLogout();
' * call UserLogout() when no return user in CheckReturnUser() (was App.Logout());
' + _OPT_LOGOUT_AFTER_TIMEOUT, (web.config "LogoutAfterTimeout");
' + add _LOGOUT_AFTER_TIMEOUT global option;
' * enhance mpDesktop.master for check _LOGOUT_AFTER_TIMEOUT and auto-lgout user with showing popup;
' * fix bugID #17094: After session time out, conceal information previously visible on the display with the system use notification (AC-11);

'D6641	20-03-09
' * fix issue with export to Excel on View Logs;
' * fix bugID #17092: Workgroup managers should be able to produce audit logs for the workgroups they are workgroup managers for;

'D6640	20-03-09
' + add _OPT_ALLOW_ADMINS_LOCAL_ONLY ("AdminLocalOnly" in web.config);
' + clsComparionCore._ADMINS_LOCAL_ONLY;
' + aeLocalhostAllowedOnly, update GetMessageByAuthErrorCode();
' * update clsComparionCorePage.Authenticate for check _ADMINS_LOCAL_ONLY whn admin and localhost;
' * check FEDRAMP_MODE on welcome page when App.ApplicationError.Status=ecErrorStatus.errAccessDenied and redirect to FedRAMP notification page;
' * fix bugID #16371: FedRAMP admin account should ONLY be able to login from the portal on the Azure Government site;

'D6639	20-03-09
' * fix bugID #18937: RTE: 'Object reference not set to an instance of an object.' [80010] [1dd763c];

'D6638	20-03-07
' * another fix for scrollers in the popups (infodocs, related to case 18935);
' * update style for Riskion tabs when scrolling row;

'D6637	20-03-07
' * update layout for Error page;
' + show msg about "Feedback" buttons in case of RTE and when submit available;
' * update code for auto-submit RTE feedbacks;

'D6636	20-03-06
' * update styles and layout for header/wg-details-container: adjust resizing for section with wokgroup selector/project name;
' * show workgroup selector as regular combobox on mpDesktop.master;

'D6635	20-03-04
' * minor updates for Projects Statistic page;
' * update roles for View Logs page for allow to get access for Workgroup Managers;
' * enhance api/manage/ DB_GetLogEvents() for get the logs filtered by WorkgroupID;
' * update web.sitemap for don't show "Manage System" for workgroup managers;
' * save workgroup ID on logs on user logout when active project (was empty workgroup in logs);
' * "Workgroups" page (_PGID_ADMIN_WORKGROUPS) now available only for admins (at_slManageOwnWorkgroup);
' * "View Logs" page (_PGID_ADMIN_VIEWLOGS) now available for Workgroup Managers (at_alManageWorkgroupUsers);

'D6634	20-03-04
' + clsComparionCore.AvailableWorkgroupsAsWM() for get the list of workgroup where active user is Workgroup manager (for case 17092);

'D6633	20-03-03
' + add draft View Logs page;

'D6632	20-03-03
' * fix bugID #18901: Need to re-arrange the buttons in Invite participants screen;

'D6631	20-03-03
' * enhance snapshots screen with extra view mode: Sankey chart;

'D6630	20-03-02
' * update clsPipeParameter: .JudgementPromtID and JudgementAltsPromtID for set as 0 when -1 and no custom wording;
' * update PipeParams for show default wording for promt (obj/alt) when no custom/empty option;
' * fix issue with default promt wording when upload ahp(z) model;

'D6629	20-03-02
' * fix bugID #18890: RA: issue with import Custom Constraints dialog in Chrome;

'D6628	20-02-29
' * update tooltips and styles for RA Main grid;

'D6627	20-02-29
' * show tooltips for RA Main Grid cells;

'D6626	20-02-27
' * fix License.aspx build issue;
' * hide "Too many items.." on Project lookup;
' * make workgroup selector more wider;

'D6625	20-02-27
' * minor updates for replace hierarhy dialog (masterpage.js);
' * force to show msg when upload Comparion2Riskion or vise versa even when have model for replace;
' * fix bugID #18844: Riskion NS: wrong hierarchies detection on model upload;

'D6624	20-02-26
' * replace all jDialog() calls with dxDialog();
' * fix bugID #18568: Family model - Evaluation Pipe - 0x800a1391 - JavaScript runtime error: 'jDialog' is undefined...;

'D6623	20-02-25
' + ec.report.js new structure _rep_dash_itemslist tha allow to group dashboard items by categories;
' * fix default pageID for Pros&Cons dashboard item;

'D6623	20-02-25
' * updates for masterpage.js: showErrorMessage() now always displays popup msg (was notify for non-erros), fix issue with call from showResMessageCallFunc when no error;

'D6622	20-02-25
' * updta RA Main: keep funded for alternatives before gettins the sub-optimal allocations (table, download) and restore funded state after;
' * fix bugID #17254: BetaNS/Beta: "Delete alternatives that aren't funded" deletes all the Alternatives;

'D6621	20-02-24
' * update masterpage.js use dxPopup instead of dialog.custom() when call moveProjectHID() for disabled ability to hide dialog by pressing ESC;
' * update dialog replaceProject() for don't show empty lines (when no data on project by ID);
' * fix bugID #18844: Riskion NS: wrong hierarchies detection on model upload;

'D6620	20-02-21
' * update clsComparionCore.ProjectCreateFromFile(): remove checking alternatives for avoid switching project .isRisk status since it cause losing RA Riskion data;
' * fix bugID #18596: RiskionNS: Losing RA data when downloading model;

'D6619	20-02-19
' + clsComparionCoreOptions.ignoreOffline;
' * update Authenticate() for set .ignoreOffline when passed specific paramerter in hash;
' * fix bugID #18826: RiskionNS: Loggin another user when project is offline;

'D6618	20-02-19
' * update clsMasterageBase.LoadMenuItems for override shortcuts when another hierarchy;
' * fix bugID #18825: RiskionNS: Opening the pipe using Ctrl+M should be based on the active workflow (Likelihood, Impact, and Controls);

'D6617	20-02-18
' * use resources on Landing.aspx;
' * fix bugID #18805: The custom wording is not applied on Add Alternatives / Objectives modal;

'D6616	20-02-18
' * update Users.aspx for open Gecko for preview pipe;
' * fix bugID #18554: Preview pipe from Measurement Methods still opens the non-R pipe;

'D6615	20-02-17
' * fix bugID #18805: The custom wording is not applied on Add Alternatives / Objectives modal;

'D6614	20-02-17
' * update code for init widget options on Dashboards; some fixes;

'D6613	20-02-17
' * update styles for RadToobar for render it more close to devextreme toolbar;

'D6612	20-02-13
' * add dashboard support for ASA, H2H, 2D analysis;

'D6611	20-02-12
' * fix issue with redirect to Gecko pipe when preview pipe called from Measurement Methods;
' * fix bugID #18554: Preview pipe from Measurement Methods still opens the non-R pipe;

'D6610	20-02-13
' * split ecReportItemType .Chart and .Sensitivity to individual items;

'D6609	20-02-12
' * update devextreme up to version 19.2.6 (was 19.2.5);
' * update fontawesome up to 5.12.1 (was 5.12.0);

'D6608	20-02-12
' * fix bugID #18774: Make the Signing in/logging in option selected by default and make the selection obvious when copying the link;

'D6607	20-02-12
' * fix bugID #18770: Revise the copy/paste TT invitation;

'D6606	20-02-11
' * updates and fixes for Dashboards resize/aspect ratio; dashboard/item dialogs;

'D6605	20-02-10
' + add ability to show/select layout on create dashboard;
' * enhance webAPI pm/report for import dashboard layouts;
' * updates for defaults in ec.report.js;
' * few updates and changes for dashboards, related resources;

'D6604	20-02-10
' * refresh for update workgroup participants list when remove system managers;
' * fix bugID #18502: After removing Admin from model, it comes right back;

'D6603	20-02-10
' * fix bugID #18575: Do we need to have a darker blue shade when a model is OPEN?

'D6602	20-02-10
' * try to fix the misplaced "Show by pages" hover for TeamTime screens;

'D6601	20-02-10
' * update dialogs for add/edit dashboard and add/edit dashboard item;
' * new approach for resize dashboard items/auto aspect ration for fit cells on the screen;

'D6600	20-02-07
' * cut code for attach users from PM to project from ImportProjectUsers() to new routine ImportUsersFromProjectManager();
' * update CheckProjectManagerUsers() that returns the boolean for now and use ImportUsersFromProjectManager();
' * enhance CheckProjectManagerUsers() with using ImportUsersFromProjectManager();
' * call CheckProjectManagerUsers() on webAPI pm/project/ open;
' * try to fix users list on open model (restore missing from PM and vise versa);

'D6599	20-02-06
' * fix bugID #18427: RTE: 'Object reference not set to an instance of an object.' [60202] [afebe52];

'D6598	20-02-06
' * revert back downloading the file via memory buffer in DownloadFile() (was .TransmitFile);
' * fix bugID #18743: Gamma: Error when downloading Datagrid;

'D6597	20-02-05
' * update ec.report.js dashboard rutines for use callback functions after load/update data;
' * update dashboards for pass callback functions;
' * redo dialog for dashboard properties: now 6 or 8 cells per row and square or adaptive cell aspects;
' * fix issues with render grid layout in ec.dashboard.grid.js;
' - ecReportItemType.Image;
' * redo dashboard approach for calculate cells count (adaptive and have a min limit), cell sizes/aspect;

'D6596	20-02-05
' * fix bugID #18740: Button labels are truncated unnecessarily;

'D6595	20-02-05
' * rollback changes for RawResponseStart() due to closing connection before pushing content;
' * fix bugID #18740: Button labels are truncated unnecessarily;

'D6594	20-02-04
' * update ParseOptions() for webapi pm/report;
' * fix issue with dropbownButton caret;
' * update layout for Dashboard title;
' * hide unused dashbord item types, some properties;
' + allow to use aspect ratio for dashboards;
' * minor update for dashboard width/sroller;
' * fix issue with saving dashboard item comments;

'D6593	20-02-04
' * workaround for response with a valid filename when non-English chars for FF;
' + add DownloadFile(), DownloadContent(), DownloadStream() to clsComparionCorePage;
' * replace almost every piece of code for download any content with just added functions;
' * fix bugID #18564: Family model - Project List - Incorrect character encoding in downloaded file;

'D6592	20-02-04
' * fix scrolls issue on Synthsize;
' * fix bugID #18479: Unnecesary scrolls (Edge) and do not use gray as default color;

'D6591	20-02-03
' * update most calls for download file with calling SafeFileName() and encode filename for a passing valid non-english chars;
' * fix bugID #18564: Family model - Project List - Incorrect character encoding in downloaded file;

'D6590	20-02-03
' * call Gecko' pipe for preview from the Measurement Methods screen;
' * fix for bugID #18554: Preview pipe from Measurement Methods still opens the non-R pipe;

'D6589	20-02-03
' * update all cookies timeout for end of 2019 with  new date for end of 2037;
' * fix bugID #18569: Family model - Evaluation Pipe - "Information" masage about help is annoying and can't be turned off;

'D6588	20-02-03
' * update clsProject.ProjectManager: create clsUser in case of missing in .ProjectManager.UsersList();
' + clsComparionCore.ProjectCheckPropertiesFromPM() (moved from webapi on Upload());
' * call ProjectCheckPropertiesFromPM() after restore model from the snapshot (enhance SnapshotRestoreProject());
' * fix bugID #18687: RiskionNS: RTE in Measurement Methods screen;

'D6587	20-02-03
' * 18715: The sub-menu from User Name redirects to the first page instead of the selected menu;

'D6586	20-01-31
' * don't show Release notes for Riskion (projects list popup and user menu item);
' * minor updates for WhatsNew featuer: show release notes;

'D6585	20-01-30
' * update wording for snapshots on call dashboards/reports webAPI;
' * update ec.sa.js for declare default options, add getChangedOptions();
' * update Charts, SA pages for return the list of params on add page to report/dashboard;
' * update Dashboard screen for init widget params, based on .ContentOptions for each item;

'D6584	20-01-30
' * update clsMasterPageBase for check header controls and add timestamp to js/css but try to process the head_JSFiles placeholder;

'D6583	20-01-30
' * enhance RA Alternatives Attributes: fix alignment, auto-parse hyperlinks;

'D6582	20-01-30
' * update navigation for fix issue with  Project Lookup references and Project Statistic position/access;
' * fix bugID #18502: After removing Admin from model, it comes right back;

'D6581	20-01-29
' * show Release Notes for the latest version from popup on Projects List, but Release Notes list from User menu;

'D6580	20-01-29
' * fix bugID #18682: Funding pools enhancements;

'D6579	20-01-29
' - temporary hide "Share page link" due to security reasons;
' * enhance sidebarmenu,js: option for ability to show the user menu as content menu (with submenus in compare with regular dropdownbutton);

'D6578	20-01-28
' + centerPopup() to masterpage.js;
' + add to api/pm/report/: private ReorderReportItems(), SetItemProperties() and public Edit_Item, Clone_Item;
' * improve api/pm/report/ Add_Item, add sorting items by index after change items collection; minor enhanceents;
' * enhance ec.report.js with an extra methods, fixes for dashboards;
' + add ability to add, edit, clone dashboard items; improve dialogs on add/edit dashboard and add/edit item;

'D6577	20-01-28
' * update EULA version up to "200128";

'D6576	20-01-28
' * fix issue with swipe/mouse drag on Dashboards;
' * allow to show Dashboard tile content menu by right click at the cursor position, fix sisue with show tile menu in expanded mode;
' * minor updates for Dashboards screen;

'D6575	20-01-27
' * ParseJSONParams is public now, enhance for avoid crashes on parse jObject (json sub-objects);
' * Dashboards webAPI: Add_Report(), Edit_Report(), Clone_Report() now saving the report Options, correct wording for Logs;
' + Dashboards webAPI methods: ParseOptions(), Delete_Item();
' * updates for forms, saving data, default values, etc in ec.reports.js;
' * updates for render tiles, enhance toolabr buttons, add context menu for items; bunch of fixes;

'D6574	20-01-27
' + clsReport.Options;
' + add ability to add DSA to Dashboards;
' * fix loading/init DSA on dashboards;

'D6573	20-01-26
' * rename ecReportItemType.AlternativesChart to ecReportItemType.Chart, remove ecReportItemType.ObjectivesChart;
' + Sensitivity, Counter, Image, Page to ecReportItemType;
' * updates and fixes for ec.reports.js, add dashboardsCheckDefaults();
' * fix issue with render some items on Dashboards, fix drag-n-drop functionality;
' * redo approach for get content and resize items on Dashboards.aspx;
' * improve process for load data for some items on Dashboards screen;

'D6572	20-01-26
' * enhance GetReportCategory, GetReportType, GetReportItemType;
' * fix issue with adding Charts to reports/dashboard due t a list of custom params;
' * updates and improvements for dashboards client side;
' * move most dashboard code to ec.reports.js;

'D6571	20-01-25
' * another approach for dashboards: add real data via webAPI, add functionality for add/edit/clone/delete dashboards;

'D6570	20-01-24
' * update styles for Riskion tabs;
' * force to hide any overflow for body and main table for avoid to show scrollers (can be an issue on pretty narrow screens when no scrollers);
' * fix bugID #17999: NS: Chrome/Firefox -- Screens have unnecessary scrolls (broken due to new menu changes?);

'D6568	20-01-23
' * check the system workgroup license only for a critical params on clsMasterPageBase;
' * always show System workgroup for admin when instance is locke due to a license limits;
' * fix for check ecLicenseParameter.MaxWorkgroupsTotal when calling CheckLicense() for a critical params only;
' + aeTotalWorkgroupsLimit, update GetMessageByAuthErrorCode();
' * update clsLicense.CheckAllParameters();
' * update Logon() for return ecLicenseParameter.MaxWorkgroupsTotal when occured;
' * fix issue with user login and showing available workgroups when specific system license limits is exceeded (i.e. MaxModels);

'D6566	20-01-22
' * fix bugID #18667: RiskionNS: Menus in Impact Landing page says "Sources" instead of "Objectives";

'D6565	20-01-22
' + "Erase Password" button on manage Workgroup Participants screen;
' * fix bugID #18655: Be able to set blank password to multiple users;

'D6563	20-01-21
' * redo approach for render and resize grid on Users.aspx;

'D6562	20-01-21
' * fix bugID #18655: Be able to set blank password to multiple users;

'D6561	20-01-21
' * fix bugID #18654: Password is being set when adding new users even "Generate Random Password" is left uncheck;

'D6560	20-01-21
' * fix bugID #18649: Wording issue for pipe for controls;

'D6559	20-01-20
' * fixes for License page: fix page title and parse message ablut license error/limit;

'D6558	20-01-16
' * update styles for grid checked/active rows;
' * fix bugID #18602: Inconsistent UI: Make Grid Alternating White/Gray;

'D6557	20-01-16
' * hide tooltips on show CS context menu;
' * fix bugID #18637: CS Issues;

'D6556	20-01-15
' + ec_tabs, ec_tabs_nocontent style that overrides styles for dxTabPanel;
' * update Details, Permissions, Charts, Grids, both Invite, RiskTreatments pages for use new styles for tabs;
' * fix bugID #18619: Inconsistent UI: Inconsistent tabs;

'D6555	20-01-14
' * fix issues with resize on expand/collapse dashboard items; fix issue with init/resize on load charts/sa data;
' * add pre-loader for frames, charts, SA cells on Dashboards;
' * update toolbar and UI buttons for Dashboards; update dashboard styles;

'D6554	20-01-12
' * update default colors for active dxButtonGroup item(s);
' * update colors for ec_slider (blue, green);
' * fix bugID #18601: Inconsistent normal and hover colors for the icons;

'D6553	20-01-12
' + _DEF_LOCK_TT_CONSENSUS_VIEW (3 mins by default);
' * update TeamTimeStartSession() for pass optional parameter is it started from Consensus View for setup a short timeout;
' * fix issue with setup a lock timeout on TT screen, update lock timeout depend on mode when came from Consensis View screen;
' * fix bugID #18553: Do we need time out to automatically stop a TT meeting?

'D6552	20-01-09
' * Personal Settings: hide section for setup psw and some optins when SSO;
' * fix issue with non-parsed msg for license screen when license limit text have templates;
' * hide information about the password for Token.aspx; UserInfo.aspx;
' * restrict to call webAPI /account/ "user_password" when SSO;
' * clsMasterPageBase: always return empty string on call GetPageLink() when SSO is enabled;
' * update login screen and show login form when SSO/SSO only when localhost request;
' * force to DebugDisableAutoComplete4Logon when SSO;
' * don't fill the login form and never store psw/rememberme when SSO enabled;
' * pass all query params on calling SSO for pass it with a return URL;
' + clsComparionCore.SSO_User;
' * update SAML/Assertion.aspx: track query params from targetURL, detect the debug mode; set SSO_User when assertion completed;
' * fetch access to Password.aspx when SSO;
' * update all pages when used _TEMPL_URL_RESETPSW for hide it when SSO;
' * hide icon and functions for set user psw on Users.aspx (project, workgroup);
' * update Start.apsx?: hide password field and regular login form when SSO enabled; show link to /?action=sso_start when user already registered;
' * return msg instead of the real link when calling CreateResetPswURL() while SSO enabled;
' * updates for URLWithParams() when absolure URL with params due to issues on anti-XSS;
' * fix bugID #17981: Implement SAML for user sign-on;
' * fix bugID #16371: FedRAMP admin account should ONLY be able to login from the portal on the Azure Government site;

'D6551	20-01-08
' * update Riskion sitemap tab/workflow for identify model: Details/Description and Visual Structuring as workflow uner the Identify tab;

'D6550	20-01-07
' * update SAML config;
' + _PGID_SSO_ASSERT;
' * update code OpenSSO for specify the root URL as targetURL on get response from IdP;
' + customizable web.config option "SSO_Only" (isSSO_Only property);
' * redo approach for show welcome screen and regular login form, only TT login form, only SSO button or combinations;
' * update code for check SSO option and current session and open SSO when required to force it;
' * fix bugID #17981: Implement SAML for user sign-on;

'D6549	20-01-07
' + ec.reports.js (move most reports related code from mpDesktop.master and masterpage.js);
' + allow to choose the Dashboard on add page to report/dashboard; fix issue wth select dest report type;
' * updates for "Add page to..." button and related dialogs;

'D6548	20-01-07
' ! new enum values in ecReportType (available to use as [Flag] in future);
' + ecReportType.Dashboard;
' * update  clsReportItem: Options splitted to ItemOptions and ContentOptions;
' * updates for webAPI /pm/report/;
' * update resize for Project Details;
' * minor updates for Dashboard screen and embedded frames;

'D6547	20-01-07
' * redo styles for Dashboards;
' + add ability to expand/collapse cells on Dashboard;

'D6546	20-01-06
' * try to reset expired TT session no delend on the lock status and current user;
' * fix bugID #18553: Do we need time out to automatically stop a TT meeting?

'D6545	20-01-06
' * fix bugID #18555: The Add button changed in the latest gamma;

'D6544	20-01-06
' * fix bugID #18539: Remove the page title on the left hand side of the workflow menu;

'D6543	20-01-06
' * minor updates for dashboard data structures, add support of URL (iframe);
' * draft "Add new Item" element on Dashboards;

'D6542	20-01-06
' * supress transform cells on click for dxTileView;
' * update layouts and few fixes for Dashboard cells;
' * redo approach for render cell content with abiltiy to specify a custom routine for cell resize;

'D6541	20-01-05
' * enhance layouts for dashboard cells; add scrollable area for content;

'D6540	20-01-04
' * redo approach for render cells on Dashboard; improve UI/layouts for cells;

'D6539	20-01-04
' * enhancements for Dashboard darft screen: support list of dashbpoards, allow to show "empty" items;

'D6538	20-01-04
' * draft code for draggable TileView items on dashboard screen;

'D6537	20-01-03
' + add new Dashboard page (blank for now);

'D6536	20-01-03
' * rename Dashboard.aspx to Charts.aspx; update references;

'D6535	20-01-02
' * replace all templateButtonSubmenu() with a DropDownButton (master page (usermenu), Project list, CS, Hierarchy, Participants list);
' * update some styles;
' * fix bugID #18527: The caret icon looks misplaced ;

'D6534	19-12-30
' * update DevExtreme up to 19.2.5 (was 19.1.4);
' * update Font Awesome up to 5.12.0 (was 5.10.1);

'D6533	19-12-30
' * on get the last visited model/page on call Authenticate() when DebugRestoreLastPage is enabled, check is model availabel to open;
' * fix bugID #18494: The last page visited for an overridden model is still remembered;

'D6532	19-12-30
' + _SSO_MODE (_OPT_SSO_MODE), add isSSO() that can be customize via UI;
' + _SSO_ONLY for allow login only via SAML IdP;
' + ecAuthenticateWay.awSSO, dbActionType.actSSOLogin; update related routines (logs, stat);
' + clsComparionCorePage.OpenSSO();
' * enhance Authenticate() with an optional parameter for ignore SSO; call OpenSSO() when SSO used on perform login;
' * update welcome screen for use OpenSSO; show SSO button only when isSSO;
' * update /SAML/Assertion.aspx for auto-create user on get response from IdP; disable debug mode;
' * force to open IdP page on open welcome screen when _SSO_ONLY is enabled;

'D6531	19-12-29
' * updates for Report Generator / List of Reports;

'D6530	19-12-27
' + allow to specify the name for item. that will be added to report;
' * new layout for Report Generator page;

'D6529	19-12-26
' * add resources for "Add page to report" dialog;
' + confirmation about opening the Report after adding the page to report;
' * pre-load reports on Report Generator page;

'D6528	19-12-26
' * update webAPI pm/report for create document based on ReportType, add dummy methods for Spreadsheets, Presentations;

'D6527	19-12-26
' * "Add to Report": allow to select the report type on create new; fix issues when no reports;

'D6526	19-10-25
' * new "Add Page to Report" dialog UI;

'D6525	19-12-24
' * updates for "Add to Report" dialog UI;
' * add doDownload() to masterpage.js and replace all similar code for download;
' - clsReportItem.StartWithPageBreak;
' + ecReportItemType.PageBreak;

'D6524	19-12-24
' * add ability to select the destination report on "Add page to report" (even create a new);
' * updates for Reports webAPI;

'D6523	19-12-24
' * fix bugID #18453: Responsive: images in information documents;

'D6522	19-12-23
' + "Add page to report" added for project details, obj/alts editors, datagrid, dashboard (not fully supported);
' * minor fixes and updates;

'D6521	19-12-23
' + enhance webAPI pm/report/ with methods for manage reports itself: add, edit, clone, delete, download; export all as .json;
' * update Project Generator page and allo to manage reports;
' + clsMasterPagebase.FetchWrongObject();
' * fix issue with calling AddReport(); redo approach for load/save reports: now called on load/save ProjectManager.Parameters;

'D6520	19-12-20
' * fix bugID #18441: Use the same icon used in other screens for view only and login another user;

'D6519	19-12-20
' * fix bugID #18442: Help widget won't show up on first click (broken);

'D6518	19-12-17
' * update way how to specify the readonly paramter for clling Gecko in view only mode (now plain, was as a part of hash);
' * fix bugID #18398: Open the responsive view only pipe from evaluation progress;

'D6517	19-12-17
' * enhance submenu for Download toolbar button on Project List: now items depend on selection: sinlge/multi;
' * fix issue with download ahpz file(s) from Projects List;
' * show Download toolbar button for Riskion as regular button with ability to download only ahps;
' * fix bugID #18416: Can't upload the single ahpz model downloaded from the model's list download button;

'D6516	19-12-17
' * fix bugID #18417: The send mail window is being remembered when logging in;

'D6515	19-12-16
' * another approach for Report Genertor layout/UI;

'D6514	19-12-16
' * fix bugID #18415: Revert back to show Dynamic first rather than Performance Sensitivity;

'D6513	19-12-16
' * new UI layout with scrollable areas and extended toolbars on Report generator page;

'D6512	19-12-16
' * fix bugID #18398: Open the responsive view only pipe from evaluation progress;

'D6511	19-12-13
' + add method "list" to webAPI pm/report;
' + add draft /Project/Report/Default.aspx (Project Generator) page;

'D6510	19-12-12
' - _OPT_HELP_LOAD_ONLOCAL;
' + _OPT_HELP_LOAD_ONDEMAND (true by default), clsComparionCorePage.LoadOnlineHelpOnDemand();
' * on-line help wodget and content now loading on demand only when user pressed F1 or (?) icon;

'D6509	19-12-12
' * allow to the init help article for page than is not a part of workflow navigation;
' * fix bugID #18395: Single clicking to change the active tab on the models list doesn't work (broken);

'D6508	19-12-12
' + _OPT_HELP_LOAD_ONLOCAL for avoid laoding the online help scripts immediately for local instances;
' * update mpDesktop.master for load online scripts/init help widget on demand for local instances;

'D6507	19-12-11
' * minor updates for getting list of online participants;
' * updates for track active user as on-line;
' * minor updates for TeamTime client side code on analyze changes;
' * fix bugID #18343: In TT, welcome page and info docs keep reloading every few seconds;

'D6506	19-12-11
' * fix bugID #18388: TeamTime: Can't select Show Users by Pages;

'D6505	19-12-10
' * fix bugID #18388: TeamTime: Can't select Show Users by Pages;

'D6504	19-12-09
' * update template for an empty infodoc (_TEMPL_EMPTY_INFODOC, used when no html/body tags) - now default css will be loaded from theme folder;
' * update all calls when parse _TEMPL_EMPTY_INFODOC;
' * update some preloaders for iframe on TeamTime;
' * TeamTime.GetUsersPageEnabled is off by default;
' * enhance JS_SafeNumber for keep only _OPT_JSMaxDecimalPlaces (10) digits after the point;
' * update TeamTime client side for try to avoid page redraw on callbacks;
' * fix issue with flickering welcome/thank you page frames;
' * fix bugID #18343: In TT, welcome page and info docs keep reloading every few seconds;

'D6503	19-12-09
' + clsReportItem.Options is Dictionary(Of String, String);
' + ecReportType;
' + add few properties to clsReport* classes, add extra checks and sorters;

'D6502	19-12-08
' + ECCore/clsReport: clsReportsCollection, clsReport, clsReportItem, ecReportItemType;

'D6501	19-12-08
' * enhance CombinedReport page with an option for choose format;
' * improve webAPI pm/report/ download() for use a custom format on create document and use GemBox' api for download cerated document;

'D6500	19-12-08
' * fix issue with resize content on Report/Custom.aspx;
' * improve Inconsistency Report with ability to use Responsive pipe for view and open pipe for specific node/user (FB18349);

'D6499	19-12-07
' * fix issue with removed reference to JQuery libs (_URL_JQUERY_JS,  _URL_JQUERY_UI_JS);
' * fix bugID #18376: TeamTime: unable to start TT session;

'D6498	19-12-05
' * update resize/layout issues for ctrlMultiPairwise.ascx;
' * fix bugID #18319: Change the color to normal but make it italic;

'D6497	19-12-05
' * update Model Participants sxcreen for allow to set psw, get link for current user in any case; aloow to view pipe for Admin;
' * fix bugID #18361: No need to limit a PM on some actions (reset password, login another user, get link) of another PM?

'D6496	19-12-04
' * fix issue with missing participants on Evalaution progress (due to a different case in user-emails in the system and int he model itself);
' * fix bugID #18360: Beta: Missing Participants in Evaluation Progress list;

'D6495	19-12-04
' * updates for save User.LastVisisted in clsComparionCorePage.Page_LoadComplete even for ajax/callbacks/ping/etc;

'D6494	19-12-04
' * try to fix possible RTE on call .doInherit() for jProject and jUserTeamTime;
' * try to fix bugID #17943: RTE: 'Non-static method requires a target.' [99900] [845724e];

'D6493	19-12-04
' + add draft method for create pdf with uploaded image as webAPI/pm/dashboard "gembox_pdf_img" method;
' * enhance DownloadFile() for specify way how to download file: as attachment or try to load inline;

'D6492	19-12-03
' * update Users.aspc for check isAuthorized and fetch access for avoid RTE;
' * fix bugID #18344: RTE: 'Object reference not set to an instance of an object.' [20020] [d56d65a];

'D6491	19-12-03
' - clsComparionCorePage.isHTML5Page;

'D6490	19-12-03
' * enhance Details.aspx project statistic with a counter hyperlinks to relative pages;
' * never show "#" (ModelID) column on Projects list, show "Starred" column in Basic mode; update columns version;

'D6489	19-12-03
' * updates and fixes for Remove model participants (Users.aspx);
' * fix issue with show users with judgments on Detail.aspx;

'D6488	19-12-03
' + show "Add Page to Report" for pages when onGetReportLink is specified;
' + add addReportPage() to mpDesktop.master for analyze and combine the params for report link;
' + add list2params() and params2list() to misc.js;
' * update Dashboard.aspx for support URI params as init values;
' * simplify header on Dashboard screen;

'D6487	19-12-02
' * enhance misc.js with str2Blob() function and canvas.toBlob() prototype;
' * update masterpage.js: enhace uploadBinaryData() for use headers depend on browser;
' * update showErrorMessage() for hide loading panel and show error dialog for parent window when possible;
' * update ec.chart.js for upload blob data (was DataURL);
' * hide iframe for report dynamic content on CombinedReport.aspx;

'D6486	19-11-29
' * update clsTextModel.ParseUserLine with ability to use space as delimeter for email and name;
' * update upload dialog and add ".txt" as allowed extension;
' + add "Text model" as one more item on download formats (Details screen);

'D6485	19-11-28
' * call Antigua_ResetCredentials() on SetActiveProject and on terminate/disconnect CS when PM;
' * fix issue with geting the previously opened in CS the project from session on call RichEditor;
' * fix bugID #18308: Infodocs from another model is getting cached and is displayed on another model?

'D6484	19-11-28
' * fix context menu on RA Funding Pools screen;
' * fix issue with header indexing/render on Funding Pools when pools are ignored;
' * fix validation for FP limit inputs;
' * fix bugID #18242: Duplicate Funding Pools when Ignored;
' * fix bugID #18243: Can't specify pool limit when funding pool is ignored -- wrong error message;

'D6483	19-11-27
' * fix bugID #18297: Beta: Creating New model is redirecting to CS?

'D6482	19-11-27
' * update masterpage.js checkProjectOnlineAndCopyLink() for try to copy link at first for avoid to show the copy dialog for FF/Chrome;

'D6481	19-11-26
' * fix issue with createProject() params on Projects list;
' * fix bugID #18295: RiskionNS: wrong new model dialog;

'D6480	19-11-26
' * fix confirmation on set/edit license for workgroup with a different type (Comparion vs Riskion);

'D6479	19-11-26
' * update callAPI() in masterpage.js: encode "&" on pass data;
' * fix bugID #18291: RTE: 'Unterminated string. Expected delimiter: ". Path 'description', line 1, position 81.' [99900] [8010144];

'D6478	19-11-26
' * update RA Main: try to colorize the removed constraints (cells/inputs) on infeasibility analysis;

'D6477	19-11-26
' * show button/dialog for infeasibility solutions only when more than 1 solution found;
' * fix issue with removed constraints list/tooltip;

'D6476	19-11-26
' * redo approach for show removed constraints on Infeasibility analysis;
' * update for list of possible solutions: sort by benefit, show active, fix select scenario (solution);

'D6475	19-11-26
' * redo layout for infeasibility analysis options dialo, add option for solutions count;
' * enhance RA Main with show list of infeasibility analysis solutions;
' * pre-load all possible solutions with removed constraints on perform infeasibility analysis;
' - InfeasibilityResult;
' + add InfeasibilityRemovedConstraints, InfeasibilityOptimalValue and InfeasibilityScenarioIndex to RAScenario;
' * update RASolver.GetInfeasibilityResults() for resturn list of RAScenario (was list of InfeasibilityResult);
' * enhance RAScenario_Comparer with support new option raScenarioField.InfeasOptimalValue;

'D6474	19-11-25
' * enhance CombinedReport export form builder with "list" support;
' * fix bugID #18180: UI to implement and test Report Generator Options;

'D6473	19-11-25
' * update callAPI() calls with a string arguments for avoid to use encodeURI* funcs (unable to stingify to JSON properly in these cases);
' * fix bugID #18260: RTE: 'After parsing a value an unexpected character was encountered: p. Path 'passcode'... [99900] [1f88652];

'D6472	19-11-25
' * enhance sidebarmenu.js/updateHelpPage() with code for check the links and auto-open if only one article;

'D6471	19-11-25
' * fix webAPI/project/users issue with getting list of online participants when no active project;
' * update Details screen for update form data when current project has been changed outside;
' * fix bugID #18275: Project Details - can't copy links to the clipboard;

'D6470	19-11-21
' + _PGID_EXPORT_DASHBOARD_ALTS, _PGID_EXPORT_DASHBOARD_OBJS;
' + add dummy Project/Analysis/Export.aspx

'D6469	19-11-20
' * update mpEmpty.master for allow to show "Non-commercial use only" watermark;

'D6468	19-11-20
' * fix hasFilters() on Project list shen header filter applied;
' * totally reset all filters on Projects list when call resetFilters();
' * try to fix tooltip issues on Projects list: use another element and try to repaint on update project name;
' * fix bugID #18248: IE: Projects list tooltip is doubled and has HTML tags;

'D6467	19-11-21
' * update RA Main grid: show "$" constraint values as cost;

'D6466	19-11-21
' - disable checking for release notes in SL shell;

'D6464	19-11-20
' * RA_OPT_ALLOW_FREEZE_TABLES = false due to a render lags;
' + RA_OPT_CC_ALLOW_DOLLAR_VALUES;
' + RAConstraint.isDollarValues (when linked to cat attrib and ends with a $ sign);
' * try to switch to Gurobi when model is infeasibility when XA and possivble to use Gurobi;
' * mark columns that used as Solver priority with  dark green header on RA Main;
' * update SyncLinkedConstraintsValues() for update RAConstraint with .isDollarValues (use Alt' Cost instead of '1');
' * Gurobi solver as default SolverLibrary;
' * update SetAlternativeCost() for check linked cat attribute and update value when .isDollarValues;

'D6463	19-11-20
' * fix bugID #17725: BetaNS: The download button for datagrid is missing Reports > Datagrid;

'D6461	19-11-19
' + reObjectType.Uploads;
' * enahnce callAjax() in masterpage.js for allow to submit multipart request with a raw form data;
' + add method uploadBinaryData() to masterpage.js;
' + webAPI service/?action=upload;
' + add "Inline upload" test button on Combined Reports screen;

'D6460	19-11-18
' * update Password.aspx for ignore cases when user set a new psw exactly as before (new = old);
' * fix bugID #18208: Reset Password -- Can't save password using the old password and no clear error message;

'D6459	19-11-18
' * masterpage.js: call cancelPing() on open new model, on close model;
' * fix bugID #18210: Why do we need a prompt when switching model?

'D6458	19-11-18
' * redo header layout;
' * hide footer "Non-Commercial Use only" block;
' * fix bugID #18217: Remove the For Commercial text at the top since it is already at the footer;

'D6457	19-11-18
' * update Microsoft.ReportViewer.dlls from ReportViewerRedist2010;
' * update ctrlReport.ascx for enable export actions;
' * fix bugID #17579: BetaNS: The Download options for Predefined Reports are missing;

'D6456	19-11-18
' * fix bugID #18211: Rename "projects" to "models";

'D6455	19-11-14
' + clsComparionCorePage.HasParam();
' + ReportOptions() session public value on CombinedReport.aspx;
' + add dialog with export options absed ona list of params;
' * enhance webAPI pm/dashboats GenerateWordReport() for create document with a specific options;
' + GetReportOptions on webAPI pm/dashboard;
' * implement bugID #18180: UI to implement and test Report Generator Options;

'D6454	19-11-13
' * fix crashing on get the user e-mail for show header on Password.aspx;
' * fix bugID #18199: RTE: 'Object reference not set to an instance of an object.' [40051] [66bf8b6];

'D6453	19-11-13
' * update UpdateWorkgroupUsersGroupID() for avoid crash on get the list of projects when user was Wkg Manager before (Users.aspx in wkg mode);
' * fix bugID #18198: RTE: 'Conversion failed when converting the nvarchar value '1@eci.com' to data type int.' [80010] [4c52a41];

'D6452	19-11-13
' * try to fix bugID #18197: RTE: 'Object reference not set to an instance of an object.' [99900] [354e5b3];

'D6451	19-11-12
' * update params on call SAML SSO;
' * update /SAML/Assertion.aspx;

'D6450	19-11-11
' * create a SAML /Assert/ test page;

'D6449	19-11-11
' * fix RTE on Efficient Frontier (caused by SessionState wrong access);
' * fix bugID #18166: RTE with efficient frontier;

'D6448	19-11-08
' * fix issue with open Gecko pipe popup without ability to maximize/resize;

'D6447	19-11-08
' * update Edit Account page: reorder psw fields, update error msg (related to #18153);

'D6446	19-11-08
' * update Workgroup edit form: always show "Back" button and add "Please wait" popup on form submit;
' * replace RadComboBox control for ECAM with a plain text input field; remove unused references;
' * fix issue with dialog when confirm using a different type of the license (Comparion vs Riskion):
' * auto-switch to just created workgroup and redirect to projects list when system workgroup is active and first regular workgroup created;
' * Workgroups List update: ask about edit System workgrpoup when system license is wrong or missing;
' * update issue with reload active/system workgroup after the edit workgroup license;
' * webAPI account updates: return messages from resources, u[date msg when call to change psw but current psw is wrong/mising;
' * welcome screen updates for set focus on load and auto-submit when "Remember me" is enabled;
' * welcome screen for Admin: don't show contact info when errors, replace all alerts with messages below the form;
' * update layout for "Install" page: hide controls when no DB, new loading popup, focus buttons, etc;
' + try to perform auto-logon for default Admin account when DB has been created and "Continue" button pressed;
' * update layout for Set password page;
' * update web.sitemap: auto collapse sidebar menu for most Administrative pages;
' * clsApplicationUser.PasswordStatus: allow to set -1 for .CannotBeDeleted account when assigned "-9" value;
' * update DBParse_ApplicationUser() for keep negative PasswordStatus (ask psw) for CannotBeDeleted users;
' * force to set a PasswordStatus for create a new psw on next login when default Admin account has been created (CheckCanvasMasterDBDefaults());
' * update SQL orders in DBWorkgroupDelete() for prevent error when tables has dependencies;
' * fix bugID #18154: New workgroup button is disabled after applying license to system workgroup;
' * fix bugID #18155: WG edit: Invalid JSON parameter;

'D6445	19-11-07
' * fix possible RTE on verify TT users;
' * try to fix bugID #18069: NS: Issues when logged in same user on different sites that have same db?

'D6444	19-11-07
' * _EULA_REVISION = "191107";
' * fix bugID #18150: betaNS: update EULA;

'D6443	19-11-06
' + webAPI /project/?action=List_History;
' * update code on SetActiveProject(): set .LastModified for ActiveWorkspace on open/close project;
' + jProject.LastVisited that as .LastModified fro workspace;
' + masterpage.js add getPeriodString();
' + add "Projects History" to Project Menu and as Alt+H Shortcut;

'D6442	19-11-05
' * update code for test SAML;

'D6441	19-11-05
' + webAPI /projects/?action=update;
' + allow to edit project directly on the grid by double click;
' * fix bugID #18051: BetaNS: can't select and copy model name;

'D6440	19-11-05
' * update dialog on create new project when Templates tab;
' * fix issue with passing wrong project ID on create new model (originally from the template);

'D6439	19-11-05
' * fix bugID #18142: betaNS: missing Anytime Invitation Summary email;

'D6438	19-11-03
' * update Logout shortcut description;
' * add shortcut for close active project (CtrlAltC);

'D6437	19-11-02
' * another layout for footer: user table and separate cells for items; update styles;
' * redo styles and layouts for landing pages; simplify resize/dynamic columns/links;
' * updates for background message;
' * fix bugID #17999: NS: Chrome -- Screens have unnecessary scrolls (broken due to new menu changes?)

'D6436	19-11-01
' * fix issue with Gecko popup window resize;
' * redo user reset psw dialog on Manage Participants screen: disable window dragging, replace plain input field to password without auto-complete;

'D6435	19-10-31
' + webAPI pm/combinedgroups;
' + add button for "Select By Group" on TT Select Participants screen;
' * replace internal "Select All" checkbox in grid with own control;
' * "silent" saving data on TT Select participants;
' * lot of minor fixes and updates on TT Select Participants;
' * fix bugID #18107: NS: Missing Feature -- Select TT Participants by group;

'D6434	19-10-30
' * fix issue with join to TT session for new users when login by MeetingID is not allowed;
' * fix bugID #18113: Can join the meeting even new users are not allowed to join;

'D6433	19-10-30
' * minor updates for Sensitivity page: updates for resize, expand/collpase charts on 4asa;

'D6432	19-10-28
' * force to re-init all settings on change anything on /Install/Settings.aspx;
' * another approach for select Keypad # on TT Select participants: dialog with buttons for all possible keypads;
' * enhance checker on TT Select participants and reset keypads mode to online when kepads ar not alllowed;
' * fix bugID #16053: BetaNS: Access Mode / Keypad number dropdowns are not working;

'D6431	19-10-27
' * fix issue with removing participants with e-mail that contains comma on /Project/Users.aspx;
' * fix bugID #18092: Can't remove some participants in this specific model;

'D6430	19-10-27
' + jUserTeamTimeOptions;
' * enhance webAPI project/user TeamTime_UsersList() for auto-attach current user;
' + webAPI project/user TeamTime_User_Update(), TeamTime_UsersList_Update();
' + clsMasterPageBase.FetchWithCode() is public for now;
' * enhance clsMasterPageBase.ParseJSONParams() for allow to parse the native arrays/lists (added as jArray);
' * enhance TT Select participants list: include/exclude user (single, multi), change Access Mode, Keypad#, Set Keypads for all selected;

'D6429	19-10-24
' + _OPT_KEYPADS_AVAIL, Options.KeypadsAvailable;
' * replace all _KEYPADS_AVAILABLE for new property Options.KeypadsAvailable;
' + clsWorkspace.TeamTimStatusAsSyncMode;
' + jUserTeamTime;
' + webAPI /project/user/TeamTime_UsersList;
' - remove server side code for TT Select participants screen;
' * update way to getting the data for TT Select Participants via webAPI;
' * code refactoring and using webAPI instead of local ajax calls on TT Select Participants;

'D6428	19-10-24
' * fix for show/hide project_action element with "nowide*" classes when narrow screen and switch AdvancedMode;
' * fix for show Current page title when basic mode;

'D6427	19-10-23
' * show instruction on show fullscreen mode only once;
' * update styles/icon for "Go To Top" icon on workflow menu row when Basic mode;
' * fix bugID #18084: Download button is not aligned in Chrome (broken);

'D6426	19-10-23
' + add GemBox.Pdf.dll;
' * update Combined Reports: add GemBox "PDF" button;
' + webAPI/pm/dashboard add Download_GemBox_PDF dummy method;

'D6425	19-10-23
' * update styles for login form: different colors for regular and TT forms;
' * update preloader for login screen form;

'D6424	19-10-23
' * fix bugID #18083: Hide the HTML statistics icon;
' * fix bugID #18085: Update message when changing access code;

'D6423	19-10-23
' ! remove all settings/properties/declarations for Survey DB;
' * fix layout issue for Install screen;

'D6422	19-10-22
' * redo approach for Access Mode/Keypad # controls on TT Select Participants;

'D6421	19-10-22
' * update layout and grid rendering issues for TT Select participants screen;

'D6420	19-10-22
' * fix issue with wrong result for single eval mode or TT evaluation on call CheckUserWorkgroups();
' * fix bugID #18081: NS: Can't join TT meeting (broken);

'D6419	19-10-22
' - hide "use workgroup wording" option on create new model;

'D6418	19-10-22
' + _KEYPADS_AVAILABLE;
' * update TT evaluation pipe, select TT participants for check _KEYPADS_AVAILABLE;
' - hide Keypads options due to unable to use it for now;

'D6417	19-10-22
' + add "releasenotes" to sitemap parser roles;
' + "Release Notes" user menu;
' + add Release notes dialog on projects list with ability to open, ignore or postpone it;
' * fix bugID #18073: NS: Add release notes link at the footer;

'D6416	19-10-21
' - disable detach an empty workgroups (DBUserWorkgroupDelete) on call CheckUserWorkgroups();
' * ignore CheckUserWorkgroups() when single eval mode or logged by MeetingID;
' * fix bugID #18071: User got detached/deleted from all her workgroups;

'D6415	19-10-21
' * always show workgroup, even if only one attached;
' * update knownissues list;

'D6414	19-10-21
' * hide checkbox on Known Issues dialog;
' * rename Infinite Scrolling to Unlimited Scrolling;

'D6413	19-10-21
' * fix bugID #18068: Resizing info doc size in teamtime CR not working;

'D6412	19-10-18
' * fix "Quick help sample" feature on Rich Editor;
' * fix bugID #18062: Remove the quick help sample link;

'D6411	19-10-18
' * revert back changes from D6410 and avoid to redirect to the responsive pipe when logged as anonymous AND has a dynamic survey;
' * fix styles for workflow items (back, project actions);

'D6410	19-10-17
' + clsComparionCore.CanUseEvalURLForProject() (check for Riskion and dynamic Surveys);
' * update PageURL for check CanUseEvalURLForProject();
' * update clsComparionCorePage.Authenticate() for check is it anonymous, CanUseEvalURLForProject();
' * update invitation page and participants list for check CanUseEvalURLForProject();
' * avoid to redirect to the responsive pipe when logged as anonymous or model has a dynamic survey(s) that rebuilt pipe;

'D6409	19-10-16
' * clean-up styles, fix errors;
' * update styles for footer elements;
' - disable changes list on version click;
' * fix bugID #18052: NS: Make Perfromance Sensitivity first from the menu (instead of Dynamic);
' * fix bugID #17999: NS: Chrome -- Screens have unnecessary scrolls (broken due to new menu changes?);

'D6408	19-10-16
' * disable _OPT_SHOW_HELP_AUTHORIZED;
' * update styles and layout for welcome screen; auto-narrow login form;

'D6407	19-10-16
' * fix issue with getting .CanView on call jProject.FillProjectData() when PO and project other than Template;
' * minor optimization on check projects list in clsComparionCore.CheckProjectsList();
' * update clsComparionCorePage.Authenticate for Logout active user before calling Logon in Authenticate function because it can keep cached projects list from the prev user;
' * fix bugID #18050: NS: Bugs for Project Organizer;

'D6406	19-10-15
' + ecProjectStateOnOpen.psAnytimePipe;
' * update GetActionResultByProject for return _DEF_PGID_ONVIEWPROJECT when psAnytimePipe;
' * fix issue with showing anytime pipe when force pipe required;
' - disable collapse/expand sidebar menu on master page header/footer swipe;
' * fix bugID #17777: BetaNS: Inconsistency Report view only pipe and login another user not working correctly;

'D6405	19-10-15
' * update wording "Define Project" -> "Define Model";

'D6404	19-10-14
' + add Starred to web.sitemap, hide start icon at the header;
' + add "starred" to "roles", update clsMasterPageBase;
' * update Project Details info: hide data about participants for project other than  active and archived;
' + show "Templates" tab for Project Organizers On Projects List screen;
' * update jProject.FillProjectData() and GetActionResultByProject() for allwo to open templates for Project Owners;
' * allow to open templates for Project Organizers even when not attached (not an Owner);

'D6403	19-10-14
' * update clsComparionCorePage.Authenticate for ignore evl site URL when getting custom URL on open model (call jProject.GetActionResultByProject());
' * fix issue with getting URL for anytime evaluation when forced to use evluation site but EvalURL is an empty (clsPage.PageURL());
' * fix bugID #17777: BetaNS: Inconsistency Report view only pipe and login another user not working correctly;

'D6402	19-10-14
' + clsComparionCore.isAntiguaAuthorized();
' + clsComparionCorePage.AnonAntiguaProject();
' * update permissions for _PGID_EVALUATE_INFODOC: access for ppEveryone;
' * update RichEditor for get the project data based on AnonAntiguaProject();
' * update Infodoc.aspx for check permissions (allow access for Authorized or when Antigua anonymous);
' * add code for show Antigua infodocs on Infodoc.aspx;
' * enhance initFrameLoader() in masterpage.js for allow to use the custom bg image/size;
' * update Structuring code for show infodoc in the hovered tooltips (fix type and add preloader);
' * fix bugID #17853: betaNS: unable to open infodoc when Brainstroming anonymous;

'D6401	19-10-14
' * fix bugID #18043: NS: Make Wireframe a draft feature;

'D6400	19-10-11
' * update CWSw.UpdateUserInfo() for send E-mail about reset/set new psw when psw has been changed;
' * fix bugID #18012: NS: Update email notification when resetting password;

'D6399	19-10-11
' * update jProject.GetActionResultByProject for check clsProject.Comment in addition to ProjectManager.ProjectDescription for open description page when non empty;
' * update clsComparionCorePage.Authorize for get the URL on open model under PM via call jProject.GetActionResultByProject();
' * fix issue with opening model with non empty description when loggeb by accesscode/hash on Project Description screen for PMs;

'D6398	19-10-10
' + _OPT_IGNORE_NEW_MASTERPRJ ("IgnoreNewMasterPrj");
' * update CWSw.UpdateMasterProjects for check _OPT_IGNORE_NEW_MASTERPRJ and avoid to auto update default option sets when specified;
' * update webAPI/project List() for check _OPT_IGNORE_NEW_MASTERPRJ and avoid to auto update default option sets when specified;

'D6397	19-10-10
' * update Users.apsx for return the right msg when change password (from STMP or when nothing to change);;
' * fix bugID #18012: NS: Update email notification when resetting password;

'D6396	19-10-10
' * fix check in  navOpenPage() for Comparion;
' * fix issue with active tab on load Riskion Grids;
' * fix bugID #18024: NS: Tabs to change screens stopped working;

'D6395	19-10-09
' * improve sidebarmenu.js navOpenPage() with abiltiy to keep the hierarchy (for pages that have mirrors);
' * fix bugID #18005: Riskion NS - Events Grid/Chart and Objectives Grid/Chart are not switching properly for Impact;

'D6394	19-10-09
' + add Comparion 2020 announcement to welcome screen and SL shell/projects list on show Release note;
' * fix bugID #18009: Add announcement for the Comparion 2020 Release;

'D6393	19-10-09
' * fix bugID #18012: NS: Update email notification when resetting password;

'D6392	19-10-08
' * new shortcut for show/hide shortcuts menu now is Alt+F1;
' * use F1 for show/hide Help widget;
' * fix bugID #18002: NS: The shift+esc keys to open the shortcuts list is not working for chrome;

'D6391	19-10-08
' * fix bugID #18001: NS: Use the KO help page on rich text editor;

'D6390	19-10-08
' * update jProject.GetActionResultByProject() for check project On-line status for Evaluators and Viewers;
' * update _DEF_PGID_ONVIEWPROJECT = _PGID_ANALYSIS_GRIDS_ALTS;
' * fix bugID #17978: NS: Can't open the model for Viewer users;

'D6389	19-10-07
' * hide "Add Snapshot" icon when Basic mode;
' * update code for render "Has Structuring Data" on Landing page and sidebar menu;
' * update color for sidebar shortcut menu;

'D6388	19-10-04
' * fix bugID #17978: NS: Can't open the model for Viewer users;

'D6387	19-10-04
' + add administrative pages to navigation;
' + add /Admin/default.aspx reloader;
' + minor styles updates;
' + webAPI/project snapshot_add created;
' + "Add Snapshot" to workflow menu line;

'D6386	19-10-03
' * try to fix issue with closing TT window/tab when called from Consensus View screen;

'D6385	19-10-03
' * add extra checks to jProject.doInherit();
' * try to fix bugID #17943: RTE: 'Non-static method requires a target.' [99900] [845724e]

'D6384	19-10-02
' * update styles for dxTabs fo Riskion tabs navigation;

'D6383	19-10-02
' * another approach for init/reset Help Widget (sidebarmenu.js/updateHelpPage());

'D6382	19-10-02
' * update clsComparionCorePage.Authenticate for an extra check "redirect=no" for avoid to show responsive pipe on login;

'D6381	19-10-02
' * fix issue with Help widget suggestions;

'D6380	19-10-01
' * update styles for selected tabs (dxTab, dxButtonGroup);

'D6379	19-10-01
' * hide Riskion responsive links;

'D6378	19-10-01
' * update Riskion anytime evluation when joined pipes: check steps for judgemtns and surveys for detect an "empty" pipes;
' * fix issue with error on saving blank or the same psw when asked on user login;

'D6377	19-10-01
' * update styles for footer, workflow, updates for master page;

'D6376	19-09-30
' * inverted styles for workflow menu;

'D6375	19-09-30
' * replace ComponentSpace dll with another version from the samples;
' + add dummy link to SSO on login screen;

'D6374	19-09-30
' + ComponentSpace library (4.6.2);
' + saml.config, certificates;

'D6373	19-09-30
' * show warning when user made a judgment while view only pipe;

'D6372	19-09-30
' * update CheckUserLockWhenInvalidPassword() for send e-mail on user lock with link to non-responsive site;

'D6371	19-09-30
' * minor update for get KO token when no connection to the server (no valid response);
' * fix bugID #17945: betaNS: 404 Error when clicked the user name from the Models List;

'D6370	19-09-28
' * redo grid option buttons on Projects list screen (use dxButtonGroup);
' * fix styles for workgroups selector at the header;
' * fix bugID #17921: NS: Some Filter by column is disabled;

'D6369	19-09-27
' * fix issue with refresh "Status" column vaklue on Projects list when change project On-line status;

'D6368	19-09-27
' * update styles for workflow menu and remove carets for Tabs;
' * fix issue for dxTreeList with lines for the goal node and for selected row;
' * fix bugID #17374: BetaNS: The hierarchy level is not that obvious;

'D6367	19-09-27
' * fix bugID #17936: NS: Need to update password lock email when auto-unlock is disabled;

'D6366	19-09-25
' * update updateHelpPage() in sidebarmenu.js for reset/reload help widget content on change page;

'D6365	19-09-25
' * fix bugID #17930: NS: Model list commands icons have different colors;

'D6364	19-09-25
' + new property Options.EvalURLPostfix that used on call CreateLogonURL as an encrypted extra parameters;
' + add ignoreval, ignoreeval, ignoreoffline and ignorepsw to _PARAMS_LOGON list;
' + update Authenticate for analyze "ignoreoffline" and pass to Logon();
' * analyze "ignorepsw" and avoid to ask user psw when logged by PM via hash;
' * enhance Inconsistency Report, Participants List and Evalaution progress for fill Options.EvalURLPostfix with params for ignore prj offline status and user psw status;
' * fix bugID #17777: BetaNS: Inconsistency Report view only pipe and login another user not working correctly;

'D6363	19-09-24
' + clsComparionCorePage.PrepareProjectForOpenPipe();
' * update Users.apsx, Evaluators.aspx, Custom.aspx for use PrepareProjectForOpenPipe() before opening the pipe;
' * fix bugID #17851: BetaNS: Button labels needs changing (broken?);

'D6362	19-09-24
' - hide "Get Link and copy to clipboard" on Project Details;
' * enahcne Authenticate() with adding "ignoreval" for force to show non-responsive pipe;
' * fix issues with opening pipe for other user for Inconsistency report screen;
' * fix bugID #17777: BetaNS: Inconsistency Report view only pipe and login another user not working correctly;

'D6361	19-09-23
' * update Projects list columns/context menu for jusn an Evaluator/Viewer;
' * fix bugID #17906: NS: Issues for Workgroup Members Project list;

'D6360	19-09-20
' + add an extra "Close" button on Project list for active project;

'D6359	19-09-20
' * update dx styles with replace preloader embedded gifs with our animated gif files;
' + Options.EvalSiteOnly with ability to override as custom setting;
' - clsComparionCorePage.isEvalURL_EvalSite property;
' * optimize SetActiveWorkgroup for avoid redundant inits when the same wkg assigned;
' + clsComparionCore.CheckIsEvalSiteOnly(), clsComparionCore.isEvalURL_EvalSite;
' * replace all calls for isEvalURL_EvalSite;
' * enhance PageURL() with an extra param for check is it eval page and replace URL when required;
' * update most calls for PageURL() when anytime evaluation pipe is or can be used;
' * update clsMasterPageBase.LoadMenuItems() for ignore/skip non-responsive pipe when .isEvalURL_EvalSite;
' * try to remove most .focus() call in js for keep the page unfocused if it was, except the welcome screen;
' * enhance openProjectURL() in masterpage.js for analyze the URL and open ServicePage.aspx?action=eval_* as a new tab;
' * try to close opened Gecko tab in case when close project and this is possible (opener was not reloaded);
' * force to open responsive pipe for the most cases when .isEvalURL_EvalSite;
' * fix bugID #17662: BetaNS: Allow only Evaluator hash link to access responsive Anytime;
' * fix bugID #17776: BetaNS: New user added from particpants page can access non-R pipe;

'D6358	19-09-19
' * redo approach for transfer request on RTE for fix issue with invalid response for a regular pages;
' * extra fix bugID #17864: NS: Modify the no. of failed attempts on the email based on the admin settings (it is always 3);

'D6357	19-09-19
' * hide unloaded sidebar menu in case of RTE/issues with loading/init;
' * fix bugID #17901: NS: Change from "Project" to "Model";

'D6356	19-09-19
' * fix issue with parsing %%Model*%%/%%Project*%% templates in js;
' * don't show submenu for a single child item when user menu;
' * fix issue with permissions for Archived, Deleted for non PMs;
' * minor fix for responsive site URL on projects list when Gecko only;
' * force to disable Advanced mode when simple evaluator;

'D6355	19-09-18
' * fix bugID #17893: NS: Download with snapshots is not working (Model Details page);

'D6354	19-09-18
' * fix issue with _TEMPL_LIST_ALL_ and _TEMPL_LIST_NO_PRJ lists when have extra templates that is not for public using;
' * fix crashing on call SendMailPage.GetTooltipHints();
' * fix bigID #17900: RTE: 'Index was outside the bounds of the array.' [10071] [52d4132];

'D6353	19-09-18
' * fix bugID #17864: NS: Modify the no. of failed attempts on the email based on the admin settings (it is always 3);

'D6352	19-09-17
' + add custom beforeInitNavigation() on init navigation from master page code;
' + add support for "projectslist" extra and set these node items as selected on loading ProjectsList;
' * force to show "Manage Projects" workflow item (Comparion) or tab (Riskion) as active/selected on Projects list;

'D6351	19-09-17
' + add ability to show submenus for User menu items;
' * fix issue with counter on faled login attempt from the login screen;
' * fix bugID #17662: BetaNS: Allow only Evaluator hash link to access responsive Anytime;

'D6350	19-09-17
' + _TEMPL_PSWLOCK_ATTEMPTS ("%%pswlock_attemps%%");
' * fix bugID #17864: NS: Modify the no. of failed attempts on the email based on the admin settings (it is always 3);

'D6349	19-09-17
' * update style for Riskion naviagtion (tabs, workflows);
' * adaptive styles and scrollable view for Riskion tabs;
' * updates for Comparion workflow styles;
' * update styles for Projects List active project row;

'D6348	19-09-16
' + "BOGGSAT Killer" default option set;
' + ecProjectStateOnOpen to ProjectsWebAPI;
' + _DEF_PGID_ONNEW_BOGGSAT, _DEF_PGID_ONNEW_BOGGSAT_RISK;
' * enhance webApi /project/create for analyze new project and source project names for "boggsat" and open TT Structuring by default if contains;

'D6347	19-09-16
' + _PARAMS_LOGON as list of all params that can be using during the perform user authenticate;
' * enhnce GetParamsWithoutAuthKeys() with using _PARAMS_LOGON;
' * enhance welcome screen with adding server func GetURLParam() and passing all params from _PARAMS_LOGON to webAPI when call user logon;
' * update sidebarmenu.js for allow to collapse sidebar menu even when no workflows (useful for Riskion and "collapsemenu" in sitemap);
' * force to collapse sidebar menu for Riskion Project Description/Details screens;
' * enhance Project Detaikls screen with adding confirmation on reset access code;l add button for quick copy project link to clipboard;
' * enhance popup with project Links with adding link to model and link to model with a force anytime pipe;
' * try to ignore Landing page when show "Manage Projects" (with adding "projectslist" role to sitemap node);
' * re-organize context submenu for Riskion projects and show "Copy link to model" in the main section (was for each hierarchy);

'D6346	19-09-12
' * enhance clsComparionCorePage.CheckUserLockWhenInvalidPassword();
' * update Start.aspx for check user' PasswordStatus and lock it after max attempts;
' * save log event about wrong psw attempt when filler Sign in form on Start.aspx;
' * fix bugID #17858: CR: Responsive pipe -- Make Password locking work based on the System Setting;

'D6345	19-09-12
' * fix issue with init General Link tab when anonymous by default;
' * fix issue with saving phone number on user signup (was only for Riskion);
' * fix bugID #17857: CR: Phone number from sign-up link;

'D6344	19-09-11
' * update English.resx in the Gecko project;
' * fix bugID #17830: BetaNS: Resource not parsed;

'D6343	19-09-11
' * fix bugID #17851: BetaNS: Button labels needs changing (broken?)

'D6342	19-09-10
' * fix bugID #17830: BetaNS: Resource not parsed;

'D6341	19-09-09
' * separate message when unable to change project status (on-line, lock) when brainstorming;
' * enhance setProjectOnline() in masterpage.js for show the real error message when possible (was only on projects list);
' * reject request for project lock/unlock when current page is brainstorming (mpDesktop.master);
' * fix issue with empty/redundant error popup when try to change project on-line/lock on Brainstorm screen;

'D6340	19-09-07
' * add "Manage Projects" to user menu;
' * fix dialog for Solver settings on RA Main;
' * try to fix issue with running Gurobi solver (when DLL can't be loaded);

'D6339	19-09-06
' * try to fix issues with wipeout projects on cofirmation, fix refresh the list;
' * fix bugID #17307: betaNS: Confirmation about wipeout projects marked as deleted;

'D6338	19-09-05
' * another approach for check outdated projects marked as deleted on Projects list;
' * fix bugID #17307: betaNS: Confirmation about wipeout projects marked as deleted;

'D6337	19-09-05
' + add %%page_*-#%% templates for auto-parse references to the pages;
' * fix bugID #17823: BetaNS: Need to update page path;

'D6336	19-09-05
' * clean web api functions listing: remove some public consts and properties;
' * api/pm/params: redo "set_pipe_param" and add "get_pipe_param";
' * enhance msgUseAIP() in sidebarmenu.js for show confirmation to disable AIP;

'D6335	19-09-03
' * move "Participants" under the "Identify/Structure" for Riskion (hide below each hierarchy);
' * update default colors for BowTie per EF request;

'D6334	19-09-03
' * fix bugID #17809: BetaNS: Need to update message when TT is ongoing (incorrect page path);

'D6333	19-09-03
' * try to force set .High value for Step Function intervals on load scale, before claling graphs in AT, TT;
' * fix bigID #17811: BetaNS: Non-responsive pipe step function graph is incorrect;

'D6332	19-09-02
' * update TeamTime evalaution pipe when call SF graph, check and fix the last one point for a +inf value;
' * fix bigID #17804: Teamtime: issue with step function graph;

'D6331	19-09-02
' * fix bugID #17807: BetaNS: Update the Resource Center link to KO;

'D6330	19-08-30
' + _PGID_REPORT_COMBINED;
' + draft "Combined Report" page with links to download draft docs via web api (pm/dashboard/);

'D6329	19-08-30
' * enable Data Mapping and fix reference;
' * fix issue with redundant (?) icon on sidebar menu and on the Landing page when "no_report" in sitemap;

'D6328	19-08-30
' * fix crashing in HasPermission() when CS anonymous user;
' * improve clsComparionCore.isRiskEnabled with single-time getting value and ability to force override value;
' * check model when CS anonymous and show IsRiskEnabled depend on the model but System Workgroup;
' * fix bugID #17788: BetaNS: RTE when joining CS meeting (broken);

'D6327	19-08-28
' * fix bugID #17780: Beta: TeamTime menus are missing;

'D6326	19-08-28
' + check master projects on get projects list for PMs;
' + api/project/ Restore_Defaults() for restore default option sets;
' + add "Restore default option sets" on master pages screen (Projects list);

'D6325	19-08-28
' * disable renaming Goal nodes on Create model when "use Workgroup wording" is enabled;
' * ignore option "Use Workgroup wording" when create model under the Basic mode;
' * enhance clsComparionCorePage.GetOptionalStartURL() for try to check the URL as server local file and reset if not exists;
' * fix bugID #17781: BetaNS: Evaluator got redirected to TT page (last visited page) and has RTE?

'D6324	19-08-27
' * fix bugID #17715: BetaNS: Issue when filtering columns in the projects list;

'D6323	29-08-27
' * update _DEF_PGID_ONNEWPROJECT[_RISK] for set as _PGID_PROJECT_DESCRIPTION per EF request;

'D6322	19-08-27
' + add support for "auth" resource for sitemap (allowed only when user authorized);
' * hide navigation on set password screen;
' * hide unavalable workflow menu items (was gray out);
' * fix bugID #17775: BetaNS: Viewer can access TeamTime Brainstorming?

'D6321	19-08-27
' + add headers for PipeParams screen;
' + add ability to copy all settings from one hierarchy to another for Riskion;

'D6320	19-08-26
' * update wording for Wkg templates/samples;
' * update "Choice..." default option sets;

'D6319	19-08-26
' - temporary disable check wipeout projects on Projects list due to an issue;

'D6318	19-08-26
' * use responsive link for PMs on Evaluation progress, Partcipants List when responsive pipe only;
' * fix bugID #17662: BetaNS: Allow only Evaluator hash link to access responsive Anytime;
' * fix bugID #17772: BetaNS: Rename the menu names for sample models/templates;

'D6317	19-08-26
' * show "User workgroup wording" only when Advanced mode;
' * minor update for "Create project" dialog wording and layout;
' + add "Brief description" field on "Create Project" dialog;
' * enhance api/project Create() with ability to specify the project description;
' * minor updates for Infodoc.aspx and Rich Editor for valid parse text when project description is plain text;

'D6316	19-08-25
' * updates for TeamTime Structuring screen: update button groups, layouts, styles fix sizes, etc.;

'D6315	19-08-23
' * restore WorkgroupTemplates.aspx page functionality;
' * fix issue with misplacing selection links on WorkgroupTemplates.aspx page;
' * fix bugID #16400: BetaNS: Add sample projects/templates page issues;

'D6314	19-08-23
' * tune up Projects List grid columns filtering, etc;
' + add methods for correct detect filtering/search on Projects list;
' + show notification when filter of search is applied on Projects List with ability to quick reset it;
' * fix bugID #17715: BetaNS: Issue when filtering columns in the projects list;

'D6313	19-08-22
' * update sitemap items and Landing Page for "Define project";

'D6312	19-08-22
' - hide PM Instruction on login sue to an old links/content;
' * another aproach for get the links to evaluation on Eval Progress and Participants list;
' * set another color for responsive pipe links on Eval Progress and Participants screens;
' * fix bugID #17662: BetaNS: Allow only Evaluator hash link to access responsive Anytime;

'D6311	19-08-21
' * fix bugID #17752: BetaNS: Resource not parsed;

'D6310	19-08-20
' * fix bugID #17715: BetaNS: Issue when filtering columns in the projects list;

'D6309	19-08-20
' * global resources replacement for use _TEMPL_PROJECT, _TEMPL_PROJECTS, _TEMPL_MODEL, _TEMPL_MODELS;
' * replace hardcoded text with resourecs on Project Details screen;

'D6308	19-08-20
' + _TEMPL_PROJECT, _TEMPL_PROJECTS, _TEMPL_MODEL, _TEMPL_MODELS;
' * enhnace resource parser with parse new templates for "Model(s)" and "Project(s)";

'D6307	19-08-20
' + clsComparionCore.ProjectWordingUpdateWithWorkgroupWording();
' * enhance /api/project Create() method with extra parameter for UseWorkgroupWording;
' + add Option "Use workgroup wording" when Create New Project dialog on Projects List;
' * fix bugID #17744: betaNS: add option to choose the wording source on new model create dialog;

'D6306	19-08-19
' * update masterpage.js: try to ignore ajax errors happens when page reloads/leave;

'D6305	19-08-19
' * fix bugID #17743: BetaNS: Switching to another workgroup from CS screen should redirect to the projects list?

'D6304	19-08-18
' * fix issue wih init first tab on Anytime invitations when page loaded with other active;
' * fix issue when "Send", "Download mailmerge" is disabled on Anytime invitation even when user(s) selected;
' * fix bugID #17047: RiskionNS: Option to invite both for Likelihood and Impact Evaluation;

'D6303	19-08-18
' + add resources for Riskion invitations (Anytime General link tab);
' + add option for invite to Anytime for both hierarchies when Riskion;
' * fix bugID #17047: RiskionNS: Option to invite both for Likelihood and Impact Evaluation;

'D6302	19-08-18
' * reduce chat icon size;
' * fix issue with "Has advanced mode options" icons wrapping;
' * update master page styles, projects list grid setting for byeer fit when mobile view;

'D6301	19-08-17
' * enhance Create New Project dialog on Riskion Projects list with adding ability to specify the project type;
' * ehnance /api/project/ create routine for alolow to specify/update project type;
' * fix minor issue with parseReply() in masterpage.js;
' * allow to reload in the background the navigation pages list due to change wordings on switch Riskion Project type (Details.aspx);
' * fix bugID #17022: RiskionNS: Model Type option on Create Project modal;

'D6300	19-08-16
' * fix bugID #17728: BetaNS: Hide the print/ print preview icons on Reports pages since they are not working?

'D6299	19-08-15
' * fix bugID #17695: BetaNS: Download CSV file just shows a white page;

'D6298	19-08-15
' * minor updates and fixes when error occured while model uploading;
' * rollback changes for hide pagination on Project List;

'D6297	19-08-14
' * update getProjectLinkDialog() for always use non-responsive links;

'D6296	19-08-14
' * try to implement the smart way to show/hide pagination/page settings on Projects List;
' * fix bugID #17721: BetaNS: Hide the pagination options when there is nothing to paginate?

'D6295	19-08-13
' + show project name on RichEditor when edit project description;
' * fix bugID #17716: BetaNS: There is no selected no. of items per page in the projects list;

'D6294	19-08-13
' * redo approch for get the main placeholder sizes on call resize_custom (sidebarmenu.js);
' * fix issue with load/switch tabs on Riskion Details;
' * fix issue with call onSetPage when Riskion and "hid" is not specified in sitemap object;
' * fix bugID #17709: RiskionNS: infinite loop when looking for details;

'D6293	19-08-13
' * fix issue with broken RadAjaxManager (RA);

'D6292	19-08-09
' * fix bugID #17643: BetaNS: Remaining changes in the landing page menus discussed scrum;

'D6291	19-08-09
' * hide all role groups when user can't evaluate project on Anytime Invitations - General Link tab;
' * update mpDesktop.master, OpenLinkWithReturnUser() for use regular pipe even when responsive only;
' * force to use regular link on "Get Links" dialog and when gettings links for Participants;
' + add "Get Evaluation" link to context menu on Projects List when responsive site is specified;
' * fix bugID #17680: BetaNS: Hide "Viewer" role when inviting participants;
' * fix bugID #17662: BetaNS: Allow only Evaluator hash link to access responsive Anytime;

'D6290	19-08-08
' * fix minor issue with return error message on call clsComparionCore.CanUserCreateNewProject();
' * try to reload list of projects after projects wipeout on call /api/project/?action=check_wipeout;

'D6289	19-08-08
' + /api/project/?action=check_wipeout;
' + masterpage.js: add onCheckWipeout();
' * enhance Projects List with an extra pop[up on load for check projects marked as deleted;
' * fix bugID #17307: betaNS: Confirmation about wipeout projects marked as deleted;

'D6288	19-08-08
' * use non-responsive link when isResponsiveOnly and choosen Viewer as group for a new users when creating an Invitation Link;
' * fix bugID #17680: BetaNS: Hide "Viewer" role when inviting participants;

'D6287	19-08-08
' - _COOKIE_PASSCODE, _COOKIE_RET_ORIG_PASSCODE; don't remember the access code on login form;
' * resources minor update;

'D6286	19-08-08
' * update onSetPage() function for check real pageID (can be important for Riskion when a few pages with a different navpgid);
' * fix issue with reloading a Dashboard screen when calling onSetPage twice;
' * fix bugID #17667: RiskionNS: The Grid Events/Threats tabs are not working;

'D6285	19-08-07
' * fix bugID #17685: BetaNS: Clicking the workflow menu no longer redirect to the landing page (broken?);

'D6284	19-08-07
' * fix bugID #17684: Hide legend when navigation box is hidden;

'D6283	19-08-07
' * improve masterpage.js resize_custom(force_redraw, width, height) with passing client width and height sizes;

'D6282	19-08-07
' * fix bugID #17686: BetaNS: Advanced options not showing / hiding on Grid tab until you switch to another tab;

'D6281	19-08-06
' * upgrade devextreme up to 19.1.5 (was 19.1.4);
' * upgrade fontawesome up to 5.10.1 (was 5.9.0);

'D6280	19-08-06
' * update sidebarmenu.js for try to open ruist workflow when click on Riskion Tab and keep last visited page even when option is disabled/Basic Mode;

'D6279	19-08-02
' * fix bugID #17666: BetaNS: Get Link (with access code and meeting ID) should be non-R?

'D6278	19-08-01
' * update client-size code for most pages when set properties (disabled) for jDialog_btn* with using dxDialogBtnDisable();

'D6277	19-08-01
' + dxDialogBtnDisable(), and dxDialogBtnFocus() to misc.js;
' * update code for create dialog when calling dxDialog for init ID for buttons ('jDialog_btnOK', 'jDialog_btnCancel');
' * fix bugID #17570: BetaNS: Clicking "removed constraint(s)" only reloads the page;

'D6276	19-07-31
' * fix link to "Project Results" on Projects list context menu to Alts grid;
' * enhance menu_setOption() for call parent window code when exists/possible;
' * update Grids.aspx for allow to set tabs status when switching AIP on embedded Synthesize.aspx;
' * fix bugID #17654: BetaNS: Bugs on the left nav when switching AIJ/AIP;

'D6275	19-07-31
' * update PipeParams init input focus;
' * fix bugID #17653: Long pages are auto-scrolled;

'D6274	19-07-31
' * redo init for Groups on Anytime invitations screen and reload link on show tab with general links;
' * fix bugID #17649: Anytime General invitation does not remember the specified "assign to group" option;

'D6273	19-07-31
' * update /api/help-auth/ for pass [readers] data on get OAuth token;

'D6272	19-07-31
' * try to open the responsive pipe in the same tab/window;
' * fix bugID #17602: BetaNS: Make invite links responsive as the default;

'D6271	19-07-30
' * fix bugID #17649: Anytime General invitation does not remember the specified "assign to group" option;

'D6270	19-07-30
' * fix bugID #17638: BetaNS: Workflow name should be "Collect Input";

'D6269	19-07-29
' * fix issue with "lose" EvalURL on Evaluation progress screen;
' * add Create/Set new password page to list of pages when no AllowLinks;
' * update clsMasterPageBase.LoadMenuItems() for pre-analyze pageID and hide some shortcuts (Project list, Brainstorming);
' * fix bugID #17602: BetaNS: Make invite links responsive as the default;

'D6268	19-07-29
' * fix bugID #17632: BetaNS: Use same "Collect my input icon";

'D6267	19-07-28
' + clsComparionCorePage.isEvalURL_EvalSite;
' + url_on_open_blank [=false] to masterpage.js, update openProjectURL() and open url as popup when specified (once time);
' * update Project List for open pipe as popup when eval site only, fix GetLinks;
' * update Details.aspx: add onSetPage() and fix GetLink when eval site only;
' * update Users.aspx for get the right links when eval site only and change "View pipe" as opening as hardcoded path;
' *  fix bugID #17602: BetaNS: Make invite links responsive as the default;

'D6266	19-07-25
' + refresh current project title in Basic Mode when calles onSetPage();
' * allow to switch tabs on Grids.aspx and for Charts without page reloading;
' * fix bugID #17615: BetaNS: Clicking the Alternatives Grid menu opens the Objectives Grid tab instead;

'D6265	19-07-25
' * fix issue with restore last visited page when release mode and option enabled;

'D6264	19-07-25
' * fix bugID #17618: BetaNS: Objective/Alternative tab names should be from wording template not hard coded;

'D6263	19-07-25
' * update anytime Invitation screen for show options depend on _OPT_EVAL_SITE_ONLY;
' * upadte Projects List for get the right link when _OPT_EVAL_SITE_ONLY is enabled;
' * fix bugID #17602: BetaNS: Make invite links responsive as the default;

'D6262	19-07-25
' + _OPT_ALLOW_IGNORE_EULA thatis false by default;
' * update CheckPermissions() for check new option and force to show EULA in case when not accepted;
' * fix bugID #17608: BetaNS: Can bypass the EULA agreement;

'D6261	19-07-24
' * fix issue with setup ActSess, that used in help_injection.js;
' * fix bugID #17611: BetaNS: Anytime/Teamtime instructions page shows a message to login;

'D6260	19-07-24
' * always allow grouping on Projects List (was only in Advanced mode);
' * fix EULA frame resize issue;
' * add confirmation on leave EULA page when agreement is not accepted yet;
' + clsMasterPageBase.master prop AllowLinks that false when EULA page is active;
' * update mpDesktop.master for disable/hide most shell elements that allow to navigate away when AllowLinks is false;
' * fix bugID #17608: BetaNS: Can bypass the EULA agreement;

'D6259	19-07-24
' + _PGID_ANALYSIS_DASHBOARD_ALTS, _PGID_ANALYSIS_DASHBOARD_OBJS, _PGID_ANALYSIS_GRIDS_ALTS, _PGID_ANALYSIS_GRIDS_OBJS;
' * show Alternatives/Objectives Grid, Alternatives/Objectives Chart;

'D6257	19-07-24
' * update GetPipeActionStepType() for return the right value for multi-PW steps;

'D6256	19-07-23
' * update anytime evaluation for init context sensitive help for KO;

'D6255	19-07-23
' * update clsMasterPageBase.GetSitemapJSON for parse eval site links when regular pipe is not allowed (_OPT_EVAL_SITE_ONLY);
' * fix issue with "Default Option Sets" page permissions for project owners/PMs ("Manage System" is not available for now);
' * update masterpage.js for fix issue when ko/help is specified for usermenu: show Landing or a single child instead;
' * update clsComparionCore.CheckProjectsList() for ignore marked as deleted projects for non-PMs;
' * update clsComparionCore.CheckUserWorkgroups() for detach user from workgroup when no PM permissions and no projects;

'D6254	19-07-23
' * update clsPairwiseData (PW scale) for draw "Eq/Equal" bigger and bold;

'D6253	19-07-23
' * add "Description" and "Project Details" to sitemaps, allwo to load each page by link;
' + _DEF_PGID_ONSELECTPROJECT_INFODOC, _DEF_PGID_ONSELECTPROJECT_RISK_INFODOC;
' * check project description on open model and choose the project description page when is not an empty;

'D6252	19-07-22
' * show state of Advanced mode on switcher caption;

'D6251	19-07-22
' * replace "message" to "messageHtml" on create devextreme dialogs (W1013);

'D6250	19-07-22
' * fix bugID #17597: BetaNS: Evaluator Pipe icons (help, full screen, shortcuts) are not working;

'D6249	19-07-22
' - hide mouseover tooltip for GPW control (twinslider.js, related to case 17599);

'D6248	19-07-22
' * fix bugID #17598: BetaNS: Update the Help icon on the Welcome page;

'D6247	19-07-19
' * update api/project/?action=upload for init project properties (name, descriptiomn, guid, etc);
' + api/project/?action=Project_HID_Move;
' * update masterpage.js onUpload() for show msg/dialog when upload prj with another hierarchies;
' * fix bugID #17308: RiskionNS/BetaNS: check model hierarchies on upload and confirm/move data;

'D6246	19-07-19
' * fix bugID #17590: BetaNS: Message when database is down;

'D6245	19-07-18
' * fix issue with "Settings" links under System manage for workgroup managers;
' * fix issue with hang up Workgroup Samples/Templates when no workgroup ID specified;
' + add Workgroup Samples/Templates  bbelow "manage Workgroup";
' * fix bugID #17580: BetaNS: Workgroup Manager can view Manage System menu?

'D6244	19-07-17
' * update Grids.aspx and Dashboard.aspx for tabs styles, update resizes, loaders;
' * fix bugID #17450: Need to merge Objectives Grid and Alternatives Grid screens in CR;

'D6243	19-07-17
' * fix bugID #17565: BetaNS:Inconsistent menu names for navigation and Landings;

'D6242	19-07-17
' * update clsMasterPageBase for return only eval site links when _OPT_EVAL_SITE_ONLY is enabled;
' * fix issue with properly set a "ko" context-help tag for workflows;

'D6241	19-07-17
' * show Infeasibility Analisys button only for Guroby solver on RA Main screen;

'D6240	19-07-16
' * update styles for tabs on Grids.aspx;

'D6239	19-07-16
' + _PAGESLIST_ANTIGUA, update _PAGESLIST_TEAMTIME;
' * update clsComparionCore.HasPermission() for check TT pages and Antigua separately.
' * fix bugID #17572: BetaNS: Can't join TeamTime CS (broken);

'D6238	19-07-15
' * update updateHelpPage() and onMenuItemClick() when showing landing pages for usermenu;
' * fix bugID #17568: BetaNS: Can't go to Manage Workgroups page (broken);

'D6237	19-07-15
' * fix bugID #17565: BetaNS:Inconsistent menu names for navigation and Landings;

'D6236	19-07-12
' * fix bugID #17554: BetaNS: Model again not upgrading after upload;

'D6235	19-07-11
' * fix issue with a disabled toolbar on Projects List;
' * redo code for load/init KO scripts in mpDesktop.master;

'D6234	19-07-10
' * update /api/help-auth/ for call app.knowledgeowl.com/oauth2/token when getting the token;

'D6233	19-07-10
' * update DevExtreme up to 19.1.4 (was 18.2.7);
' * update icon sets (fontawesome up to 5.9.0, was 5.8.1; ionicons up to 4.5.10, was 4.5.5);

'D6232	19-07-10
' * fix bugID #17536: BetaNS: Hide the developer initials and HTML icons on the left nav for CR;

'D6231	19-07-09
' + /api/help-auth;
' * enhance clsMasterPageBase with parsing "ko" attribute in sitemap for get the tag for KO context sensitive help page;
' * improve switching pages/load navigation and specify KO help tag when required;

'D6230	19-07-08
' + add _OPT_HELP_ROBOHELP;
' + enhance mpDesktop.master with loading knowledgeowl help with ability to hide the riginal button and show on click help icon;

'D6229	19-07-08
' * fix bugID #17516: BetaNS: Incorrect tooltip and pop-out when Advanced Mode is on;

'D6228	19-07-08
' * fix bugID #17526: RiskionNS - should sensitivities be under Reports section?

'D6227	19-07-03
' * fix bugID #17509: BetaNS: issue with open pipe from Inconsistency report for active user;

'D6226	19-07-02
' + ignoreAdvancedSwitch to masterpage.js that false by default, but true when active page is below the usermenu;
' * don't hide (but collapse) the sidebar menu panel when Basic mode but no workflow and usermenu items;
' * update Projects List and show relative caption for project name on depend on project tab;
' * show "Default Option Sets" tab for Project list when opened as active project or called to see the list;
' * update master page onSelectProject() for check the active project type and open related tab by default;
' * fix bugID #17358: BetaNS: Not clear way to return to the Default Option Sets list after opening one;

'D6225	19-07-02
' + clsCompationCorePage.DownloadFile();
' + add test call for download xlsx report for api/pm/dashboard;

'D6224	19-07-01
' * minor fix for Set user Psw dialog on Project Participants screen;
' * reset hash of tokens (login hashed) when called DBUserUpdate();
' * update Authenticate() for avoid showing default page (List of projects) when log by hash when user is already logged in;
' * fix bugID #17334: BetaNS: New User is being asked to Create New Password repeatedly;

'D6223	19-07-01
' * fix bugID #15625: Data Mapping: Chain icon gets disabled after doing Save As;

'D6222	19-06-28
' * optimize layout issues in mobile view, fix issues with icons, alignment, tooltips, etc;
' * fix issues with render Project Description page;
' * update Projects List: prevent serach for some columns (like project type);

'D6221	19-06-28
' * fix issue with set the same password on asking a setup new password after login (/Password.asxp);
' * fix issue with error msg while set new psw as blank while this is no allowed;
' * fix issue with hide sidebar menu items when "hide" used;
' * add "System Settings" menu item under "System Manage";
' * fix bugID #17469: Can't set password as "blank" even it is allowed on the setting;

'D6220	19-06-26
'  * fix bugID #17474: BetaNS: RA's summary menus are missing in Advanced Mode -- broken;

'D6219	19-06-25
' * fix issue with nodes in the Tree list on Charts screen;
' * improve switching modes (Alternatives, Objectives) with trying to keep the current WRT/selected node;
' * fix bugID #17327: BetaNS: Combined Charts -- Issue and confusion with selecting WRT Node on charts;

'D6218	19-06-25
' * fix bugID #17471: BetaNS: Landing page icon is cut-off;

'D6217	19-06-25
' + onShowIFrame, use for show Snapshots, Project Logs;
' * show Project Logs as dialog without loading the model;

'D6216	19-06-24
' * fix issue with ProjectLogs screen when crashing when no active project;
' * allow to see the project logs from Projects List without opening the model (can be useful when model corrupted/can't be read);
' * fix bugID #17471: BetaNS: Landing page icon is cut-off;

'D6215	19-06-24
' * update clsComparionCore.ProjectCreate() for allow to copy model from any project, except the marked as deleted;
' * fix bugID #17470: BetaNS: Create Template from existing models wiped out the model's data (objectives, alternatives etc.);

'D6214	19-06-24
' * update wording "project" -> "model";

'D6123	19-06-20
' * use .PipeParameters.EvaluateDiagonals and disable ForceAllDisgonals on call CreatePipe() for evaluate intensities;
' * fix bugID #17238: BetaNS/Beta: Pipe settings for Assessing Scales is not working;

'D6122	19-06-18
' * fix bugID #17435: RiskionNS: Resource not parsed;

'D6121	19-06-18
' * improve Landing page for render content when all pages are not available (i.e. RA when no alts);
' * fix bugID #17357: BetaNS: Can't go to Allocate screens (broken);

'D6120	19-06-17
' * update AltsContribution, Infodocs, Participant Roles for show readonly when Archived;
' * update Participant Roles for hide Users List when Template project;
' * hide Participants List when Template project;
' * fix bugID #17320: BetaNS: Pages that should be VIEW ONLY for non-active projects (SL);

'D6119	19-06-17
' * add icon to sidebarmenu for restricted items when no alts, AIP;
' * enhance Landing page with show restricted items when no alts, AIP;

'D6118	19-06-17
' * enhance rferences to wireframes: now support any custom name, started with pageID[+<space>+<custom_text>].ext
' * fix bugID #17436: BetaNS: pre-Candidate Release changes;

'D6117	19-06-14
' * totally redo Personal Settings screen;
' * enhance webapi account/option with ability to save options from Personal Settings screen;
' * fix bugID #17436: BetaNS: pre-Candidate Release changes;

'D6116	19-06-14
' + add indicator about extra option(s) in Advanced mode next to switcher at the footer;
' + flash elements, changed visibility on switch Advanced Mode;

'D6115	19-06-14
' * rollback clsPipeParameters: UseCISForIndividuals, UseWeights, SynthesisMode now are not depend Advanced mode;
' * rollback clsCalculationsManager: SynthesisMode, UseUserWeights, UseCombinedForRestrictedNodes now are not depend Advanced mode;
' * update Synthesise.aspx for just update toolbar when switch Advanced mode;

'D6114	19-06-13
' * update wording on open model with on-line users;

'D6113	19-06-13
' * update clsCalculationsManager: SynthesisMode, UseUserWeights, UseCombinedForRestrictedNodes for ignore Basic mode and return real values;
' * fix issue with initial (real) option values on Synthesise screen when loaded in Basic mode and switched to Advanced;
' + add ability to use confirmation before switching Advanced mode (confirmSwitchAdvancedMode);

'D6112	19-06-12
' * change bgcolor on login screen to white; fix Comparion logo size;
' * update Notification.aspx for show Riskion logo when System license;
' * _IFDEF_RestoreLastPage is true by default;
' * new layout for Personal Settings screen;

'D6111	19-06-12
' + add clsPipeParameters.isAdvancedMode;
' * update clsPipeParameters: UseCISForIndividuals, UseWeights, SynthesisMode for return default values when not in Advanced mode;
' * update clsCalculationsManager: SynthesisMode, UseUserWeights, UseCombinedForRestrictedNodes for retrun default values when not in Adavnced mode;
' * update Synthesise.aspx for reload data/up[date toolbar when switching Advanced Mode;

'D6110	19-06-12
' * fix show priorities in the tree for ctrlShowResults2.ascx;
' * fix bugID #17371: BetaNS: No objective priorities displayed in Overall Results pipe step;

'D6109	19-06-12
' * fix crashing on Landing page when no active project;

'D6108	19-06-12
' * update PipeParams for disable all input element when project is ReadOnly; Reject add ajax calls;
' * fix bugID #17318: BetaNS: Pages that should be VIEW ONLY (AD);

'D6107	19-06-12
' * fix issue with Riskion logo on login screen and for mpDesktop.master;

'D6106	19-06-10
' * fix bugID #17375: BetaNS: Project Statistics icon not working;

'D6105	19-06-10
' * redo approach for show list of available EULA on edit workgroup; update preview EULA;
' * fix bugID #17426: BetaNS: Update the EULA links (commercial or non-com) on Create Workgroup page;

'D6104	19-06-10
' * fix issue with crashing while call hasPermission for TT/RA pages when no project;
' * fix bugID #17429: BetaNS: RTE when switch workgroup from Time Periods page;

'D6103	19-05-10
' * fix focus on ctrlMultiPairwise;
' * fix bugID #17366: Risk (beta) - Scales not showing for pairwise of probabilities;

'D6102	19-06-10
' + add Page Wireframe feature;

'D6101	19-05-28
' + fix bugID #15873: Missing Roles Report Screen;

'D6100	19-05-25
' * fix bugID #17341: betaNS: Update wording for popups;

'D6099	19-05-24
' * fix bugID #17197: BetaNS: "Create as Template or Default option Set" option is always disabled;

'D6098	19-05-24
' * fix bugID #17314: betaNS: Edit project name on project description tab;
' * fix bugID #17318: BetaNS: Pages that should be VIEW ONLY (AD);

'D6097	19-05-24
' * update list pof pages for Templates and Default Option sets;
' * fix bugID #17317: BetaNS: Issues (missing pages, pages to hide) with Templates, Archives and Default Option Sets;

'D6096	19-05-23
' * update list of pages for Archived projects, minor fixes;

'D6095	19-05-23
' * fix bugID #17316: BetaNS: Projects tabs get disabled when switch the Advance mode on or off;

'D6094	19-05-23
' * update UI/styles and button code on click for Notification.aspx;
' * fix bugID #17093: System Use Notification - for control AC-8;

'D6093	19-05-22
' * update checker for FedRAMP USE notification;
' * show System User notification for every session (was once for user);
' * fix bugID #17093: System Use Notification - for control AC-8;

'D6092	19-05-21
' + _PGID_STRUCTURE_MISSING_ROLES_REPORT;
' +  add dummy MissingRoels.aspx page;

'D6091	19-05-21
' * fix bugID #17210: BetaNS: Templates, Archived Projects and Default option sets only shows Iterate workflow;

'D6090	19-05-20
' * fix bugID #17031: RiskionNS: Redirect user to the specific pipe step to evaluate a Control is not working;

'D6089	19-05-20
' * force to open TT pipe for all users (even PMs) when logged by meetingID;
' * fix bugID #17286: BetaNS: Access PM TT link goes inside the model (both when meeting started or not);

'D6088	19-05-18
' * fix issue with resize Evaluation progress screen;
' * fix issue with tooltip on "Online" and "Lock" icons at the workflow row on switch status;
' * fix bugID #17167: betaNS: Issue with scroll bars in Evaluation progress;

'D6087	19-05-17
' + clsComparionCore.DBUserUnlock() that aloow to reset Password Status and/or change the password;
' * replace code for reset .PasswordStatus, .UserPassword for call DBUserUnlock();
' - clsComparionCorePage.ResetUserPassword() and call DBUserUnlock instead;
' * update CWSw: ResetUserPasswordLock() and UpdateUserInfo() with using new function;
' * minor update for CheckUserLockWhenInvalidPassword();
' * fix issue when auto unlock is enabled, and user tries to get access with a wrong psw after reset psw (due to locked account);

'D6086	19-05-17
' * temporary enable "Start meeting" pipe button for PM when inactive TT session;
' * force to redirect PMs to TT pipe when session inactive;

'D6085	19-05-17
' + iterateButtonsAndClick() to mpDesktop.master;
' * enhance mpDesktop.master.onKeyUp() with try to emulate button click on press [Ctrl]Enter or ESC keys;
' * specify 'button_enter' and 'button_esc' classes for most dialogs on Projects List and in masterpage.js;
' * fix issue on Spyron.js/isModifiedForm() when leave/reload the page;

'D6084	19-05-16
' * fix bugID #17286: BetaNS: Access PM TT link goes inside the model (both when meeting started or not);

'D6083	19-05-16
' * fix bugID #17093: System Use Notification - for control AC-8;

'D6082	19-05-16
' * updates for Personal Settings screen: update Application settings section, add "back" button, fix title;
' * fix landing page crashing when no active project/sitemap;
' * never show option "Don't show overview page more than once..." on landing pages;

'D6081	19-05-16
' + add _PGID_FEDRAMP_NOTIFICATION, /Notification.aspx;
' + clsComparionCorePage.isNotificationAccepted(), _COOKIE_FEDRAMP_NOTIFICATION;
' * enhance clsComparionCorePage.onPreRenderComplete for check isNotificationAccepted();
' * fix bugID #17093: System Use Notification - for control AC-8;

'D6080	19-05-15
' + _TEMPL_OBJECTIVES_LIKELIHOOD, _TEMPL_OBJECTIVES_IMPACT, _TEMPL_ALTERNATIVES_LIKELIHOOD and _TEMPL_ALTERNATIVES_IMPACT (as adding "(i)" or (l)" at the end of template);
' * enhance PrepareTaskCommon() for detect HID specific resources and parse it with using the wording from the related hierarchy;
' * fix bugID #17188: RiskionNS: Landing page has wrong "Likelihood" wording;

'D6079	19-05-14
' * fix bugID #17261: BetaNS: Login with meeting ID goes to Anytime when meeting is not yet started;

'D6078	19-05-14
' + InfodocService.isHTML();
' * valid call for SenMail() form Invite screens (Anytime, TeamTime);
' * fix bugID #17271: BetaNS: Pasting HTML text into Anytime Invitation makes the invitation include HTML tags;

'D6077	19-05-14
' * update CheckUserLockWhenInvalidPassword() for check user unlocks and try to count wrong attempts more careful;

'D6076	19-05-13
' * fix issue with show popups on load (especially WhatsNew and KnownIssues);
' * fix case #17196: BetaNS -- Possible issues with projects list;

'D6075	19-05-13
' + /api/account/passwordremind;
' * update PSW Reminder dialog (moved to masterpge.js from welcome screen) for use API instead of ajax;
' + show psw reminder psw dialog on login screen when passed "?remind(er)=true" or ?"action=remind%";

'D6074	19-05-13
' * update wording for a new psw lock options;
' * enhance /Install/Settings.aspx with a client-size checkForm() fpor dynamic option changes;
' + _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK and new UI option;
' * update checkers for auto-unlock an accounts depend on _DEF_PASSWORD_ATTEMPTS_AUTOUNLOCK option;
' + add dummy _FEDRAMP_MODE option;
' + web.config options (available on /Install/Settings.aspx): "AutoUnLockPsw" and "FedRAMP";

'D6073	19-05-12
' * fix bugID #17225: BetaNS: Projects list not loaded on first login;

'D6072	19-05-10
' + _OPT_LOCK_PSW_PERIOD (LockPswPeriod), _OPT_LOCK_PSW_TIMEOUT (LockPswTimeout);
' + add options on /Install/Settings.asx for user lock period and timeout;

'D6071	19-05-10
' * fix bugID #17253: BetaNS: "Associate Risk Model" redirects to Projects list instead of opening the modal to select a project;

'D6070	19-05-10
' * update CheckUserLockWhenInvalidPassword for use lock period check on DB logs via server time (was SQL server);
' * fix bugID #17090: Unsuccessful login attempt handling per control AC-7;

'D6069	19-05-10
' * fix 17251: BetaNS: Meeting URL is not being parsed on the signing up page;

'D6068	19-05-10
' ! move CheckUserPasswordStatus() [was CheckUserPasswordStatus] inside the Authenticate();
' * enhance DBSaveLogEvent with adding extra details as error message and login attempts when wrong psw;
' * count wrong login attempts prior saving log event in CheckUserPasswordStatus();
' * fix bugID #17090: Unsuccessful login attempt handling per control AC-7;

'D6067	19-05-09
' * ehnance /api/account/logon with call CheckUserPasswordStatus();
' * show welcome screen authentication error as alert (was notify);
' * fix bugID #17090: Unsuccessful login attempt handling per control AC-7;

'D6066	19-05-09
' * fix bugID #17244: BetaNS: Missing image in Groups details;

'D6065	19-05-09
' * use CheckUserPasswordStatus() on Start.aspx;
' * enhance Logon() and check PasswordStatus for suer and lock timeout: reset if required and put a log event;
' - remove  check for PasswordStatus with lastvisited timeout from DBParse_ApplicationUser (moved to Logon());
' * enhance CheckUserPasswordStatus() with putting log event on user lock;
' * update CWSw methods for put log events when rset user lock status or reset password;
' * fix bugID #17128: Log entries when account is locked and unlocked;

'D6064	19-05-09
' * fix bugID #17243: BetaNS: Selected scenario is not remembered when project is reopened;

'D6063	19-05-07
' * fix bugID #17218: RTE when switched workgroup;

'D6062	19-05-08
' + clsApplicationUser.LastVisited;
' + _DEF_PASSWORD_ATTEMPTS_PERIOD (def as 15 mins), _DEF_PASSWORD_ATTEMPTS_LOCK (def as 30 mins);
' * enhance DBParse_ApplicationUser for parse LastVisted, reset PasswordStatus when lock timeout is exceeded;
' * enhance DBSaveLogonEvent for get the real UserID (serach by UserEmail) when user is not logged in;
' + clsComparionCorePage.CheckUserPswStatus() for check last login events and update PasswordStatus if required;
' + _TEMPL_PSWLOCK_TIMEOUT (%pswlock_timeout%%), update ParseAllTemplates();
' * fix bugID #17090: Unsuccessful login attempt handling per control AC-7;

'D6061	19-05-03
' + _PGID_LANDING_RISK_IDENTIFY;
' + _DEF_PGID_ONSELECTPROJECT_RISK and _DEF_PGID_ONNEWPROJECT_RISK as _PGID_LANDING_RISK_IDENTIFY;
' * allow to open default pages for Risk projects;
' * fix bugID #17188: RiskionNS: Landing page has wrong "Likelihood" wording;

'D6060	19-05-03
' * set _DEF_PGID_ONSELECTPROJECT and _DEF_PGID_ONNEWPROJECT as _PGID_LANDING_STRUCTURE;
' * enhance jProject.GetActionResultByProject() with retrun properly default page URL;
' * force to collapse sidebar menu for landing pages and evaluation pipe;
' * fix bugID #17211: BetaNS: Change the text for empty projects list;

'D6059	19-05-03
' * fix issue with select/toolbar buttons enable when show "Deleted projects";
' * fix bugID #17209: BetaNS: Can create a new project from template even there is no templates?
' * fix bugID #17208: BetaNS: Permanently delete projects is not working;

'D6058	19-05-03
' * fix issue with navigate on joined riskion pipe when view only mode;
' + add loading on show embedded Risk Results pipe step;
' + add "back button" to Risk Results when came from evaluation pipe;
' * fix issue with getting the date for "view only" user on Risk results;
' * fix bugID #17025: RiskionNS: Risk Results in the pipe issues;

'D6057	19-05-03
' * fix bugID #17188: RiskionNS: Landing page has wrong "Likelihood" wording;

'D6056	19-05-02
' * update Projects List: try to fully re-init grid on change tab (onSetPage);
' * for empty workgroup show dialogs for start creating project only for "Active" projects list tab;
' * fix issue with templates list when template has been just deleted;
' * fix issue with showing PM isntructions splash more than once for each workgroup type in a session;
' * fix bugID #17036: BetaNS: Changes for the Projects List screen from Eileen;
' * fix bugID #17196: BetaNS -- Possible issues with projects list;

'D6056	19-05-01
' * update Projects List for show special block with buttons for download sample, create new model when no projects in workgroup;
' * fix bugID #17036: BetaNS: Changes for the Projects List screen from Eileen;

'D6055	19-05-01
' * fix issuen with finish TT/close window;
' * fix bugID #17187: BetaNS: RTE when stopping a TT meeting;

'D6054	19-05-01
' + _PGID_SURVEY_EDIT_POST, rename _PGID_SURVEY_EDIT to _PGID_SURVEY_EDIT_PRE;
' * update survey editor pages for track new _PGID and parameter "type";
' + api/project/landing_info geting survey questions count;
' * fix bugID #17169: Redesign Define Project Landing page based on wireframe screens;

'D6053	19-04-30
' * enhance jProject.GetActionResultByProject with ability to ignore Anitgua/TeamTime status/confirms;
' * another approach on ignore active TT sessions on upload/upgrade/copy model for force to open new model;
' * fix bugID #17168: BetaNS: Uploading a model while there is an active TT ongoing on the workgroup;

'D6052	19-04-29
' + api/project/landing_info, fix issue with loading landing info counters;

'D6051	19-04-29
' * update api/project/upload for check "anitgua" and "*teamtime" confirmation/messages and reset it for avoid confusing action;
' * fix bugID #17168: BetaNS: Uploading a model while there is an active TT ongoing on the workgroup;

'D6050	19-04-29
' * hide calling data mapping from data grid for non-localhost;
' * fix bugID #17178: BetaNS: Data Mapping shows RTE;

'D6049	19-04-25
' * update RA CustomConstraints screen for allow to import attributes for Controls;
' * fix bugID #17146: Riskion: Import Events Categories as Constraints for RA?

'D6048	19-04-25
' * update anytime evaluation for properly call RiskResults page when need to show as part of joined pipe;
' * update RiskResutls for hide links/menus/toolbars/etc when called as part of joined pipe for evaluators;
' * fix bugID #17025: RiskionNS: Risk Results in the pipe issues;

'D6047	19-04-24
' * fix issue with missing sidebar menu for administrative pages in Riskion workgroups;
' + allow to show landing pages when no active project;
' * enhance landing pages content generator for rollback to projects list page when no content;
' + show landing page when click administrative link from user menu in basic mode;
' * fix bugID #17056: BetaNS: no sidebar navigation for administrative pages when Basic Mode;

'D6046	19-04-22
' * fix RTE on PipeParams when no active project;

'D6045	19-04-22
' * enhance sidebarmenu.js/onMenuItemClick() for check hierarchy before call onSetPage();
' * fix bugID #17131: RiksionNS: Can't go to likelihood workflow when came from Impact's measure options screen;

'D6044	19-04-22
' * update sidebarmenu.js for tab navigation: show landing when stay on the same tab/hierarchy;
' * fix issue with switching hierarchy for landing pages;
' * fix bugID #17008: RiskionNS: show landing pages for all workflow items by default;

'D6043	19-04-22
' * fix bugUID #17132: Admin's hash link is displayed on invite page (should not be);

'D6042	19-04-21
' * update clsComparionCorePage.Init() for check active project on load any model page for outdated version and upgrade it;
' * fix bugID #17130: BetaNS: Outdated Model not being upgraded during upload?

'D6041	19-04-16
' * fix bugID #17120: BetaNS: TT hash link when meeting is not yet ongoing;

'D6040	19-04-15
' * fix issue with detect the right hierarchy on masterpage.js: initTopLineMenu();
' * fix bugID #17021: RiskionNS: Short cut to go to specific Likelihood/Impact page from the Projects list;
' * fix bugID #17048: RiskionNS: Wrong menu/workflow is selected when evaluating pipe;

'D6039	19-04-15
' * fix bugID #17122: BetaNS: Make the default Thank you and Rewards page centered?

'D6038	19-04-11
' * enable On-line switch on workflow menu line for Basic mode;

'D6037	19-04-10
' * update Projects list: return context menu for Comparion and Riskion (with Likelihood and Impact sub-links);
' + init isRiskion, curPrjID, curPrjHID on master pages;
' - remove warning on switch to Riskion workgroup;
' * enhance getProjectLinkDialog (masterpage.js) for pass extra title text;
' * update sidebarmenu.js: improve detect the active tab/workflow/page when Riskion and specific hierarchy, not exact pageID, etc.;
' * fix bugID #17021: RiskionNS: Short cut to go to specific Likelihood/Impact page from the Projects list;
' * fix bugID #17048: RiskionNS: Wrong menu/workflow is selected when evaluating pipe;

'D6036	19-04-10
' * update wording for don't show known issues popup on login;

'D6035	19-04-10
' * set show known issues as enabled by default;
' * minor updates for Latest changes, Known issues popup sizes;
' * force to create js resources with key as lower case, update resString() for getting by resource key as lowercase as well;

'D6034	19-04-09
' * redo popup with Known issues and add it to popups queue on Projects List (once in a day);
' * enhance Known Issues popup with option "Don't show...";
' + /api/account/ add option "show_issues";
' * rename routines for WhatsNew in masterpage.js;
' + clsComparionCorePage.ShowKnownIssues() property (based on cookies);
' * fix bugID #17084: betaNS: Updates for known issues page;

'D6033	19-04-08
' + enhance sidebarmenu.js with trying to detect the "mirror" for current page when Tab clicked;
' + store last visited page below the each tab, show first subchild when click on Tab and Landing is already displayed;
' * fix bugID #16450: RiskionNS: Syncing of menus when switching between Likelihood and Impact workflows;

'D6032	19-04-05
' * fix bugID #17061: RiskionNS: Missing menus when select Risk Sensitivities;

'D6031	19-04-05
' * fix bugID #17076: RiskionNS: Menus disappeared when going to Time Periods Settings page;

'D6029	19-04-04
' + draft icomoon font with icons;
' * move GetSynthesizeData() and all related code from dashboard.aspx to new webapi/pm/dahsboard;

'D6028	19-04-03
' + _PGID_KNOWN_ISSUES, /knownissues.txt;
' + api/service/ new action knownissues;
' + Known Issues page (added to PMs usermenu);

'D6027	19-04-01
' * rename /LandingPage.aspx to Info.aspx and now used only for show external docs or help topics;
' * move and rename /DocMedia/Landing/Comparion.aspx to /Project/Landing.aspx;
' + _PGID_LANDING_COMMON;
' * update /Project/Landing.aspx with analyse navpageid;
' * enhance openPage() in sidebarmenu.js for auto-show landing page for tab/workflow when no specific content and have childs;
' * fix bugID #17008: RiskionNS: show landing pages for all workflow items by default;

'D6026	19-04-01
' * rename "What's New" to "Recent changes" and set a limit for a month;

'D6025	19-03-29
' * updates for Projects list grid columns, columns chooser;
' * fix bugID #16387: BetaNS: Delete projects page should include "Delete Date/Time" and sort it by Desc;

'D6024	19-03-29
' * fix bugID #16378: Enhance Workgroup Manage Participants screen with adding last visited date;

'D6023	19-03-29
' * minor optimization for check user permissions in clsMasterPageBase;
' * fix RTE in Landing pages when trying to open when no active project (now landings are available when opened project only);

'D6022	19-03-27
' * enhance webAPI service/Session_State with extra params;
' * api/service/Session_State: get the active wkg projects list, created/visited/modified during the some period;
' + _DEF_PING_TIMEOUT, init ping_interval in masterpage.js based on that option value;
' * enhance masterpage.js, ping_params: now this is can be function or plain string;
' * minor layout update for ajax call error message block size;
' * jProject.GetProjectsList() now works with a list of project (was optional ProjectID);
' * fix issue with re-assign isAdvancedMode cookie on each page request, even callback;
' * update routines for grid resize on Project list, restore toolbar repaint on call resizeGrid();
' + onGetPingParams(), onPingRecieved() to Proejcts list client size code;
' + mark active project row on Projects list;
' * fix bugID #17037: BetaNS: Projects screen columns when browser is resized;
' * fix bugID #16449: BetaNS: Projects list is not updated until user relogin;

'D6021	19-03-27
' * fix bugID #16949: BetaNS: Can't delete the admin from a project;

'D6020	19-03-27
' * remove responssive links from Riskion.sitemap;
' + options EvalURL4Anytime_Riskion and EvalURL4TeamTime_Riskion;
' * fix issue with disabling some toolbar button after a short time when page loaded/resized on Project List;
' * force to show PM Instructions when no active projects in workgroup;
' * fix bugID #17026: RiskionNS: Hide Responsive Menus (Anytime and TT);

'D6019	19-03-26
' * fix bugID #17026: RiskionNS: Hide Responsive Menus (Anytime and TT);

'D6018	19-03-25
' + clsComparionCore.GetMaxUploadSize based on web.config MaxRequestLength option;
' * enhance upload dialog with list of allowed extensions and max upload size;
' * fix some issues on proejct upload;
' * disable "refresh" button while loading dataSource on Projects list;

'D6017	19-03-25
' * update DevExtreme up to 18.2.7 (was 18.2.4);
' * update fontawesome up to 5.8.1 (wa 5.6.3);
' * update ionicons up to 4.5.5 (was 4.5.0);

'D6016	19-03-25
' + options EvalURL4Anytime and EvalURL4TeamTime;
' * fix bugID #17019: Hide the Responsive TT from the menu;

'D6015	19-03-25
' * fix bugID #17016: BetaNS: "Hide offline users" option not working correctly;

'D6014	19-03-22
' * fix bugID #17007: Riskion: Mixed model status lost on Save As...

'D6013	19-03-22
' * fix for switch languages code;
' - hide old languages;
' * minor bg style update for welcome screen;

'D6012	19-03-22
' * fix bugID #17006: Be able to send Workgroup and Project info to the chat app? (betaHTML);

'D6011	19-03-22
' * use new API (1.3) for socialintents chat;
' * improve chat form wih extra params on start (project, wkg, sessionID);
' * enhance ping functionality on Comparion.aspx for refresh active project/workgroup names;
' * fix bugID #17006: Be able to send Workgroup and Project info to the chat app?

'D6010	19-03-21
' * update ProjectsList: add virtual scrolling, fix issue with refresh projects/license info; show pushable grid option buttons, etc.;
' * force to update Last Visited when update or lock project (DBProjectLockInfoWrite, DBProjectUpdate);
' * fix possible issue when no real data loaded by pressing "Refresh" on Projects list;

'D6009	19-03-21
' * fix issue with "Starred" column on Projects List;
' * fix issue with Projects Info on Projects List when navigate between pages, change page size, etc;
' * try to avoid reload projects list on data source object when re-init grid;

'D6008	19-03-21
' * hide old menu styles, old navigation;
' * fix RTE on load TT Status screen;
' * dirty fix for select "Manage Project" workflow item on List.aspx;

'D6007	19-03-20
' + _DB_COMMAND_TIMEOUT = 180;
' * enhance GetDBCommand() with set CommandTimeout;

'D6006	19-03-20
' + clsComparionCore.ApplicationErrorInitAndSaveLog();
' + try to save crash infoon any RTE not depend on HookErrors option;
' * enhance for RTE hook and show messages;
' * improve way to display the errors on ajax/WebAPI calls (masterpage.js);
' * ehnance Error.aspx for allow to expand details, submit crash info even when displayed as a truncated piece of content;

'D6005	19-03-19
' * hide "Home" button, but add "Manage Projects" to Workflow menu;
' * hide workflow items "Go Top"/"Page title" when Basic mode;

'D6004	19-03-19
' * fix issue with visible workflow menu items for Basic Mode when loaded in Advanced;

'D6003	19-03-18
' * fix bugID #16934: BetaNS - FF: Can't edit pipe questions/heading;

'D6002	19-03-18
' * fix issue with show infodoc icons on Hierarchy.aspx;
' * fix bugID #16972: BetaNS: infodoc icons not updated until page is refreshed (broken);

'D6001	19-03-18
' * try to fix bugID #16985: BetaNS: Can't paste image on infodocs for Chrome but can in IE and FF;

'D6000	19-03-18
' * fix issue with auto-collapse sidebar when "collapsedmenu" specified in sitemap;

'D5657	20-01-23
' * fix issue with submit direct value evalution step (disable code on CheckFormOnSubmit() in ctrlDirectData.ascx);
' * fix issue with init/load infodocs on Direct inout for Risk with Controls evaluation;
' * fix bugID #18666: RiskionNS: Missing infodoc for an event for "vulnerabilities from events to events";

'D5099	19-03-15
' * fix bugID #16972: BetaNS: infodoc icons not updated until page is refreshed (broken);

'D5098	19-03-15
' * fix bugID #16973: Automatically switching to Advanced Mode after adding alternatives?

'D5097	19-03-15
' * updates for sidebarmenu.js: fix issue with collapse panel, hide pinner for narrow screens;

'D5095	19-03-13
' + add "auto-collapse sidebar menu" option and update sidebarmenu.js;

'D5094	19-03-13
' * fix bugID #16970: BetaNS: Can't go to another workflow when current page is Measurement Methods screen;

'D5093	19-03-13
' * fix bugID #16967: betaNS: unable to close infodoc window;

'D5092	19-03-12
' * join project description, project details and download screens;
' * enhance joined Project details screen for load required tab and switch wiyj the single form object;
' * update permissions for project details tabs due to use it as new download screen (must be valid when no active project);
' - _PGID_PROJECT_STATUS, _PGID_PROJECT_UNLOCK, _PGID_PROJECT_LOCK, _PGID_PROJECT_UNDELETE;
' - Properies.aspx, Download.aspx, Status.aspx;
' * fix issue when try to download a huge count of projects at the same time from projects List: download by parts as 150 models at once;
' * fix issue with initWorkflow when current page is missing in navigation sitemap;
' * fix bugID #16953: betaNS: Redesign Manage Project screens;

'D5091	19-03-12
' * merge Project Online Status and Evaluation progress pages: now Evaluation Status;

'D5090	19-03-11
' * fix bugID #16954: For new model timeperiods start date should be current date instead of 2016;

'D5089	19-03-11
' * redo approach for store clsWorkspce.isStarred due to not possible to track both states when set disabled/unknown;
' * fix bugID #16945: BetaNS: Session expires for the PM when disabling a participant on a model;

'D5088	19-03-11
' * fix bugID #16960: Show a Heading for the Screen Title when in Basic Mode;

'D5087	19-03-09
' * update styles for popup tooltips (langs, shortcuts);
' * update styles/layout for shortcuts;
' * always show sorted by keys the list of shortcuts;

'D5086	19-03-09
' * update Projects List and Account Settings for show content depend on Advanced Mode;
' * update MeasurementMethods screen: hide row with mode and "Detailed" option, support to switch on change Advanced mode;
' * fix bugID #16950: BetaNS: add option for switch between the Basic and Advanced modes;

'D5085	19-03-09
' * keep going to landing page when basic mode;
' * navigate on Go Top link when basic mode;

'D5084	19-03-08
' * enhance Comparion landing with update on switch isAdvancedMode;
' + show labels for project actions, some layout updates;
' + auto-collapse/expand sidebar menu when switching isAdvancedMode;
' + mastrpage.js add urlWithParams(), clsComparionCorePage.PageWithExtraParameter();
' * minor update for GetClientIP(), use that routine on get user IPO on save LogEvent, on save last visited;

'D5083	19-03-08
' + isAdvancedMode option based on cookies, add option to master page;
' + webAPI account new option "AdvancedMode";
' + add callback routine on switch isAdvancedMode: onSwitchAdvancedMode() and option reloadOnAdvanced when need fullpage reload;

'D5082	19-03-08
' * update styles and layout for footer icons;
' + add dummy "Advanced mode" option to footer;

'D5081	19-03-07
' * fix switching language: keep the original page with all params;

'D5080	19-03-07
' + add "On-line status" icon to workflow;
' + add "Lock status" icon to workflow;
' * update webAPI unlock: rename to set_lock with extra parameter;
' + masterpage.js: onChangeActiveProject, even called on the push for active project;
' * fix bugID #16464: BetaNS: Unlock project not yet implemented;

'D5079	19-03-07
' * move isJustEvaluator() to clsComparionCorePage;
' + clsComparionCorePage.CanViewActiveProject();
' * update references to isJustEvaluator() and use CanViewActiveProject();
' * fix issue with show workflow menu for just evaluator on Projects List;
' * fix issue with enable toolbar on Projects List when data source loaded;
' * fix bugID #16947: BetaNS: Tabs and buttons in the Projects List suddenly became disabled;

'D5078	19-03-07
' * update admin Hash/Token decoder for show group when invite link (wkgrole param);
' * update Anytime invitation for properly init, reduce calls for link update on show, fix link creator;
' * fix bugID #16157: betaNS: Sign up/ Login screen doesn't work;

'D5077	19-03-06
' * update anytime invitations screen (proper init tabs, some options);

'D5076	19-03-06
' + api/project/ unlock;
' * fix bugID #16464: BetaNS: Unlock project not yet implemented;

'D5075	19-03-06
' * fix bugID #16930: BetaNS: Missing icons (view only and login as another user) in Inconsistency Reports screen;

'D5074	19-03-05
' * fix issue with direct link icon in Firefox;
' + clsMasterPageBase.isJustEvaluator;
' * hide starred buttons for just evaluators;
' * show "What's New" at the footer only for PMs;
' + add call cancelPing() on page unload (prevent error msg on refresh page in FireFox);
' + add "Collect my input" link to header for just evaluator, single eval mode;
' * fix bugID #16288: BetaNS: Add also the "Collect my input" link so user can easily return to the pipe;

'D5073	19-03-04
' * fix layout issues for anytime evaluation controls;
' * fix bugID #16290: BetaNS Firefox: Pipe elements are vertically centered;

'D5072	19-03-04
' * disable auto-browse file on "Create from file" dialog/Projects List;
' * fix issue with top navigation for evaluators;
' * fix issue with Rich Editor as popup (called from the pipe);
' * fix issue with verbal PW scale alignment, some layout fixes (FB16006);
' * fix issue with jDialog call on ctrlPairwise.ascx;
' * fix issue with resize framed infodocs (FB16421);

'D5071	19-03-01
' * allow to set focus to Rich editor when opening it;
' * try to use common call point for OpenRichEditor();
' * add another way to show rich editor as dialog;
' * update RichEditor for show/close when is dialog mode;
' * fix issue with upload images on RichEditor;
' * fix bugID #16453: BetaNS: Rich text editor not always 'on-top' for Firefox browser;

'D5070	19-03-01
' * disable toolbar until load dataset on Projects List;
' * fix bugID #16458: BetaNS: Default option set dropdown is missing when clicked New Project while page is still loading;

'D5069	19-03-01
' * fix bugID #16457: BetaNS: FAQ button is missing;

'D5068	19-03-01
' + _OPT_ALLOW_REVIEW_ACCOUNT (false);
' * fix bugID #16456: BetaNS: Hide the Consultant Review option;

'D5067	19-02-28
' * fix issue with adding google script to header in clsMasterPageBase;
' * fix bugID #16463: BetaNS: Utility Curve control is not working in IE (choice-beta);

'D5066	19-02-28
' * fix declarations for Riskion pages on call AddPage();
' * disabling auto-fix URLs on get all pages when called Pages() property;
' * fix bugID #16442: BetaNS: Riskion menus and screens issues;

'D5065	19-02-28
' * fix issue with show msg when open locked project;
' * fix issue with re-open model with ignored online participants;
' * fix bugID #16447: BetaNS: Resource not parsed;
' * fix bugID #16434: BetaNS: open the model issue when online participants;

'D5064	19-02-27
' * simplify background message/spin icon at the footer;
' * fix issue with selected item at the sidebar menu;
' * fix issue with NavigationPageID in case of big pguid (new pguid generator);
' * fix issue with js code for "Iterate" pages;

'D5063	19-02-27
' * getSessionStateJSON based on the webAPI code;
' * pre-load hash for session state for avoid reload some objects on the first ping callback;
' * minor optimization for parse sitemaps;
' + add hidden /api/service Application_State() method;
' * remove global waiting panel on Projects List since grid using his one;
' * avoid to save service pages (_PGID* >= 90000) as last visited;

'D5062	19-02-27
' + api/accounts/ Pages_Statistic for get the code for pages statistic (admins only);
' * enhance clsmasterPageBase.Parse4Stat() for get the Hierarchy, Riskion specific names, show #PGID for missing pages, etc;
' * enhance popup with pages statistic: show content via webAPI only on demand, minor layout changes;\
' * fix Riskion navigation for case when only workflow items and no submenus;
' * fix issue with LandingPage when no navigation since init NavigationPageID with a passed param;
' * fix issue with missing navigation for Reports;
' * fix bugID #16442: BetaNS: Riskion menus and screens issues;

'D5061	19-02-27
' * fix issue with footer "What's New" link;
' * updates for shortcuts popup, fix issue with a Riskion Brainstorming links;

'D5060	19-02-27
' * upgrade TimyMCE up to 5.1;
' * fix init TimyMCE editor, few related fixes;
' * fix bugID #16420: BetaNS: Infodoc editor not loading properly (broken);

'D5059	19-02-26
' * enhance sitemap parser for support riskion hierarchy tag and modify page links for switch by passcode when required;
' * update styles for workflow menu: allow to show static (disabled) menu items;
' * enhance initTopLineMenu() in sidebarmenu.js with tries to show workflow or even sidebar menu when no active tab/workflow below;
' * fix issue with sidebar menu loader (blank content) when no navigation items detected;

'D5058	19-02-26
' * try to avoid caching js files as using ?dt= param based on \bin\Application.dll modify datetime;
' * fix bugID #16409: betaNS: js error on login with mobile phone;

'D5057	19-02-25
' + NavPageURL();
' * update clsMasterPageBase: show submenu items even if this is single child;
' * track PageID duplicates on parse sitemap in clsMasterPageBase and extend URLs  with virtual navpgid;
' * minor layout fixes for Riskion navigation (mpDesktop.master), hide warning on Rickion pages;
' * allow to see Riskion workgroups for non-local instances;
' * update Riskion.sitemap: remove invalid help URLs, supress negative pageIDs, etc.;
' * fix getting valid resources for page name, title when Opportunity model;
' * dirty fixes for cases when trying to perform page redirect on call Authenticate from WebAPI pages (return an error with destination URL);
' + enhance clsComparionCorePage.Init: check "pgid=" parameter for set CurrentPageID and "navpgid=" for set NavigationPageID;
' * allow to navigate between th Riskion pages when a different tabs/workflow/hierarchy;

'D5056	19-02-23
' * hide "Upload" for objective sets dialog since no real API for store;
' + add abiltiy to download objective set from dialog on Structure screen;

'D5055	19-02-23
' + masterPage.js: uploadDialog(), cell from Projects list and on upload node sets on Structure;
' * add from datasets dialog updates on Structure screen;

'D5054	19-02-22
' * bind data, called via /workgroup/nodeset/?action=list to UI on Add items on Strcture screen;

'D5053	19-02-22
' * enhance clsTextModel: update methods for real alternatives, add ReadAlternativesToHierarchy();
' * update jNode: try to parse Infodocs if isMHT, hide some props for now;
' * enhance jNodeSet with auto-parse .Content as list of clsNode and cache it;
' * updates for Add items datasets dialog on Strcture page;

'D5052	19-02-22
' * fix issue with upload model on Projects List (when no extra actions required);
' * fix bugID #16165: BetaNS: compare oData v4 API with simple REST;

'D5051	19-02-21
' * fix bugID #16417: Gamma: Excel download of Participant Specific Links are always for responsive;

'D5050	19-02-21
' * fix issue with message when search and no matches found on Projects  list;
' * fix issue with blank/wrong screen on pipe (reported by Hal), projects list (reported by EF) and probably some others;
' * fix issue with display full What's new list even when shown this day; updates for popups queue;
' * fix issue with redundant params in direct link;

'D5049	19-02-19
' + /api/service/?action=whatsnew;
' * fix dialogs with PM instructions and What's New;
' + dlgMaxWidth(), dlgMaxHeigth(), showWhatsNew() to masterpage.js;
' * update queue for popups on Projects list, add "What's New" with limit 20 events by click on version number at the footer;

'D5048	19-02-19
' * updates for "What's new" dialog, add messages/popups queue on Projects Lis when started;

'D5047	19-02-19
' * fix bugID #16388: BetaNS: Missing 'reset' image in online participants page;

'D5046	19-02-19
' * upadte CanChangeProjectOnlineStatus() for return true in case of max online license limit exceded and project is online;
' + allow to copy ink to project even in case when can't do it online;
' * fix issue with show message when unable to set project online via webapi;
' * fix bugID #16399: BetaNS: unable to switch project off-line when license llimit is exceeded;

'D5045	19-02-19
' * updates for Riskion menu tabs;

'D5044	19-02-18
' + What's new on Projects list;
' * minor updates for store navigation structure when toggle;

'D5043	19-02-18
' + Comparion2018.sitemap;
' + add processing for 'resname' in sitemap roles for force to use resource name instead of listed in clsPageAction;
' * updates for /api/account/?action=allowed_pages;
' * add ability to toggle navigation structure by pressing Ctrl+Alt+N;
' * fix bugID #16367: BetaNS: Re-do Comparion menus;

'D5042	19-02-15
' + /api/workgroup/nodeset/ with list and restore_defaults methods;
' ! rename forder with node sets samples from /DataSets/ to /NodeSets/;
' + WebAPI/jWorkgroup with jNodeSet;
' * move some NodeSet code from clsComparionCore to clsNodeSet;

'D5041	19-02-14
' + clsNodeSet;
' + add base NodeSet* methods to clsComparionCore;
' + add samples for Comparion datasets;

'D5040	19-02-13
' ! move GetProjectsList() form project webapi to jProject as sherad function for now;
' * optimize loading streams data info when asked GetProjectsList for a single project;
' * fix issue with show toggled stared project status at the header;
' * fix issue with project undelete and wipeout;
' * fix issue with show project status (TT sessio, Lock info, etc);
' * fix issue on refresh starred projects dialog; force to load from server when on projects list by "Refresh" click;
' * fix RTE on switch workgroup;
' + jProject.ComplexStatus, use it on server and client side (added getComplexStatus() to masterpage.js);
' * update Update.aspx page (upgrade project version to the latest one) for a proper redirect and replace current page in history;
' * fix issue with return URL on call /api/project/ Upgrade method (passed pgID instead of URL);
' * fix bugID #16165: BetaNS: compare oData v4 API with simple REST;

'D5039	19-02-11
' + /api/account/ add method Allowed_Pages;
' * fix issues with api/project/: IterateProjectsList(), Replace method (return jProjectShort); rename Active* to ActiveProject*;
' * update mpDesktop.master: escape chars in project title tooltip; fix callAPI /api/project routes; move some code to initNavigation();
' * update Projects List: fix issue on create, archive, delete, copy projects;
' * fix issue with SaveAs dialog on Projects List;
' + allow to switch between tabs without grid re-init/reload data via api: use standalone funcs for columns list and filter;
' * fix issue with ping (was not passed right hash to track changes only);
' * update masterpage.js: new way to show Project Replace dialog, few issues for callAPI when list if project IDs.
' + StringFuncs.RemoveBadTags();
' * force to strip dangerous tags from passed form/URI when call CheckVar() and clsMaterPagebase.ParseJSONParams();

'D5038	19-02-09
' * update Google Analytics scripts/code;
' - /api/projects/ Can_Create;
' * Web API service session_state now return jProject (was jProjectShort);
' * updates for Projects API: Upgrade (check version) and for close (use active when no ID passed);
' * move openProject() and all related stuff from List.aspx to masterpage.js;
' + add events on DataSource push data and update active project UI elements on master page;
' + add "Refresh" and "Close" buttons to Favorite projects dialog;
' * reorganize code sections in masterpage.js;

'D5037	19-02-10
' * replace old callAPI on projects List with a new calls;
' * add handlers for callAPI and replace most callAjax to callAPI on Projects List screen;
' * few updates and fixes for masterpage.js, add funcs for show error messages on get reply;

'D5036	19-02-09
' * get list of master projects and templates from list of loaded model on Projects List (was created by server);
' * update for Starred Projects dialog;
' * rapid loading Starred Projects when Projects List page;
' + "Clear All" button for Starred Projects;
' * minor updates for FetchIf*Project functions: return checked project;
' + use DataSource.push() for reflect changed on tolggle starred (was whole data source reload);
' * update onToggledProject() for switch icons(s), allow to pass list of projects and just an ID;
' - remove all ajax call on Project List page since using webAPI;
' + masterpage.js add getItemByID();

'D5035	19-02-08
' * fix issue with open project on Project Lookup screen;
' * update loader on Projects Lookup screen
' * fix bugID #16356: BetaNS: Can't open model from Project Look up?

'D5034	19-02-08
' * fix issue with call frameLoaderInit on iframe (mpDesktop.master, List.aspx);
' * redo approach for show Snapshots list dialog;
' * enhance show/hide LoadingPanel for apply on parent when available (covers parent' content for dialogs);
' * fix bugID #16357: BetaNS: Snapshots list not showing (broken);

'D5033	19-02-08
' ! /api/ renames: user>account, project>pm, add projects/user;
' * fill /api/projects with all required methods;
' * update callAPI for auto-encode data and suppress request method (force to use POST);
' * update all /api/ methods (except /service/) for return jActionResult objects;
' * updates for clsMasterPageBase.Fetch*() default messages, add FetchCantEditProject(), FetchIfCantEditProject();
' * update Project List loading data with get all license data and track sever errors/messages; allow to force reload from DB on get the list;
' + clsComparinCore.CanActiveUserModifyProject();
' * enhance clsComparionCore.ProjectReplace() for extra checks and return error message;
' * jUser, jAppUserShort, jOnlineUser; jProjectShort, jProject;

'D5032	19-02-07
' * jProjectShort, jProject, jProjectsList;
' * enhance GetMethodsList() for get the list by alphabetically sorted;
' + Str2ProjectStatus(), Str2ProjectType();
' ! clsProject.DBUpdated rename to clsProject.IgnoreDBVersion;
' * /api/projects/?action=list now return jProjectsList (was just a List(of jProject));
' + add a bunch of methods to /api/projects;

'D5031	19-02-07
' ! rename SendResponse to SendResponseJSON and force to response as application/json;
' * masterPage.js add isValidReply() and use it instead of parseReply on get answer by callAPI();
' * use CustomStore with calling WebAPI as dataSource for Projects List grid;

'D5030	19-02-06
' * draft for export charts;

'D5029	19-02-06
' + Ctrl+Alt+S for switch menu styles (add extra CSS classes for override rules);

'D5028	19-02-06
' + different look for workflow per Eileen request: add .workflow-no-arrow, .workflow-no-icon;

'D5027	19-02-06
' * fix issue with json serialization for most WebAPI methods;
' * enhance SendResponse(Data As Object) with check type of Data object;
' - jData;
' * update Project List for tyr to use json, called from WebAPI /api/projects/action=list (still have issues with filtering);

'D5026	19-02-06
' + enhance /api/ list reference;
' * rename /api/auth/ to /api/user/;
' + new /api services: manage, project/hierarchy, workgroup;
' * rename some WebAPI methods, update used calls;
' * improve enhance GetMethodsList() and enhance the response on get api help;
' * move most WebAPI common code to mpWebAPI.master;

'D5025	19-02-05
' + /api/ folder with a list of available services: /auth/, /projects/ (empty for now) and /service/;
' + clsComparionCorePage.GetParam() that works and overriden GetParam() but can check GET vars (CheckVar[string]);
' + StringFuncs.GetMethodsList();
' * update masterpage.js: fix issue with auto-start Ping when shouldn't be (after perform ajax call);
' * new approach for callAPI in masterpage.js, based on callAjax but with custom URI;
' * clsMasterPageBase: add FetchWithCode(), ParseJSONParams() and extend all Fetch* routines with extra optional params;
' * replace old API calls on login, EULA and for Ping with a new WebAPI calls;

'D5024	19-02-05
' - remove oData NuGet package and all dependencies;
' - remove WebApiConfig, all routing, controllers;
' - upgrade NuGet packages (Newton.JSON, TinyMCE, SCSS compiler);
' + add ExpertChoice.WebAPI: clsJsonObject, jResultAction, jAppUser, jProject, jNode, jContribution*;
' * enhance clsMaterPageBase with ResponseError(), Fetch*(), SendResponse();
' + mpWebAPI.master;

'D5021	19-02-04
' * fix issue with resize/scroll favorite projects list;
' * try to optimize calls for refresh Projects List grid content when a bucnh of callbacks;
' * fix bugID #16273: Allow to mark projects in the projects list;

'D5020	19-02-04
' * update styles for workflow and sidebar menu per ER/Eileen requests;

'D5019	19-02-02
' * fix few issues on Ping.aspx;
' * check if the regular ajax in progress when time to call ping query to the web server (skip it in this case);
' + masterpage.js: getRandomString(), getRandomHex();
' * masterpage.js: totally new approach for _call_error, remove statusCode in callAjax;
' * masterpage.js: add cancelPing(), update onPing(), analyze ping response in onPingData() and ask page reload when critical changes;
' * fix bugID #16214: BetaNS: redo approach for ping/keep user session;

'D5018	19-02-01
' ! move /Application/Models/ to /CanvasWebBase/Models;
' ! rename ProjectListItem to ProjectItem;
' * move GetSessionUserName(), GetActiveProjectDataJSON() to clsComparionCorePage;
' - disable unused WebAPI: ProjectListItem, LogEventItem, NodeItem;
' - remove clsComparionCorePage.JS_SessionData();
' + draft /Ping.aspx version;
' - remove _ping* vars, funcs and related from misc.js;
' + add draft ping* vars and funcs to masterpage.js; call doPing() after each ajax request (with a timer);

'D5017	19-02-01
' * fix RTE on call clsWorkspaceByDateDescComparer;
' * fix bugID #16273: Allow to mark projects in the projects list;

'D5016	19-01-31
' * redo way to refresh icons on toggle starred;
' * fix issue with toggle starred for active project no mpDeskyop.master;
' * fix issue with login from welcome screen when no "Remember me" checkbox (due to settings);
' * force to reset clsComparionCore.CurrentWorkspace when set value for .Workspaces or call DBWorkspace_Update();
' * move some code from pages to masterpage.js: setProjectOnline(), closePopup(), getProjectLinkDialog();
' + new msterpage.js code: askSetProjectOnlineAndAction(), checkProjectOnlineAndCopyLink();
' * update Details, Evaluate/Invite, TeamTime/Invite, List.aspx for use moved/new masterpage.js funcs;
' * fix bugID #16180: BetaNS: Prompt to switch project online is not showing up for all screens;

'D5015	19-01-31
' * use more "light" icons at the mpDesktop.master header;
' + restore _PGID_RESOURCE_CENTER page, add User menu item for resources;
' * move Feedback button to user menu and add pre-loader for Customer feedback page;
' * updates for masterpage.js: frameLoaderInit() and onFormNarrow();

'D5014	19-01-31
' * try to fix crashing on get starred projects (List.aspx);
' * fix bugID #16273: Allow to mark projects in the projects list;

'D5013	19-01-30
' - _PGID_PROJECTSLIST_STARRED, remove Favorites.aspx;
' * load starred projects list via ajax from Projects List and show in dialog on mpDesktop.master;
' + add "Starred" icon next to Project title at the header for mpDesktop.master;
' * fix bugID #16273: Allow to mark projects in the projects list;

'D5012	19-01-30
' * allow to toggle starred on Favorites (with reload projects list in the backend);
' * fix bugID #16273: Allow to mark projects in the projects list;

'D5011	19-01-30
' * move code for fill ProjectListItem from ProjectsController to ProjectListItem and make it share: FillProjectData(), CreateFromBaseAndFillData();
' * update list for Favorites;
' + add ability to open project from Favorites;

'D5010	19-01-30
' * updates for starred projects page, starred marks on Projects List, way to open starred list;

'D5009	19-01-30
' * fix layout issues for Install/ pages, remove link to Decisions.aspx;
' * update HasPermission for ignore check when don't show Wkg Participants while no ShowDraft;
' * fix bugID #16305: BetaNS: Missing Workgroup Participants in gwcomparionns;

'D5008	19-01-29
' * fix bugID #16305: BetaNS: Missing Workgroup Participants in gwcomparionns;

'D5007	19-01-29
' + check model when upload Comparion to Riskion and show about hierarchy destination (can be added to model replace dialog);
' * fix bugID #16245: Upload Comparion model to Riskion enhancement;

'D5006	19-01-28
' + clsWorkspace: StatusData int fields and StatusLikelihood/StatusImpact as properties;
' + clsWorkspace.isStarred;
' * update DBParse_Workspace and DBWorkspaceUpdate for use updated clsWorkspace.StatusData* fields;
' * enable _OPT_ALLOW_STARRED_PROJECTS;
' * draft for starred projects based on clsWorkspace store;

'D5005	19-01-28
' * update preloder;
' * fix issue with align alternatives name on ctrlShowResults2 grid;
' * minor updates for Projects List;

'D5004	19-01-28
' * fix bugID #16142: betaNS: add floating button for return back to normal view when fullscreen;

'D5003	19-01-28
' * minor fix for clsMasterPageBase.GetPageTitle() when root;
' * encode passcode on start/resume TT session;
' * ignore online users when TT or bran=instorming session is active;
' * fix bugID #16230: BetaNS: Open TT meeting on another tab;

'D5002	19-01-28
' * update swipe for toggle sidebar menu;
' * wide workgroup selector on expand when narrow screen;
' + masterpage.js: add onFormNarrow(), use on Project Details;

'D5001	19-01-27
' * another approach for show error message on ajax callback;
' * enhance materpage.js:showBackgroundMessage() with show still or spin icons (by default);
' * fix issue with lock to antigua on Structuring screen: unlock when session finished or not in active/pause state;
' * fix issue with save encoded symbols while edit account User Name;
' * redo option ShowLandingPages() (now store as cookie, was Extra user option);

'D5000	19-01-26
' * redo layout for Project Status (on-line) screen;
' * optimization and fixes for master page styles (rules) for make it possible to show on a very narrow screens (320px+);
' * update Project List for show it on small screen resolutions wihtou the scrollers;
' + allow to use swipe for toggle sidebar menu by any swipe over the header and footer;

'D4778	21-11-11
' * fix templates for e-mail invitations;
' * Projects List: wide scroller, another columns resize mode;
