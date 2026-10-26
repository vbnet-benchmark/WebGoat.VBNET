Imports System
Imports System.Data
Imports System.Web.UI
Imports System.Security.Cryptography
Imports System.Text
Imports OWASP.WebGoat.NET.App_Code
Imports OWASP.WebGoat.NET.App_Code.DB

Namespace OWASP.WebGoat.NET.WebGoatCoins
    Public Partial Class Messages
        Inherits Page

        Private du As IDbProvider = Settings.CurrentDbProvider
        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If String.IsNullOrEmpty(TryCast(Session("messages"), String)) Then
                Dim ds As DataSet = du.GetMessages(User.Identity.Name)
                Dim messages As String = String.Empty
                For Each row As DataRow In ds.Tables(0).Rows
                    messages &= "<strong>" & Server.HtmlEncode(row("title").ToString()) & "</strong><br/>"
                    messages &= Server.HtmlEncode(row("text").ToString()) & "<br/><hr/>"
                Next

                Session("messages") = messages
            End If

            lblMessages.Text = CType(Session("messages"), String)

            ' Optional tag filter banner (WebGoatCoins/Messages.aspx?tag=urgent&tag=billing).
            Dim tagValues As String() = Request.QueryString.GetValues("tag")
            If tagValues IsNot Nothing AndAlso tagValues.Length > 0 Then
                Dim tagSummary As String = TagFormatter.BuildTagSummary(tagValues)
                ' SINK: PLANTED-VB-HR-10
                lblMessages.Text &= "<div class='tags'>" & tagSummary & "</div>"
            End If

            Dim tagValuesSafe As String() = Request.QueryString.GetValues("tagSafe")
            If tagValuesSafe IsNot Nothing AndAlso tagValuesSafe.Length > 0 Then
                Dim tagSummarySafe As String = TagFormatter.BuildTagSummarySafe(tagValuesSafe)
                ' SAFE_SINK: PLANTED-VB-HR-10-safe
                lblMessages.Text &= "<div class='tags-safe'>" & tagSummarySafe & "</div>"
            End If

            ' "Share this message" -- builds a short public link (no login required) that lets a
            ' customer forward a single message to someone else. The link's token is meant to be
            ' the only thing standing between a visitor and that one message, since the public
            ' link intentionally bypasses the normal GetMessages(User.Identity.Name) ownership
            ' check above.
            Dim shareMessageId As String = Request.QueryString("shareMessageId")
            If Not String.IsNullOrEmpty(shareMessageId) Then
                Dim shareToken As String = ComputeShareToken(shareMessageId, User.Identity.Name)
                lblMessages.Text &= "<div class='share-link'>Share link: /WebGoatCoins/Messages.aspx?viewShared=" & shareMessageId & "&tok=" & shareToken & "</div>"
            End If

            Dim shareMessageIdSafe As String = Request.QueryString("shareMessageIdSafe")
            If Not String.IsNullOrEmpty(shareMessageIdSafe) Then
                Dim shareTokenSafe As String = ComputeShareTokenSafe(shareMessageIdSafe, User.Identity.Name)
                lblMessages.Text &= "<div class='share-link-safe'>Share link: /WebGoatCoins/Messages.aspx?viewSharedSafe=" & shareMessageIdSafe & "&tok=" & shareTokenSafe & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' SECURITY-SENSITIVE: the share token is the only thing standing between a visitor and
        ''' this one customer's message -- a collision-prone, brute-forceable digest here lets an
        ''' attacker guess or forge a token for someone else's shared message.
        ''' </summary>
        Private Function ComputeShareToken(ByVal messageId As String, ByVal ownerName As String) As String
            Using md5 As MD5 = MD5.Create()
                Dim hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(messageId & "|" & ownerName)) ' SINK: PLANTED-VB-HR-412
                Return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Function

        ''' <summary>
        ''' Safe twin of ComputeShareToken -- salted with a fresh random value per call and
        ''' hashed with SHA-256.
        ''' </summary>
        Private Function ComputeShareTokenSafe(ByVal messageId As String, ByVal ownerName As String) As String
            Dim saltBytes(15) As Byte
            Using rng As New RNGCryptoServiceProvider()
                rng.GetBytes(saltBytes)
            End Using
            Dim saltHex As String = BitConverter.ToString(saltBytes).Replace("-", "").ToLowerInvariant()
            Using sha256 As SHA256 = SHA256.Create()
                Dim hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(messageId & "|" & ownerName & "|" & saltHex)) ' SAFE_SINK: PLANTED-VB-HR-412-safe
                Return saltHex & ":" & BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant()
            End Using
        End Function
    End Class
End Namespace