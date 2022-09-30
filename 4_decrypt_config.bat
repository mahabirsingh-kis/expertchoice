SET app=%cd%\Application
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pdf connectionStrings %app%
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pdf system.web/sessionState %app%
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -pdf system.net/mailSettings/smtp %app%