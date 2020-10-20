using EFCore.Audit.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace EFCore.Audit.IntegrationTest.Helpers
{
    public class PersonDbDesignContext : IDesignTimeDbContextFactory<PersonDbContext>
    {
        public PersonDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PersonDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost,1433 ;User=sa;Password=P@ssw0rd123", b => b.MigrationsAssembly("EFCore.Audit.IntegrationTest"));

            return new PersonDbContext(optionsBuilder.Options);
        }
    }
}
