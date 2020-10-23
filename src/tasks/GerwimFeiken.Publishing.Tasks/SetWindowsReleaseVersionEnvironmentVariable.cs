using System;
using System.Collections.Generic;
using System.Net;
using GerwimFeiken.Publishing.Tasks.Repositories;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace GerwimFeiken.Publishing.Tasks
{
    public class SetWindowsReleaseVersionEnvironmentVariable : Task
    { 
        public SetWindowsReleaseVersionEnvironmentVariable() : this(null)
        {
        }

        public SetWindowsReleaseVersionEnvironmentVariable(IRegistry registry)
        {
            this.EnvironmentVariableKey = "GerwimFeiken_Publishing_WindowsReleaseId";
            this.LtscVersionsDict = new Dictionary<string, string>
            {
                {
                    "1607", "ltsc2016"
                },
                {
                    "1809", "ltsc2019"
                },
            };

            this.Registry = registry ?? new Registry();
        }

        public string EnvironmentVariableKey { get; set; }

        [Output]
        public string Output { get; set; }

        private Dictionary<string, string> LtscVersionsDict { get; } // see https://docs.microsoft.com/en-us/windows-server/get-started/windows-server-release-info for the list

        private IRegistry Registry { get; set; }


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
            string releaseId = Registry.Read(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId");

            // Convert releaseId to LTSC version if available
            if (LtscVersionsDict.TryGetValue(releaseId, out string convertedReleaseId))
                return convertedReleaseId;

            return releaseId;
        }
    }
}
