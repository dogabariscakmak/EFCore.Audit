using EFCore.Audit.IntegrationTest.Helpers;
using EFCore.Audit.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EFCore.Audit.IntegrationTest
{
    [Collection("DbServer")]
    public class AuditDbContextAsyncTest : TestBase
    {

        private readonly DbServerFixture fixture;

        public AuditDbContextAsyncTest(DbServerFixture fixture) : base(fixture.TestSettings.MssqlConnectionString)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task Add_and_update_entities_preGenerated_and_onAddGeneratedProperties_Generates_two_audit_and_one_audit_metaDataEntity()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                PersonEntity personEntity;
                AddressEntity addressEntity1;
                AddressEntity addressEntity2;
                PersonAttributesEntity personAttributesEntity1;
                PersonAttributesEntity personAttributesEntity2;
                //Arrange and Act
                using (var context = CreateContext(transaction))
                {
                    personEntity = PersonTestData.FirstOrDefault();
                    context.Persons.Add(personEntity);
                    context.SaveChanges();
                    personEntity = context.Persons.FirstOrDefault();
                    personEntity.FirstName = $"{personEntity.FirstName}Modified";
                    context.SaveChanges();

                    addressEntity1 = AddressTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(0);
                    await context.Addresses.AddAsync(addressEntity1);
                    addressEntity2 = AddressTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(1);
                    await context.Addresses.AddAsync(addressEntity2);
                    await context.SaveChangesAsync();
                    addressEntity1 = await context.Addresses.FirstOrDefaultAsync(x => x.PersonId == personEntity.Id && x.Type == addressEntity1.Type);
                    addressEntity1.PostalCode = 12345;
                    addressEntity2 = await context.Addresses.FirstOrDefaultAsync(x => x.PersonId == personEntity.Id && x.Type == addressEntity2.Type);
                    addressEntity2.PostalCode = 54321;
                    await context.SaveChangesAsync();

                    personAttributesEntity1 = PersonAttributeTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(0);
                    await context.PersonAttributes.AddAsync(personAttributesEntity1);
                    personAttributesEntity2 = PersonAttributeTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(1);
                    await context.PersonAttributes.AddAsync(personAttributesEntity2);
                    await context.SaveChangesAsync();
                    personAttributesEntity1 = context.PersonAttributes.FirstOrDefault(x => x.Id == personAttributesEntity1.Id);
                    personAttributesEntity2 = context.PersonAttributes.FirstOrDefault(x => x.Id == personAttributesEntity2.Id);
                    personAttributesEntity1.DummyString = "changingDummyString";
                    personAttributesEntity1.Attribute = AttributeType.Profession;
                    personAttributesEntity1.AttributeValue = "Graphic Designer";
                    context.Remove(personAttributesEntity2);
                    await context.SaveChangesAsync();
                }
                using (var context = CreateContext(transaction))
                {
                    //Assert
                    List<AuditMetaDataEntity> auditMetaDatas = context.AuditMetaDatas.Include(amd => amd.AuditChanges).ToList();
                    List<AuditEntity> audits = context.Audits.Include(a => a.AuditMetaData).ToList();

                    Assert.Equal(5, auditMetaDatas.Count);
                    Assert.Equal(10, audits.Count);

                    /************************************************************************************************************************/

                    Assert.Single(auditMetaDatas,
                                  (x => x.DisplayName == "PersonEntity" && x.Schema == default && x.Table == "Persons" && x.SchemaTable == "Persons"
                                     && x.ReadablePrimaryKey == "Id=caf3feb5-730e-40a3-9610-404a17b0deba"
                                     && x.HashPrimaryKey == "Id=caf3feb5-730e-40a3-9610-404a17b0deba".ToGuidHash()
                                     && x.AuditChanges.Count == 2));

                    Assert.Equal(2, auditMetaDatas.Count(x => x.DisplayName == "PersonAttributesEntity" && x.Schema == default && x.Table == "PersonAttributes" && x.SchemaTable == "PersonAttributes"
                                                           && x.AuditChanges.Count == 2));

                    Assert.Single(auditMetaDatas,
                                  (x => x.DisplayName == "AddressEntity" && x.Schema == default && x.Table == "Addresses" && x.SchemaTable == "Addresses"
                                     && x.ReadablePrimaryKey == "PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Home"
                                     && x.HashPrimaryKey == "PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Home".ToGuidHash()
                                     && x.AuditChanges.Count == 2));

                    Assert.Single(auditMetaDatas,
                                  (x => x.DisplayName == "AddressEntity" && x.Schema == default && x.Table == "Addresses" && x.SchemaTable == "Addresses"
                                     && x.ReadablePrimaryKey == "PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Work"
                                     && x.HashPrimaryKey == "PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Work".ToGuidHash()
                                     && x.AuditChanges.Count == 2));

                    /************************************************************************************************************************/

                    Assert.Single(audits, 
                                  (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Added
                                     && x.NewValues == "{\"Id\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"FirstName\":\"Ofella\",\"Gender\":1,\"LastName\":\"Andrichuk\"}"
                                     && x.OldValues == default && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                  (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Modified
                                     && x.NewValues == "{\"FirstName\":\"OfellaModified\"}"
                                     && x.OldValues == "{\"FirstName\":\"Ofella\"}" && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                  (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Added
                                     && x.NewValues == "{\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Type\":1,\"City\":\"Azteca\",\"HouseNumber\":\"844\",\"PostalCode\":50188,\"Street\":\"Vahlen\"}"
                                     && x.OldValues == default && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                  (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Added
                                     && x.NewValues == "{\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Type\":2,\"City\":\"Tawun\",\"HouseNumber\":\"7\",\"PostalCode\":32921,\"Street\":\"Welch\"}"
                                     && x.OldValues == default && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                  (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Modified
                                     && x.NewValues == "{\"PostalCode\":54321}"
                                     && x.OldValues == "{\"PostalCode\":50188}" && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                 (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Modified
                                    && x.NewValues == "{\"PostalCode\":12345}"
                                    && x.OldValues == "{\"PostalCode\":32921}" && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Equal(2, audits.Count(x => x.AuditMetaData.DisplayName == "PersonAttributesEntity" && x.Id != default && x.EntityState == EntityState.Added
                                                   && x.OldValues == default && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                 (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Modified
                                    && x.NewValues == "{\"Attribute\":2,\"AttributeValue\":\"Graphic Designer\"}"
                                    && x.OldValues == "{\"Attribute\":1,\"AttributeValue\":\"Married\"}" && x.ByUser != default && x.DateTimeOffset != default));

                    Assert.Single(audits,
                                 (x => x.AuditMetaData != default && x.Id != default && x.EntityState == EntityState.Deleted
                                    && x.NewValues == default && x.ByUser != default && x.DateTimeOffset != default));
                }
            }
        }

        [Fact]
        public async Task Add_and_update_one_entity_with_preGeneratedProperties_Generates_two_audit_and_one_auditMetaDataEntity()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                EntityEntry entityEntry;
                PersonEntity personEntity;
                string notModifiedFirstName;
                //Arrange and Act
                using (var context = CreateContext(transaction))
                {
                    personEntity = PersonTestData.FirstOrDefault();
                    notModifiedFirstName = personEntity.FirstName;
                    entityEntry = await context.Persons.AddAsync(personEntity);
                    await context.SaveChangesAsync();

                    personEntity = await context.Persons.FirstOrDefaultAsync();
                    personEntity.FirstName = $"{personEntity.FirstName}Modified";
                    await context.SaveChangesAsync();
                }

                using (var context = CreateContext(transaction))
                {
                    //Assert
                    AuditMetaDataEntity auditMetaData = await context.AuditMetaDatas.Include(amd => amd.AuditChanges).FirstOrDefaultAsync();
                    AuditEntity auditAdded = await context.Audits.Include(a => a.AuditMetaData).OrderBy(x => x.DateTimeOffset).FirstOrDefaultAsync();
                    AuditEntity auditModified = await context.Audits.Include(a => a.AuditMetaData).OrderByDescending(x => x.DateTimeOffset).FirstOrDefaultAsync();

                    Assert.Single(context.AuditMetaDatas);
                    Assert.Equal("PersonEntity", auditMetaData.DisplayName);
                    Assert.Equal("Persons", auditMetaData.Table);
                    Assert.Equal(entityEntry.ToReadablePrimaryKey(), auditMetaData.ReadablePrimaryKey);
                    Assert.Equal(entityEntry.ToReadablePrimaryKey().ToGuidHash(), auditMetaData.HashPrimaryKey);
                    Assert.Equal(2, auditMetaData.AuditChanges.Count());

                    Assert.Equal(2, context.Audits.Count());
                    Assert.Equal(auditMetaData, auditAdded.AuditMetaData);
                    Assert.Equal(EntityState.Added, auditAdded.EntityState);
                    Assert.NotNull(auditAdded.ByUser);
                    Assert.Null(auditAdded.OldValues);
                    Assert.Equal(auditMetaData, auditModified.AuditMetaData);
                    Assert.Equal(EntityState.Modified, auditModified.EntityState);
                    Assert.NotNull(auditModified.ByUser);
                    Assert.NotNull(auditModified.OldValues);

                    PersonEntity auditAddedValues = JsonConvert.DeserializeObject<PersonEntity>(auditAdded.NewValues);
                    Assert.Equal(personEntity.Id, auditAddedValues.Id);
                    Assert.Equal(notModifiedFirstName, auditAddedValues.FirstName);
                    Assert.Equal(personEntity.LastName, auditAddedValues.LastName);
                    Assert.Equal(personEntity.Gender, auditAddedValues.Gender);
                    Assert.Null(auditAddedValues.Addresses);
                    Assert.Null(auditAddedValues.Attributes);

                    PersonEntity auditModifiedNewValues = JsonConvert.DeserializeObject<PersonEntity>(auditModified.NewValues);
                    Assert.Equal(default, auditModifiedNewValues.Id);
                    Assert.Equal(personEntity.FirstName, auditModifiedNewValues.FirstName);
                    Assert.Equal(default, auditModifiedNewValues.LastName);
                    Assert.Equal(default, auditModifiedNewValues.Gender);
                    Assert.Null(auditModifiedNewValues.Addresses);
                    Assert.Null(auditModifiedNewValues.Attributes);

                    PersonEntity auditModifiedOldValues = JsonConvert.DeserializeObject<PersonEntity>(auditModified.OldValues);
                    Assert.Equal(default, auditModifiedOldValues.Id);
                    Assert.Equal(notModifiedFirstName, auditModifiedOldValues.FirstName);
                    Assert.Equal(default, auditModifiedOldValues.LastName);
                    Assert.Equal(default, auditModifiedOldValues.Gender);
                    Assert.Null(auditModifiedOldValues.Addresses);
                    Assert.Null(auditModifiedOldValues.Attributes);
                }
            }
        }

        [Fact]
        public async Task Add_one_entity_with_onAddGenerated_properties_one_entity_with_pre_generated_properties_Generates_two_auditentity_and_two_auditMetaDataEntities()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                EntityEntry personEntityEntry;
                EntityEntry personAttributesEntityEntry;
                PersonEntity person;
                PersonAttributesEntity personAttribute;
                //Arrange and Act
                using (var context = CreateContext(transaction))
                {
                    person = PersonTestData.FirstOrDefault(p => p.Id != default);
                    personAttribute = PersonAttributeTestData.FirstOrDefault(p => p.PersonId == person.Id);

                    personEntityEntry = await context.Persons.AddAsync(person);
                    personAttributesEntityEntry = await context.PersonAttributes.AddAsync(personAttribute);
                    await context.SaveChangesAsync();
                }

                using (var context = CreateContext(transaction))
                {
                    //Assert
                    List<AuditMetaDataEntity> auditMetaDatas = await context.AuditMetaDatas.Include(amd => amd.AuditChanges).ToListAsync();
                    List<AuditEntity> audits = await context.Audits.Include(a => a.AuditMetaData).ToListAsync();

                    Assert.Equal(2, context.AuditMetaDatas.Count());
                    Assert.Equal(2, context.Audits.Count());

                    Assert.Equal("Persons", auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").Table);
                    Assert.Equal(personEntityEntry.ToReadablePrimaryKey(), auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").ReadablePrimaryKey);
                    Assert.Equal(personEntityEntry.ToReadablePrimaryKey().ToGuidHash(), auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").HashPrimaryKey);
                    Assert.Single(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").AuditChanges);

                    Assert.Equal("PersonAttributes", auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").Table);
                    Assert.Equal(personAttributesEntityEntry.ToReadablePrimaryKey(), auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").ReadablePrimaryKey);
                    Assert.Equal(personAttributesEntityEntry.ToReadablePrimaryKey().ToGuidHash(), auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").HashPrimaryKey);
                    Assert.Single(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").AuditChanges);

                    Assert.Equal(2, context.Audits.Count());
                    Assert.Equal(EntityState.Added, auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").AuditChanges.FirstOrDefault().EntityState);
                    Assert.NotNull(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").AuditChanges.FirstOrDefault().ByUser);
                    Assert.Null(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").AuditChanges.FirstOrDefault().OldValues);
                    Assert.Equal(EntityState.Added, auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").AuditChanges.FirstOrDefault().EntityState);
                    Assert.NotNull(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").AuditChanges.FirstOrDefault().ByUser);
                    Assert.Null(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").AuditChanges.FirstOrDefault().OldValues);

                    PersonEntity personAuditValues = JsonConvert.DeserializeObject<PersonEntity>(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonEntity").AuditChanges.FirstOrDefault().NewValues);
                    Assert.Equal(person.Id, personAuditValues.Id);
                    Assert.Equal(person.FirstName, personAuditValues.FirstName);
                    Assert.Equal(person.LastName, personAuditValues.LastName);
                    Assert.Equal(person.Gender, personAuditValues.Gender);
                    Assert.Equal(person.Addresses, personAuditValues.Addresses);
                    Assert.Null(personAuditValues.Attributes);
                    PersonAttributesEntity personAttributeAuditValues = JsonConvert.DeserializeObject<PersonAttributesEntity>(auditMetaDatas.FirstOrDefault(x => x.DisplayName == "PersonAttributesEntity").AuditChanges.FirstOrDefault().NewValues);
                    Assert.Equal(personAttribute.Id, personAttributeAuditValues.Id);
                    Assert.Equal(personAttribute.PersonId, personAttributeAuditValues.PersonId);
                    Assert.Equal(personAttribute.Attribute, personAttributeAuditValues.Attribute);
                    Assert.Equal(personAttribute.AttributeValue, personAttributeAuditValues.AttributeValue);
                    Assert.Null(personAttributeAuditValues.DummyString);
                }
            }
        }

        [Fact]
        public async Task Add_one_entity_with_preGenerated_properties_Generates_one_auditEntity_and_auditMetaDataEntity()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                EntityEntry entityEntry;
                PersonEntity addedPerson;
                //Arrange and Act
                using (var context = CreateContext(transaction))
                {
                    addedPerson = PersonTestData.FirstOrDefault();
                    entityEntry = await context.Persons.AddAsync(addedPerson);
                    await context.SaveChangesAsync();
                }

                using (var context = CreateContext(transaction))
                {
                    //Assert
                    AuditMetaDataEntity auditMetaData = await context.AuditMetaDatas.Include(amd => amd.AuditChanges).FirstOrDefaultAsync();
                    AuditEntity audit = await context.Audits.Include(a => a.AuditMetaData).FirstOrDefaultAsync();

                    Assert.Single(context.AuditMetaDatas);
                    Assert.Equal("PersonEntity", auditMetaData.DisplayName);
                    Assert.Equal("Persons", auditMetaData.Table);
                    Assert.Equal(entityEntry.ToReadablePrimaryKey(), auditMetaData.ReadablePrimaryKey);
                    Assert.Equal(entityEntry.ToReadablePrimaryKey().ToGuidHash(), auditMetaData.HashPrimaryKey);
                    Assert.Single(auditMetaData.AuditChanges);
                    Assert.Equal(audit, auditMetaData.AuditChanges.FirstOrDefault());

                    Assert.Single(context.Audits);
                    Assert.Equal(auditMetaData, audit.AuditMetaData);
                    Assert.Equal(EntityState.Added, audit.EntityState);
                    Assert.NotNull(audit.ByUser);
                    Assert.Null(audit.OldValues);

                    PersonEntity auditValues = JsonConvert.DeserializeObject<PersonEntity>(audit.NewValues);
                    Assert.Equal(addedPerson.Id, auditValues.Id);
                    Assert.Equal(addedPerson.FirstName, auditValues.FirstName);
                    Assert.Equal(addedPerson.LastName, auditValues.LastName);
                    Assert.Equal(addedPerson.Gender, auditValues.Gender);
                    Assert.Null(auditValues.Addresses);
                    Assert.Null(auditValues.Attributes);
                }
            }
        }

    }
}
