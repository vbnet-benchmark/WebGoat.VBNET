Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Xml
Imports System.Xml.XPath
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class XPathInjection
        Inherits System.Web.UI.Page

        Private xml As String = "<?xml version=""1.0"" encoding=""ISO-8859-1""?><sales><salesperson><name>David Palmer</name><city>Portland</city><state>or</state><ssn>123-45-6789</ssn></salesperson><salesperson><name>Jimmy Jones</name><city>San Diego</city><state>ca</state><ssn>555-45-6789</ssn></salesperson><salesperson><name>Tom Anderson</name><city>New York</city><state>ny</state><ssn>444-45-6789</ssn></salesperson><salesperson><name>Billy Moses</name><city>Houston</city><state>tx</state><ssn>333-45-6789</ssn></salesperson></sales>"

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Request.QueryString("state") IsNot Nothing Then
                RunStateSearch(Request.QueryString("state"))
            End If
        End Sub

        Private Function LoadSalesDocument() As XmlDocument
            Dim xDoc As New XmlDocument()
            xDoc.LoadXml(xml)
            Return xDoc
        End Function

        Private Function DescribeMatches(list As XmlNodeList) As String
            If list Is Nothing OrElse list.Count = 0 Then
                Return "<div class='failure'>No matching salesperson found.</div>"
            End If
            Dim names As New List(Of String)()
            For Each node As XmlNode In list
                Dim nameNode As XmlNode = node.SelectSingleNode("name")
                If nameNode IsNot Nothing Then
                    names.Add(HttpUtility.HtmlEncode(nameNode.InnerText))
                End If
            Next
            Return "<div class='success'>Found: " & String.Join(", ", names) & "</div>"
        End Function

        ''' <summary>
        ''' "Find by state" -- the lesson's original lookup, now wired up end to end: reachable
        ''' both via the hardcoded city links' ?state= query string (Page_Load) and via the free-text
        ''' search box below (btnStateSearch_Click). Both paths funnel into this one inline sink.
        ''' </summary>
        Private Sub RunStateSearch(state As String)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            ' SINK: PLANTED-VB-HR-361
            Dim list As XmlNodeList = xDoc.SelectNodes("//salesperson[state='" + state + "']")
            lblStateResult.Text = DescribeMatches(list)
        End Sub

        Protected Sub btnStateSearch_Click(sender As Object, e As EventArgs)
            RunStateSearch(txtStateSearch.Text)
        End Sub

        ''' <summary>
        ''' Safe twin of RunStateSearch: the state value is escaped as an XPath string literal
        ''' before it is spliced into the predicate.
        ''' </summary>
        Protected Sub btnStateSearchSafe_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim xpath As String = "//salesperson[state=" & XPathSafetyHelpers.XPathStringLiteral(txtStateSearchSafe.Text) & "]"
            ' SAFE_SINK: PLANTED-VB-HR-361-safe
            Dim list As XmlNodeList = xDoc.SelectNodes(xpath)
            lblStateResultSafe.Text = DescribeMatches(list)
        End Sub

        ''' <summary>
        ''' The submitted city name is trimmed of incidental whitespace by this private helper
        ''' before it reaches the sink -- trimming does not remove or escape any XPath-significant
        ''' character, so a value such as "Portland' or '1'='1" still reaches the predicate intact.
        ''' </summary>
        Private Function BuildCityPredicateValue(city As String) As String
            If city Is Nothing Then
                Return String.Empty
            End If
            Return city.Trim()
        End Function

        ''' <summary>
        ''' "Find by city" -- indirect tier: the predicate value flows through BuildCityPredicateValue
        ''' before reaching the sink.
        ''' </summary>
        Protected Sub btnCitySearch_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim cityPredicateValue As String = BuildCityPredicateValue(txtCitySearch.Text)
            ' SINK: PLANTED-VB-HR-362
            Dim list As XmlNodeList = xDoc.SelectNodes("//salesperson[city='" & cityPredicateValue & "']")
            lblCityResult.Text = DescribeMatches(list)
        End Sub

        ''' <summary>
        ''' Safe twin of btnCitySearch_Click: the submitted city is checked against the shared
        ''' allow-list before it is ever folded into the predicate.
        ''' </summary>
        Protected Sub btnCitySearchSafe_Click(sender As Object, e As EventArgs)
            If Not XPathSafetyHelpers.IsValidIdentifier(txtCitySearchSafe.Text) Then
                lblCityResultSafe.Text = "<div class='failure'>City must contain only letters, numbers, spaces, '-' or '_'.</div>"
                Return
            End If
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim cityPredicateValue As String = BuildCityPredicateValue(txtCitySearchSafe.Text)
            ' SAFE_SINK: PLANTED-VB-HR-362-safe
            Dim list As XmlNodeList = xDoc.SelectNodes("//salesperson[city='" & cityPredicateValue & "']")
            lblCityResultSafe.Text = DescribeMatches(list)
        End Sub

        ''' <summary>
        ''' "Find by name and state" -- interprocedural tier: the dual-source 'and'-joined predicate
        ''' is built in the separate SalesDirectoryService class (App_Code\SalesDirectoryService.vb).
        ''' </summary>
        Protected Sub btnDualSearch_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim list As XmlNodeList = SalesDirectoryService.FindByNameAndState(xDoc, txtDualName.Text, txtDualState.Text)
            lblDualResult.Text = DescribeMatches(list)
        End Sub

        Protected Sub btnDualSearchSafe_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim list As XmlNodeList = SalesDirectoryService.FindByNameAndStateSafe(xDoc, txtDualNameSafe.Text, txtDualStateSafe.Text)
            lblDualResultSafe.Text = DescribeMatches(list)
        End Sub

        ''' <summary>
        ''' "Bulk search" -- substituted type/polymorphism tier: a comma-separated batch of states is
        ''' loop/OR-joined into one predicate in SalesDirectoryService.FindByStates.
        ''' </summary>
        Protected Sub btnBulkSearch_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim list As XmlNodeList = SalesDirectoryService.FindByStates(xDoc, txtBulkStates.Text)
            lblBulkResult.Text = DescribeMatches(list)
        End Sub

        Protected Sub btnBulkSearchSafe_Click(sender As Object, e As EventArgs)
            Dim xDoc As XmlDocument = LoadSalesDocument()
            Dim list As XmlNodeList = SalesDirectoryService.FindByStatesSafe(xDoc, txtBulkStatesSafe.Text)
            lblBulkResultSafe.Text = DescribeMatches(list)
        End Sub

        ''' <summary>
        ''' "Remember my last search" -- diversity tier, second-order: the search term is only
        ''' staged into Session here; it is not used in any XPath expression until
        ''' btnRunRememberedSearch_Click reads it back, on a later postback (or a later visit --
        ''' Session outlives a single postback).
        ''' </summary>
        Protected Sub btnSaveSearch_Click(sender As Object, e As EventArgs)
            Session("lastStateSearch") = txtRememberState.Text
            lblRememberStatus.Text = "Saved. Click ""Run My Last Search"" to search for it."
        End Sub

        ''' <summary>
        ''' Reads the Session-staged value back and splices it into the predicate -- the actual
        ''' sink for this tier.
        ''' </summary>
        Protected Sub btnRunRememberedSearch_Click(sender As Object, e As EventArgs)
            Dim savedState As String = TryCast(Session("lastStateSearch"), String)
            If Not String.IsNullOrEmpty(savedState) Then
                Dim xDoc As XmlDocument = LoadSalesDocument()
                ' SINK: PLANTED-VB-HR-365
                Dim list As XmlNodeList = xDoc.SelectNodes("//salesperson[state='" & savedState & "']")
                lblRememberedResult.Text = DescribeMatches(list)
            Else
                lblRememberedResult.Text = "<div class='failure'>No saved search yet.</div>"
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of btnSaveSearch_Click: stages the value into its own Session key, unescaped,
        ''' exactly like the vulnerable path -- the escaping happens only when it is read back out.
        ''' </summary>
        Protected Sub btnSaveSearchSafe_Click(sender As Object, e As EventArgs)
            Session("lastStateSearchSafe") = txtRememberStateSafe.Text
            lblRememberStatusSafe.Text = "Saved. Click ""Run My Last Search (Hardened)"" to search for it."
        End Sub

        ''' <summary>
        ''' Safe twin of btnRunRememberedSearch_Click: the value read back from Session is escaped
        ''' as an XPath string literal before it is spliced into the predicate.
        ''' </summary>
        Protected Sub btnRunRememberedSearchSafe_Click(sender As Object, e As EventArgs)
            Dim savedState As String = TryCast(Session("lastStateSearchSafe"), String)
            If Not String.IsNullOrEmpty(savedState) Then
                Dim xDoc As XmlDocument = LoadSalesDocument()
                Dim xpath As String = "//salesperson[state=" & XPathSafetyHelpers.XPathStringLiteral(savedState) & "]"
                ' SAFE_SINK: PLANTED-VB-HR-365-safe
                Dim list As XmlNodeList = xDoc.SelectNodes(xpath)
                lblRememberedResultSafe.Text = DescribeMatches(list)
            Else
                lblRememberedResultSafe.Text = "<div class='failure'>No saved search yet.</div>"
            End If
        End Sub
    End Class
End Namespace
