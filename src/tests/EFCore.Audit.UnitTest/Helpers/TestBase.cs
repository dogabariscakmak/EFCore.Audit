﻿using EFCore.Audit.TestCommon;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace EFCore.Audit.UnitTest.Helpers
{
    //https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/#unit-testing
    //https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/sharing-databases
    public class TestBase : IDisposable
    {
        private static readonly object _lock = new object();
        private bool _disposed;

        public DbConnection Connection { get; }

        public List<PersonEntity> PersonTestData { get; private set; }
        public List<AddressEntity> AddressTestData { get; private set; }
        public List<PersonAttributesEntity> PersonAttributeTestData { get; private set; }

        public TestBase()
        {
            Connection = new SqliteConnection("Filename=:memory:");

            Connection.Open();

            ReloadTestData();
            CreateDatabase();
        }

        public PersonDbContext CreateContext(DbTransaction transaction = null)
        {
            var context = new PersonDbContext(new DbContextOptionsBuilder<PersonDbContext>().UseSqlite(Connection).Options,
                                              new UserProvider());

            if (transaction != null)
            {
                context.Database.UseTransaction(transaction);
            }

            return context;
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
                    Connection.Close();
                    Connection.Dispose();
                }

                _disposed = true;
            }
        }

        private void CreateDatabase()
        {
            lock (_lock)
            {
                using (var context = CreateContext())
                {
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    context.SaveChanges();
                }
            }
        }

        private void ReloadTestData()
        {
            if (PersonTestData == default)
            {
                PersonTestData = new List<PersonEntity>();
            }
            if (AddressTestData == default)
            {
                AddressTestData = new List<AddressEntity>();
            }
            if (PersonAttributeTestData == default)
            {
                PersonAttributeTestData = new List<PersonAttributesEntity>();
            }

            PersonTestData.Clear();
            AddressTestData.Clear();
            PersonAttributeTestData.Clear();

            using (StreamReader file = File.OpenText("persontestdata.json"))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                PersonTestData.AddRange((List<PersonEntity>)serializer.Deserialize(file, typeof(List<PersonEntity>)));
            }
            using (StreamReader file = File.OpenText("addresstestdata.json"))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                AddressTestData.AddRange((List<AddressEntity>)serializer.Deserialize(file, typeof(List<AddressEntity>)));
            }
            using (StreamReader file = File.OpenText("personattributetestdata.json"))
            {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                PersonAttributeTestData.AddRange((List<PersonAttributesEntity>)serializer.Deserialize(file, typeof(List<PersonAttributesEntity>)));
            }
        }
    }
}
