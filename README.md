# Authenty-ME
Top-Tier Licensing Systems for .NET

#### Note: this project is being completely rewritten and will contain new security, new private algorithms and authentication/authorization methods, until then no further updates will be made.

### FAQ:

1. What programs is this authentication system intended for?
   - This program is aimed at programs that really need a secure authentication and easy to understand and integrate into their applications.
   
2. What is the price of this Authenty?
   - It is totally free!, Although, if you want special functions, you can opt for the paid plans (which are really cheap)
   
3. I have a question, suggestion or criticism, where should I send it?
   - Please enter our discord server ( https://biitez.dev/discord )

4. What makes Authenty different from others?
   - What makes Authenty different from the others are more than one thing, one of the most important is ***security***. We use an incredibly powerful encryption for communication, all applications have different RSA keys, private keys are stored securely on our server, everything is worked with temporary sessions created and secured under our server, so everything will be incredibly fast and safe.
   
### Some features:
- [x] Data is secured under **highly advanced encryption**
- [x] Encrypted SSL traffic
- [x] Creation of many licenses at once
- [x] Secure server-side variable control (very recommended)
- [x] Limit 1 PC per license
- [x] Advanced HWID control
- [x] Multiple managers / operators for your application with customized permissions
- [x] Many easy to use and understand APIs
- [x] Full control of users (change password, email, ban them from your application)
- [x] Full control of your application (you can pause your application from the panel, ensure its integrity, etc)
- [x] Simple auto-updater
- [x] Very easy integration
- [x] Friendly and easy to understand panel
- [x] Easy to understand [documentation](https://docs.biitez.dev/authenty)
- [x] Continuous support and development
- [x] No need to configure the cloud or a database, just register in the panel and integrate authenty in your program
- [x] Uptime 99.9%


# Integration steps:
## First of all, you need to integrate our class in your project (can be found from the panel = https://biitez.dev/login) 

### 1. After you have integrated our authentication system, you must initialize the program by writing the following code:

```csharp
using System;
using Authenty;

namespace ExampleApplication
{
    class Program
    {
        public static Licensing exampleApp = new Licensing(new AppSettings()
        {
            ApplicationId = 1234567, // You will find this in the panel of your application
            ApplicationKey = "MD5 Application Key", // You will find this in the panel of your application
            RsaPubKey = "RSA Public Key", // This is the RSA public key of your application, you will find it in the panel.
            ApplicationVersion = "1.0.0" // This is only necessary if you want to use the auto-updater
        });

        static void Main(string[] args)
        {
            // Securely connecting to our API with your application data
            exampleApp.Connect();
            
            // all errors will be handled by our class
        }
    }
}
```

### 2. After having it integrated, you must choose between two direct modes of login after writing the license:
   - Entering directly with the license you created
   - Registering using a username along with a password and the license you created, after then the customer will only have to log in with the username and your password

Then, you already have the chosen mode, if you want to integrate the first one, follow these steps:
## 3. Mode: Log In with username & password
###    - (3.1). To log in to the program, you must place this in the code:
```csharp
Console.Write(" Username: ");
string username = Console.ReadLine();

Console.Write(" Password: ");
string password = Console.ReadLine();


if (exampleApp.Login(username, password))
{
   Console.WriteLine("Logged in!");
   
   // Your code or method where your program will go after being logged
}
```

###    - (3.2). To register in the program (using the license), you must place this in the code:
```csharp
Console.Write(" Username: ");
string username = Console.ReadLine();

Console.Write(" Password: ");
string password = Console.ReadLine();

Console.Write(" Email: ");
string email = Console.ReadLine();

Console.Write(" License: ");
string licenseKey = Console.ReadLine();

if (exampleApp.Register(username, password, email, licenseKey))
{
  Console.WriteLine("Registered!");
  
  // Your code or method where your program will go after being logged
}
```

## 3. Mode: Only with license
```csharp
Console.Write(" License: ");
string licenseKey = Console.ReadLine();

if (exampleApp.LicenseLogin(licenseKey))
{
   Console.WriteLine("Logged In!");

   // Your code or method where your program will go after being logged
}
```

## 4. Extend subscription for temporary licenses
```csharp
Console.Write(" Username: ");
string username = Console.ReadLine();

Console.Write(" Password: ");
string password = Console.ReadLine();

Console.Write(" License: ");
string licenseKey = Console.ReadLine();

if (exampleApp.ExtendSubscription(username, password, licenseKey))
{
   Console.WriteLine("Your account time has been extended!");
}
```

## 5. Control server-side variables
```csharp
Console.Write(" License: ");
string licenseKey = Console.ReadLine();

// Console.WriteLine(license.GetVariable("Variable Secret Code")); // The program will close due to the fact that only the method can be used after the user logs in.

if (license.LicenseLogin(licenseKey))
{
  Console.WriteLine("Logged In!");

  Console.WriteLine(license.GetVariable("Variable Secret Code")); // Will return the value of the variable placed in the panel
}

```

## 6. APIs & Extra
- The documentation of the APIs can be found in our [complete documentation](https://docs.biitez.dev/authenty) 


## NOTE:
- This project is only worked by me in my free time, so if you find any bug, please contact me directly by discord (biitez#8568) or telegram (@biitez), I hope it meets your expectations!

Web Page: https://biitez.dev/home/authenty
Documentation Page: https://docs.biitez.dev/authenty
