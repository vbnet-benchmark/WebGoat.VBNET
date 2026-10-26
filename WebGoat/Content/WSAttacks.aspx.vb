Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml
Imports System.IO
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class WSAttacks
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        End Sub

        ''' <summary>
        ''' "Legacy Price Feed" -- parses a pasted partner price-feed document, re-enabling
        ''' external-entity resolution so older marketplace exports that still declare a DOCTYPE
        ''' (for a currency-symbol entity) keep importing instead of throwing.
        ''' </summary>
        Protected Sub btnProcessLegacy_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtLegacyFeed.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As New XmlDocument()
                doc.XmlResolver = New XmlUrlResolver()
                ' SINK: PLANTED-VB-HR-236
                doc.LoadXml(feedXml)
                lblLegacyResult.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of btnProcessLegacy_Click: the resolver is never touched, so an attacker's
        ''' DOCTYPE/external entity is rejected instead of resolved.
        ''' </summary>
        Protected Sub btnProcessLegacySafe_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtLegacyFeedSafe.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As New XmlDocument()
                ' SAFE_SINK: PLANTED-VB-HR-236-safe
                doc.LoadXml(feedXml)
                lblLegacyResultSafe.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' "Modern Price Feed" -- same marketplace feed, parsed via XmlReaderSettings so a feed
        ''' carrying a DOCTYPE is not rejected outright (DtdProcessing.Parse), without separately
        ''' hardening the resolver (which keeps its own live default).
        ''' </summary>
        Protected Sub btnProcessModern_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtModernFeed.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As XmlDocument = ParseModernFeed(feedXml)
                lblModernResult.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        Private Function ParseModernFeed(ByVal feedXml As String) As XmlDocument
            Dim settings As New XmlReaderSettings()
            settings.DtdProcessing = DtdProcessing.Parse
            settings.XmlResolver = New XmlUrlResolver()
            Dim doc As New XmlDocument()
            Using reader As XmlReader = XmlReader.Create(New StringReader(feedXml), settings)
                ' SINK: PLANTED-VB-HR-237
                doc.Load(reader)
            End Using
            Return doc
        End Function

        ''' <summary>
        ''' Safe twin of btnProcessModern_Click/ParseModernFeed: DOCTYPE support is dropped
        ''' entirely (the strict, safe default) rather than allowed without hardening the resolver.
        ''' </summary>
        Protected Sub btnProcessModernSafe_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtModernFeedSafe.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim doc As XmlDocument = ParseModernFeedSafe(feedXml)
                lblModernResultSafe.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        Private Function ParseModernFeedSafe(ByVal feedXml As String) As XmlDocument
            Dim settings As New XmlReaderSettings()
            settings.DtdProcessing = DtdProcessing.Prohibit
            Dim doc As New XmlDocument()
            Using reader As XmlReader = XmlReader.Create(New StringReader(feedXml), settings)
                ' SAFE_SINK: PLANTED-VB-HR-237-safe
                doc.Load(reader)
            End Using
            Return doc
        End Function

        ''' <summary>
        ''' "Bulk Partner Feed Import" -- imports every file in a multi-file upload via the
        ''' cross-file PartnerFeedImportService, carried over from a single-file importer so older
        ''' per-category export files keep working.
        ''' </summary>
        Protected Sub btnImportBatch_Click(sender As Object, e As EventArgs)
            If filePartnerFeeds.HasFiles Then
                Dim importedCount As Integer = PartnerFeedImportService.ImportBatch(filePartnerFeeds.PostedFiles)
                lblBatchResult.Text = "<div class='success'>Imported " & importedCount.ToString() & " partner feed file(s)</div>"
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of btnImportBatch_Click, routed through PartnerFeedImportService.ImportBatchSafe.
        ''' </summary>
        Protected Sub btnImportBatchSafe_Click(sender As Object, e As EventArgs)
            If filePartnerFeedsSafe.HasFiles Then
                Dim importedCount As Integer = PartnerFeedImportService.ImportBatchSafe(filePartnerFeedsSafe.PostedFiles)
                lblBatchResultSafe.Text = "<div class='success'>Imported " & importedCount.ToString() & " partner feed file(s)</div>"
            End If
        End Sub

        ''' <summary>
        ''' "Auto-Detect Format" -- a single shared call site (parser.Parse) whose exploitability
        ''' depends entirely on which concrete PartnerFeedParser the factory returns for the
        ''' pasted feed's own content.
        ''' </summary>
        Protected Sub btnAutoDetect_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtAutoFeed.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim parser As PartnerFeedParser = PartnerFeedParserFactory.GetParser(feedXml)
                Dim doc As XmlDocument = parser.Parse(feedXml)
                lblAutoResult.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' Same shared call site as btnAutoDetect_Click, exercised against a feed whose own
        ''' content resolves to the modern/safe parser instead.
        ''' </summary>
        Protected Sub btnAutoDetectSafe_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtAutoFeedSafe.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Dim parser As PartnerFeedParser = PartnerFeedParserFactory.GetParser(feedXml)
                Dim doc As XmlDocument = parser.Parse(feedXml)
                lblAutoResultSafe.Text = "<div class='success'>Imported price feed, root element: " & doc.DocumentElement.Name & "</div>"
            End If
        End Sub

        ''' <summary>
        ''' "Preview Partner Feed" -- stashes the raw, unparsed feed text in Session so a second,
        ''' later request (Content/WSDL.aspx's "Confirm Import" step, simulating a separate
        ''' WSDL-advertised confirmation endpoint) can finish the import. No parsing happens here.
        ''' </summary>
        Protected Sub btnPreviewFeed_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtPreviewFeed.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Session("pendingPartnerFeed") = feedXml
                lblPreviewResult.Text = "<div class='success'>Feed staged. Go to the WSDL Exploits page and click Confirm Import to finish.</div>"
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of btnPreviewFeed_Click, staging under a separate Session key so
        ''' WSDL.aspx.vb's btnValidateOnly_Click reads the untouched, independent value.
        ''' </summary>
        Protected Sub btnPreviewFeedSafe_Click(sender As Object, e As EventArgs)
            Dim feedXml As String = txtPreviewFeedSafe.Text
            If Not String.IsNullOrEmpty(feedXml) Then
                Session("pendingPartnerFeedSafe") = feedXml
                lblPreviewResultSafe.Text = "<div class='success'>Feed staged. Go to the WSDL Exploits page and click Validate Only to finish.</div>"
            End If
        End Sub
    End Class
End Namespace
