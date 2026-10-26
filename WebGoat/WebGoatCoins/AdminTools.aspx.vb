Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Web
Imports System.Web.UI
Imports Microsoft.VisualBasic
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class AdminTools
        Inherits Page

        Protected du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)

        End Sub

        ''' <summary>
        ''' "Open Customer Document" -- direct tier. The submitted document name is appended
        ''' straight onto the legacy document viewer's command line and the process is launched
        ''' with no validation at all, so any whitespace-separated extra token in the submitted
        ''' name reaches the viewer as an additional argument it was never meant to receive (e.g.
        ''' "invoice.txt /export:C:\inetpub\wwwroot\WebGoat\dump.txt" would make the viewer write
        ''' its internal buffer to an attacker-chosen path instead of just opening invoice.txt).
        ''' </summary>
        Protected Sub btnOpenDocument_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim docName As String = txtDocName.Text
            Shell("C:\LegacyTools\DocViewer.exe " & docName, AppWinStyle.NormalFocus, True, 2000) ' SINK: PLANTED-VB-HR-536
            lblDocumentResult.Text = "Opened " & HttpUtility.HtmlEncode(docName) & " with the legacy viewer."
        End Sub

        ''' <summary>
        ''' Safe twin of btnOpenDocument_Click: the submitted name must be a bare file name (no
        ''' path separators, nothing that could be mistaken for another argument) before the
        ''' legacy viewer is ever launched, and the viewer is launched via ProcessStartInfo with
        ''' the name passed as its own ArgumentList entry instead of being concatenated into a
        ''' single Shell() command-line string.
        ''' </summary>
        Protected Sub btnOpenDocumentSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim rawName As String = txtDocNameSafe.Text
            Dim safeName As String = If(rawName, String.Empty)
            If safeName.Length = 0 OrElse safeName <> Path.GetFileName(safeName) Then
                lblDocumentResultSafe.Text = "<div class='failure'>Enter a plain document file name, no path.</div>"
                Return
            End If

            Dim startInfo As New ProcessStartInfo With {
                .FileName = "C:\LegacyTools\DocViewer.exe",
                .UseShellExecute = False,
                .CreateNoWindow = True
            }
            startInfo.ArgumentList.Add(safeName)

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SAFE_SINK: PLANTED-VB-HR-536-safe
                proc.WaitForExit(2000)
            End Using

            lblDocumentResultSafe.Text = "Opened " & HttpUtility.HtmlEncode(safeName) & " with the legacy viewer."
        End Sub

        ''' <summary>
        ''' Indirect tier: the "Run Scheduled Export Batch" button hands the submitted label to
        ''' this same-class helper, one hop away from the button handler, which actually launches
        ''' the export batch script -- routed through cmd.exe, so any shell metacharacter in the
        ''' label reaches a real command-line interpreter (e.g. a label of
        ''' "nightly & net user attacker Passw0rd! /add" runs the export, then separately creates
        ''' a new OS user account).
        ''' </summary>
        Private Sub RunExportBatch(ByVal label As String)
            Dim scriptPath As String = "C:\LegacyTools\export.bat"
            Shell("cmd.exe /c " & scriptPath & " " & label, AppWinStyle.Hide, True, 30000) ' SINK: PLANTED-VB-HR-537
        End Sub

        Protected Sub btnRunExport_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim label As String = txtExportLabel.Text
            RunExportBatch(label)
            lblExportResult.Text = "Export batch started for label " & HttpUtility.HtmlEncode(label) & "."
        End Sub

        ''' <summary>
        ''' Safe twin of RunExportBatch: the label must be a plain alphanumeric token before the
        ''' export script is ever run, and the script is launched directly (never through
        ''' cmd.exe) via ProcessStartInfo, with the label passed as its own ArgumentList entry
        ''' instead of being concatenated into a single Shell() command-line string.
        ''' </summary>
        Private Sub RunExportBatchSafe(ByVal label As String)
            Dim startInfo As New ProcessStartInfo With {
                .FileName = "C:\LegacyTools\export.bat",
                .UseShellExecute = False,
                .CreateNoWindow = True
            }
            startInfo.ArgumentList.Add(label)

            Using proc As New Process()
                proc.StartInfo = startInfo
                proc.Start() ' SAFE_SINK: PLANTED-VB-HR-537-safe
                proc.WaitForExit(30000)
            End Using
        End Sub

        Protected Sub btnRunExportSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim label As String = txtExportLabelSafe.Text
            If label Is Nothing OrElse Not Regex.IsMatch(label, "^[A-Za-z0-9_-]+$") Then
                lblExportResultSafe.Text = "<div class='failure'>Enter a plain alphanumeric label.</div>"
                Return
            End If
            RunExportBatchSafe(label)
            lblExportResultSafe.Text = "Export batch started for label " & HttpUtility.HtmlEncode(label) & "."
        End Sub

        ''' <summary>
        ''' Interprocedural tier: "Quick Actions" hands the submitted action name straight to
        ''' AdminQuickActionDispatcher.RunQuickAction (App_Code), which invokes it by name via
        ''' CallByName against a fresh CustomerAccountQuickActions collaborator -- any public
        ''' member of that collaborator is reachable this way, not only the three the UI's own
        ''' button is meant to offer.
        ''' </summary>
        Protected Sub btnQuickAction_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim actionName As String = txtQuickAction.Text
            AdminQuickActionDispatcher.RunQuickAction(New CustomerAccountQuickActions(), actionName)
            lblQuickActionResult.Text = "Quick action """ & HttpUtility.HtmlEncode(actionName) & """ submitted."
        End Sub

        ''' <summary>
        ''' Safe twin of btnQuickAction_Click: routes through
        ''' AdminQuickActionDispatcher.RunQuickActionSafe, which maps the submitted key to one of
        ''' three allow-listed, compiler-checked calls instead of handing the raw string to
        ''' CallByName.
        ''' </summary>
        Protected Sub btnQuickActionSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim actionKey As String = txtQuickActionSafe.Text
            AdminQuickActionDispatcher.RunQuickActionSafe(New CustomerAccountQuickActions(), actionKey)
            lblQuickActionResultSafe.Text = "Quick action """ & HttpUtility.HtmlEncode(actionKey) & """ submitted."
        End Sub

        ''' <summary>
        ''' Substituted type/polymorphism-dependent tier: "Batch Quick Actions" is the loop/batch
        ''' variant of the Quick Actions feature above -- it hands the whole comma-separated list
        ''' to AdminQuickActionDispatcher.RunQuickActions, which invokes each entry by name via
        ''' CallByName in turn. No genuine polymorphism exists near either dispatch feature's host
        ''' code in this project (every existing MustInherit/MustOverride hierarchy here is
        ''' already the carrier for a different CWE's plants -- see AdminQuickActionDispatcher.vb's
        ''' own class comment), so this batch-loop shape stands in, honestly disclosed, mirroring
        ''' this project's own FieldFormatPolicyRegistry.vb precedent for the identical situation.
        ''' </summary>
        Protected Sub btnBatchActions_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim actionsCsv As String = txtBatchActions.Text
            AdminQuickActionDispatcher.RunQuickActions(New CustomerAccountQuickActions(), actionsCsv)
            lblBatchActionsResult.Text = "Batch actions """ & HttpUtility.HtmlEncode(actionsCsv) & """ submitted."
        End Sub

        ''' <summary>
        ''' Safe twin of btnBatchActions_Click: routes through
        ''' AdminQuickActionDispatcher.RunQuickActionsSafe, which maps every entry in the list
        ''' through the same closed allow-list as the single-action safe twin before any action
        ''' runs.
        ''' </summary>
        Protected Sub btnBatchActionsSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim actionsCsv As String = txtBatchActionsSafe.Text
            AdminQuickActionDispatcher.RunQuickActionsSafe(New CustomerAccountQuickActions(), actionsCsv)
            lblBatchActionsResultSafe.Text = "Batch actions """ & HttpUtility.HtmlEncode(actionsCsv) & """ submitted."
        End Sub

        ''' <summary>
        ''' Diversity tier: "Run Named Diagnostic" dispatches directly at this call site (no
        ''' shared dispatcher class, unlike the two Quick Actions features above) against a
        ''' dedicated LegacyDiagnosticsPanel collaborator, gated behind the same administrator
        ''' check AddNewCustomer.aspx.vb's CreateCustomer already uses. The gate narrows WHO can
        ''' reach this feature, not WHAT diagnostic name they may supply once they can -- an
        ''' authenticated admin's own request is still attacker-influenced data from the
        ''' application's point of view, so the marker sits on the CallByName line, never on the
        ''' role-check line.
        ''' </summary>
        Protected Sub btnDiagnostic_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not du.IsAdminCustomerLogin(User.Identity.Name) Then
                lblDiagnosticResult.Text = "ACCESS DENIED."
                Return
            End If

            Dim diagnosticName As String = txtDiagnosticName.Text
            Dim panel As New LegacyDiagnosticsPanel()
            CallByName(panel, diagnosticName, CallType.Method) ' SINK: PLANTED-VB-HR-540
            lblDiagnosticResult.Text = "Diagnostic """ & HttpUtility.HtmlEncode(diagnosticName) & """ completed."
        End Sub

        ''' <summary>
        ''' Safe twin of btnDiagnostic_Click: the same administrator gate applies, and the
        ''' submitted key is translated through a closed allow-list mapping it to one of three
        ''' intended, compiler-checked diagnostic calls -- the raw key is never itself passed to
        ''' CallByName.
        ''' </summary>
        Protected Sub btnDiagnosticSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not du.IsAdminCustomerLogin(User.Identity.Name) Then
                lblDiagnosticResultSafe.Text = "ACCESS DENIED."
                Return
            End If

            Dim diagnosticKey As String = txtDiagnosticNameSafe.Text
            Dim panel As New LegacyDiagnosticsPanel()
            Dim allowedDiagnostics As New Dictionary(Of String, Action) From {
                {"diskspace", AddressOf panel.CheckDiskSpace},
                {"dbconnectivity", AddressOf panel.CheckDatabaseConnectivity},
                {"flushcache", AddressOf panel.FlushSessionCache}
            }

            Dim selectedDiagnostic As Action = Nothing
            If diagnosticKey IsNot Nothing AndAlso allowedDiagnostics.TryGetValue(LCase(diagnosticKey), selectedDiagnostic) Then
                selectedDiagnostic() ' SAFE_SINK: PLANTED-VB-HR-540-safe
                lblDiagnosticResultSafe.Text = "Diagnostic """ & HttpUtility.HtmlEncode(diagnosticKey) & """ completed."
            Else
                lblDiagnosticResultSafe.Text = "<div class='failure'>Unrecognized diagnostic.</div>"
            End If
        End Sub

    End Class
End Namespace
