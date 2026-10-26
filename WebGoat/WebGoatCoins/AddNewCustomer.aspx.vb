Imports System
Imports System.IO
Imports System.Web.UI
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class AddNewCustomer
        Inherits Page

        Protected du As IDbProvider = Settings.CurrentDbProvider

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs)
            ' Admin housekeeping: once a batch of prospective signups is rejected or expires,
            ' their staged ID/proof-of-address uploads are no longer needed and can be purged in
            ' one request rather than one at a time. The admin gate narrows WHO can reach this
            ' feature, not WHAT document names they may submit once they can -- the marker sits
            ' on the mutation inside the loop, never on the role-check line.
            If du.IsAdminCustomerLogin(User.Identity.Name) Then
                Dim purgeDocumentsCsv As String = Request.QueryString("purgeDocuments")
                If Not String.IsNullOrEmpty(purgeDocumentsCsv) Then
                    For Each documentName As String In purgeDocumentsCsv.Split(","c)
                        Try
                            Dim documentPath As String = Server.MapPath("~/WebGoatCoins/uploads/signup/" & documentName)
                            ' SINK: PLANTED-VB-HR-610
                            File.Delete(documentPath)
                        Catch ex As Exception
                            Console.WriteLine(ex.Message)
                        End Try
                    Next
                End If

                ' Hardened twin: each entry in the batch is independently reduced to its bare
                ' file name and re-checked against the fixed, server-controlled list of pending
                ' signup documents before that particular entry is deleted -- one bad entry in
                ' the list never lets the whole batch skip validation.
                Dim purgeDocumentsCsvSafe As String = Request.QueryString("purgeDocumentsSafe")
                If Not String.IsNullOrEmpty(purgeDocumentsCsvSafe) Then
                    For Each documentName As String In purgeDocumentsCsvSafe.Split(","c)
                        Dim safeDocumentName As String = Path.GetFileName(documentName)
                        If safeDocumentName.Length = 0 OrElse safeDocumentName = "." OrElse safeDocumentName = ".." OrElse
                            Not AttachmentRegistry.IsKnownPendingSignupDocument(safeDocumentName) Then
                            Continue For
                        End If

                        Try
                            Dim documentPathSafe As String = Server.MapPath("~/WebGoatCoins/uploads/signup/" & safeDocumentName)
                            ' SAFE_SINK: PLANTED-VB-HR-610-safe
                            File.Delete(documentPathSafe)
                        Catch ex As Exception
                            Console.WriteLine(ex.Message)
                        End Try
                    Next
                End If
            End If
        End Sub

        Protected Sub CreateCustomer(ByVal sender As Object, ByVal e As EventArgs)
            If Not du.IsAdminCustomerLogin(User.Identity.Name) Then
                InvalidUserNameOrPasswordMessage.Text = "ACCESS DENIED."
                InvalidUserNameOrPasswordMessage.Visible = True
                Return
            End If

            If String.IsNullOrEmpty(Username.Text) OrElse
                String.IsNullOrEmpty(Email.Text) OrElse
                String.IsNullOrEmpty(Password.Text) Then
                InvalidUserNameOrPasswordMessage.Text = "Fields Username, Email, Password should be filled."
                InvalidUserNameOrPasswordMessage.Visible = True
                Return
            End If

            Dim success As Boolean = du.CreateCustomer(
                Username.Text, Email.Text, Password.Text, IsAdmin.Checked, 1, "blue")
            If success Then
                InvalidUserNameOrPasswordMessage.Visible = False

                ' Attach the signed "confirm your new account" link that would be emailed to the
                ' new customer -- simulated here as response headers, since this goat app has no
                ' real mail server. The server is meant to re-check the signature against the
                ' identity/event/issued-at fields before honoring a later confirmation click.
                Dim issuedAt As DateTime = DateTime.UtcNow
                Dim confirmSig As String = AccountNotificationSignatureUtil.ComputeNotificationSignature(Email.Text, "AccountCreated", issuedAt)
                Response.AppendHeader("X-Account-Confirmation-Link", "/WebGoatCoins/CustomerLogin.aspx?confirmAccount=1&email=" & Server.UrlEncode(Email.Text) & "&ts=" & issuedAt.ToString("O") & "&sig=" & confirmSig)

                Dim confirmSigSafe As String = AccountNotificationSignatureUtil.ComputeNotificationSignatureSafe(Email.Text, "AccountCreated", issuedAt)
                Response.AppendHeader("X-Account-Confirmation-Link-Safe", "/WebGoatCoins/CustomerLogin.aspx?confirmAccountSafe=1&email=" & Server.UrlEncode(Email.Text) & "&ts=" & issuedAt.ToString("O") & "&sig=" & confirmSigSafe)
            Else
                InvalidUserNameOrPasswordMessage.Text = "Error user creating."
                InvalidUserNameOrPasswordMessage.Visible = True
            End If

            Dim s As String = Page.User.Identity.Name
        End Sub
    End Class
End Namespace