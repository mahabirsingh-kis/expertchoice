Namespace Pages.external_classes
    Public Class Constants
        Public Const Sess_SignUp As String = "Sess_SignUp"
        Public Const Sess_SignUp_ProjName As String = "Sess_SignUp_ProjName"
        Public Const Sess_SignUp_Passcode As String = "Sess_SignUp_Passcode"
        Public Const Sess_SignUpMode As String = "Sess_SignUpMode"
        Public Const Sess_Requirements As String = "Sess_Requirements"
        Public Const Sess_ShowMessage As String = "Sess_ShowMessage"
        Public Const Sess_InviteMessage As String = "Sess_InviteMessage"
        Public Const Sess_Project_Status As String = "Sess_Project_Status"
        Public Const Sess_RoleGroup As String = "Sess_Role_Group"
        Public Const Sess_ShowEqualOnce As String = "Sess_ShowEqualOnce"
        Public Const Sess_ExpectedValue As String = "Sess_ExpectedValue"
        Public Const Sess_ForceError As String = "Sess_ForceError"
        Public Const Sess_PipeWarning As String = "Sess_PipeWarning"
        Public Const Sess_LoginMethod As String = "Sess_LoginMethod"
        Public Const Sess_RemoveAnonymCookie As String = "Sess_RemoveAnonymCookie"
        Public Const Sess_FromComparion As String = "Sess_FromComparion"
        Public Const Cook_Extreme As String = "Cook_Extreme"
        Public Const SessionModel As String = "SessionModel"
        Public Const SessionQhNode As String = "SessionQhNode"
        Public Const SessionViewOnlyUserId As String = "SessionViewOnlyUserId"
        Public Const SessionIsPipeViewOnly As String = "SessionIsPipeViewOnly"
        Public Const SessionParamStep As String = "SessionParamStep"
        Public Const SessionNonRMode As String = "SessionNonRMode"
        Public Const SessionNonRNode As String = "SessionNonRNode"
        Public Const SessionNonRMtType As String = "SessionNonRMtType"
        Public Const SessionIsInterResultStepFound As String = "SessionIsInterResultStepFound"
        Public Const SessionKoToken As String = "SessionKnowledgeOwlToken"
        Public Const SessionKoExpiresIn As String = "SessionKnowledgeOwlExpiresIn"
        Public Const KoTokenUrl As String = "https://app.knowledgeowl.com/oauth2/token"
        Public Const KoProjectId As String = "5c775aa76e121c7c09b9dd85-5c775b388e121c822d196668"
        Public Const KoClientId As String = "5c775b388e121c822d196668"
        Public Const KoClientSecret As String = "9dbaffdc6cd182cab7a696b5116fe8eadbfc7d669dda5fdf"
    End Class

    Public Enum EcSettingType
        MaxPasswordAttempts = 0
        LockPasswordTimeout = 1
    End Enum

    Friend Class KnowledgeOwlAuthToken
        Public Property access_token As String
        Public Property expires_in As Integer
        Public Property token_type As String
        Public Property scope As Object
    End Class
End Namespace