﻿using EnvDTE;
using SamirBoulema.TGit.Helpers;
using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace SamirBoulema.TGit.Commands
{
    public class ContextMenuCommands
    {
        private readonly DTE _dte;
        private readonly OptionPageGrid _generalOptions;
        private readonly OleMenuCommandService _mcs;

        public ContextMenuCommands(OleMenuCommandService mcs, DTE dte, OptionPageGrid generalOptions)
        {
            _dte = dte;
            _mcs = mcs;
            _generalOptions = generalOptions;
        }

        public void AddCommands()
        {
            CommandHelper.AddCommand(_mcs, ShowLogContextCommand, PkgCmdIDList.ShowLogContext);
            CommandHelper.AddCommand(_mcs, DiskBrowserContextCommand, PkgCmdIDList.DiskBrowserContext);
            CommandHelper.AddCommand(_mcs, RepoBrowserContextCommand, PkgCmdIDList.RepoBrowserContext);

            CommandHelper.AddCommand(_mcs, BlameContextCommand, PkgCmdIDList.BlameContext);

            CommandHelper.AddCommand(_mcs, MergeContextCommand, PkgCmdIDList.MergeContext);

            CommandHelper.AddCommand(_mcs, PullContextCommand, PkgCmdIDList.PullContext);
            CommandHelper.AddCommand(_mcs, FetchContextCommand, PkgCmdIDList.FetchContext);
            CommandHelper.AddCommand(_mcs, CommitContextCommand, PkgCmdIDList.CommitContext);
            CommandHelper.AddCommand(_mcs, RevertContextCommand, PkgCmdIDList.RevertContext);
            CommandHelper.AddCommand(_mcs, DiffContextCommand, PkgCmdIDList.DiffContext);
            CommandHelper.AddCommand(_mcs, PrefDiffContextCommand, PkgCmdIDList.PrefDiffContext);
        }

        private void ShowLogContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:log /path:\"{currentFilePath}\" /closeonend:{_generalOptions.CloseOnEnd}");
        }
        private void DiskBrowserContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            ProcessHelper.Start(currentFilePath);
        }
        private void RepoBrowserContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            ProcessHelper.StartTortoiseGitProc($"/command:repobrowser /path:\"{currentFilePath}\"");
        }
        private void BlameContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            int currentLineIndex = ((TextDocument)_dte.ActiveDocument.Object(string.Empty)).Selection.CurrentLine;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:blame /path:\"{currentFilePath}\" /line:{currentLineIndex}");
        }
        private void MergeContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:merge /path:\"{currentFilePath}\"");
        }
        private void PullContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:pull /path:\"{currentFilePath}\"");
        }
        private void FetchContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:fetch /path:\"{currentFilePath}\"");
        }
        private void CommitContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:commit /path:\"{currentFilePath}\" /logmsg:\"{GitHelper.GetCommitMessage(_generalOptions.CommitMessage, _dte)}\" /closeonend:{_generalOptions.CloseOnEnd}");
        }
        private void RevertContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:revert /path:\"{currentFilePath}\"");
        }
        private void DiffContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();
            ProcessHelper.StartTortoiseGitProc($"/command:diff /path:\"{currentFilePath}\"");
        }

        private void PrefDiffContextCommand(object sender, EventArgs e)
        {
            var currentFilePath = _dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(currentFilePath)) return;
            _dte.ActiveDocument.Save();

            var revisions = ProcessHelper.GitResult(Path.GetDirectoryName(currentFilePath), $"log -2 --pretty=format:%h {FileHelper.GetExactFileName(currentFilePath)}");
            if (!revisions.Contains(","))
            {
                MessageBox.Show("Could not determine the last committed revision!", "TGit", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                ProcessHelper.StartTortoiseGitProc($"/command:diff /path:\"{FileHelper.GetExactPathName(currentFilePath)}\" /startrev:{revisions.Split(',')[0]} /endrev:{revisions.Split(',')[1]}");
            }
        }
    }
}
