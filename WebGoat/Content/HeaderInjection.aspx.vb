Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Collections
Imports System.Collections.Specialized

Namespace OWASP.WebGoat.NET
    Partial Public Class HeaderInjection
        Inherits System.Web.UI.Page

        ''' <summary>
        ''' The small, fixed set of CDN/edge-cache hint headers the "batch" feature below
        ''' understands, in the order its comma-separated value list maps onto them positionally.
        ''' </summary>
        Private Shared ReadOnly BatchHeaderNames As String() = {"X-Goat-Cache-Region", "X-Goat-Cache-Tier", "X-Goat-Cache-Note"}

        Protected Sub Page_Load(sender As Object, e As EventArgs)
            If Not Request.QueryString("Cookie") Is Nothing Then
                Dim cookie As HttpCookie = New HttpCookie("UserAddedCookie")
                cookie.Value = Request.QueryString("Cookie")

                Response.Cookies.Add(cookie)
            ElseIf Not Request.QueryString("Header") Is Nothing Then
                Dim newHeader As NameValueCollection = New NameValueCollection()
                newHeader.Add("newHeader", Request.QueryString("Header"))
                Response.Headers.Add(newHeader)
            End If

            'Headers
            lblHeaders.Text = Request.Headers.ToString().Replace("&", "<br />")

            'Cookies
            Dim colCookies As ArrayList = New ArrayList()
            For i As Integer = 0 To Request.Cookies.Count - 1
                colCookies.Add(Request.Cookies(i))
            Next

            gvCookies.DataSource = colCookies
            gvCookies.DataBind()

            ' Let the page attach a whole batch of CDN/edge-cache hint headers at once, submitted
            ' as a single comma-separated list of values that map positionally onto the small,
            ' fixed set of header names above -- e.g. for a single redirect that wants to set
            ' several edge-cache hints together, rather than one query parameter per header.
            Dim headerBatch As String = Request.QueryString("headerBatch")
            If Not String.IsNullOrEmpty(headerBatch) Then
                Dim batchValues As String() = headerBatch.Split(","c)
                For i As Integer = 0 To Math.Min(batchValues.Length, BatchHeaderNames.Length) - 1
                    ' SINK: PLANTED-VB-HR-445
                    Response.AppendHeader(BatchHeaderNames(i), batchValues(i))
                Next
            End If

            Dim headerBatchSafe As String = Request.QueryString("headerBatchSafe")
            If Not String.IsNullOrEmpty(headerBatchSafe) Then
                Dim batchValuesSafe As String() = headerBatchSafe.Split(","c)
                For i As Integer = 0 To Math.Min(batchValuesSafe.Length, BatchHeaderNames.Length) - 1
                    Dim strippedBatchValue As String = batchValuesSafe(i).Replace(Chr(13), "").Replace(Chr(10), "")
                    ' SAFE_SINK: PLANTED-VB-HR-445-safe
                    Response.AppendHeader(BatchHeaderNames(i), strippedBatchValue)
                Next
            End If
        End Sub
    End Class
End Namespace