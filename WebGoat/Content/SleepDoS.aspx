<%@ Page Title="" Language="VB" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="SleepDoS.aspx.vb" Inherits="OWASP.WebGoat.NET.SleepDoS" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h1 class="title-regular-4 clearfix">Sleep Denial of Service</h1>
    <p class="tiny">A few diagnostic tools below let you simulate a slow backend by supplying how
    long the server should wait before responding. None of them should let you request an
    unreasonably long wait -- if one does, every other visitor's request has to wait behind yours
    for a worker thread.</p>

    <hr />
    <h3>Simulate Network Latency</h3>
    <p class="tiny">Enter a delay in milliseconds to simulate a slow network response.</p>
    <p>
        Delay (ms): <asp:TextBox ID="txtDirectDelayMs" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateDirect" runat="server" Text="Simulate"
            CssClass="button medium blue" onclick="btnSimulateDirect_Click" />
        <asp:Label ID="lblDirectResult" runat="server"></asp:Label>
    </p>
    <p>
        Delay (ms): <asp:TextBox ID="txtDirectDelayMsSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateDirectSafe" runat="server" Text="Simulate (Hardened)"
            CssClass="button medium blue" onclick="btnSimulateDirectSafe_Click" />
        <asp:Label ID="lblDirectResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Simulate Backend Processing Time</h3>
    <p class="tiny">Enter how long the (simulated) backend should take to process your request.</p>
    <p>
        Delay (ms): <asp:TextBox ID="txtIndirectDelayMs" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateIndirect" runat="server" Text="Process"
            CssClass="button medium blue" onclick="btnSimulateIndirect_Click" />
        <asp:Label ID="lblIndirectResult" runat="server"></asp:Label>
    </p>
    <p>
        Delay (ms): <asp:TextBox ID="txtIndirectDelayMsSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateIndirectSafe" runat="server" Text="Process (Hardened)"
            CssClass="button medium blue" onclick="btnSimulateIndirectSafe_Click" />
        <asp:Label ID="lblIndirectResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Queue Simulated Job</h3>
    <p class="tiny">Queue a background job that takes the given number of milliseconds to
    complete. Handled by App_Code/DoSSimulationService.vb and App_Code/DelayExecutor.vb.</p>
    <p>
        Delay (ms): <asp:TextBox ID="txtJobDelayMs" runat="server"></asp:TextBox>
        <asp:Button ID="btnQueueJob" runat="server" Text="Queue Job"
            CssClass="button medium blue" onclick="btnQueueJob_Click" />
        <asp:Label ID="lblJobResult" runat="server"></asp:Label>
    </p>
    <p>
        Delay (ms): <asp:TextBox ID="txtJobDelayMsSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnQueueJobSafe" runat="server" Text="Queue Job (Hardened)"
            CssClass="button medium blue" onclick="btnQueueJobSafe_Click" />
        <asp:Label ID="lblJobResultSafe" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Preview Throttle Policy</h3>
    <p class="tiny">Preview how a given throttle policy ("raw" or "capped") would apply a
    requested delay. Resolved by App_Code/ThrottleDelayPolicy.vb.</p>
    <p>
        Throttle mode: <asp:TextBox ID="txtPolicyMode" runat="server"></asp:TextBox>
        Delay (ms): <asp:TextBox ID="txtPolicyDelayMs" runat="server"></asp:TextBox>
        <asp:Button ID="btnPreviewPolicy" runat="server" Text="Preview"
            CssClass="button medium blue" onclick="btnPreviewPolicy_Click" />
        <asp:Label ID="lblPolicyResult" runat="server"></asp:Label>
    </p>

    <hr />
    <h3>Simulate Batch Job Durations</h3>
    <p class="tiny">Enter a comma-separated list of per-step delays (ms) to simulate a multi-step
    batch job, e.g. "100,250,100".</p>
    <p>
        Step delays (ms): <asp:TextBox ID="txtBatchDelays" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateBatch" runat="server" Text="Run Batch"
            CssClass="button medium blue" onclick="btnSimulateBatch_Click" />
        <asp:Label ID="lblBatchResult" runat="server"></asp:Label>
    </p>
    <p>
        Step delays (ms): <asp:TextBox ID="txtBatchDelaysSafe" runat="server"></asp:TextBox>
        <asp:Button ID="btnSimulateBatchSafe" runat="server" Text="Run Batch (Hardened)"
            CssClass="button medium blue" onclick="btnSimulateBatchSafe_Click" />
        <asp:Label ID="lblBatchResultSafe" runat="server"></asp:Label>
    </p>
</asp:Content>
