using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PasswordSystemLaba2
{
    [Serializable]
    internal class User
    {
        private static int maxID = 0;

        public int Id { get; private set; }
        public string Login { get; private set; }
        public string Password { get; set; }
        public bool isAdmin { get; private set; }
        public bool isBlocked { get; set; }

        public User() { }
        public User(string login, string password, bool isAdmin = false, bool isBlocked = false)
        {
            this.Id = maxID++;

            this.Login = login;
            this.Password = password;
            this.isAdmin = isAdmin;
            this.isBlocked = isBlocked;
        }

        public static void UpdateMaxId(List<User> users)
        {
            foreach (User user in users)
            {
                if (user.Id > maxID)
                    maxID = user.Id;
            }
            maxID++;
        }
        public void showInfo()
        {
            Console.WriteLine($"{Id}\t{Login}\t{Password}\tisAdmin:{isAdmin}\tisBlocked:{isBlocked} ");
        }
    }

  
}
