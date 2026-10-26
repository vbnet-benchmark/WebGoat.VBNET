Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml

Namespace OWASP.WebGoat.NET
    Partial Public Class WSDL
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        End Sub

        ''' <summary>
        ''' "Confirm Import" -- the second half of the two-step partner feed import started by
        ''' Content/WSAttacks.aspx's "Preview Partner Feed" tool: reads the feed text staged in
        ''' Session by a PRIOR request to a DIFFERENT page/class and finishes the import,
        ''' re-enabling external-entity resolution the same way the preview step's own "legacy"
        ''' rationale calls for.
        ''' </summary>
        Protected Sub btnConfirmImport_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = TryCast(Session("pendingPartnerFeed"), String)
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As New XmlDocument()
                doc.XmlResolver = New XmlUrlResolver()
                ' SINK: PLANTED-VB-HR-240
                doc.LoadXml(feedXml)
                lblConfirmResult.Text = "<div class='success'>Confirmed import, root element: " & doc.DocumentElement.Name & "</div>"
                Session("pendingPartnerFeed") = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of btnConfirmImport_Click: reads the independently-staged
        ''' "pendingPartnerFeedSafe" Session value (from WSAttacks.aspx.vb's
        ''' btnPreviewFeedSafe_Click) and the resolver is never touched.
        ''' </summary>
        Protected Sub btnValidateOnly_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = TryCast(Session("pendingPartnerFeedSafe"), String)
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As New XmlDocument()
                ' SAFE_SINK: PLANTED-VB-HR-240-safe
                doc.LoadXml(feedXml)
                lblValidateResult.Text = "<div class='success'>Validated feed, root element: " & doc.DocumentElement.Name & "</div>"
                Session("pendingPartnerFeedSafe") = Nothing
            End If
        End Sub
    End Class
End Namespace
