Imports System
Imports System.Threading

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Second hop of the "Queue Simulated Job" processing-delay feature (Content/SleepDoS.aspx's
    ''' "Interprocedural" section) -- actually blocks the current request thread for the duration
    ''' it is given. Called only from DoSSimulationService.vb's QueueSimulatedDelay/
    ''' QueueSimulatedDelaySafe, which is itself called from SleepDoS.aspx.vb's button handler, so
    ''' a value reaching here has already crossed two method boundaries from the original request
    ''' field with no validation at either hop.
    ''' </summary>
    Public Class DelayExecutor
        ''' <summary>
        ''' Blocks the calling thread for exactly the requested number of milliseconds, with no
        ''' upper bound at all -- a caller that hands this an attacker-chosen value (minutes,
        ''' hours) ties up whatever thread-pool/worker thread is running the current request for
        ''' that entire duration. At scale (many concurrent requests each requesting a long delay)
        ''' this exhausts the server's available worker threads and denies service to other users
        ''' (CWE-400, "Sleep DOS").
        ''' </summary>
        Public Shared Sub ExecuteDelay(ByVal delayMs As Integer)
            Thread.Sleep(delayMs) ' SINK: PLANTED-VB-HR-638
        End Sub

        ''' <summary>
        ''' Safe twin of ExecuteDelay: the duration is clamped to a small, fixed ceiling before the
        ''' thread is ever blocked, so the attacker's only influence is "sleep for up to 2 seconds",
        ''' never an arbitrarily long duration.
        ''' </summary>
        Public Shared Sub ExecuteDelaySafe(ByVal delayMs As Integer)
            Thread.Sleep(Math.Min(Math.Max(delayMs, 0), 2000)) ' SAFE_SINK: PLANTED-VB-HR-638-safe
        End Sub
    End Class
End Namespace
