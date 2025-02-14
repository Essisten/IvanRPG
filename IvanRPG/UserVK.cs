using System;
using System.Collections.Generic;

namespace IvanRPG
{
    class UserVK
    {
        public static Dictionary<long, UserVK> Users = new();
        public readonly long ID;
        public string[] cache = new string[4];
        public UserVK(long id)
        {
            ID = id;
        }
        public static UserVK AddUser(long id)
        {
            if (!Users.TryGetValue(id, out UserVK user))
            {
                user = new(id);
                Users.Add(id, user);
            }
            return user;
        }
    }
}