Imports System
Imports System.Collections.Generic

Namespace OWASP.WebGoat.NET.App_Code
    ''' <summary>
    ''' Fixed, server-controlled lists of the file names this project's genuine file-mutation
    ''' features (order attachments on WebGoatCoins/Orders.aspx, debug logs on
    ''' WebGoatCoins/Debug.aspx, uploaded customer documents on WebGoat/MainPage.aspx, staged
    ''' checkout receipts on WebGoatCoins/Buy.aspx, pending signup documents on
    ''' WebGoatCoins/AddNewCustomer.aspx) are actually allowed to touch. Every list below is a
    ''' literal, hardcoded set baked in at compile time -- nothing in this class is ever written
    ''' from request-derived input, session state, or any other attacker-reachable source, so a
    ''' lookup against one of these lists is a genuine authorization boundary rather than a mere
    ''' existence check: an attacker-chosen name that happens to exist on disk, but is not in the
    ''' relevant list, is still rejected.
    ''' </summary>
    Public Class AttachmentRegistry

        Private Shared ReadOnly KnownOrderAttachments As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "receipt-1001.txt",
            "receipt-1002.txt",
            "shipping-confirmation-1001.txt"
        }

        Private Shared ReadOnly KnownDebugLogs As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "session-trace.log",
            "replica-healthcheck.log"
        }

        Private Shared ReadOnly KnownUploadedDocuments As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "proof-of-address.pdf",
            "id-scan.pdf"
        }

        Private Shared ReadOnly KnownStagedReceipts As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "pending-receipt-draft.tmp"
        }

        Private Shared ReadOnly KnownPendingSignupDocuments As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "signup-id-front.jpg",
            "signup-id-back.jpg",
            "signup-proof-of-address.jpg"
        }

        Public Shared Function IsKnownOrderAttachment(ByVal fileName As String) As Boolean
            Return KnownOrderAttachments.Contains(fileName)
        End Function

        Public Shared Function IsKnownDebugLog(ByVal fileName As String) As Boolean
            Return KnownDebugLogs.Contains(fileName)
        End Function

        Public Shared Function IsKnownUploadedDocument(ByVal fileName As String) As Boolean
            Return KnownUploadedDocuments.Contains(fileName)
        End Function

        Public Shared Function IsKnownStagedReceipt(ByVal fileName As String) As Boolean
            Return KnownStagedReceipts.Contains(fileName)
        End Function

        Public Shared Function IsKnownPendingSignupDocument(ByVal fileName As String) As Boolean
            Return KnownPendingSignupDocuments.Contains(fileName)
        End Function
    End Class
End Namespace
