﻿using EnvDTE;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using Process = System.Diagnostics.Process;

namespace SamirBoulema.TGIT.Helpers
{
    public class ProcessHelper
    {
        private readonly FileHelper fileHelper;
        private string solutionDir;
        private readonly string tortoiseGitProc, git;
        private readonly DTE dte;
        private OutputBox outputBox;
        private readonly Stopwatch stopwatch;

        public ProcessHelper(DTE dte)
        {
            this.dte = dte;
            fileHelper = new FileHelper(dte);
            tortoiseGitProc = fileHelper.GetTortoiseGitProc();
            git = fileHelper.GetMSysGit();
            stopwatch = new Stopwatch();
        }

        public bool StartProcessGit(string commands, bool showAlert = true)
        {
            solutionDir = fileHelper.GetSolutionDir();
            if (string.IsNullOrEmpty(solutionDir)) return false;

            string output = string.Empty;
            string error = string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{solutionDir}\" && \"{git}\" {commands}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                output = proc.StandardOutput.ReadLine();
            }
            while (!proc.StandardError.EndOfStream)
            {
                error += proc.StandardError.ReadLine();
            }
            if (!string.IsNullOrEmpty(output))
            {
                return true;
            }
            if (!string.IsNullOrEmpty(error) && showAlert)
            {
                MessageBox.Show(error, "TGIT error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public void StartTortoiseGitProc(string args)
        {
            try
            {
                Process.Start(tortoiseGitProc, args);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, $"{tortoiseGitProc} not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public string StartProcessGitResult(string commands)
        {
            solutionDir = fileHelper.GetSolutionDir();
            if (string.IsNullOrEmpty(solutionDir)) return string.Empty;

            string output = string.Empty;
            string error = string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{solutionDir}\" && \"{git}\" {commands}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                output += proc.StandardOutput.ReadLine();
            }
            while (!proc.StandardError.EndOfStream)
            {
                error += proc.StandardError.ReadLine();
            }

            return string.IsNullOrEmpty(output) ? error : output;
        }

        public string GitResult(string workingDir, string commands)
        {
            string output = string.Empty;
            string error = string.Empty;
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c cd /D \"{workingDir}\" && \"{git}\" {commands}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                output += proc.StandardOutput.ReadLine() + ",";
            }
            while (!proc.StandardError.EndOfStream)
            {
                error += proc.StandardError.ReadLine();
            }

            return string.IsNullOrEmpty(output) ? error : output.TrimEnd(',');
        }

        public void Start(string application)
        {
            Process.Start(application);
        }

        public void StartProcessGui(string application, string args, string title)
        {
            var dialogResult = DialogResult.OK;
            if (!StartProcessGit("config user.name") || !StartProcessGit("config user.email"))
            {
                dialogResult = new Credentials(dte).ShowDialog();
            }

            if (dialogResult == DialogResult.OK)
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.EnableRaisingEvents = true;
                    process.Exited += process_Exited;
                    process.OutputDataReceived += OutputDataHandler;
                    process.ErrorDataReceived += OutputDataHandler;
                    process.StartInfo.FileName = application;
                    process.StartInfo.Arguments = args;
                    process.StartInfo.WorkingDirectory = solutionDir;

                    outputBox = new OutputBox(dte);

                    stopwatch.Reset();
                    stopwatch.Start();
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    outputBox.Text = title;
                    outputBox.Show();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, $"{application} not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (string.IsNullOrEmpty(outLine.Data)) return;
            var process = sendingProcess as Process;
            if (process == null) return;

            outputBox.BeginInvoke((Action)(() => outputBox.textBox.AppendText(outLine.Data + "\n")));
            outputBox.BeginInvoke((Action)(() => outputBox.textBox.Select(0, 0)));
        }

        private void process_Exited(object sender, EventArgs e)
        {
            var process = sender as Process;
            if (process == null) return;

            stopwatch.Stop();
            var exitCodeText = process.ExitCode == 0 ? "Succes" : "Error";

            outputBox.BeginInvoke((Action)(() => outputBox.textBox.AppendText($"{Environment.NewLine}{exitCodeText} ({stopwatch.ElapsedMilliseconds} ms @ {process.StartTime})")));
            outputBox.BeginInvoke((Action)(() => outputBox.okButton.Enabled = true));
        }
    }
}