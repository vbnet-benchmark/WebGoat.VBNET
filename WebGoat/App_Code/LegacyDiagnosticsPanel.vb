Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' The small, fixed set of legacy diagnostic checks WebGoatCoins/AdminTools.aspx's "Run Named
    ''' Diagnostic" feature exposes to a logged-in administrator -- one method per check the
    ''' feature's own UI intends to offer. Dispatched by name directly at the page's own call site
    ''' (AdminTools.aspx.vb), which can invoke any public member here, not only the three the UI
    ''' shows.
    ''' </summary>
    Public Class LegacyDiagnosticsPanel

        ''' <summary>Reports free space on the application's data volume.</summary>
        Public Sub CheckDiskSpace()
            ' In a real deployment this would report free space on the application's data volume.
        End Sub

        ''' <summary>Attempts a lightweight round-trip against the configured database provider.</summary>
        Public Sub CheckDatabaseConnectivity()
            ' In a real deployment this would attempt a lightweight round-trip against the
            ' configured database provider.
        End Sub

        ''' <summary>Clears the in-memory session cache.</summary>
        Public Sub FlushSessionCache()
            ' In a real deployment this would clear the in-memory session cache.
        End Sub

    End Class
End Namespace
