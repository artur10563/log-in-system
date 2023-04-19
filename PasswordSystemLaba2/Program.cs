using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Timers;
using System.Windows;

namespace PasswordSystemLaba2
{
    internal class Program
    {
        enum Menu
        {
            Exit = 0,
            Login = 1,
            Register = 2,
            ChangePassword = 3,
            ShowUserList = 4,
            Ban = 5,
            Unban = 6,
        }


        private static bool done = false;

        private static readonly string DB = "db.dat";
        private static readonly string LOGS = "logs.txt";

        private static readonly List<User> users = new List<User>();
        private static User curUser = null;


        //Handle access 
        private static void handleUnauthorizedAccess()
        {

            while (true)
            {
                Console.Write(
                $"\n1 - Login" +
                $"\n2 - Register" +
                $"\n0 - Exit" +
                $"\n=>");

                if (!int.TryParse(Console.ReadLine(), out int option)) option = int.MaxValue; //Drop user to default case
                Enum.TryParse(option.ToString(), out Menu menu);

                Console.Clear();
                switch (menu)
                {
                    case Menu.Login:
                        curUser = UserHandler.loginAsUser(users);
                        return;

                    case Menu.Register:
                        curUser = UserHandler.createUser(users);

                        return;

                    case Menu.Exit:
                        done = true;
                        return;

                    default:
                        Console.WriteLine("Invalid menu option. Try Again.");
                        break;
                }

            }
        }
        private static void handleUserAccess()
        {
            while (true)
            {
                Console.Write(
                    $"\n3 - Change password" +
                    $"\n4 - Show account info" +
                    $"\n0 - Log off" +
                    $"\n=>");


                if (!int.TryParse(Console.ReadLine(), out int option)) option = int.MaxValue; //Drop user to default case
                Enum.TryParse(option.ToString(), out Menu menu);
                Console.Clear();

                switch (menu)
                {
                    case Menu.ChangePassword:
                        UserHandler.changePassword(curUser);
                        break;

                    case Menu.ShowUserList:
                        UserHandler.showUserList(curUser, users);
                        break;

                    case Menu.Exit:
                        Logger.logWrite("Logged off", curUser);
                        curUser = null;
                        return;
                    default:
                        Console.WriteLine("Invalid menu option. Try Again.");
                        break;
                }
            }
        }
        private static void handleAdminAccess()
        {
            while (true)
            {
                Console.Write(
                    $"\n3 - Change password" +
                    $"\n4 - Show user list" +
                    $"\n5 - Ban" +
                    $"\n6 - Unban" +
                    $"\n0 - Log off" +
                    $"\n=>");

                if (!int.TryParse(Console.ReadLine(), out int option)) option = int.MaxValue; //Drop user to default case
                Enum.TryParse(option.ToString(), out Menu menu);
                Console.Clear();

                switch (menu)
                {
                    case Menu.ChangePassword:
                        UserHandler.changePassword(curUser);
                        break;

                    case Menu.ShowUserList:
                        UserHandler.showUserList(curUser, users);
                        break;

                    case Menu.Ban:
                        UserHandler.banUnbanUser(curUser, true, users);
                        break;

                    case Menu.Unban:
                        UserHandler.banUnbanUser(curUser, false, users);
                        break;

                    case Menu.Exit:
                        Logger.logWrite("Logged off", curUser);
                        curUser = null;
                        return;

                    default:
                        Console.WriteLine("Invalid menu option. Try Again.");
                        break;

                }
            }
        }

        //5 хвилин
        private static Timer timer = new Timer(5 * 60 * 1000); 

        private static bool shouldAnswer = true;
        private static void askQuestion(object sender, ElapsedEventArgs e)
        {
            if (curUser != null)
            {
                MessageBoxResult btnRes = MessageBox.Show("Are you still here?", $"Question for {curUser.Login}", MessageBoxButton.YesNo);
                if (btnRes != MessageBoxResult.Yes)
                {
                    curUser = null;
                    
                }

            }
        }

        static void Main(string[] args)
        {
            //Update log path
            Logger.LogFile = LOGS;
            UserHandler.loadUsers(DB, users);

            timer.Elapsed += askQuestion;
            timer.Start();


            while (!done)
            {
                switch (curUser)
                {
                    //Login and register
                    case null:
                        handleUnauthorizedAccess();
                        break;

                    //User access
                    case { isAdmin: false }:
                        handleUserAccess();
                        break;

                    //Admin access
                    case { isAdmin: true }:
                        handleAdminAccess();
                        break;
                }
            }




            timer.Stop();
            UserHandler.saveUsers(DB, users);


        }
    }
}
