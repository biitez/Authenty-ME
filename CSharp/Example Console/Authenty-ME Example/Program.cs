using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Authenty;

namespace Authenty_ME_Example
{
    class Program
    {

        public static Licensing exampleApp = new Licensing(new AppSettings()
        {
            ApplicationId = 1234567, // https://share.biitez.dev/i/c6nhw.png
            ApplicationKey = "Your Application Key", // https://share.biitez.dev/i/2cp6q.png
            RsaPubKey = "Your Application RSA Public-Key", // https://share.biitez.dev/i/874we.png
            ApplicationVersion = "1.0.0" // Use it only if you use the auto updater
        });

        static void Main(string[] args)
        {
            exampleApp.Connect();

            Console.Title = "Authenty.ME | Professional Licensing Solution";

            Console.WriteLine();
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. License-Login (AIO)");
            Console.WriteLine("4. Extend Subscription Time");

            Console.Write("> ");

            switch (Console.ReadLine())
            {
                case "1":

                    Console.WriteLine();

                    Console.Write("Username: ");
                    string username = Console.ReadLine();

                    Console.Write("Password: ");
                    string password = Console.ReadLine();

                    if (exampleApp.Login(username, password))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine("\nLogged In!");

                        Console.WriteLine("\nWait 5 seconds please ...");

                        Thread.Sleep(5000);

                        MethodLoggedIn();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Username Or Password");                        
                    }

                    break;

                case "2":

                    Console.WriteLine();

                    Console.Write("Username: ");
                    string usernameToRegister = Console.ReadLine();

                    Console.Write("Email: ");
                    string emailToRegister = Console.ReadLine();

                    Console.Write("Create a Password: ");
                    string passwordToRegister = Console.ReadLine();

                    Console.Write("License: ");
                    string licenseToRegister = Console.ReadLine();

                    if (exampleApp.Register(usernameToRegister, passwordToRegister, emailToRegister, licenseToRegister))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine("\nSuccessfully Registered !");

                        // Here you can do what you would do if he were logged in, there will be no need to re-log

                        Console.WriteLine("\nWait 5 seconds please ...");

                        Thread.Sleep(5000);

                        MethodLoggedIn();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid Username Or Password");
                    }

                    break;

                case "3":

                    Console.WriteLine();

                    Console.Write("License: ");
                    string license = Console.ReadLine();

                    if (exampleApp.LicenseLogin(license))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine("\nLogged In!");

                        Console.WriteLine("\nWait 5 seconds please ...");

                        Thread.Sleep(5000);

                        MethodLoggedIn();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid License");
                    }

                    break;

                case "4":

                    Console.WriteLine();

                    Console.Write("Username: ");
                    string usernameAccount = Console.ReadLine();

                    Console.Write("Username: ");
                    string passwordAccount = Console.ReadLine();

                    Console.Write("LicenseKey: ");
                    string licenseKey = Console.ReadLine(); // It will take the time of the license and it will be added to the user, the license after being used, will be unusable.

                    if (exampleApp.ExtendSubscription(usernameAccount, passwordAccount, licenseKey))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        Console.WriteLine("Your account time has been extended!");

                        Console.ReadLine();

                        Environment.Exit(0);
                    }

                    break;

                default: throw new Exception("Invalid option");
            }

            Console.ReadLine();

        }

        public static void MethodLoggedIn()
        {            
            while (true)
            {
                Console.Clear();

                Console.WriteLine();

                Console.WriteLine("1. Get User Info");
                Console.WriteLine("2. Get Variable");

                Console.Write("> ");

                switch (Console.ReadLine())
                {
                    case "1":

                        Console.WriteLine();

                        Console.WriteLine($"Username: {exampleApp.UserInfo.Username}");
                        Console.WriteLine($"Email: {exampleApp.UserInfo.Email}");                        
                        Console.WriteLine($"HWID: {exampleApp.UserInfo.HWID}");
                        Console.WriteLine($"Level: {exampleApp.UserInfo.Level}");
                        Console.WriteLine($"Expire-Date: {exampleApp.UserInfo.ExpireDate}");

                        break;


                    case "2":

                        Console.Write("\nVariable Secret-Code: ");

                        string secretKey = Console.ReadLine();

                        Console.WriteLine($"Variable Value: {exampleApp.GetVariable(secretKey)}");

                        break;
                }

                Console.WriteLine("\nEnter to return to the menu");
                Console.ReadLine();
            } 
        }
    }
}
