Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Dispatches a single named Quick Action (interprocedural tier), or a comma-separated batch
    ''' of them (substituted type/polymorphism-dependent tier -- this project's existing
    ''' MustInherit/MustOverride hierarchies (AttachmentPathResolver, FulfillmentWebhookKeySource,
    ''' BackupFormatStrategy, PaymentHandoffResolver, PartnerFeedParser,
    ''' NotificationChannelHeaderFormatter, CookieSecurityPolicy, PreferenceValue) are all
    ''' domain-mismatched to action-name dispatch and each is already the carrier for a different
    ''' CWE's plants, mirroring FieldFormatPolicyRegistry.vb's own documented substitution for the
    ''' same situation) against WebGoatCoins/AdminTools.aspx's Quick Actions feature.
    ''' </summary>
    Public Class AdminQuickActionDispatcher

        ''' <summary>
        ''' Interprocedural tier: invokes a single Quick Action by name against the supplied
        ''' target. The action name is resolved dynamically, so any public member of the target --
        ''' not only the three CustomerAccountQuickActions.vb's own UI intends to offer -- becomes
        ''' callable.
        ''' </summary>
        Public Shared Sub RunQuickAction(ByVal target As Object, ByVal actionName As String)
            CallByName(target, actionName, CallType.Method) ' SINK: PLANTED-VB-HR-538
        End Sub

        ''' <summary>
        ''' Substituted type/polymorphism-dependent tier: the batch variant of RunQuickAction --
        ''' invokes one Quick Action per comma-separated entry in a caller-supplied list, each one
        ''' resolved dynamically exactly like the single-action case above.
        ''' </summary>
        Public Shared Sub RunQuickActions(ByVal target As Object, ByVal actionNamesCsv As String)
            If actionNamesCsv Is Nothing Then
                Return
            End If

            Dim actionNames As String() = actionNamesCsv.Split(","c)
            For Each rawName As String In actionNames
                Dim actionName As String = rawName.Trim()
                If actionName.Length = 0 Then
                    Continue For
                End If
                CallByName(target, actionName, CallType.Method) ' SINK: PLANTED-VB-HR-539
            Next
        End Sub

        ''' <summary>
        ''' Safe twin of RunQuickAction: the caller-supplied key is translated through a closed
        ''' allow-list mapping it to one of the three intended, compiler-checked actions -- the
        ''' raw key is never itself passed to CallByName.
        ''' </summary>
        Public Shared Sub RunQuickActionSafe(ByVal target As CustomerAccountQuickActions, ByVal actionKey As String)
            If target Is Nothing OrElse actionKey Is Nothing Then
                Return
            End If

            Dim allowedActions As New Dictionary(Of String, Action) From {
                {"unlock", AddressOf target.UnlockAccount},
                {"resend", AddressOf target.ResendWelcomeEmail},
                {"purgecart", AddressOf target.PurgeExpiredCart}
            }

            Dim selectedAction As Action = Nothing
            If allowedActions.TryGetValue(LCase(actionKey), selectedAction) Then
                selectedAction() ' SAFE_SINK: PLANTED-VB-HR-538-safe
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of RunQuickActions: every entry in the caller-supplied list is translated
        ''' through the same closed allow-list before any action runs; an unrecognized entry is
        ''' simply skipped.
        ''' </summary>
        Public Shared Sub RunQuickActionsSafe(ByVal target As CustomerAccountQuickActions, ByVal actionKeysCsv As String)
            If target Is Nothing OrElse actionKeysCsv Is Nothing Then
                Return
            End If

            Dim allowedActions As New Dictionary(Of String, Action) From {
                {"unlock", AddressOf target.UnlockAccount},
                {"resend", AddressOf target.ResendWelcomeEmail},
                {"purgecart", AddressOf target.PurgeExpiredCart}
            }

            Dim actionKeys As String() = actionKeysCsv.Split(","c)
            For Each rawKey As String In actionKeys
                Dim actionKey As String = rawKey.Trim()
                If actionKey.Length = 0 Then
                    Continue For
                End If

                Dim selectedAction As Action = Nothing
                If allowedActions.TryGetValue(LCase(actionKey), selectedAction) Then
                    selectedAction() ' SAFE_SINK: PLANTED-VB-HR-539-safe
                End If
            Next
        End Sub

    End Class
End Namespace
