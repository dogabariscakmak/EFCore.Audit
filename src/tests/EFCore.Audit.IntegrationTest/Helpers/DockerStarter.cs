using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EFCore.Audit.IntegrationTest.Helpers
{
    public class DockerStarter : IDisposable
    {
        public string DockerComposeExe { get; private set; }
        public string ComposeFile { get; private set; }
        public string WorkingDir { get; private set; }
        public int SleepInMs { get; private set; }

        public DockerStarter(string dockerComposeExe, string composeFile, string workingDir, int sleepInMs)
        {
            DockerComposeExe = dockerComposeExe;
            ComposeFile = composeFile;
            WorkingDir = workingDir;
            SleepInMs = sleepInMs;
        }

        public void Start()
        {
            var startInfo = generateInfo("up");
            _dockerProcess = Process.Start(startInfo);
            Thread.Sleep(SleepInMs);
        }

        private Process _dockerProcess;

        public void Dispose()
        {
            _dockerProcess.Close();

            var stopInfo = generateInfo("down");
            var stop = Process.Start(stopInfo);
            stop.WaitForExit();
        }

        private ProcessStartInfo generateInfo(string argument)
        {
            var procInfo = new ProcessStartInfo
            {
                FileName = DockerComposeExe,
                Arguments = $"-f {ComposeFile} {argument}",
                WorkingDirectory = WorkingDir
            };
            return procInfo;
        }
    }
}
