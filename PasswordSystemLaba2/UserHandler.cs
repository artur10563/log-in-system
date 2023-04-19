using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace PasswordSystemLaba2
{

    internal static class UserHandler
    {

        private enum Buttons
        {
            Enter = 13,
            Backspace = 8
        }

        // хоча б один математичний символ і однa цифрa
        private static readonly Regex regex = new Regex(@"(?=.*\d)(?=.*[\+\-\*/])");

        //Password input with stars
        private static string passwordInput()
        {
            List<char> password = new List<char>();
            char ch;
            while (true)
            {
                ch = (char)Console.ReadKey(true).KeyChar;

                if (ch == (char)Buttons.Enter)
                {
                    break;
                }

                if (ch == (char)Buttons.Backspace)
                {
                    if (password.Count() != 0)
                    {
                        password.RemoveAt(password.Count() - 1);
                        Console.Write("\b \b");
                    }
                }

                else
                {
                    password.Add(ch);
                    Console.Write("*");
                }
            }

            return new string(password.ToArray());
        }



        //Change password 
        internal static void changePassword(User user)
        {

            Console.Write("Enter old password: ");
            string oldPass = passwordInput();


            if (user.Password.CompareTo(oldPass) != 0)
            {
                Console.WriteLine("\nIncorrect old password entered. Please try again.");
                return;
            }

            Console.Write("\nEnter new password: ");
            string newPass1 = passwordInput();

            Console.Write("\nEnter new password again: ");
            string newPass2 = passwordInput();

            if (newPass1.CompareTo(newPass2) != 0)
            {
                Console.WriteLine("\nNew passwords do not match. Please try again.");
                Logger.logWrite("Failed to change password", user);
                return;
            }
            if (newPass1.CompareTo(oldPass) == 0)
            {
                Console.WriteLine("\nNew password cant be same as old password");
                Logger.logWrite("Failed to change password", user);
                return;
            }

            user.Password = newPass2;
            Console.WriteLine("\nPassword changed successfully.");
            Logger.logWrite("Changed password", user);
        }

        //Get users from file
        internal static void loadUsers(string DB, List<User> users)
        {
            if (File.Exists(DB))
            {
                using (FileStream stream = new FileStream(DB, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    while (stream.Position < stream.Length)
                    {
                        User u = (User)formatter.Deserialize(stream);
                        users.Add(u);

                    }
                }
            }
            else { users.Add(new User("ADMIN", "", true)); };
            User.UpdateMaxId(users);
        }

        //Save users to file
        internal static void saveUsers(string DB, List<User> users)
        {
            using (FileStream stream = new FileStream(DB, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                users.ForEach(user => formatter.Serialize(stream, user));
            }
        }

        //Register
        internal static User createUser(List<User> users)
        {

            Console.Clear();
            Console.WriteLine("Enter 0 to cancel.");

            while (true)
            {
                Console.Write("Login: ");
                string login = Console.ReadLine().Trim();

                if (login == "0") return null;


                Console.Write("Password: ");
                string password = passwordInput();
                if (!regex.IsMatch(password))
                {
                    Console.WriteLine("Password must contain atleast one digit and mathematical operation symbol.\nTry again ");
                    continue;
                }

                //Unique logins only
                if (users.Any(us => us.Login == login))
                {
                    Console.Clear();
                    Logger.logWrite("Failed to create user.");
                    Console.WriteLine($"User '{login}' already exists. Try again or enter 0 to cancel.");
                }
                else
                {
                    User newUser = new User(login, password);
                    users.Add(newUser);
                    Logger.logWrite("New user created", newUser);
                    return newUser;
                }

            }
        }

        //Login 
        internal static User loginAsUser(List<User> users)
        {
            Console.Clear();
            Console.WriteLine("Enter 0 to cancel.");
            int counter = 0;

            while (counter < 3)
            {
                Console.Write("Login: ");
                string login = Console.ReadLine().Trim();

                if (login == "0")
                {
                    Console.WriteLine("Login cancelled.");
                    return null;
                }

                Console.Write("Password: ");
                string password = passwordInput();



                Console.WriteLine();

                foreach (User user in users)
                {
                    //Find user with entered credentials 
                    if (user.Login == login && user.Password == password)
                    {

                        //Verify with hash function and key
                        Security.HashedString hashRes = Security.Encrypt(password);

                        MessageBox.Show(hashRes.key.ToString(), "Key");

                        int key;
                        int counter2 = 0;
                        do
                        {

                            Console.Write("\nEnter key:");
                            counter2++;
                        } while (!int.TryParse(Console.ReadLine(), out key) && counter2 < 3);

                        if (counter2 >= 3)
                        {
                            Console.WriteLine("\nFailed to log in.");
                            Logger.logWrite("Failed to log in", user);
                            break;
                        }

                        //If key is correct, Security.Verify returns true
                        if (Security.Verify(password, hashRes.hashedString, key))
                        {

                            if (user.isBlocked)
                            {
                                Console.WriteLine($"User '{login}' is blocked.");
                                Logger.logWrite("Attempt to Log in", user);
                                return null;
                            }

                            Console.WriteLine($"You logged in as {login}");
                            Logger.logWrite("Logged in", user);
                            return user;
                        }
                    }
                }
                Console.Clear();
                Console.WriteLine("Invalid login or password. Try again or enter 0 to cancel.");
                counter++;
            }
            return null;
        }

        //Ban/Unban
        /// <summary>
        /// true - ban; 
        /// false - unban
        /// </summary>
        /// <param name="curUser"></param>
        /// <param name="ban"></param>
        internal static void banUnbanUser(User curUser, bool ban, List<User> users)
        {
            if (!(curUser != null && curUser.isAdmin)) { Console.WriteLine("Only admin can un/ban users!"); return; }

            List<User> blockedUsers = users.Where(u => u.isBlocked == !ban).ToList(); //List of users which can be banned or unbanned
            foreach (User u in blockedUsers)
            {
                u.showInfo();
            }

            Console.WriteLine("Enter id of user to un/ban: ");


            if (!int.TryParse(Console.ReadLine(), out int banID)) { Console.WriteLine("Invalid value"); return; };


            User banned = users.FirstOrDefault(user => user.Id == banID);


            if (banned != null)
            {
                if (banned.isAdmin)
                {
                    Console.WriteLine(@"Can`t un/ban admin");
                    return;
                }
                Console.Clear();
                banned.isBlocked = ban;
                Console.WriteLine($"{banned.Id} {banned.Login} have been " + (ban ? $"banned" : "unbanned"));
                Logger.logWrite($"{banned.Id} {banned.Login} have been " + (ban ? $"banned" : "unbanned"), curUser);

            }
            else Console.WriteLine($"No user with id: {banID}.");
        }

        /// <summary>
        /// Admin: get info about all users;
        /// User : get info about self
        /// </summary>
        /// <param name="curUser"></param>
        /// <param name="users"></param>
        internal static void showUserList(User curUser, List<User> users)
        {
            if ((curUser == null)) { Console.WriteLine("Log in to see list of users!"); return; }


            if (!curUser.isAdmin)
            {
                curUser.showInfo();
                Logger.logWrite("Reviewed info", curUser);
            }
            if (curUser.isAdmin)
                foreach (var user in users)
                {
                    user.showInfo();
                    Logger.logWrite("Reviewed all users info", curUser);
                }

            return;
        }

    }
}
