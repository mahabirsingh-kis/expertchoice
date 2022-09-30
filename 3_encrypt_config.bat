SET app=%cd%\Application
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pef connectionStrings %app% -prov "EC_RSAProtectedConfigurationProvider"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pef system.web/sessionState %app% -prov "EC_RSAProtectedConfigurationProvider"
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pef system.net/mailSettings/smtp %app% -prov "EC_RSAProtectedConfigurationProvider"