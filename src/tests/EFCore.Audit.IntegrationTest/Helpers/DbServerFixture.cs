using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EFCore.Audit.IntegrationTest.Helpers
{
    public class DbServerFixture : IDisposable
    {
        public TestSettings TestSettings { get; private set; }

        private readonly DockerStarter dockerStarter;
        private bool _disposed;

        public DbServerFixture()
        {
            var config = new ConfigurationBuilder().AddJsonFile("testsettings.json").Build();

            bool IsGithubAction = false;
            Boolean.TryParse(Environment.GetEnvironmentVariable("IS_GITHUB_ACTION"), out IsGithubAction);

            TestSettings = new TestSettings()
            {
                IsDockerComposeRequired = Convert.ToBoolean(config.GetSection(TestSettings.Position)["IsDockerComposeRequired"]),
                MssqlConnectionString = config.GetSection(TestSettings.Position)["MssqlConnectionString"],
                DockerComposeFile = config.GetSection(TestSettings.Position)["DockerComposeFile"],
                DockerWorkingDir = config.GetSection(TestSettings.Position)["DockerWorkingDir"],
                DockerComposeExePath = config.GetSection(TestSettings.Position)["DockerComposeExePath"],
                TestDataPersonFilePath = config.GetSection(TestSettings.Position)["TestDataPersonFilePath"],
                TestDataAddressFilePath = config.GetSection(TestSettings.Position)["TestDataAddressFilePath"],
                TestDataPersonAttributeFilePath = config.GetSection(TestSettings.Position)["TestDataPersonAttributeFilePath"],
                IsGithubAction = IsGithubAction
            };

            if (TestSettings.IsDockerComposeRequired && !TestSettings.IsGithubAction)
            {
                dockerStarter = new DockerStarter(TestSettings.DockerComposeExePath, TestSettings.DockerComposeFile, TestSettings.DockerWorkingDir, 20 * 1000);
                dockerStarter.Start();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (TestSettings.IsDockerComposeRequired && !TestSettings.IsGithubAction)
                    {
                        dockerStarter.Dispose();
                    }
                }

                _disposed = true;
            }
        }
    }

    [CollectionDefinition("DbServer")]
    public class DbServerCollection : ICollectionFixture<DbServerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
