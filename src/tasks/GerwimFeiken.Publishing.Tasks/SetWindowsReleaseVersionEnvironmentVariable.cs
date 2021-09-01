using System;
using System.Collections.Generic;
using System.Net;
using GerwimFeiken.Publishing.Tasks.Repositories;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Version = GerwimFeiken.Publishing.Tasks.Repositories.Version;

namespace GerwimFeiken.Publishing.Tasks
{
    public class SetWindowsReleaseVersionEnvironmentVariable : Task
    { 
        public SetWindowsReleaseVersionEnvironmentVariable() : this(null, null)
        {
        }

        public SetWindowsReleaseVersionEnvironmentVariable(IRegistry registry, IVersion version)
        {
            this.EnvironmentVariableKey = "GerwimFeiken_Publishing_WindowsReleaseId";
            this.LtscVersionsDict = new Dictionary<int, string>
            {
                {
                    14393, "ltsc2016"
                },
                {
                    17763, "ltsc2019"
                },
                {
                    20348, "ltsc2022"
                },
            };

            this.Registry = registry ?? new Registry();
            this.Version = version ?? new Version();
        }

        public string EnvironmentVariableKey { get; set; }

        [Output]
        public string Output { get; set; }

        private Dictionary<int, string> LtscVersionsDict { get; } // see https://docs.microsoft.com/en-us/windows-server/get-started/windows-server-release-info for the list

        private IRegistry Registry { get; set; }

        private IVersion Version { get; set; }


        public override bool Execute()
        {
            string windowsReleaseId = GetWindowsVersion();
            Log.LogMessage(MessageImportance.High, $"[GerwimFeiken.Publishing.Tasks]: Setting environment variable {EnvironmentVariableKey} to {windowsReleaseId}");
            Environment.SetEnvironmentVariable(EnvironmentVariableKey, windowsReleaseId, EnvironmentVariableTarget.User);

            Output = windowsReleaseId;
            return true;
        }

        private string GetWindowsVersion()
        {
            var version = this.Version.GetOsVersion();
            // Convert releaseId to LTSC version if available
            if (version.Major == 10 && LtscVersionsDict.TryGetValue(version.Build, out string convertedReleaseId))
                return convertedReleaseId;

            string displayVersion = Registry.Read(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "DisplayVersion");
            string releaseId = Registry.Read(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId"); // ReleaseId is deprecated as of version 2009: https://twitter.com/bytenerd/status/1395071115072966656

            return string.IsNullOrWhiteSpace(displayVersion) ? releaseId : displayVersion;
        }
    }
}
