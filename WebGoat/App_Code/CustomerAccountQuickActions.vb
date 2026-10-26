Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' The small, fixed set of customer-account maintenance actions
    ''' WebGoatCoins/AdminTools.aspx's "Quick Actions" feature is meant to expose to an
    ''' administrator -- one method per action the feature's own UI intends to offer.
    ''' Dispatched by name via AdminQuickActionDispatcher.vb, which can invoke any public member
    ''' here regardless of which one the UI itself ever shows.
    ''' </summary>
    Public Class CustomerAccountQuickActions

        ''' <summary>Clears the account's lockout flag so the customer can sign in again.</summary>
        Public Sub UnlockAccount()
            ' In a real deployment this would clear a lockout flag for the account the
            ' administrator is currently viewing.
        End Sub

        ''' <summary>Re-queues the account's welcome email.</summary>
        Public Sub ResendWelcomeEmail()
            ' In a real deployment this would re-queue the account's welcome email.
        End Sub

        ''' <summary>Deletes the account's abandoned shopping cart.</summary>
        Public Sub PurgeExpiredCart()
            ' In a real deployment this would delete the account's abandoned shopping cart.
        End Sub

    End Class
End Namespace
