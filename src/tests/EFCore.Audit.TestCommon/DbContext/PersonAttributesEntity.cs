using Newtonsoft.Json;
using System;
using System.Linq;

namespace EFCore.Audit.TestCommon
{
    [Auditable]
    public class PersonAttributesEntity
    {
        public long Id { get; set; }
        public AttributeType Attribute { get; set; }
        public string AttributeValue { get; set; }
        public Guid PersonId { get; set; }
        public PersonEntity Person { get; set; }

        [NotAuditable]
        public string DummyString { get; set; }

        public PersonAttributesEntity()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            DummyString = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [JsonConstructor]
        public PersonAttributesEntity(long id, AttributeType attribute, string attributeValue, Guid personId)
        {
            Id = id;
            Attribute = attribute;
            AttributeValue = attributeValue;
            PersonId = personId;
        }
    }

    public enum AttributeType
    {
        NotSet = 0,
        MaritalStatus = 1,
        Profession = 2,
        Nationality = 3
    }
}
