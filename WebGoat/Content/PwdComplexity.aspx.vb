Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Text.RegularExpressions
Imports OWASP.WebGoat.NET.App_Code

Namespace OWASP.WebGoat.NET
    Partial Public Class PwdComplexity
        Inherits System.Web.UI.Page

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

        End Sub

        ''' <summary>
        ''' "Create Account" -- direct tier. The submitted password must consist only of letters and
        ''' digits before the (simulated) account is created; written as a nested quantifier instead
        ''' of the equivalent flat character class, so a long run of allowed characters followed by
        ''' one disallowed character forces the backtracking engine through exponentially many ways
        ''' to partition that run before it can conclude the match fails. Empirically confirmed
        ''' catastrophic against this runtime's regex engine: 88ms at 20 trailing 'a's, 2.66s at 25,
        ''' times out (10s cap) at 30.
        ''' </summary>
        Protected Sub btnCreateAccount_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not Regex.IsMatch(txtNewPassword.Text, "^([a-zA-Z0-9]+)+$") Then ' SINK: PLANTED-VB-HR-466
                lblCreateResult.Text = "<div class='failure'>Password may only contain letters and digits.</div>"
                Return
            End If
            lblCreateResult.Text = "<div class='success'>Account created for " & HttpUtility.HtmlEncode(txtNewUsername.Text) & ".</div>"
        End Sub

        ''' <summary>
        ''' Safe twin of btnCreateAccount_Click: the nested quantifier is replaced by the flat form
        ''' describing the exact same language -- "(X+)+" and "X+" always match identical strings --
        ''' but the flat form gives the engine exactly one way to consume any given input, so there is
        ''' nothing left to backtrack over.
        ''' </summary>
        Protected Sub btnCreateAccountSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not Regex.IsMatch(txtNewPasswordSafe.Text, "^[a-zA-Z0-9]+$") Then ' SAFE_SINK: PLANTED-VB-HR-466-safe
                lblCreateResultSafe.Text = "<div class='failure'>Password may only contain letters and digits.</div>"
                Return
            End If
            lblCreateResultSafe.Text = "<div class='success'>Account created for " & HttpUtility.HtmlEncode(txtNewUsernameSafe.Text) & ".</div>"
        End Sub

        ''' <summary>
        ''' Indirect tier: the format check for the "Security Answer Format Hint" section lives in
        ''' this private helper, one hop away from the button handler that calls it. The hint is
        ''' purely informational -- diagnostic-only, never gates saving the answer. Same nested-
        ''' quantifier shape as btnCreateAccount_Click (letters-only here); empirically confirmed
        ''' catastrophic the same way: 80ms/20, 2.6s/25, timeout/30.
        ''' </summary>
        Private Function LooksLikeWellFormedAnswer(ByVal answer As String) As Boolean
            Return Regex.IsMatch(answer, "^([a-zA-Z]+)+$") ' SINK: PLANTED-VB-HR-467
        End Function

        Protected Sub btnCheckAnswerFormat_Click(ByVal sender As Object, ByVal e As EventArgs)
            If LooksLikeWellFormedAnswer(txtSecurityAnswer.Text) Then
                lblAnswerFormatHint.Text = "<div class='tiny'>Looks like a plain word -- that's fine.</div>"
            Else
                lblAnswerFormatHint.Text = "<div class='tiny'>Heads up: your answer contains spaces, numbers or punctuation. That's still accepted.</div>"
            End If
        End Sub

        ''' <summary>
        ''' Safe twin of LooksLikeWellFormedAnswer: caps the input length before the pattern is ever
        ''' evaluated. The nested quantifier's worst case is exponential in the length of a matching
        ''' run of letters, so bounding that length to a small constant bounds the worst case to a
        ''' small constant too (2^15 trivial partitions, not 2^100+).
        ''' </summary>
        Private Function LooksLikeWellFormedAnswerSafe(ByVal answer As String) As Boolean
            If answer Is Nothing OrElse answer.Length > 15 Then
                Return False
            End If
            Return Regex.IsMatch(answer, "^([a-zA-Z]+)+$") ' SAFE_SINK: PLANTED-VB-HR-467-safe
        End Function

        Protected Sub btnCheckAnswerFormatSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If LooksLikeWellFormedAnswerSafe(txtSecurityAnswerSafe.Text) Then
                lblAnswerFormatHintSafe.Text = "<div class='tiny'>Looks like a plain word -- that's fine.</div>"
            Else
                lblAnswerFormatHintSafe.Text = "<div class='tiny'>Heads up: your answer contains spaces, numbers, punctuation, or is longer than 15 characters. That's still accepted.</div>"
            End If
        End Sub

        ''' <summary>
        ''' Interprocedural tier: "Update Password Policy" delegates the actual complexity check to
        ''' App_Code/PasswordPolicyValidator.vb, a genuinely separate call chain from the three
        ''' handlers above. See that class for the empirically-measured backtracking numbers and the
        ''' safe twin's timeout-bound technique.
        ''' </summary>
        Protected Sub btnUpdatePolicy_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not PasswordPolicyValidator.MeetsComplexityPolicy(txtPolicyPassword.Text) Then
                lblPolicyResult.Text = "<div class='failure'>New password does not meet the complexity policy (letters, digits, underscore only).</div>"
                Return
            End If
            lblPolicyResult.Text = "<div class='success'>Password policy satisfied -- password updated.</div>"
        End Sub

        Protected Sub btnUpdatePolicySafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not PasswordPolicyValidator.MeetsComplexityPolicySafe(txtPolicyPasswordSafe.Text) Then
                lblPolicyResultSafe.Text = "<div class='failure'>New password does not meet the complexity policy (letters, digits, underscore only).</div>"
                Return
            End If
            lblPolicyResultSafe.Text = "<div class='success'>Password policy satisfied -- password updated.</div>"
        End Sub

        ''' <summary>
        ''' Type/polymorphism-dependent tier (substituted -- see App_Code/FieldFormatPolicyRegistry.vb
        ''' for why no existing MustInherit hierarchy in this project fits). "Preview Field Format"
        ''' resolves and applies the "promoCode" field kind's configured pattern via the registry;
        ''' diagnostic-only, since it never saves or redeems anything.
        ''' </summary>
        Protected Sub btnPreviewPromo_Click(ByVal sender As Object, ByVal e As EventArgs)
            If FieldFormatPolicyRegistry.Validate("promoCode", txtPromoPreview.Text) Then
                lblPromoPreviewResult.Text = "<div class='tiny'>This value would be ACCEPTED by the promoCode format rule.</div>"
            Else
                lblPromoPreviewResult.Text = "<div class='tiny'>This value would be REJECTED by the promoCode format rule.</div>"
            End If
        End Sub

        Protected Sub btnPreviewPromoSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If FieldFormatPolicyRegistry.ValidateSafe("promoCode", txtPromoPreviewSafe.Text) Then
                lblPromoPreviewResultSafe.Text = "<div class='tiny'>This value would be ACCEPTED by the promoCode format rule.</div>"
            Else
                lblPromoPreviewResultSafe.Text = "<div class='tiny'>This value would be REJECTED by the promoCode format rule.</div>"
            End If
        End Sub

        ''' <summary>
        ''' Diversity tier: "Submit Account Recovery Code" uses a genuinely different catastrophic
        ''' shape from the four sections above -- overlapping alternation between two DIFFERENT-LENGTH
        ''' alternatives over the same digit alphabet ("one digit" or "two digits"), not a nested
        ''' quantifier. Empirically confirmed catastrophic against this runtime's regex engine (unlike
        ''' a same-length overlap such as "[A-Z]|[A-Z0-9]", which this engine optimizes away without
        ''' backtracking blowup): a run of digits with no following "-XXXX" suffix gives the engine a
        ''' Fibonacci-sized number of ways to partition the run into 1-and-2-digit groups before it can
        ''' conclude there is no overall match -- measured 4ms/20 digits, 506ms/32 digits, 3.4s/36
        ''' digits, timeout past that.
        ''' </summary>
        Protected Sub btnSubmitRecoveryCode_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not Regex.IsMatch(txtRecoveryCode.Text, "^(?:\d|\d\d)+-[A-Z]{4}$") Then ' SINK: PLANTED-VB-HR-470
                lblRecoveryResult.Text = "<div class='failure'>Recovery code format is invalid. Expected digits-ABCD (e.g. 48213-QXTR).</div>"
                Return
            End If
            lblRecoveryResult.Text = "<div class='success'>Recovery code accepted.</div>"
        End Sub

        ''' <summary>
        ''' Safe twin of btnSubmitRecoveryCode_Click: the "one digit or two digits, repeated" grouping
        ''' never actually constrained anything -- any non-empty run of digits can already be split
        ''' into 1-and-2-digit groups some way -- so replacing it with a plain "one or more digits"
        ''' describes the identical language with exactly one way to consume any given input.
        ''' Empirically confirmed linear: 0ms even at 200 digits.
        ''' </summary>
        Protected Sub btnSubmitRecoveryCodeSafe_Click(ByVal sender As Object, ByVal e As EventArgs)
            If Not Regex.IsMatch(txtRecoveryCodeSafe.Text, "^\d+-[A-Z]{4}$") Then ' SAFE_SINK: PLANTED-VB-HR-470-safe
                lblRecoveryResultSafe.Text = "<div class='failure'>Recovery code format is invalid. Expected digits-ABCD (e.g. 48213-QXTR).</div>"
                Return
            End If
            lblRecoveryResultSafe.Text = "<div class='success'>Recovery code accepted.</div>"
        End Sub
    End Class
End Namespace
