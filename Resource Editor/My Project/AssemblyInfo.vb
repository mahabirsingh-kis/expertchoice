Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("Tyro Resource Editor")> 
<Assembly: AssemblyDescription("")> 
<Assembly: AssemblyCompany("Expert Choice, Inc")> 
<Assembly: AssemblyProduct("Resource Editor")> 
<Assembly: AssemblyCopyright("Copyright © Expert Choice, Inc [2006, 2017]")> 
<Assembly: AssemblyTrademark("")> 

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("eba94087-3790-4ec4-bd70-ba36c5670558")> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("1.2.0799.0")> 
<Assembly: AssemblyFileVersion("1.2.0799.0")> 

'D0799  10-10-08    1.2
' * update for clsResourceReader: make it serializable;
' + allow to edit Original resource;
' + don't add missed resources by default: only when it's real updated;
' + mark missed destination lines and equal values;
' + allow to pass command line params as source and destination files;
' + move down on Ctrl+Enter press;
' + add app icon;
' * minor updates;

'D0798  10-10-07    1.1
' * convert to VS2008;
' * re-design for main form: use grid instead listview, remove some buttons;
' * update for clsResourceParameter: add Comment field;
' * update for clsResourceReader: allow to read comments as well;
' * update code for load strings and sync;
' * update code for edit resources;
' * ask user before exit about unsaved data;
' * lot of minor changes;
