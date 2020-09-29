using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotChocoloteSubscriptionTest
{
    public class TestEventTopic : IEquatable<TestEventTopic>
    {
        public TestEventTopic(string key)
        {
            Key = key;
        }

        public string Key { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as TestEventTopic);
        }

        public bool Equals(TestEventTopic other)
        {
            return other != null &&
                   Key == other.Key;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key);
        }
    }
}
