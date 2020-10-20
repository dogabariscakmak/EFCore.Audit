using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCore.Audit.TestCommon
{
    public class UserProvider : IAuditUserProvider
    {
        private readonly IEnumerable<string> _users;

        public UserProvider()
        {
            _users = new string[] { "Ivette", "Chanelle", "Romaine", "Neoma", "Otha", "Nickole", "Lilliam", "Jerrell", "Oscar", "Roxann" };
        }

        public string GetUser()
        {
            Random random = new Random();
            return _users.ElementAt(random.Next(0, 10));
        }
    }
}
