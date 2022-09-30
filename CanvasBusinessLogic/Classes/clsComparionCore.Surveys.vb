Imports SpyronControls.Spyron.Core
Imports ECSecurity.ECSecurity

Namespace ExpertChoice.Data

    Partial Public Class clsComparionCore

        ' D0250 + D0379 ===
        Public Property SurveysManager() As clsSurveysManager   'L0021 + D0379
            Get
                If _SurveysManager Is Nothing Then
                    _SurveysManager = New clsSurveysManager()
                    _SurveysManager.ConnectionString = CanvasMasterConnectionDefinition.ConnectionString    ' D6423 SpyronMasterConnectionDefinition.ConnectionString
                    _SurveysManager.ProviderType = CanvasMasterConnectionDefinition.ProviderType            ' D6423 SpyronMasterConnectionDefinition.ProviderType  ' D0458
                    _SurveysManager.SurveyStorageType = SpyronControls.Spyron.Core.SurveyStorageType.sstDatabaseStream
                End If
                Return _SurveysManager
            End Get
            Set(ByVal value As clsSurveysManager)
                _SurveysManager = value
            End Set
        End Property
        ' D0250 + D0379  ==

        ' D0285 ===
        Public ReadOnly Property isSpyronAvailable() As Boolean
            Get
                If clsLicense.SPYRON_ALLOW_FOR_ALL Then Return True ' D0820
                If Not _SPYRON_AVAILABLE Then Return False ' D0494
                Dim fPassed As Boolean = False
                'If Options.ShowDraftPages Or Not isDraftPage(_PAGESLIST_SPYRON) Then ' D0315 + D0459 -D0460
                Dim SysWG As clsWorkgroup = SystemWorkgroup
                If Options.CheckLicense And Not SysWG Is Nothing Then    ' D0315
                    fPassed = SysWG.License.CheckParameterByID(ecLicenseParameter.SpyronEnabled, Nothing, True) ' D0913
                    If fPassed And Not ActiveWorkgroup Is Nothing And Not SysWG Is ActiveWorkgroup Then
                        fPassed = ActiveWorkgroup.License.CheckParameterByID(ecLicenseParameter.SpyronEnabled, Nothing, True)   ' D0913
                    End If
                End If
                'End If
                Return fPassed
            End Get
        End Property
        ' D0285 ==

        ' D0247 ===
        Public Property ActiveSurveysList() As ArrayList
            'L0021 
            Get
                If _SurveysList Is Nothing Then
                    If Not ActiveWorkgroup Is Nothing Then
                        _SurveysList = SurveysManager.LoadSurveyList(ActiveWorkgroup.ID) ' D0379 + D0533
                        If _SurveysList IsNot Nothing Then _SurveysList.Sort(New clsSurveyInfoComparer) ' D1039
                    End If
                End If
                Return _SurveysList
            End Get
            Set(ByVal value As ArrayList)
                _SurveysList = value
            End Set
        End Property
        ' D0247 ==

    End Class

End Namespace