﻿using System.ComponentModel;
using System.IO;
using NDesk.Options;
using Sep.Git.Tfs.Core;
using StructureMap;

namespace Sep.Git.Tfs.Commands
{
    [Pluggable("bootstrap")]
    [RequiresValidGitRepository]
    [Description("bootstrap [parent-commit]\n" +
        " info: if none of your tfs remote exists, always checkout and bootstrap your main remote first.\n")]
    public class Bootstrap : GitTfsCommand
    {
        private readonly RemoteOptions _remoteOptions;
        private readonly Globals _globals;
        private readonly TextWriter _stdout;
        private readonly Bootstrapper _bootstrapper;

        public Bootstrap(RemoteOptions remoteOptions, Globals globals, TextWriter stdout, Bootstrapper bootstrapper)
        {
            _remoteOptions = remoteOptions;
            _globals = globals;
            _stdout = stdout;
            _bootstrapper = bootstrapper;
        }

        public OptionSet OptionSet
        {
            get { return _remoteOptions.OptionSet; }
        }

        public int Run()
        {
            return Run("HEAD");
        }

        public int Run(string commitish)
        {
            var tfsParents = _globals.Repository.GetLastParentTfsCommits(commitish);
            foreach (var parent in tfsParents)
            {
                GitCommit commit = _globals.Repository.GetCommit(parent.GitCommit);
                _stdout.WriteLine("commit {0}\nAuthor: {1} <{2}>\nDate:   {3}\n\n    {4}",
                    commit.Sha,
                    commit.AuthorAndEmail.Item1, commit.AuthorAndEmail.Item2,
                    commit.When.ToString("ddd MMM d HH:mm:ss zzz"),
                    commit.Message.Replace("\n","\n    ").TrimEnd(' '));
                _bootstrapper.CreateRemote(parent);
                _stdout.WriteLine();
            }
            return GitTfsExitCodes.OK;
        }
    }
}
