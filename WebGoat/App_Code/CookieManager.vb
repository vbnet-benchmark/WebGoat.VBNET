Imports System
Imports System.Collections.Generic
Imports System.Web
Imports System.Web.Security

Namespace OWASP.WebGoat.NET.App_Code
    Public Class CookieManager
        Public Sub New()
        End Sub

        Public Shared Function SetCookie(ticket As FormsAuthenticationTicket, cookieId As String, cookieValue As String) As HttpCookie
            Dim encrypted_ticket As String = FormsAuthentication.Encrypt(ticket)

            Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encrypted_ticket)

            If ticket.IsPersistent Then
                cookie.Expires = ticket.Expiration
            End If

            Return cookie
        End Function

        ''' <summary>
        ''' Builds a persistent cookie carrying a customer-chosen preference value (e.g. a UI
        ''' theme), for CustomerSettings.aspx's "remember my preference across visits" option.
        ''' </summary>
        Public Shared Function CreatePreferenceCookie(cookieName As String, cookieValue As String, persistDays As Integer) As HttpCookie
            Dim cookie As New HttpCookie(cookieName, cookieValue)

            If persistDays > 0 Then
                cookie.Expires = DateTime.Now.AddDays(persistDays)
            End If

            Return cookie
        End Function

        ''' <summary>
        ''' Same as CreatePreferenceCookie, but marks the cookie HttpOnly so no client-side
        ''' script can read it back.
        ''' </summary>
        Public Shared Function CreatePreferenceCookieSafe(cookieName As String, cookieValue As String, persistDays As Integer) As HttpCookie
            Dim cookie As New HttpCookie(cookieName, cookieValue)

            If persistDays > 0 Then
                cookie.Expires = DateTime.Now.AddDays(persistDays)
            End If

            cookie.HttpOnly = True

            Return cookie
        End Function

        ''' <summary>
        ''' Builds one tracking cookie per recently-viewed product ID, for MainPage.aspx's
        ''' "recently viewed products" strip.
        ''' </summary>
        Public Shared Function CreateTrackingCookies(baseName As String, productIds As List(Of String)) As List(Of HttpCookie)
            Dim cookies As New List(Of HttpCookie)()

            For i As Integer = 0 To productIds.Count - 1
                Dim cookie As New HttpCookie(baseName & "_" & i.ToString(), productIds(i))
                cookie.Expires = DateTime.Now.AddDays(7)
                cookies.Add(cookie)
            Next

            Return cookies
        End Function

        ''' <summary>
        ''' Same as CreateTrackingCookies, but each cookie is marked HttpOnly.
        ''' </summary>
        Public Shared Function CreateTrackingCookiesSafe(baseName As String, productIds As List(Of String)) As List(Of HttpCookie)
            Dim cookies As New List(Of HttpCookie)()

            For i As Integer = 0 To productIds.Count - 1
                Dim cookie As New HttpCookie(baseName & "_" & i.ToString(), productIds(i))
                cookie.Expires = DateTime.Now.AddDays(7)
                cookie.HttpOnly = True
                cookies.Add(cookie)
            Next

            Return cookies
        End Function
    End Class
End Namespace