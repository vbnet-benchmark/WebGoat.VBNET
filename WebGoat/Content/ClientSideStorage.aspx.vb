Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.IO
Imports System.Text
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Web.Script.Serialization

Namespace OWASP.WebGoat.NET
    Partial Public Class ClientSideStorage
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            ' Cookie "recentViews" carries a Base64-encoded snapshot of this visitor's
            ' recently-viewed product names, so the catalog strip can be redrawn across
            ' requests without a server-side session lookup -- a "remember what I looked at"
            ' client-side-storage idiom, the whole point of this lesson page.
            Dim recentViewsCookie As HttpCookie = Request.Cookies("recentViews")
            If recentViewsCookie IsNot Nothing AndAlso Not String.IsNullOrEmpty(recentViewsCookie.Value) Then
                ' SINK: PLANTED-VB-HR-121
                Dim recentViews As List(Of String) = CType((New BinaryFormatter()).Deserialize(New MemoryStream(Convert.FromBase64String(recentViewsCookie.Value))), List(Of String))
                Response.Write("<div class='recent-views'>Recently viewed: " & String.Join(", ", recentViews) & "</div>")
            End If

            Dim recentViewsCookieSafe As HttpCookie = Request.Cookies("recentViewsSafe")
            If recentViewsCookieSafe IsNot Nothing AndAlso Not String.IsNullOrEmpty(recentViewsCookieSafe.Value) Then
                Dim recentViewsSafeJson As String = Encoding.UTF8.GetString(Convert.FromBase64String(recentViewsCookieSafe.Value))
                ' SAFE_SINK: PLANTED-VB-HR-121-safe
                Dim recentViewsSafe As List(Of String) = (New JavaScriptSerializer()).Deserialize(Of List(Of String))(recentViewsSafeJson)
                Response.Write("<div class='recent-views-safe'>Recently viewed: " & String.Join(", ", recentViewsSafe) & "</div>")
            End If

            ' Cookie "savedFilters" carries a Base64-encoded snapshot of this visitor's
            ' last-used catalog search filters (category + keyword), restored through a
            ' private helper so the same restore logic can be reused from more than one
            ' place on this page.
            Dim savedFiltersCookie As HttpCookie = Request.Cookies("savedFilters")
            If savedFiltersCookie IsNot Nothing Then
                Dim filtersDict As Dictionary(Of String, String) = TryCast(DeserializeSavedFilters(savedFiltersCookie.Value), Dictionary(Of String, String))
                If filtersDict IsNot Nothing Then
                    Response.Write("<div class='saved-filters'>Restored filters: " & String.Join(", ", filtersDict.Select(Function(kv) kv.Key & "=" & kv.Value)) & "</div>")
                End If
            End If

            Dim savedFiltersCookieSafe As HttpCookie = Request.Cookies("savedFiltersSafe")
            If savedFiltersCookieSafe IsNot Nothing Then
                Dim filtersDictSafe As Dictionary(Of String, String) = DeserializeSavedFiltersSafe(savedFiltersCookieSafe.Value)
                If filtersDictSafe IsNot Nothing Then
                    Response.Write("<div class='saved-filters-safe'>Restored filters: " & String.Join(", ", filtersDictSafe.Select(Function(kv) kv.Key & "=" & kv.Value)) & "</div>")
                End If
            End If
        End Sub

        ''' <summary>
        ''' Restores the "savedFilters" cookie's payload. Shared by Page_Load so the restore
        ''' logic lives in one place rather than being duplicated inline.
        ''' </summary>
        Private Function DeserializeSavedFilters(ByVal raw As String) As Object
            If String.IsNullOrEmpty(raw) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(raw)
            Dim ms As New MemoryStream(bytes)
            ' SINK: PLANTED-VB-HR-122
            Return (New BinaryFormatter()).Deserialize(ms)
        End Function

        ''' <summary>
        ''' Same as DeserializeSavedFilters, but restores from the newer JSON-encoded format via
        ''' a plain JavaScriptSerializer (no type resolver), so only a Dictionary(Of String,
        ''' String) can ever be reconstructed.
        ''' </summary>
        Private Function DeserializeSavedFiltersSafe(ByVal raw As String) As Dictionary(Of String, String)
            If String.IsNullOrEmpty(raw) Then Return Nothing
            Dim bytes As Byte() = Convert.FromBase64String(raw)
            Dim json As String = Encoding.UTF8.GetString(bytes)
            ' SAFE_SINK: PLANTED-VB-HR-122-safe
            Return (New JavaScriptSerializer()).Deserialize(Of Dictionary(Of String, String))(json)
        End Function
    End Class
End Namespace