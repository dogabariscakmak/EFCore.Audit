using System;

namespace EFCore.Audit.TestCommon
{
    [Auditable]
    public class AddressEntity
    {
        public Guid PersonId { get; set; }
        public AddressType Type { get; set; }
        public int PostalCode { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string City { get; set; }
        public PersonEntity Person { get; set; }
    }

    public enum AddressType
    {
        NotSet = 0,
        Home = 1,
        Work = 2
    }
}
