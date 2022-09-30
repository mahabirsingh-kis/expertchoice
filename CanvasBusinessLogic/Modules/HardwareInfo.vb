Option Explicit On
Imports System.Management

Namespace ExpertChoice.Service

    Public Module HardwareInfo

        ' https://msdn.microsoft.com/en-us/library/windows/desktop/aa364993(v=vs.85).aspx
        Private Declare Function GetVolumeInformation Lib "kernel32" Alias "GetVolumeInformationA" _
                                                        (ByVal lpRootPathName As String, _
                                                        ByVal lpVolumeNameBuffer As String, _
                                                        ByVal nVolumeNameSize As Integer, _
                                                        ByRef lpVolumeSerialNumber As UInteger, _
                                                        ByRef lpMaximumComponentLength As Integer, _
                                                        ByRef lpFileSystemFlags As Integer, _
                                                        ByVal lpFileSystemNameBuffer As String, _
                                                        ByVal nFileSystemNameSize As Integer) As Integer

        'Public Function GetDriveSerialNumber(strDriveLetter As String) As UInteger
        '    Dim serialNum As UInteger
        '    Dim maxNameLen As Integer
        '    Dim flags As Integer
        '    If GetVolumeInformation(strDriveLetter, Nothing, 0, serialNum, maxNameLen, flags, Nothing, 0) <> 0 Then Return serialNum Else Return 0
        'End Function

        Public Function GetDriveSerialNumber(ByVal Drive As Char) As UInteger
            Const SLen As Integer = 200
            Dim drvserial As UInteger
            Dim mydrvlabel As String = Space$(SLen)
            Dim myfilesys As String = Space$(SLen)
            Dim i As Integer
            Dim j As Integer
            If GetVolumeInformation(Drive + ":\", mydrvlabel, SLen, drvserial, i, j, myfilesys, SLen) <> 0 Then Return drvserial Else Return 0
        End Function

        ''' <summary>
        ''' Get ProcessorID
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetProcessorId() As [String]
            Dim mc As New ManagementClass("win32_processor")
            Dim moc As ManagementObjectCollection = mc.GetInstances()
            Dim Id As [String] = [String].Empty
            For Each mo As ManagementObject In moc
                Id = mo.Properties("processorID").Value.ToString()
                Exit For
            Next
            Return Id
        End Function

        ''' <summary>
        ''' Retrieving HDD Volume Serial No.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetHDDVolumeSerial() As [String]
            Dim mangnmt As New ManagementClass("Win32_LogicalDisk")
            Dim mcol As ManagementObjectCollection = mangnmt.GetInstances()
            Dim result As String = ""
            For Each strt As ManagementObject In mcol
                result += Convert.ToString(strt("VolumeSerialNumber"))
            Next
            Return result
        End Function

        ''' <summary>
        ''' Retrieving System MAC Address.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetMACAddress() As String
            Dim mc As New ManagementClass("Win32_NetworkAdapterConfiguration")
            Dim moc As ManagementObjectCollection = mc.GetInstances()
            Dim MACAddress As String = [String].Empty
            For Each mo As ManagementObject In moc
                If MACAddress = [String].Empty Then
                    If CBool(mo("IPEnabled")) = True Then
                        MACAddress = mo("MacAddress").ToString()
                    End If
                End If
                mo.Dispose()
            Next
            MACAddress = MACAddress.Replace(":", "")
            Return MACAddress
        End Function

        ''' <summary>
        ''' Retrieving Motherboard Manufacturer.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBoardMaker() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BaseBoard")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("Manufacturer").ToString()
                Catch
                End Try
            Next
            Return "Board Maker: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving Motherboard Product Id.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBoardProductId() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BaseBoard")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("Product").ToString()
                Catch
                End Try
            Next
            Return "Product: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving BIOS Maker.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBIOSmaker() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BIOS")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("Manufacturer").ToString()
                Catch
                End Try
            Next
            Return "BIOS Maker: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving BIOS Serial No.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBIOSserNo() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BIOS")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("SerialNumber").ToString()
                Catch
                End Try
            Next
            Return "BIOS Serial Number: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving BIOS Caption.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetBIOScaption() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BIOS")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("Caption").ToString()
                Catch
                End Try
            Next
            Return "BIOS Caption: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving System Account Name.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetAccountName() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_UserAccount")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("Name").ToString()
                Catch
                End Try
            Next
            Return "User Account Name: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving Physical Ram Memory.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetPhysicalMemory() As String
            Dim oMs As New ManagementScope()
            Dim oQuery As New ObjectQuery("SELECT Capacity FROM Win32_PhysicalMemory")
            Dim oSearcher As New ManagementObjectSearcher(oMs, oQuery)
            Dim oCollection As ManagementObjectCollection = oSearcher.[Get]()

            Dim MemSize As Long = 0
            Dim mCap As Long = 0

            ' In case more than one Memory sticks are installed
            For Each obj As ManagementObject In oCollection
                mCap = Convert.ToInt64(obj("Capacity"))
                MemSize += mCap
            Next
            MemSize = CLng((MemSize / 1024) / 1024)
            Return MemSize.ToString() + "MB"
        End Function

        ''' <summary>
        ''' Retrieving No of Ram Slot on Motherboard.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetNoRamSlots() As String
            Dim MemSlots As Integer = 0
            Dim oMs As New ManagementScope()
            Dim oQuery2 As New ObjectQuery("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray")
            Dim oSearcher2 As New ManagementObjectSearcher(oMs, oQuery2)
            Dim oCollection2 As ManagementObjectCollection = oSearcher2.[Get]()
            For Each obj As ManagementObject In oCollection2
                MemSlots = Convert.ToInt32(obj("MemoryDevices"))
            Next
            Return MemSlots.ToString()
        End Function

        'Get CPU Temprature.
        ''' <summary>
        ''' method for retrieving the CPU Manufacturer
        ''' using the WMI class
        ''' </summary>
        ''' <returns>CPU Manufacturer</returns>
        Public Function GetCPUManufacturer() As String
            Dim cpuMan As String = [String].Empty
            'create an instance of the Managemnet class with the
            'Win32_Processor class
            Dim mgmt As New ManagementClass("Win32_Processor")
            'create a ManagementObjectCollection to loop through
            Dim objCol As ManagementObjectCollection = mgmt.GetInstances()
            'start our loop for all processors found
            For Each obj As ManagementObject In objCol
                If cpuMan = [String].Empty Then
                    ' only return manufacturer from first CPU
                    cpuMan = obj.Properties("Manufacturer").Value.ToString()
                End If
            Next
            Return cpuMan
        End Function

        ''' <summary>
        ''' method to retrieve the CPU's current
        ''' clock speed using the WMI class
        ''' </summary>
        ''' <returns>Clock speed</returns>
        Public Function GetCPUCurrentClockSpeed() As Integer
            Dim cpuClockSpeed As Integer = 0
            'create an instance of the Managemnet class with the
            'Win32_Processor class
            Dim mgmt As New ManagementClass("Win32_Processor")
            'create a ManagementObjectCollection to loop through
            Dim objCol As ManagementObjectCollection = mgmt.GetInstances()
            'start our loop for all processors found
            For Each obj As ManagementObject In objCol
                If cpuClockSpeed = 0 Then
                    ' only return cpuStatus from first CPU
                    cpuClockSpeed = Convert.ToInt32(obj.Properties("CurrentClockSpeed").Value.ToString())
                End If
            Next
            'return the status
            Return cpuClockSpeed
        End Function

        ''' <summary>
        ''' method to retrieve the network adapters
        ''' default IP gateway using WMI
        ''' </summary>
        ''' <returns>adapters default IP gateway</returns>
        Public Function GetDefaultIPGateway() As String
            'create out management class object using the
            'Win32_NetworkAdapterConfiguration class to get the attributes
            'of the network adapter
            Dim mgmt As New ManagementClass("Win32_NetworkAdapterConfiguration")
            'create our ManagementObjectCollection to get the attributes with
            Dim objCol As ManagementObjectCollection = mgmt.GetInstances()
            Dim gateway As String = [String].Empty
            'loop through all the objects we find
            For Each obj As ManagementObject In objCol
                If gateway = [String].Empty Then
                    ' only return MAC Address from first card
                    'grab the value from the first network adapter we find
                    'you can change the string to an array and get all
                    'network adapters found as well
                    'check to see if the adapter's IPEnabled
                    'equals true
                    If CBool(obj("IPEnabled")) = True Then
                        gateway = obj("DefaultIPGateway").ToString()
                    End If
                End If
                'dispose of our object
                obj.Dispose()
            Next
            'replace the ":" with an empty space, this could also
            'be removed if you wish
            gateway = gateway.Replace(":", "")
            'return the mac address
            Return gateway
        End Function

        ''' <summary>
        ''' Retrieve CPU Speed.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCpuSpeedInGHz() As System.Nullable(Of Double)
            Dim GHz As System.Nullable(Of Double) = Nothing
            Using mc As New ManagementClass("Win32_Processor")
                For Each mo As ManagementObject In mc.GetInstances()
                    GHz = 0.001 * DirectCast(mo.Properties("CurrentClockSpeed").Value, UInt32)
                    Exit For
                Next
            End Using
            Return GHz
        End Function

        ''' <summary>
        ''' Retrieving Current Language
        ''' </summary>
        ''' <returns></returns>
        Public Function GetCurrentLanguage() As String
            Dim searcher As New ManagementObjectSearcher("root\CIMV2", "SELECT * FROM Win32_BIOS")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return wmi.GetPropertyValue("CurrentLanguage").ToString()
                Catch
                End Try
            Next
            Return "BIOS Maker: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving Current Language.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetOSInformation() As String
            Dim searcher As New ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
            For Each wmi As ManagementObject In searcher.[Get]()
                Try
                    Return Convert.ToString((Convert.ToString(DirectCast(wmi("Caption"), String).Trim() + ", ") & DirectCast(wmi("Version"), String)) + ", ") & DirectCast(wmi("OSArchitecture"), String)
                Catch
                End Try
            Next
            Return "OS Maker: Unknown"
        End Function

        ''' <summary>
        ''' Retrieving Processor Information.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetProcessorInformation() As [String]
            Dim mc As New ManagementClass("win32_processor")
            Dim moc As ManagementObjectCollection = mc.GetInstances()
            Dim info As [String] = [String].Empty
            For Each mo As ManagementObject In moc
                Dim name As String = DirectCast(mo("Name"), String)
                name = name.Replace("(TM)", "™").Replace("(tm)", "™").Replace("(R)", "®").Replace("(r)", "®").Replace("(C)", "©").Replace("(c)", "©").Replace("    ", " ").Replace("  ", " ")

                'mo.Properties["Name"].Value.ToString();
                'break;
                info = Convert.ToString((Convert.ToString(name & Convert.ToString(", ")) & DirectCast(mo("Caption"), String)) + ", ") & DirectCast(mo("SocketDesignation"), String)
            Next
            Return info
        End Function

        ''' <summary>
        ''' Retrieving Computer Name.
        ''' </summary>
        ''' <returns></returns>
        Public Function GetComputerName() As [String]
            Dim mc As New ManagementClass("Win32_ComputerSystem")
            Dim moc As ManagementObjectCollection = mc.GetInstances()
            Dim info As [String] = [String].Empty
            For Each mo As ManagementObject In moc
                'mo.Properties["Name"].Value.ToString();
                'break;
                info = DirectCast(mo("Name"), String)
            Next
            Return info
        End Function

        ''' <summary>
        ''' Detect if this OS runs in a virtual machine
        ''' Microsoft themselves say you can see that by looking at the motherboard via wmi
        ''' </summary>
        ''' <returns>false</returns> if it runs on a fysical machine
        Public Function DetectVirtualMachine() As Boolean
            Using searcher = New System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem")
                Using items = searcher.[Get]()
                    For Each item As ManagementObject In items
                        Dim manufacturer As String = item("Manufacturer").ToString().ToLower()
                        If (manufacturer = "microsoft corporation" AndAlso item("Model").ToString().ToUpperInvariant().Contains("VIRTUAL")) OrElse manufacturer.Contains("vmware") OrElse item("Model").ToString() = "VirtualBox" Then
                            Return True
                        End If
                    Next
                End Using
            End Using
            Return False
        End Function

    End Module

End Namespace