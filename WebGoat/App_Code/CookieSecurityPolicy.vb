Imports System.Web

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Decides how a non-authentication tracking cookie gets hardened before it is sent to the
    ''' client. Dispatch is chosen at runtime (CustomerSettings.aspx's cookiePolicy query
    ''' parameter) so legacy, script-reliant themes can opt out of the strict default.
    ''' </summary>
    Public MustInherit Class CookieSecurityPolicy
        Public MustOverride Function Apply(cookie As HttpCookie) As HttpCookie
    End Class

    ''' <summary>
    ''' The historical behavior: the cookie is returned exactly as constructed, so any theme's
    ''' client-side script can still read it back via document.cookie.
    ''' </summary>
    Public Class LegacyCookiePolicy
        Inherits CookieSecurityPolicy

        Public Overrides Function Apply(cookie As HttpCookie) As HttpCookie
            Return cookie
        End Function
    End Class

    ''' <summary>
    ''' The modern default: the cookie is marked HttpOnly so no client-side script, legacy or
    ''' otherwise, can ever read it back.
    ''' </summary>
    Public Class StrictCookiePolicy
        Inherits CookieSecurityPolicy

        Public Overrides Function Apply(cookie As HttpCookie) As HttpCookie
            cookie.HttpOnly = True
            Return cookie
        End Function
    End Class
End Namespace
