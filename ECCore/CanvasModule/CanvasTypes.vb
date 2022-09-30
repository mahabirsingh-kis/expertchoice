Namespace Canvas

    Public Module CanvasTypes

        ''' <summary>
        ''' Has to be a bit mask
        ''' </summary>
        Public Enum ProjectType
            ptRegular = 0
            ptRiskAssociated = 1
            ptOpportunities = 2
            ptMixed = 4 ' when we set opportunity for each event
            ptMyRiskReward = 8    ' D6798
        End Enum

        Public Enum ResultsView
            rvNone = -1
            rvBoth = 0
            rvIndividual = 1
            rvGroup = 2
        End Enum

        Public Enum GraphicalPairwiseMode
            gpwmLessThan9 = 0
            gpwmLessThan99 = 1
            gpwmInfinite = 2
        End Enum

        Public Enum IdealViewType
            ivtNormalized = 0
            ivtMaxIsOne = 1
        End Enum

        Public Enum WRTInfoDocsShowMode 'C1045
            idsmBoth = 0
            idsmOnlyParent = 1
            idsmOnlyChildren = 2
        End Enum

        'Public Enum DiagonalsEvaluation
        '    deAll = 0
        '    deFirst = 1
        '    deFirstAndSecond = 2
        'End Enum

        Public Enum DiagonalsEvaluationAdvanced
            deAll = 0
            deMedium = 1
            deMinimal = 2
        End Enum

        Public Enum PairwiseEvaluationOrder
            peoDiagonals = 0
            peoRows = 1
            peoColumns = 2
        End Enum

        Public Enum PairwiseType
            ptNumerical = 0
            ptVerbal = 1
            ptGraphical = 2
        End Enum

        Public Enum ModelEvaluationOrder
            meoObjectivesFirst = 0
            meoAlternativesFirst = 1
        End Enum

        Public Enum ObjectivesEvaluationDirection
            oedTopToBottom = 0
            oedBottomToTop = 1
        End Enum

        Public Enum AlternativesEvaluationMode
            aemOnePairAtATime = 0
            aemOnePairAtATimeWRTAlt = 3 'C0096
            aemAllAlternatives = 1
            aemAllCoveringObjectives = 2
        End Enum

        Public Enum ShowInfoDocsMode 'C0099
            sidmFrame = 0
            sidmPopup = 1
        End Enum

        Public Enum ResultsSortMode 'C0820
            rsmNumber = 0
            rsmName = 1
            rsmPriority = 2
            rsmCombined = 3     ' D0937
        End Enum

        Public Enum TTUsersSorting
            ttusEmail = 0
            ttusName = 1
            'ttusVariance = 2   ' -D1561
            ttusKeypadID = 2    ' D1561
        End Enum

        Public Function GetCurrentCanvasModuleVersion() As Version 'C0030
            Return System.Reflection.Assembly.GetExecutingAssembly.GetName.Version
        End Function

    End Module

End Namespace