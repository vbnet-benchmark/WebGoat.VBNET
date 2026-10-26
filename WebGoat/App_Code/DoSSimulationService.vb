Imports System

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' First hop of the "Queue Simulated Job" processing-delay feature (Content/SleepDoS.aspx's
    ''' "Interprocedural" section). Modeled as a tiny job-queueing facade in front of
    ''' DelayExecutor.vb -- a shape real "simulate a slow background job" diagnostic tools commonly
    ''' take (accept a requested duration, hand it off to whatever actually executes the wait).
    ''' Neither method here validates or bounds the value it is given; the value is simply
    ''' forwarded one hop further, to DelayExecutor.vb, which is where the thread is actually
    ''' blocked.
    ''' </summary>
    Public Class DoSSimulationService
        ''' <summary>
        ''' Forwards the requested delay, unexamined, to DelayExecutor.ExecuteDelay.
        ''' </summary>
        Public Shared Sub QueueSimulatedDelay(ByVal requestedDelayMs As Integer)
            DelayExecutor.ExecuteDelay(requestedDelayMs)
        End Sub

        ''' <summary>
        ''' Safe twin of QueueSimulatedDelay: forwards to DelayExecutor.ExecuteDelaySafe instead,
        ''' which applies the actual bound.
        ''' </summary>
        Public Shared Sub QueueSimulatedDelaySafe(ByVal requestedDelayMs As Integer)
            DelayExecutor.ExecuteDelaySafe(requestedDelayMs)
        End Sub
    End Class
End Namespace
