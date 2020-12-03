# Authenty-ME
Very secure and free (Authentication/Licensing) system for .NET

### FAQ:
1. What is Authenty?
   - It is a free and advanced authentication system for programs created in .NET.

2. What programs is this authentication system intended for?
   - This program is aimed at programs that really need a secure authentication and easy to understand and integrate into their applications.
   
3. What is the price of this Authenty?
   - We have 3 plans, the basic one (free) that gives you permission to integrate your program, be able to create licenses among many other things (plans detailed in https://authenty.me), the Premium (cost: $9.99 Lifetime) and the Advanced (cost: $14.99)
   
4. I have a question, suggestion or criticism, where should I send it?
   - You can send it through our direct contact (https://authenty.me/support) or enter our discord server (https://discord.gg/UmJbzMd)

5. What makes Authenty different from others?
   - What makes Authenty different from others is quite a few things, one of the most important of which is ***safety***. We use an incredibly powerful data encryption, which    would not require traffic to be sent through an insecure channel since it would not be possible for an attacker to understand the messages sent between the client and the server side (obviously, authorized recipients could do so). However, in our library or class, requests are sent through a secure channel and attackers will not even be able to see the encrypted message :).
   
### Some features:
- [x] Data is secured under **highly advanced encryption**
- [x] Encrypted SSL traffic
- [x] Creation of many licenses at once
- [x] Secure server-side variable control
- [x] Limit 1 PC per license
- [x] Advanced HWID control
- [x] Log System
- [x] Multiple managers / operators for your application with customized permissions
- [x] Many easy to use and understand APIs
- [x] Full control of users (change password, email, ban them from your application)
- [x] Full control of your application (you can pause your application from the panel, ensure its integrity, etc)
- [x] Simple auto-updater (under renovation)
- [x] Very easy integration
- [x] Friendly and easy to understand panel
- [x] Easy to understand [documentation](https://docs.authenty.me)
- [x] Continuous support and development
- [x] No need to configure the cloud or a database, just register in the panel and integrate authenty in your program
- [x] Uptime 99.9%


# Integration steps:
## First of all, you need to integrate our class or library (can be found from the panel = https://authenty.me/login) 

### 1. After you have integrated our authentication system, you must initialize the program by writing the following code:

```csharp
class Program
{
    
    public static Authenty exampleApp = new Authenty(APPName: "Application name", new Settings()
    {
        APPKey = "Application Key", // https://prnt.sc/vfgr2a
        APPID = "240639", // https://prnt.sc/vfgr6b
        Version = "1.0" // https://prnt.sc/vfgrbb
    });
                
    static void Main(string[] args)
    {
        // Securely connecting to our API with your application data
        exampleApp.Connect();                        
            
        if (!exampleApp.Verify()) // Simply a small check if the connection to the server was successful and is fully encrypted
            MessageBox.Show("Could not verify the connection to the server!", exampleApp.APPName(), MessageBoxButtons.OK, MessageBoxIcon.Error);
             
                   
        /*
             
         Or you can simply integrate everything in the same method
         Example: https://prnt.sc/vfgsdw
             
         */
    }
}
```

### 2. After having it integrated, you must choose between two direct modes of login after writing the license:
   - Entering directly with the license you created
   - Registering using a username along with a password and the license you created, after then the customer will only have to log in with the username and your password

Then, you already have the chosen mode, if you want to integrate the first one, follow these steps:
## 3. Mode: Log In with username & password
###    - (A). To log in to the program, you must place this in the code:
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

###    - (B). To register in the program (using the license), you must place this in the code:
```csharp
Console.Write(" Username: ");
string username = Console.ReadLine();

Console.Write(" Password: ");
string password = Console.ReadLine();

Console.Write(" License: ");
string license = Console.ReadLine();


if (exampleApp.Register(username, password, license))
{
    Console.WriteLine("You have been successfully registered!");
}
```

## 3. Mode: Only with license
```csharp
Console.Write(" License: ");
string license = Console.ReadLine();

if (exampleApp.licenseLogin(license))
{
    Console.WriteLine("Logged in!");
    // Your code or method where your program will go after being logged    
}
```

## 4. Extend subscription for temporary licenses
```csharp
Console.Write(" Username: ");
string username = Console.ReadLine();ç

Console.Write(" Password: ");
string password = Console.ReadLine();

Console.Write(" License: "); // Here is the new license that will extend the user's time
string license = Console.ReadLine();


if (exampleApp.ExtendTime(username, password, license))
{
    Console.WriteLine("Your user's time has been extended successfully!");
}
```

## 5. Control server-side variables
```csharp
Console.WriteLine(exampleApp.captureVariable("Secure variable code")); https://prnt.sc/uyxpkd
// Return the value of your variable
```

## 6. Logs
```csharp
exampleApp.Log("Logged in!"); // return bool
// Panel: https://prnt.sc/vfgw0u
```

## 7. APIs & Extra
- The documentation of the APIs can be found in our [complete documentation](https://docs.authenty.me) 


Web Page: https://authenty.me/

Documentation Page: https://docs.authenty.me/

Owner discord: biitez#1717 (quick answers)
