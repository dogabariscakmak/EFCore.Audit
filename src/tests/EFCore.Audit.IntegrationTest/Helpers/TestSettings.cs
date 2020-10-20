using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Audit.IntegrationTest.Helpers
{
    public class TestSettings
    {
        public const string Position = "TestSettings";
        public bool IsDockerComposeRequired { get; set; }
        public string MssqlConnectionString { get; set; }
        public string DockerComposeFile { get; set; }
        public string DockerWorkingDir { get; set; }
        public string DockerComposeExePath { get; set; }
        public string TestDataPersonFilePath { get; set; }
        public string TestDataAddressFilePath { get; set; }
        public string TestDataPersonAttributeFilePath { get; set; }
        public bool IsGithubAction { get; set; }
    }
}
