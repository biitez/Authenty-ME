#region usings
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;
#endregion

/*
 * © 2020, Authenty.ME
 * Thank you for choosing Authenty.ME's services!
 * 
 * This class belongs to Authenty.ME, you can edit and/or delete 
 * things from here with the only condition that all the changes 
 * you make will NOT affect the services of Authenty.ME, if you 
 * don't know how to improve it, we recommend you NOT to touch 
 * anything of this class.
 * 
 * support@authenty.me
 * https://discord.com/invite/UmJbzMd
 * 
 * .NET version API Example: 2.0.0
 * 
 */

internal class Authenty : authHelpers
{
    public static Auth auth = new Auth();
    public Authenty(string APPName, Settings settings)
    {
        Auth.appName = APPName;
        Auth.App_Key = settings.APPKey;
        Auth.App_Version = settings.Version;
        Auth.APPID = settings.APPID;
    }

    public Authenty APIConnect()
    {
        auth.Initialize();

        return this;
    }

    public bool Login(string username, string password)
    {
        if (!Auth.Started)
        {
            // documentation: https://docs.authenty.me/
            MessageBox.Show("Badly executed program, you must initialize the application before logging in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }


        controlVars.Handler.Add("password", password);
        return auth.Login(username, password);
    }

    public bool Register(string username, string password, string license)
    {
        if (!Auth.Started)
        {
            // documentation: https://docs.authenty.me/
            MessageBox.Show("Badly executed program, you must initialize the application before logging in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        return auth.Register(username, password, license);
    }

    public bool ExtendTime(string username, string password, string license)
    {
        if (!Auth.Started)
        {
            // documentation: https://docs.authenty.me/
            MessageBox.Show("Badly executed program, you must initialize the application before logging in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        return auth.ExtendTime(username, password, license);
    }

    public string captureVariable(string variable_code)
    {
        if (controlVars.Vars.ContainsKey(variable_code))
            return controlVars.Vars[variable_code];

        return auth.Variable_capture(variable_code);
    }

    public bool licenseLogin(string licenseKey)
    {
        if (!Auth.Started)
        {
            // documentation: https://docs.authenty.me
            MessageBox.Show("Badly executed program, you must initialize the application before logging in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }


        controlVars.Handler.Add("password", licenseKey);
        return auth.licenseLogin(licenseKey);
    }

    public bool Log(string message)
    {
        if (!Auth.Started)
        {
            // documentation: https://docs.authenty.me/
            MessageBox.Show("Badly executed program, you must initialize the application before logging in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
            return false;
        }
        else if (!Auth.isLogged)
        {
            MessageBox.Show("You can only send logs to the server when you are logged in!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else
        {
            if (auth.sendLog(message))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public bool Verify()
    {
        return Auth.Started;
    }

    public string APPName()
    {
        return Auth.appName;
    }
}

internal class Settings
{
    public string APPKey { get; set; }
    public string APPID { get; set; }
    public string Version { get; set; }
}

public class Info
{
    public static string Username { get; set; }
    public static string License { get; set; }
    public static string Level { get; set; }
    public static string Expires { get; set; }
    public static string IP { get; set; }
    public static string HWID { get; set; }
}

public class controlVars
{
    public static Dictionary<string, string> Handler = new Dictionary<string, string>();

    public static Dictionary<string, string> Vars = new Dictionary<string, string>();
}

public class App
{
    public static void configuration(Dictionary<string, object> data)
    {
        Version = (string)data["version"];
        updaterLink = (string)data["updater_link"] == null ? "Undefined" : (string)data["updater_link"];

        Enabled = (string)data["enabled"] == "1";
        HWIDLock = (string)data["hwidlock"] == "1";
        Freemode = (string)data["freemode"] == "1";
        devMode = (string)data["devmode"] == "1";
        hashcheck = (string)data["hashcheck"] == "1";
        enableUpdater = (string)data["optionalupdater"] == "1";

        if (hashcheck)
            hashProgram = (string)data["hash"] == null ? "Undefined" : (string)data["hash"];
    }

    public static string Version { get; set; }
    public static bool Freemode { get; set; }
    public static bool Enabled { get; set; }
    public static bool hashcheck { get; set; }
    public static string hashProgram { get; set; }
    public static bool devMode { get; set; }
    public static bool HWIDLock { get; set; }
    public static bool enableUpdater { get; set; }
    public static string updaterLink { get; set; }

}

internal class authHelpers
{
    public static X509Certificate2 RSAPubKey = new X509Certificate2(Convert.FromBase64String("MIID+zCCAuOgAwIBAgIUWVWtE8Tz0PlAoLLF1OXpaxGUDyUwDQYJKoZIhvcNAQEFBQAwgYwxCzAJBgNVBAYTAlVTMQswCQYDVQQIDAJVUzETMBEGA1UEBwwKQ2FsaWZvcm5pYTERMA8GA1UECgwIQXV0aGVudHkxETAPBgNVBAsMCEF1dGhlbnR5MREwDwYDVQQDDAhBdXRoZW50eTEiMCAGCSqGSIb3DQEJARYTc3VwcG9ydEBhdXRoZW50eS5tZTAeFw0yMDExMDQwMTMxNDJaFw0zMDExMDIwMTMxNDJaMIGMMQswCQYDVQQGEwJVUzELMAkGA1UECAwCVVMxEzARBgNVBAcMCkNhbGlmb3JuaWExETAPBgNVBAoMCEF1dGhlbnR5MREwDwYDVQQLDAhBdXRoZW50eTERMA8GA1UEAwwIQXV0aGVudHkxIjAgBgkqhkiG9w0BCQEWE3N1cHBvcnRAYXV0aGVudHkubWUwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDNapIj8qAnFKg0tqwb/IOOOc0m/uS8G0KVElglu1EhnnhSsZ7T+DwO19gXQ7+dH+7wAm65ljmjI2mcUV0zPiFNdyaPRrW1CWBS6GDZRJMfMdiI6oDyLDuoRIzn76nNGLTIkhX9WGNcbCQ0/nQPDBmbs6v5zd4hfWhMzpMvysEvVewi6aU3UX3bKU6aqg1WSmDd7hgVMee58i4VcJffCj5iFsoz6XEVgOSR/gLiPITE9u4o2REb0vZ5d/eVKSU4Avxt3IU0/1lBit7uciK5itHvv4Z8g9jtzndYxVSfb44X0YnD2fFcXXrCZOF/cW3dQhfBSbfjyHO+e7hLUtkXfp3TAgMBAAGjUzBRMB0GA1UdDgQWBBT5iE9CaoQ1YXLY/DfRXMgHZ82uETAfBgNVHSMEGDAWgBT5iE9CaoQ1YXLY/DfRXMgHZ82uETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBBQUAA4IBAQAd2ZrczkN91Z+klDWH01QZCDAFfUJkTDuNCHAO6g6S5IA31uSnZ324foUm//Y8Xd6yOh7zCXrFF71kAnZi3DCy1BVyMNS11USeQ2GlneIRVRp6+BcdT8+McLlGUsFsG+0cLI6nDKIHxqVjFZXQEhmk/0uV9/lsh5EFRxn+IRCZJugnd8S2ZXI6l9W9R0j0ypJEdiG5556a9bTVn71QhzGuE/+cbXoxd2meuztoZDTEZfN5K/7Gqae+J2TnT3thx/ywAqdL2eNNginmHln3N57VIApQFgi2KCdl3M5OynOuRltSPu0+xau824Mfoyw1s4yoIfPZjNbb3jfFhqImKy0k"));
    public static string certification_Key { get; } = "0427DA4C838F359E8433677BEAFEEF6A8B568E2A2E02031A22D20D7BAC6BB714095F7D59418DF40792CA7288AAEB4CD543BB8521DB240C6197DBB26417CB91FB61";
    public static string APIStartup = "https://api.authenty.me/";
}

public class Auth
{
    public static controlVars variables { get; }
    public static SecureConnections secure = new SecureConnections();
    public static Security GetSecurity = new Security();
    private static HttpControl httpControl = new HttpControl();
    private readonly RSACryptography RSACrypt = new RSACryptography();
    private readonly AESCryptography AESCrypt = new AESCryptography();

    
    public static bool SessionStarted { get; set; }
    public static bool Started { get; set; }
    public static bool APPInitialized { get; set; }
    public static string appName { get; set; }
    public static string API_Encrytion { get; set; }
    public static bool AIO { get; set; }
    public static string App_Key { get; set; }
    public static string App_Version { get; set; }
    public static string APPID { get; set; }

    public static bool isLogged { get; set; } = false;
    public static bool isRegistered { get; set; } = false;
    public static bool isExtended { get; set; } = false;
    public static bool logSent { get; set; }
    public static bool Task_Done { get; set; } = false;

    public void Initialize()
    {

        secure.Start_Session();

        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        EncryptConnection();

        secure.End_Session();

        app_Initialize();
    }

    private void EncryptConnection()
    {
        AESCrypt.GenerateRandomKeys();
        string result = httpControl.Post(authHelpers.APIStartup, "session_key=" + Utils.ToUrlSafeBase64(RSACrypt.Encrypt(AESCrypt.EncryptionKey)) + "&session_iv=" + Utils.ToUrlSafeBase64(RSACrypt.Encrypt(AESCrypt.EncryptionIV)));

        Started = AESCrypt.Decrypt(result) == "AES OK";
    }

    private void app_Initialize()
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?initialize", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, string>
            {
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;

            switch (GetSecurity.getMD5((string)result["status"]))
            {
                case "260CA9DD8A4577FC00B7BD5810298076": // success <3
                    App.configuration(result);

                    if (!App.Enabled)
                    {
                        MessageBox.Show("Program disabled by the administrator.", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }

                    if (App.enableUpdater)
                    {
                        if (App.Version != App.Version)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            MessageBox.Show("There is a new update found! | [" + App.Version + "]", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                            Console.ResetColor();
                            string page = !App.updaterLink.Contains("http") || !App.updaterLink.Contains("://") ? "https://" + App.updaterLink : App.updaterLink;
                            Process.Start(page);
                            Environment.Exit(0);
                        }
                    }

                    #region hash checker
                    if (App.hashcheck)
                    {
                        if (string.IsNullOrEmpty(App.hashProgram))
                        {
                            MessageBox.Show("No hashes found to check application integrity!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                        else if (GetSecurity.program_hash(Assembly.GetEntryAssembly().Location) != App.hashProgram && !App.devMode)
                        {
                            MessageBox.Show("The hash of this application does not match the hash uploaded to the server!, if you think this is a mistake, contact your developer.", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                        }
                        else if (GetSecurity.program_hash(Assembly.GetEntryAssembly().Location) != App.hashProgram && App.devMode)
                        {
                            File.AppendAllText("integrity.txt", GetSecurity.program_hash(Assembly.GetEntryAssembly().Location) + Environment.NewLine);
                            MessageBox.Show("The hash of this application was saved in the text : hash.txt !", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    #endregion

                    if (App.Freemode)
                    {
                        MessageBox.Show("Free mode activated!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    APPInitialized = true;
                    break;

                case "CB5E100E5A9A3E7F6D1FD97512215282": // error
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

            }
        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
    }


    internal bool Login(string username, string password)
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?login", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["username"] = username,
                ["password"] = password,
                ["hwid"] = GetSecurity.HWIDPC(),
                ["date"] = DateTime.Now.ToString(),
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;

            switch ((string)result["status"])
            {
                case "incorrect_hwid":
                    MessageBox.Show("The HWID is incorrect!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "hwid_reseted":
                    MessageBox.Show("Your HWID has been reset!, please login again.", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Environment.Exit(0);
                    break;

                case "incorrect_details":
                    MessageBox.Show("Invalid name or password!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "expired_time":
                    MessageBox.Show("Your subscription time has expired!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "banned_user":
                    MessageBox.Show("Your account is banned from this application!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":
                    Info.Username = (string)result["username"];
                    Info.HWID = (string)result["hwid"];
                    Info.License = (string)result["license"];
                    Info.Level = (string)result["level"];
                    Info.IP = GetSecurity.IP();
                    Info.Expires = (string)result["expires"];


                    if (Info.HWID != GetSecurity.HWIDPC() && App.HWIDLock)
                    {
                        MessageBox.Show("Your HWID does not match!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                    else
                    {
                        isLogged = true;
                        return true;
                    }                        

                    break;
            }
        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        return false;
    }


    internal bool Register(string username, string password, string license)
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?register", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["username"] = username,
                ["password"] = password,
                ["license"] = license,
                ["ip"] = GetSecurity.IP(),
                ["hwid"] = GetSecurity.HWIDPC(),
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;

            switch ((string)result["status"])
            {
                case "username_used":
                    MessageBox.Show("That username is already in use!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "invalid_license":
                    MessageBox.Show("Your license is invalid!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "error":
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":
                    return true;
            }
        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        return false;
    }


    internal bool ExtendTime(string username, string password, string license)
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?extend", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["username"] = username,
                ["password"] = password,
                ["license"] = license,
                ["hwid"] = GetSecurity.HWIDPC(),
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;

            switch ((string)result["status"])
            {
                case "incorrect_details":
                    MessageBox.Show("Invalid name or password!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "banned_user":
                    MessageBox.Show("Your account is banned from this application!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "incorrect_hwid":
                    MessageBox.Show("The HWID is incorrect!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "incorrect_license":
                    MessageBox.Show("The license entered does not exist!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "error":
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":
                    return true;
            }
        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        return false;
    }



    internal bool licenseLogin(string license)
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?license", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["license"] = license,
                ["hwid"] = GetSecurity.HWIDPC(),
                ["ip"] = GetSecurity.IP(),
                ["date"] = DateTime.Now.ToString(),
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;

            switch ((string)result["status"])
            {
                case "expired_time":
                    MessageBox.Show("Your subscription time has expired!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "incorrect_hwid":
                    MessageBox.Show("The HWID is incorrect!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "banned_user":
                    MessageBox.Show("Your account is banned from this application!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "incorrect_details":
                    MessageBox.Show("Invalid license key!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "error":
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":

                    Info.Username = (string)result["license"];
                    Info.HWID = (string)result["hwid"];
                    Info.License = (string)result["license"];
                    Info.Level = (string)result["level"];
                    Info.IP = (string)result["ip"];
                    Info.Expires = (string)result["expires"];


                    if (Info.HWID != GetSecurity.HWIDPC() && App.HWIDLock)
                    {
                        MessageBox.Show("Your HWID does not match!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                    else
                    {
                        isLogged = true;
                        return true;
                    }                        
                    break;
            }
        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        return false;
    }


    internal string Variable_capture(string var_code)
    {
        if (!isLogged)
        {
            MessageBox.Show("You must be logged in to capture variables!", appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        else if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?variable", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["action"] = "variable",
                ["username"] = Info.Username,
                ["password"] = controlVars.Handler["password"],
                ["hwid"] = GetSecurity.HWIDPC(),
                ["variable_code"] = var_code,
                ["application_key"] = App_Key,
                ["application_id"] = APPID                
            }))))) as Dictionary<string, object>;


            switch ((string)result["status"])
            {
                case "incorrect_details":
                    MessageBox.Show("Invalid name or password!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "error":
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":
                    if (!controlVars.Vars.ContainsKey((string)result["code"]))
                        controlVars.Vars.Add((string)result["code"], (string)result["var_value"]);

                    return controlVars.Vars.ContainsKey(var_code) ? controlVars.Vars[var_code] : "N/A";
            }


        }
        else
        {
            MessageBox.Show("The connection is not totally secure and encrypted!", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        return "N/A";
    }


    internal bool sendLog(string message)
    {
        if (Started)
        {
            Dictionary<string, object> result = new JavaScriptSerializer().DeserializeObject(AESCrypt.Decrypt(httpControl.Post(authHelpers.APIStartup + "?log", "data=" + AESCrypt.Encrypt(new JavaScriptSerializer().Serialize(new Dictionary<string, object>
            {
                ["username"] = Info.Username,
                ["hwid"] = GetSecurity.HWIDPC(),
                ["message"] = message,
                ["ip"] = GetSecurity.IP(),
                ["date"] = DateTime.Now.ToString(),
                ["application_key"] = App_Key,
                ["application_id"] = APPID
            }))))) as Dictionary<string, object>;
            
            switch ((string)result["status"])
            {
                case "error":
                    MessageBox.Show((string)result["info"], Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    break;

                case "success":
                    return true;
            }
        }

        return false;
    }
}

#region security application
public class Security
{
    #region ARP Poisoning
    public class ARPPoisoning
    {
        public static System.Threading.Timer timer { get; set; }
        public string lastGateway { get; set; }

        public void Start() // Credits to Outbuilt by ARP Poisoning
        {
            lastGateway = getGatewayMAC();
            timer = new System.Threading.Timer(_ => onCallback(), null, 5000, Timeout.Infinite);
        }

        public void onCallback()
        {
            timer.Dispose();
            if (getGatewayMAC() != lastGateway)
            {
                MessageBox.Show("Security breach detected!, Closing...", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
                return;
            }
        }

        public IPAddress getDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .FirstOrDefault(a => a != null);
        }

        public string GET_ARPTable()
        {
            var start = new ProcessStartInfo
            {
                FileName = @"C:\Windows\System32\arp.exe",
                Arguments = "-a",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(start))
            {
                if (process == null)
                    return string.Empty;

                using (StreamReader reader = process.StandardOutput)
                {
                    return reader.ReadToEnd();
                }

            }
        }

        public string getGatewayMAC()
        {
            return new Regex($@"({getDefaultGateway()} [\W]*) ([a-z0-9-]*)").Match(GET_ARPTable()).Groups[2].ToString();
        }

    }
    #endregion
    public string IP()
    {
        return new WebClient().DownloadString("http://icanhazip.com/");
    }

    public string HWIDPC()
    {
        return new HWIDUser().ID;
    }

    // Get the program hash
    public string program_hash(string file)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(file))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    // get the text hash
    public string getMD5(string text)
    {
        byte[] arrBytTarget;
        MD5 hash = new MD5CryptoServiceProvider();
        arrBytTarget = hash.ComputeHash(ASCIIEncoding.Default.GetBytes(text));
        return BitConverter.ToString(arrBytTarget).Replace("-", "");
    }


    internal class HWIDUser
    {
        public string ID { get; set; }
        public HWIDUser()
        {
            var volumenSerial = GetDiskId();
            var CPUID = GetCpuId();
            var WindowsID = GetWindowsId();
            ID = volumenSerial + CPUID + WindowsID;
        }


        private string GetDiskId()
        {
            string diskLetter = string.Empty;

            //Find first drive

            foreach (var compDrive in DriveInfo.GetDrives())
            {
                if (compDrive.IsReady)
                {
                    diskLetter = compDrive.RootDirectory.ToString();
                    break;
                }
            }

            if (!string.IsNullOrEmpty(diskLetter) && diskLetter.EndsWith(":\\"))
            {
                //C:\ -> C
                diskLetter = diskLetter.Substring(0, diskLetter.Length - 2);
            }
            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + diskLetter + @":""");
            disk.Get();

            var volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }



        [DllImport("user32", EntryPoint = "CallWindowProcW", CharSet = CharSet.Unicode, SetLastError = true,
        ExactSpelling = true)]
        private static extern IntPtr CallWindowProcW([In] byte[] bytes, IntPtr hWnd, int msg, [In, Out] byte[] wParam,
        IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool VirtualProtect([In] byte[] bytes, IntPtr size, int newProtect, out int oldProtect);

        const int PAGE_EXECUTE_READWRITE = 0x40;

        public static string GetCpuId()
        {
            var sn = new byte[8];

            return !ExecuteCode(ref sn) ? "ND" : string.Format("{0:X8}{1:X8}", BitConverter.ToUInt32(sn, 4), BitConverter.ToUInt32(sn, 0));
        }

        private static bool ExecuteCode(ref byte[] result)
        {
            /* The opcodes below implement a C function with the signature:
             * __stdcall CpuIdWindowProc(hWnd, Msg, wParam, lParam);
             * with wParam interpreted as an 8 byte unsigned character buffer.
             * */

            var isX64Process = IntPtr.Size == 8;
            byte[] code;

            if (isX64Process)
            {
                code = new byte[]
                {
                    0x53, /* push rbx */
                    0x48, 0xc7, 0xc0, 0x01, 0x00, 0x00, 0x00, /* mov rax, 0x1 */
                    0x0f, 0xa2, /* cpuid */
                    0x41, 0x89, 0x00, /* mov [r8], eax */
                    0x41, 0x89, 0x50, 0x04, /* mov [r8+0x4], edx */
                    0x5b, /* pop rbx */
                    0xc3, /* ret */
                };
            }
            else
            {
                code = new byte[]
                {
                    0x55, /* push ebp */
                    0x89, 0xe5, /* mov  ebp, esp */
                    0x57, /* push edi */
                    0x8b, 0x7d, 0x10, /* mov  edi, [ebp+0x10] */
                    0x6a, 0x01, /* push 0x1 */
                    0x58, /* pop  eax */
                    0x53, /* push ebx */
                    0x0f, 0xa2, /* cpuid    */
                    0x89, 0x07, /* mov  [edi], eax */
                    0x89, 0x57, 0x04, /* mov  [edi+0x4], edx */
                    0x5b, /* pop  ebx */
                    0x5f, /* pop  edi */
                    0x89, 0xec, /* mov  esp, ebp */
                    0x5d, /* pop  ebp */
                    0xc2, 0x10, 0x00, /* ret  0x10 */
                };
            }

            var ptr = new IntPtr(code.Length);

            if (!VirtualProtect(code, ptr, PAGE_EXECUTE_READWRITE, out _))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            ptr = new IntPtr(result.Length);
            return CallWindowProcW(code, IntPtr.Zero, 0, result, ptr) != IntPtr.Zero;

        }



        public static string GetWindowsId()
        {
            var windowsInfo = string.Empty;
            var managClass = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

            var managCollec = managClass.Get();

            var is64Bits = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));

            foreach (var o in managCollec)
            {
                var managObj = (ManagementObject)o;
                windowsInfo = managObj.Properties["Caption"].Value + Environment.UserName + (string)managObj.Properties["Version"].Value;
                break;
            }
            windowsInfo = windowsInfo.Replace(" ", "")
                .Replace("Windows", "")
                .Replace("windows", "");

            windowsInfo += (is64Bits) ? " 64bit" : " 32bit";

            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(windowsInfo))).Replace("-", "");
        }


    }

}

#endregion

public class SecureConnections : Auth
{
    #region secure sessions

    // this is to get a secure connection between the client -> server
    public void Start_Session()
    {
        if (SessionStarted)
        {
            MessageBox.Show("Session already started, closing for security reasons...", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
        else
        {
            string hosts_Path = Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\drivers\\etc\\hosts";

            if (File.Exists(hosts_Path))
            {
                using (StreamReader reader = new StreamReader(hosts_Path))
                {
                    if (reader.ReadToEnd().Contains("authenty.me"))
                    {
                        MessageBox.Show("DNS redirection found, closing the application...", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Environment.Exit(0);
                    }
                }
            }

            new Security.ARPPoisoning().Start();

            Auth.SessionStarted = true;
            ServicePointManager.ServerCertificateValidationCallback += pinPublicKey;
        }
    }

    public void End_Session()
    {
        if (!Auth.SessionStarted)
        {
            MessageBox.Show("The session hasn't started yet, closing for security reasons...", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        Auth.SessionStarted = false;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
    }

    private bool pinPublicKey(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        if (cert != null && cert.GetPublicKeyString() == authHelpers.certification_Key)
            return true;

        MessageBox.Show("Error when establishing a secure SSL connection with the server.", Auth.appName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(0);
        return false;
    }
    #endregion
}


internal class RSACryptography : authHelpers
{
    internal byte[] Encrypt(byte[] message)
    {
        var publicprovider = (RSACryptoServiceProvider)RSAPubKey.PublicKey.Key;
        return publicprovider.Encrypt(message, false);
    }
}

internal class HttpControl
{
    private static CookieContainer cookies;

    internal HttpControl()
    {
        cookies = new CookieContainer();
    }

    internal string Post(string url, string data)
    {
        try
        {
            var buffer = Encoding.ASCII.GetBytes(data);
            var request = (HttpWebRequest)WebRequest.Create(url);

            // Send request
            //request.Proxy = null;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = buffer.Length;
            request.CookieContainer = cookies;
            var postData = request.GetRequestStream();
            postData.Write(buffer, 0, buffer.Length);
            postData.Close();

            // Get and return response
            var response = (HttpWebResponse)request.GetResponse();
            var Answer = response.GetResponseStream();
            var answer = new StreamReader(Answer);
            return answer.ReadToEnd();
        }
        catch (Exception ex)
        {
            return "ERROR: " + ex.Message;
        }
    }
}

internal class Utils
{
    internal static string ToUrlSafeBase64(byte[] input)
    {
        return Convert.ToBase64String(input).Replace("+", "-").Replace("/", "_");
    }

    internal static byte[] FromUrlSafeBase64(string input)
    {
        return Convert.FromBase64String(input.Replace("-", "+").Replace("_", "/"));
    }
}

internal class AESCryptography
{
    internal byte[] EncryptionKey { get; }
    internal byte[] EncryptionIV { get; }

    internal AESCryptography()
    {
        EncryptionKey = new byte[256 / 8];
        EncryptionIV = new byte[128 / 8];

        GenerateRandomKeys();
    }

    internal void GenerateRandomKeys()
    {
        var random = new RNGCryptoServiceProvider();
        random.GetBytes(EncryptionKey);
        random.GetBytes(EncryptionIV);
    }

    internal string Encrypt(string plainText)
    {
        try
        {
            var aes = new RijndaelManaged
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                Key = EncryptionKey,
                IV = EncryptionIV
            };

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            var msEncrypt = new MemoryStream();
            var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            var swEncrypt = new StreamWriter(csEncrypt);

            swEncrypt.Write(plainText);

            swEncrypt.Close();
            csEncrypt.Close();
            aes.Clear();

            return Utils.ToUrlSafeBase64(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Problem trying to encrypt.", ex);
        }
    }

    internal string Decrypt(string cipherText)
    {
        try
        {
            var aes = new RijndaelManaged
            {
                Padding = PaddingMode.PKCS7,
                Mode = CipherMode.CBC,
                KeySize = 256,
                Key = EncryptionKey,
                IV = EncryptionIV
            };

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            var msDecrypt = new MemoryStream(Utils.FromUrlSafeBase64(cipherText));
            var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            var srDecrypt = new StreamReader(csDecrypt);

            string plaintext = srDecrypt.ReadToEnd();

            srDecrypt.Close();
            csDecrypt.Close();
            msDecrypt.Close();
            aes.Clear();

            return plaintext;
        }
        catch (Exception ex)
        {
            throw new CryptographicException("Problem trying to decrypt.", ex);
        }
    }
}