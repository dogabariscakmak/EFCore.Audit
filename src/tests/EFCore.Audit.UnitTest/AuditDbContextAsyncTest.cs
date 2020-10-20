using EFCore.Audit.TestCommon;
using EFCore.Audit.UnitTest.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EFCore.Audit.UnitTest
{
    public class AuditDbContextAsyncTest : TestBase
    {
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
                    await context.Persons.AddAsync(personEntity);
                    await context.SaveChangesAsync();
                    personEntity = await context.Persons.FirstOrDefaultAsync();
                    personEntity.FirstName = $"{personEntity.FirstName}Modified";
                    await context.SaveChangesAsync();

                    addressEntity1 = AddressTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(0);
                    await context.Addresses.AddAsync(addressEntity1);
                    addressEntity2 = AddressTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(1);
                    await context.Addresses.AddAsync(addressEntity2);
                    await context.SaveChangesAsync();
                    addressEntity1 = context.Addresses.FirstOrDefault(x => x.PersonId == personEntity.Id && x.Type == addressEntity1.Type);
                    addressEntity1.PostalCode = 12345;
                    addressEntity2 = context.Addresses.FirstOrDefault(x => x.PersonId == personEntity.Id && x.Type == addressEntity2.Type);
                    addressEntity2.PostalCode = 54321;
                    await context.SaveChangesAsync();

                    personAttributesEntity1 = PersonAttributeTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(0);
                    await context.PersonAttributes.AddAsync(personAttributesEntity1);
                    personAttributesEntity2 = PersonAttributeTestData.Where(x => x.PersonId == personEntity.Id).ElementAt(1);
                    await context.PersonAttributes.AddAsync(personAttributesEntity2);
                    await context.SaveChangesAsync();
                    personAttributesEntity1 = await context.PersonAttributes.FirstOrDefaultAsync(x => x.Id == personAttributesEntity1.Id);
                    personAttributesEntity2 = await context.PersonAttributes.FirstOrDefaultAsync(x => x.Id == personAttributesEntity2.Id);
                    personAttributesEntity1.DummyString = "cahangingDummyString";
                    personAttributesEntity1.Attribute = AttributeType.Profession;
                    personAttributesEntity1.AttributeValue = "Graphic Designer";
                    context.Remove(personAttributesEntity2);
                    await context.SaveChangesAsync();
                }

                using (var context = CreateContext(transaction))
                {
                    //Assert
                    List<AuditMetaDataEntity> auditMetaDatas = await context.AuditMetaDatas.Include(amd => amd.AuditChanges).ToListAsync();
                    List<AuditEntity> audits = await context.Audits.Include(a => a.AuditMetaData).ToListAsync();

                    Assert.Equal(5, auditMetaDatas.Count);
                    Assert.Equal(10, audits.Count);

                    /************************************************************************************************************************/

                    Assert.Equal(2, auditMetaDatas[0].AuditChanges.Count);
                    Assert.Equal("AddressEntity", auditMetaDatas[0].DisplayName);
                    Assert.Null(auditMetaDatas[0].Schema);
                    Assert.Equal("Addresses", auditMetaDatas[0].Table);
                    Assert.Equal("Addresses", auditMetaDatas[0].SchemaTable);
                    Assert.Equal("PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Home", auditMetaDatas[0].ReadablePrimaryKey);
                    Assert.Equal("PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Home".ToGuidHash(), auditMetaDatas[0].HashPrimaryKey);

                    Assert.Equal(2, auditMetaDatas[1].AuditChanges.Count);
                    Assert.Equal("PersonEntity", auditMetaDatas[1].DisplayName);
                    Assert.Null(auditMetaDatas[1].Schema);
                    Assert.Equal("Persons", auditMetaDatas[1].Table);
                    Assert.Equal("Persons", auditMetaDatas[1].SchemaTable);
                    Assert.Equal("Id=caf3feb5-730e-40a3-9610-404a17b0deba", auditMetaDatas[1].ReadablePrimaryKey);
                    Assert.Equal("Id=caf3feb5-730e-40a3-9610-404a17b0deba".ToGuidHash(), auditMetaDatas[1].HashPrimaryKey);

                    Assert.Equal(2, auditMetaDatas[2].AuditChanges.Count);
                    Assert.Equal("AddressEntity", auditMetaDatas[2].DisplayName);
                    Assert.Null(auditMetaDatas[2].Schema);
                    Assert.Equal("Addresses", auditMetaDatas[2].Table);
                    Assert.Equal("Addresses", auditMetaDatas[2].SchemaTable);
                    Assert.Equal("PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Work", auditMetaDatas[2].ReadablePrimaryKey);
                    Assert.Equal("PersonId=caf3feb5-730e-40a3-9610-404a17b0deba,Type=Work".ToGuidHash(), auditMetaDatas[2].HashPrimaryKey);

                    Assert.Equal(2, auditMetaDatas[3].AuditChanges.Count);
                    Assert.Equal("PersonAttributesEntity", auditMetaDatas[3].DisplayName);
                    Assert.Null(auditMetaDatas[3].Schema);
                    Assert.Equal("PersonAttributes", auditMetaDatas[3].Table);
                    Assert.Equal("PersonAttributes", auditMetaDatas[3].SchemaTable);
                    Assert.Equal("Id=2", auditMetaDatas[3].ReadablePrimaryKey);
                    Assert.Equal("Id=2".ToGuidHash(), auditMetaDatas[3].HashPrimaryKey);

                    Assert.Equal(2, auditMetaDatas[4].AuditChanges.Count);
                    Assert.Equal("PersonAttributesEntity", auditMetaDatas[4].DisplayName);
                    Assert.Null(auditMetaDatas[4].Schema);
                    Assert.Equal("PersonAttributes", auditMetaDatas[4].Table);
                    Assert.Equal("PersonAttributes", auditMetaDatas[4].SchemaTable);
                    Assert.Equal("Id=1", auditMetaDatas[4].ReadablePrimaryKey);
                    Assert.Equal("Id=1".ToGuidHash(), auditMetaDatas[4].HashPrimaryKey);

                    /************************************************************************************************************************/

                    Assert.NotNull(audits[0].AuditMetaData);
                    Assert.NotEqual(default, audits[0].Id);
                    Assert.Equal(EntityState.Added, audits[0].EntityState);
                    Assert.Equal("{\"Id\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"FirstName\":\"Ofella\",\"Gender\":1,\"LastName\":\"Andrichuk\"}", audits[0].NewValues);
                    Assert.Null(audits[0].OldValues);
                    Assert.NotNull(audits[0].ByUser);
                    Assert.NotEqual(default, audits[0].DateTimeOffset);

                    Assert.NotNull(audits[1].AuditMetaData);
                    Assert.NotEqual(default, audits[1].Id);
                    Assert.Equal(EntityState.Modified, audits[1].EntityState);
                    Assert.Equal("{\"FirstName\":\"OfellaModified\"}", audits[1].NewValues);
                    Assert.Equal("{\"FirstName\":\"Ofella\"}", audits[1].OldValues);
                    Assert.NotNull(audits[1].ByUser);
                    Assert.NotEqual(default, audits[1].DateTimeOffset);

                    Assert.NotNull(audits[2].AuditMetaData);
                    Assert.NotEqual(default, audits[2].Id);
                    Assert.Equal(EntityState.Added, audits[2].EntityState);
                    Assert.Equal("{\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Type\":1,\"City\":\"Azteca\",\"HouseNumber\":\"844\",\"PostalCode\":50188,\"Street\":\"Vahlen\"}", audits[2].NewValues);
                    Assert.Null(audits[2].OldValues);
                    Assert.NotNull(audits[2].ByUser);
                    Assert.NotEqual(default, audits[2].DateTimeOffset);

                    Assert.NotNull(audits[3].AuditMetaData);
                    Assert.NotEqual(default, audits[3].Id);
                    Assert.Equal(EntityState.Added, audits[3].EntityState);
                    Assert.Equal("{\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Type\":2,\"City\":\"Tawun\",\"HouseNumber\":\"7\",\"PostalCode\":32921,\"Street\":\"Welch\"}", audits[3].NewValues);
                    Assert.Null(audits[3].OldValues);
                    Assert.NotNull(audits[3].ByUser);
                    Assert.NotEqual(default, audits[3].DateTimeOffset);

                    Assert.NotNull(audits[4].AuditMetaData);
                    Assert.NotEqual(default, audits[4].Id);
                    Assert.Equal(EntityState.Modified, audits[4].EntityState);
                    Assert.Equal("{\"PostalCode\":12345}", audits[4].NewValues);
                    Assert.Equal("{\"PostalCode\":32921}", audits[4].OldValues);
                    Assert.NotNull(audits[4].ByUser);
                    Assert.NotEqual(default, audits[4].DateTimeOffset);

                    Assert.NotNull(audits[5].AuditMetaData);
                    Assert.NotEqual(default, audits[5].Id);
                    Assert.Equal(EntityState.Modified, audits[5].EntityState);
                    Assert.Equal("{\"PostalCode\":54321}", audits[5].NewValues);
                    Assert.Equal("{\"PostalCode\":50188}", audits[5].OldValues);
                    Assert.NotNull(audits[5].ByUser);
                    Assert.NotEqual(default, audits[5].DateTimeOffset);

                    Assert.NotNull(audits[6].AuditMetaData);
                    Assert.NotEqual(default, audits[6].Id);
                    Assert.Equal(EntityState.Added, audits[6].EntityState);
                    Assert.Equal("{\"Attribute\":1,\"AttributeValue\":\"Married\",\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Id\":2}", audits[6].NewValues);
                    Assert.Null(audits[6].OldValues);
                    Assert.NotNull(audits[6].ByUser);
                    Assert.NotEqual(default, audits[6].DateTimeOffset);

                    Assert.NotNull(audits[7].AuditMetaData);
                    Assert.NotEqual(default, audits[7].Id);
                    Assert.Equal(EntityState.Added, audits[7].EntityState);
                    Assert.Equal("{\"Attribute\":3,\"AttributeValue\":\"Turkish\",\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\",\"Id\":1}", audits[7].NewValues);
                    Assert.Null(audits[7].OldValues);
                    Assert.NotNull(audits[7].ByUser);
                    Assert.NotEqual(default, audits[7].DateTimeOffset);

                    Assert.NotNull(audits[8].AuditMetaData);
                    Assert.NotEqual(default, audits[8].Id);
                    Assert.Equal(EntityState.Deleted, audits[8].EntityState);
                    Assert.Null(audits[8].NewValues);
                    Assert.Equal("{\"Id\":1,\"Attribute\":3,\"AttributeValue\":\"Turkish\",\"PersonId\":\"caf3feb5-730e-40a3-9610-404a17b0deba\"}", audits[8].OldValues);
                    Assert.NotNull(audits[8].ByUser);
                    Assert.NotEqual(default, audits[8].DateTimeOffset);

                    Assert.NotNull(audits[9].AuditMetaData);
                    Assert.NotEqual(default, audits[9].Id);
                    Assert.Equal(EntityState.Modified, audits[9].EntityState);
                    Assert.Equal("{\"Attribute\":2,\"AttributeValue\":\"Graphic Designer\"}", audits[9].NewValues);
                    Assert.Equal("{\"Attribute\":1,\"AttributeValue\":\"Married\"}", audits[9].OldValues);
                    Assert.NotNull(audits[9].ByUser);
                    Assert.NotEqual(default, audits[9].DateTimeOffset);
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
                    AuditEntity auditAdded = (await context.Audits.Include(a => a.AuditMetaData).ToListAsync()).OrderBy(x => x.DateTimeOffset).FirstOrDefault();
                    AuditEntity auditModified = (await context.Audits.Include(a => a.AuditMetaData).ToListAsync()).OrderByDescending(x => x.DateTimeOffset).FirstOrDefault();

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
                    personAttributesEntityEntry = context.PersonAttributes.Add(personAttribute);
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
                    context.SaveChanges();
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
