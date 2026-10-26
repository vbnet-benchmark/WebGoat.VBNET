Imports System
Imports System.Threading
Imports System.Web
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    ''' <summary>
    ''' Denial of Service (CWE-400, "Sleep DOS") lesson: a request-controlled numeric value is used,
    ''' with no bound, as the duration argument to Thread.Sleep -- a single request can tie up a
    ''' server worker thread for an attacker-chosen length of time (minutes, hours), and at scale
    ''' (many concurrent requests) this exhausts the server's thread pool and denies service to
    ''' other users. Follows this project's existing Content/RegexDoS.aspx and
    ''' Content/ReadlineDoS.aspx precedent: a resource-consumption lesson page reachable directly by
    ''' its own URL without needing a Web.sitemap entry.
    ''' </summary>
    Partial Public Class SleepDoS
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        End Sub

        ''' <summary>
        ''' "Simulate Network Latency" -- direct tier. Straight-line: the submitted delay is parsed
        ''' and handed to Thread.Sleep in the same statement block, no helper, no branch.
        ''' </summary>
        Protected Sub btnSimulateDirect_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtDirectDelayMs.Text)
            Thread.Sleep(requestedMs) ' SINK: PLANTED-VB-HR-636
            lblDirectResult.Text = "Simulated a " & requestedMs & "ms slow response."
        End Sub

        ''' <summary>
        ''' Safe twin of btnSimulateDirect_Click: the requested delay is clamped to a small, fixed
        ''' ceiling before Thread.Sleep is ever called.
        ''' </summary>
        Protected Sub btnSimulateDirectSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtDirectDelayMsSafe.Text)
            Dim appliedMs As Integer = Math.Min(Math.Max(requestedMs, 0), 2000)
            Thread.Sleep(appliedMs) ' SAFE_SINK: PLANTED-VB-HR-636-safe
            lblDirectResultSafe.Text = "Simulated a " & appliedMs & "ms slow response (capped)."
        End Sub

        ''' <summary>
        ''' Indirect tier: "Simulate Backend Processing Time" delegates the actual wait to this
        ''' private helper, one hop away from the button handler that calls it.
        ''' </summary>
        Private Sub ApplyProcessingDelay(ByVal requestedMs As Integer)
            Thread.Sleep(requestedMs) ' SINK: PLANTED-VB-HR-637
        End Sub

        Protected Sub btnSimulateIndirect_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtIndirectDelayMs.Text)
            ApplyProcessingDelay(requestedMs)
            lblIndirectResult.Text = "Simulated " & requestedMs & "ms of backend processing."
        End Sub

        ''' <summary>
        ''' Safe twin of ApplyProcessingDelay: clamps before sleeping.
        ''' </summary>
        Private Sub ApplyProcessingDelaySafe(ByVal requestedMs As Integer)
            Thread.Sleep(Math.Min(Math.Max(requestedMs, 0), 2000)) ' SAFE_SINK: PLANTED-VB-HR-637-safe
        End Sub

        Protected Sub btnSimulateIndirectSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtIndirectDelayMsSafe.Text)
            ApplyProcessingDelaySafe(requestedMs)
            lblIndirectResultSafe.Text = "Simulated backend processing (capped)."
        End Sub

        ''' <summary>
        ''' Interprocedural tier: "Queue Simulated Job" delegates to
        ''' App_Code/DoSSimulationService.vb, which itself delegates to
        ''' App_Code/DelayExecutor.vb -- a genuine two-hop, cross-file call chain with no
        ''' validation anywhere along it. See DelayExecutor.vb for the actual Thread.Sleep calls.
        ''' </summary>
        Protected Sub btnQueueJob_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtJobDelayMs.Text)
            DoSSimulationService.QueueSimulatedDelay(requestedMs)
            lblJobResult.Text = "Job queued and completed after a simulated " & requestedMs & "ms."
        End Sub

        Protected Sub btnQueueJobSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtJobDelayMsSafe.Text)
            DoSSimulationService.QueueSimulatedDelaySafe(requestedMs)
            lblJobResultSafe.Text = "Job queued and completed (capped)."
        End Sub

        ''' <summary>
        ''' Type/polymorphism-dependent tier: "Preview Throttle Policy" resolves which concrete
        ''' App_Code/ThrottleDelayPolicy.vb policy applies via ThrottleDelayPolicyFactory, based on
        ''' the submitted "throttleMode" value, then calls ApplyDelay on it with NO branching here
        ''' at the call site -- whether the thread ends up blocked for the raw requested duration or
        ''' a capped one depends entirely on which concrete type the factory resolved.
        ''' </summary>
        Protected Sub btnPreviewPolicy_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim requestedMs As Integer = Integer.Parse(txtPolicyDelayMs.Text)
            Dim policy As ThrottleDelayPolicy = ThrottleDelayPolicyFactory.ResolveFor(txtPolicyMode.Text)
            policy.ApplyDelay(requestedMs)
            lblPolicyResult.Text = "Throttle policy '" & HttpUtility.HtmlEncode(txtPolicyMode.Text) & "' applied."
        End Sub

        ''' <summary>
        ''' Diversity tier: "Simulate Batch Job Durations" -- a genuinely different shape from the
        ''' four sections above. A comma-separated list of per-step delays is split and each step's
        ''' delay is slept in turn inside a loop; neither the individual values nor the number of
        ''' steps is bounded, so the TOTAL accumulated sleep across one request is effectively
        ''' unbounded even if any single value were capped.
        ''' </summary>
        Protected Sub btnSimulateBatch_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim steps() As String = txtBatchDelays.Text.Split(","c)
            Dim totalMs As Integer = 0
            For Each rawStep As String In steps
                Dim stepMs As Integer
                If Integer.TryParse(rawStep.Trim(), stepMs) Then
                    Thread.Sleep(stepMs) ' SINK: PLANTED-VB-HR-640
                    totalMs += stepMs
                End If
            Next
            lblBatchResult.Text = "Simulated batch job across " & steps.Length & " step(s), " & totalMs & "ms total."
        End Sub

        ''' <summary>
        ''' Safe twin of btnSimulateBatch_Click: both the per-step delay AND the number of steps
        ''' actually processed are bounded, so the total accumulated sleep for one request is
        ''' bounded too (at most 5 steps * 500ms = 2.5s), regardless of how many comma-separated
        ''' values or how large each one the request supplies.
        ''' </summary>
        Protected Sub btnSimulateBatchSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            Dim steps() As String = txtBatchDelaysSafe.Text.Split(","c)
            Dim totalMs As Integer = 0
            Dim processed As Integer = 0
            For Each rawStep As String In steps
                If processed >= 5 Then
                    Exit For
                End If
                Dim stepMs As Integer
                If Integer.TryParse(rawStep.Trim(), stepMs) Then
                    Dim appliedMs As Integer = Math.Min(Math.Max(stepMs, 0), 500)
                    Thread.Sleep(appliedMs) ' SAFE_SINK: PLANTED-VB-HR-640-safe
                    totalMs += appliedMs
                    processed += 1
                End If
            Next
            lblBatchResultSafe.Text = "Simulated batch job across " & processed & " step(s) (capped), " & totalMs & "ms total."
        End Sub
    End Class
End Namespace
