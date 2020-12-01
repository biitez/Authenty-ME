using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace exampleApp
{
    class Program
    { 

         internal static readonly Authenty exampleApp = new Authenty("Application name", new Settings()
         {
             APPKey = "Application Key", // https://prnt.sc/vfgr2a
             APPID = "Application ID", // https://prnt.sc/vfgr6b
             Version = "1.0" // https://prnt.sc/vfgrbb
         });         

        static void Main(string[] args)
        {
            /* If you want all in the same method, just use this:
             * 
             * Authenty exampleApp = new Authenty(APPName: "Application name", new Settings()
             * {
             *    APPKey = "9fbcf305f196e014678b6cae5215ab16", // https://prnt.sc/vfgr2a
             *    APPID = "709084", // https://prnt.sc/vfgr6b
             *    Version = "1.0" // https://prnt.sc/vfgrbb
             * }).APIConnect(); // Connecting..
             * 
             */

            Console.Title = "Authenty.ME | The most secure authentication system for your application";
            exampleApp.APIConnect();


            if (!exampleApp.Verify()) // Simply a small check if the connection to the server was successful and is fully encrypted
                MessageBox.Show("Could not verify the connection to the server!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Error);            

            Console.WriteLine("\n [1]. Login");
            Console.WriteLine(" [2]. Register");
            Console.WriteLine(" [3]. Extend time subscription");
            Console.WriteLine(" [4]. License login");

            Console.Write("\n  > ");
            switch (int.Parse(Console.ReadLine()))
            {
                case 1:

                    Console.Write("\n Username: ");
                    string usernameLogin = Console.ReadLine();
                    Console.Write(" Password: ");
                    string passwordLogin = Console.ReadLine();

                    if (exampleApp.Login(usernameLogin, passwordLogin))
                    {
                        MessageBox.Show("You have successfully logged in!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Logged();
                    }
                    // The class or library will handle the errors

                    Environment.Exit(0);
                    break;

                case 2:

                    Console.Write("\n Username: ");
                    string usernameReg = Console.ReadLine();
                    Console.Write(" Password: ");
                    string passwordReg = Console.ReadLine();
                    Console.Write(" License: ");
                    string LicenseReg = Console.ReadLine();

                    if (exampleApp.Register(usernameReg, passwordReg, LicenseReg))
                    {
                        MessageBox.Show("You have successfully registered, please log in again", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                    // The class or library will handle the errors

                    Environment.Exit(0);
                    break;

                case 3:

                    Console.Write("\n Username: ");
                    string usernameExt = Console.ReadLine();
                    Console.Write(" Password: ");
                    string passwordExt = Console.ReadLine();
                    Console.Write(" License: ");
                    string LicenseExt = Console.ReadLine();

                    if (exampleApp.ExtendTime(usernameExt, passwordExt, LicenseExt))
                    {
                        MessageBox.Show("Your subscription time has been successfully extended!, please log in again", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Environment.Exit(0);
                    }
                    // The class or library will handle the errors

                    Environment.Exit(0);
                    break;

                case 4:

                    Console.Write("\n License: ");
                    string licenseLog = Console.ReadLine();

                    if (exampleApp.licenseLogin(licenseLog))
                    {
                        MessageBox.Show("You have successfully logged in!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Logged();
                    }
                    // The class or library will handle the errors

                    Environment.Exit(0);

                    break;

                default:
                    MessageBox.Show("Invalid option!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }

            Environment.Exit(0);
        }


        public static void Logged()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n [1]. Send LOG"); // exampleApp.Log("Message"); 
                Console.WriteLine(" [2]. Get variable\n");
                Console.Write("  > ");
                switch (int.Parse(Console.ReadLine()))
                {
                    case 1:
                        bool SendLog = exampleApp.Log("Message");
                        if (SendLog)
                            MessageBox.Show("The log has been sent successfully", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;

                    case 2:
                        Console.Write("\n Variable code: "); // https://prnt.sc/vhnxsy
                        string variable_value = exampleApp.captureVariable(Console.ReadLine()); // https://prnt.sc/vhnzek
                        MessageBox.Show("Your variable value: " + variable_value, exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;


                    default:
                        MessageBox.Show("Invalid option!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
        }
    }
}
