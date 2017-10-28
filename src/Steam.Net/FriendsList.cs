using System;
using System.Collections;
using System.Collections.Generic;

namespace Steam.Net
{
    public class FriendsList : IReadOnlyList<User>
    {
        public User this[int index] => throw new NotImplementedException();

        public int Count => throw new NotImplementedException();

        public IEnumerator<User> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
