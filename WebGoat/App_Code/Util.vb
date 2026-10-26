Imports System
Imports System.Diagnostics
Imports log4net
Imports System.Reflection
Imports System.IO
Imports System.Threading
Imports System.Text.RegularExpressions

Namespace OWASP.WebGoat.NET.App_Code
    Public Class Util
        Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)
        
        ''' <summary>
        ''' Launches a local database client and feeds it a script over its standard input --
        ''' used by RecreateGoatDb() on both the MySQL and the SQLite providers to replay the
        ''' goat database's create/load scripts. Interprocedural tier: cmd and args are built by
        ''' the calling provider straight from the deployment's "Client Executable" and
        ''' host/port/database/user/password configuration fields, which RebuildDatabase.aspx
        ''' (dbtest.aspx.vb's UpdateConfigFile) accepts verbatim from its own web form -- no
        ''' validation anywhere in the chain from that form to the process launched here.
        ''' </summary>
        Public Shared Function RunProcessWithInput(ByVal cmd As String, ByVal args As String, ByVal input As String) As Integer
            Dim startInfo As New ProcessStartInfo With {
                .WorkingDirectory = Settings.RootDir,
                .FileName = cmd,
                .Arguments = args,
                .UseShellExecute = False,
                .RedirectStandardInput = True,
                .RedirectStandardError = True,
                .RedirectStandardOutput = True,
                }

            Using process As New Process()
                process.EnableRaisingEvents = True
                process.StartInfo = startInfo

                AddHandler process.OutputDataReceived, Sub(sender, e)
                                                         If e.Data IsNot Nothing Then
                                                             log.Info(e.Data)
                                                         End If
                                                     End Sub

                AddHandler process.ErrorDataReceived, Sub(sender, e)
                                                        If e.Data IsNot Nothing Then
                                                            log.Error(e.Data)
                                                        End If
                                                    End Sub

                Dim are As New AutoResetEvent(False)

                AddHandler process.Exited, Sub(sender, e)
                                               Thread.Sleep(1000)
                                               are.Set()
                                               log.Info("Process exited")
                                           End Sub

                process.Start() ' SINK: PLANTED-VB-HR-513

                Using reader As New StreamReader(New FileStream(input, FileMode.Open))
                    Dim line As String
                    Dim replaced As String
                    While (Function() line = reader.ReadLine(), line)() IsNot Nothing
                        If Environment.OSVersion.Platform = PlatformID.Win32NT Then
                            replaced = line.Replace("DB_Scripts/datafiles/", "DB_Scripts\\datafiles\\")
                        Else
                            replaced = line
                        End If

                        log.Debug("Line: " & replaced)

                        process.StandardInput.WriteLine(replaced)
                    End While
                End Using
    
                process.StandardInput.Close()
    

                process.BeginOutputReadLine()
                process.BeginErrorReadLine()
    
                'NOTE: Looks like we have a mono bug: https://bugzilla.xamarin.com/show_bug.cgi?id=6291
                'have a wait time for now.
                
                are.WaitOne(10 * 1000)

                If process.HasExited Then
                    Return process.ExitCode
                Else 'WTF? Should have exited dammit!
                    process.Kill()
                    Return 1
                End If
            End Using
        End Function

        ''' <summary>
        ''' Safe twin of RunProcessWithInput, used by RebuildDatabase.aspx's "Verify Client
        ''' Executable" diagnostic: the executable path and its arguments must both look like a
        ''' plain local command invocation (letters, digits, dots, dashes, underscores, spaces
        ''' and path separators only) before anything is launched, so none of the characters a
        ''' shell or the target client would treat specially can ever reach it.
        ''' </summary>
        Public Shared Function RunProcessWithValidatedInput(ByVal cmd As String, ByVal args As String) As String
            If cmd Is Nothing OrElse Not Regex.IsMatch(cmd, "^[A-Za-z0-9_:\\/.\- ]+$") Then
                Return "Rejected: client executable path contains unexpected characters."
            End If
            If args Is Nothing OrElse Not Regex.IsMatch(args, "^[A-Za-z0-9_.\- ]*$") Then
                Return "Rejected: arguments contain unexpected characters."
            End If

            Dim startInfo As New ProcessStartInfo With {
                .WorkingDirectory = Settings.RootDir,
                .FileName = cmd,
                .Arguments = args,
                .UseShellExecute = False,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .CreateNoWindow = True
                }

            Using process As New Process()
                process.StartInfo = startInfo
                process.Start() ' SAFE_SINK: PLANTED-VB-HR-513-safe
                Dim output As String = process.StandardOutput.ReadToEnd()
                process.WaitForExit(5000)
                If Not process.HasExited Then
                    process.Kill()
                End If
                Return output
            End Using
        End Function
    End Class
End Namespace