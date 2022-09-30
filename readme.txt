Replace the following values to customize a SelfHost version after running on FinalBuilder

SelfHostMasterDB		someDB
SelfHostDBServer		127.0.0.1\Instance
SelfHostDBUser			sa
SelfHostDBPassword		SomePassword
SelfHostwebsitename		coregamma
SelfHostDomainNameDotCom	expertchoice.com
SelfHostEmail			Mike Fomran
SelfHostFullEmail		michael@yahoo.com
SelfHostSMTPServer		somesmtp
SelfHostSMTPUser		smtpuser
SelfHostSMTPPassword		smtppass
SelfHostSMTPPort		25


onenote:///\\ecibiz\FS\OneNote\Comparion\Setups_Licenses.one#section-id={5F12F8C6-DAAA-487E-97B5-19D7CB7272F4}&end

-----------------------
How to install Silverlight 3 (by Sergey Lysikov):
	
1. Open "Control Panel\Programs and Features"
2. Uninstall ALL previous Silverlight items (Microsoft Silverlight, Microsoft Silverlight 2 SDK, Microsoft Silverlight 2 Tools for Visual Studio 2008 SP1)
3. Download and Install Microsoft® Silverlight™ 3 Tools for Visual Studio 2008 SP1:
http://www.microsoft.com/downloads/details.aspx?familyid=9442B0F2-7465-417A-88F3-5E7B5409E9DD&displaylang=en

If you are going to edit sources of Silverlight applications then also download and intall Silverlight 3 Toolkit July 2009 installer:
http://silverlight.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=24246#DownloadId=73527

-------------------------

By Sergey Lysikov:

1. Open Uninstall a Program from the Control Panel 
2. Click 'View installed updates' in the task bar on the left 
3. Remove KB949325 (it will be listed under your version of Visual Studio 2008) 
4. Install silverlight_chainer.exe from here: http://go.microsoft.com/fwlink/?LinkId=120319
5. Also you can Install (not necessary for building) Expression Blend 2.5 June 2008 Preview from here: http://expression.microsoft.com/en-us/cc643423.aspx

If this still doesnt work, run from a .NET command prompt

devenv /setup 

-------------------------

Steps to install Silverlite 2.0 RTW

1. Uninstall from Control panel - Programs and Features all MS Silverlight entries (Silverlight, Silverlight SDK, Silverlight Tools) and Blend expression
2. Download and install Microsoft Visual Studio 2008 Service Pack 1 (VS90sp1-KB945140-ENU.exe) from here: http://go.microsoft.com/fwlink/?LinkID=122094
3. Download and install Silverlight tools from here: http://go.microsoft.com/?linkid=9394666
4. Delete folders: AdvancedAnalysis\Bin\Debug\, AdvancedAnalysis\obj\Debug\, SLPipe\Bin\Debug\, SLPipe\obj\Debug\.

---------

* You can read short manual by the MS here:
  http://support.microsoft.com/kb/317604

  In couple words, you need to do:

  1. Go to the system drive\Windows\Microsoft.NET\Framework\version\
     (where version is v2.0.50727, as for me, even when I have 3.0 and
     3.5) and try to execute script InstallSqlState.sql manually under
     your primary MSSQL account (as usual, sa) or just execute wizard
     "aspnet_regsql.exe", where use your settings for Server Name,
     just select your SQL instance and provide ASPState as DB name.

     As result on this step: you will have new DB for storing
     sessions: ASPState and also two table "ASPStateTemp..." in the 
     system DB "tempdb";

  2. Open web.config file in our Application/ folder and see for line
     <sessionState mode="SQLServer" ... />

     You need to check "sqlConnectionString=" parameter and modify
     it, if required (I mean, update for your SQL instance name and etc.)


 Ideally, all should be fine after that. 


UPDATE: aspnet_regsql.exe -S <YOURMACHINE>\SQLEXPRESS -E -ssadd -sstype p



 Alexander Domanov



Unable to start debugging. The Silverlight managed debugging package isn't installed.
--------------------------------------------------------------------------------------

If you installed latest version of Silverlight (Silverlight 4), it may
break the your current silverlight development environment. After in the
Silverlight 4 installation, if you try to debug any Silverlight 3 project,
you will get a message from Visual Studio 2008, saying "Unable to start
debugging. The Silverlight managed debugging package isn't installed."


To fix this error, install the Silverlight Developer Run time. You can get
it from here : http://go.microsoft.com/fwlink/?LinkID=188039.


==================================================================
Fix for "Not found" error on run solution:

CWSw/web.config, need to add minFreeMemoryPercentageToActivateService="0", like this:

<serviceHostingEnvironment minFreeMemoryPercentageToActivateService="0" aspNetCompatibilityEnabled="true" />



-----------------------------------------------------
SSL support for Application and CWSw service (Dmitry):

Application - properties - SSL Enabled;
CWSw - properties - SSL Enabled;
CWSw - properties - Web - Project URL - https://localhost:44365/ (not strongly needed);
clientaccesspolicy.xml - <allow-from https-request-headers="*">
cwsw\crossdomain.xml -   <allow-http-request-headers-from domain="*" headers="*" secure="true" />

Use web.config.local.default configuration file OR make the following changes to the existing configs:
Application\web.config use https and port 44365 for services (https://localhost:44365/CoreWS.svc, etc.) - CWSwURL, CoreServiceURL, StructuringServiceURL, StructuringAdminServiceURL;
Application\web.config - ForceSSL = 1;
Application - Start URL - https://localhost:44336/default.aspx
CWSw\web.config -  in <basicHttpBinding> change None to Transport: <security mode="Transport" />
CWSw\web.config - <binding name="DuplexBinding" ...> remove <httpTransport /> and add:
          <httpsTransport authenticationScheme="Anonymous" manualAddressing="false" maxReceivedMessageSize="524288000" transferMode="Buffered" />          
CWSw\web.config - remove "Dual" binding and endpoint;

refreshsvc:
refreshsvc.bat - CWSPort = 44365 and 4 times http -> https, web.config - replace all mexHttpBinding with mexHttpsBinding;


