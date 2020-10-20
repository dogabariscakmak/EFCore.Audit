using System;
using System.Collections.Generic;

namespace EFCore.Audit.TestCommon
{
    [Auditable]
    public class PersonEntity
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public GenderEnum Gender { get; set; }
        public ICollection<AddressEntity> Addresses { get; set; }
        public ICollection<PersonAttributesEntity> Attributes { get; set; }
    }

    public enum GenderEnum
    {
        NotSet = 0,
        Female = 1,
        Male = 2
    }
}
