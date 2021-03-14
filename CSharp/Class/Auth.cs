using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Security.Authentication;
using System.Diagnostics;
using System.Reflection;

namespace Authenty
{
    public class Globals
    {
        public const string EndPoint = "https://api.biitez.dev/";
        public const string IDCertificacionKey = "0457B6A98C2938730F3E0B10E0A1B6C897E3E3333DDA927B18F00D123A5C795346E6B7A972CA4B8EF97F8441D1281C283015AF6572B67148958062ED9708115D86";
    }

    public class Licensing
    {
        // You can customize the error messages towards the client, (obviously) do not change the error-code, only the message
        private readonly Dictionary<string, string> _errorCustomMessages = new Dictionary<string, string>()
        {
            { "INTERNAL_ERROR", "Internal error, if this continues, contact a support." }, // general
            { "FILTERS_DETECTION", "You are not allowed to enter this application." },
            { "INVALID_SESSION_OR_APPLICATION", "The application or session was not found, if this continues, contact a support." }, // general
            { "EMPTY_FIELDS", "You cannot leave blank spaces!" }, // general
            { "INVALID_LICENSE", "The license entered is invalid, if you think this is an error, contact a support." },
            { "ALREADY_USED", "The license entered has already been used." },
            { "ALREADY_USED_USERNAME_OR_EMAIL", "An account with the same name or email already exists!" },
            { "INVALID_USERNAME_OR_PASSWORD", "Invalid username or password." },
            { "INVALID_HWID", "Your HWID is wrong for this account!, if you think this is a mistake, contact a support." },
            { "EXPIRED_SUBSCRIPTION", "Your account subscription has expired!" },
            { "USER_BANNED", "Your account has been banned from this application. Reason:" },
            { "NO_LOGGED", "You need to be logged in to grab secure-remote variables from the server!" }, // variable error
            { "UNFOUND_VARIABLE", "Variable not found, closing application for security reasons ..." },
            { "BADLY_CONNECTION", "You must connect the application to the server before executing this method!" },
            { "APP_PAUSED", "This application is paused. For more information contact a support." },
        };

        private readonly AppSettings _applicationSettings;
        private AesCryptography _aesCryptography;
        private RsaCryptography _rsaCryptography;
        private RequestController _requestController;
        private HWIDEngine _HWIDEngine;
        public UserInfo UserInfo;

        private readonly Dictionary<string, string> _remoteVariables = new Dictionary<string, string>();
        internal static (bool success, string authorizationKey, string AppKey) CommunicationEstablished = (false, null, null);

        public Licensing(AppSettings applicationSettings) => _applicationSettings = applicationSettings;
        public Licensing Connect()
        {
            WebRequest.DefaultWebProxy = new WebProxy();

            if (_applicationSettings.ApplicationId == null || string.IsNullOrEmpty(_applicationSettings.ApplicationKey) || string.IsNullOrEmpty(_applicationSettings.RsaPubKey))
                throw new Exception("Badly configured application, you cannot leave empty spaces in the configuration!");

            try { _rsaCryptography = new RsaCryptography(new X509Certificate2(Convert.FromBase64String(_applicationSettings.RsaPubKey))); }
            catch { throw new Exception("Invalid RSA Public key, check that it is well written or contact to support."); }

            _HWIDEngine = new HWIDEngine();
            _aesCryptography = new AesCryptography();
            _requestController = new RequestController();
                
            var connectionRequest = _requestController.Post(new Dictionary<string, string>()
            {
                // Asymmetrically encrypted keys (They can only be decrypted using the private RSA key of your application, which is stored securely on our servers)

                { "session_key", BaseConverters.ToUrlSafeBase64(_rsaCryptography.Encrypt(_aesCryptography.EncryptionKey)) },
                { "session_iv", BaseConverters.ToUrlSafeBase64(_rsaCryptography.Encrypt(_aesCryptography.EncryptionIv)) }
            }, new Dictionary<string, string>()
            {
                { "Application-ID", _applicationSettings.ApplicationId.ToString() },
                { "HWID-PC", _HWIDEngine.ID },
                { "Application-Version", _applicationSettings.ApplicationVersion },
                { "Application-MD5", _aesCryptography.Encrypt(CalculateMD5File()) }
            });

            switch (connectionRequest.StatusCode)
            {
                case HttpStatusCode.OK:

                    var initializeResponse =
                        JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(connectionRequest.Content.ReadAsStringAsync().Result));

                    if (!initializeResponse.ApplicationEnabled)
                        throw new ApplicationException(_errorCustomMessages["APP_PAUSED"]);
                    else if (!initializeResponse.success)
                        throw new WebException("An unexpected error has occurred, contact support if this continues.");

                    CommunicationEstablished = (true, initializeResponse.authorizationKey, _aesCryptography.Encrypt(_applicationSettings.ApplicationKey));

                    return this;

                case HttpStatusCode.Unauthorized: throw new WebException(_errorCustomMessages["FILTERS_DETECTION"]);
                case HttpStatusCode.Forbidden: throw new WebException("This application has been banned for violating the Authenty.ME TOS, for more information, contact a support.");                
                case HttpStatusCode.NotFound: throw new WebException("This application could not be found");

                default: throw new WebException("An unexpected error has occurred, contact support if this continues.");
            }          
        }

        /// <summary>
        /// Log in to your application
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public bool Login(string username, string password)
        {
            if (!CommunicationEstablished.success)
                throw new WebException(_errorCustomMessages["BADLY_CONNECTION"]);

            var loginReq = _requestController.Post(new Dictionary<string, string>()
            {
                { "type", "login" },
                { "data", _aesCryptography.Encrypt(JsonControl.Serialize(new LoginInfo
                    {
                        username = username,
                        password = password
                    }))
                }
            });

            var loginResponse = JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(loginReq.Content.ReadAsStringAsync().Result));

            if (!loginResponse.success)
            {
                if (loginResponse.errorCode == "INVALID_USERNAME_OR_PASSWORD") return false;

                Console.CursorVisible = false;

                Console.Write(_errorCustomMessages[loginResponse.errorCode]);

                if (loginResponse.errorCode == "USER_BANNED")
                    Console.WriteLine($" {loginResponse.bannedReason}");

                Console.ReadLine();
                Environment.Exit(0);
            }

            UserInfo = new UserInfo
            {
                Email = loginResponse.email,
                Username = loginResponse.username,
                ExpireDate = loginResponse.expiredate,
                Level = loginResponse.level,
                HWID = loginResponse.hwid
            };

            if (!string.IsNullOrEmpty(loginResponse.updaterVersion) &&
                !string.IsNullOrEmpty(loginResponse.updaterLink))
            {
                Console.CursorVisible = false;

                Console.WriteLine("There is a new update found! | [" + loginResponse.updaterVersion + "]");
                OpenUrl(loginResponse.updaterLink);

                Thread.Sleep(Timeout.Infinite);
            }

            if (loginResponse.InvalidApplicationHash)
                throw new Exception("The hash of this application does not match the hash uploaded to the server!, if you think this is a mistake, contact your developer.");

            return true;
        }

        /// <summary>
        /// Register in the application
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="email">Email</param>
        /// <param name="license">License key</param>
        /// <returns></returns>
        public bool Register(string username, string password, string email, string license)
        {
            if (!CommunicationEstablished.success)
                throw new WebException(_errorCustomMessages["BADLY_CONNECTION"]);

            var registerReq = _requestController.Post(new Dictionary<string, string>()
            {
                { "type", "register" },
                { "data", _aesCryptography.Encrypt(JsonControl.Serialize(new RegisterInfo
                    {
                        username = username,
                        password = password,
                        email = email,
                        license = license
                    }))
                }
            });

            var registerResponse = JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(registerReq.Content.ReadAsStringAsync().Result));

            if (!registerResponse.success)
            {
                Console.CursorVisible = false;

                Console.WriteLine(_errorCustomMessages[registerResponse.errorCode]);

                Thread.Sleep(Timeout.Infinite);
            }

            UserInfo = new UserInfo
            {
                Email = registerResponse.email,
                Username = registerResponse.username,
                ExpireDate = registerResponse.expiredate,
                Level = registerResponse.level,
                HWID = registerResponse.hwid
            };

            if (!string.IsNullOrEmpty(registerResponse.updaterVersion) &&
                !string.IsNullOrEmpty(registerResponse.updaterLink))
            {
                Console.CursorVisible = false;

                Console.WriteLine("There is a new update found! | [" + registerResponse.updaterVersion + "]");
                OpenUrl(registerResponse.updaterLink);

                Thread.Sleep(-1);
                Environment.Exit(0);
            }

            if (registerResponse.InvalidApplicationHash)
                throw new Exception("The hash of this application does not match the hash uploaded to the server!, if you think this is a mistake, contact your developer.");

            return true;


        }

        /// <summary>
        /// Will add the expiration time of the license that you place in the time of the user's account
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="license">License-Key (The expiration time of this license will be set to the user)</param>
        /// <returns></returns>
        public bool ExtendSubscription(string username, string password, string license)
        {
            if (!CommunicationEstablished.success)
                throw new WebException(_errorCustomMessages["BADLY_CONNECTION"]);

            var extendSubReq = _requestController.Post(new Dictionary<string, string>()
            {
                { "type", "extendSubscription" },
                { "data", _aesCryptography.Encrypt(JsonControl.Serialize(new ExtendSubscriptionInfo
                    {
                        username = username,
                        password = password,
                        license = license // the expiration time of this license will be taken out and added to the user's expiration date
                    }))
                }
            });

            var extendSubResponse = JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(extendSubReq.Content.ReadAsStringAsync().Result));

            if (!extendSubResponse.success)
            {
                Console.CursorVisible = false;
                Console.WriteLine(_errorCustomMessages[extendSubResponse.errorCode]);

                Thread.Sleep(Timeout.Infinite);
            }
            
            return true;
        }

        /// <summary>
        /// Login only using the license key as parameter
        /// </summary>
        /// <param name="license">License</param>
        /// <returns></returns>
        public bool LicenseLogin(string license)
        {
            if (!CommunicationEstablished.success)
                throw new WebException(_errorCustomMessages["BADLY_CONNECTION"]);

            var licenseLoginReq = _requestController.Post(new Dictionary<string, string>()
            {
                { "type", "licenseLogin" },
                { "data", _aesCryptography.Encrypt(JsonControl.Serialize(new LicenseLoginInfo { license = license })) }
            });

            var licenseLoginResponse = JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(licenseLoginReq.Content.ReadAsStringAsync().Result));

            if (!licenseLoginResponse.success)
            {
                if (licenseLoginResponse.errorCode == "INVALID_LICENSE") return false;

                Console.CursorVisible = false;

                Console.Write(_errorCustomMessages[licenseLoginResponse.errorCode]);

                if (licenseLoginResponse.errorCode == "USER_BANNED")
                    Console.WriteLine($" {licenseLoginResponse.bannedReason}");

                Console.ReadLine();
                Environment.Exit(0);
            }

            UserInfo = new UserInfo
            {
                Email = licenseLoginResponse.email,
                Username = licenseLoginResponse.username,
                ExpireDate = licenseLoginResponse.expiredate,
                Level = licenseLoginResponse.level,
                HWID = licenseLoginResponse.hwid
            };

            if (!string.IsNullOrEmpty(licenseLoginResponse.updaterVersion) &&
                !string.IsNullOrEmpty(licenseLoginResponse.updaterLink))
            {
                Console.CursorVisible = false;

                Console.WriteLine("There is a new update found! | [" + licenseLoginResponse.updaterVersion + "]");
                OpenUrl(licenseLoginResponse.updaterLink);

                Thread.Sleep(-1);
                Environment.Exit(0);
            }

            if (licenseLoginResponse.InvalidApplicationHash)
                throw new Exception("The hash of this application does not match the hash uploaded to the server!, if you think this is a mistake, contact your developer.");

            return true;


        }


        /// <summary>
        /// Obtain the value of a server-side variable, this method can only be executed AFTER the user logs in to your application.
        /// </summary>
        /// <param name="variableCode">secret 10-digit code for the variable</param>
        /// <returns></returns>
        public string GetVariable(string variableCode)
        {
            if (!CommunicationEstablished.success)
                throw new WebException(_errorCustomMessages["BADLY_CONNECTION"]);

            var variableReq = _requestController.Post(new Dictionary<string, string>()
            {
                { "type", "variable" },
                { "data", _aesCryptography.Encrypt(JsonControl.Serialize(new VariableInfo { SecretCode = variableCode })) }
            });

            var variableResponse = JsonControl.Deserialize<GeneralResponses>(_aesCryptography.Decrypt(variableReq.Content.ReadAsStringAsync().Result));

            if (!variableResponse.success)
                throw new Exception(_errorCustomMessages[variableResponse.errorCode]);

            var value = _remoteVariables.FirstOrDefault(rvr => rvr.Key.Contains(variableCode)).Value;

            if (value != null)
                return value;

            if (string.IsNullOrEmpty(variableResponse.value))
                throw new Exception("An unexpected error has occurred, for more information contact support.");

            _remoteVariables.Add(variableCode, variableResponse.value);

            return variableResponse.value;
        }

        private void OpenUrl(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                try { Process.Start(url); }
                catch
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        url = url.Replace("&", "^&");
                        Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        Process.Start("xdg-open", url);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        Process.Start("open", url);
                    else
                        throw new WebException("Invalid updater page (HTTPs required), if this continue, please contact support.");
                }
            }
            else
                throw new WebException("Invalid updater page (HTTPs required), if this continue, please contact support.");
        }

        private string CalculateMD5File()
        {
            using (var md5Instance = MD5.Create())
            {
                using (var stream = File.OpenRead(Assembly.GetEntryAssembly().Location))
                {
                    var hashResult = md5Instance.ComputeHash(stream);
                    return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }

    public class GeneralResponses
    {
        public bool success { get; set; } = false;
        public string authorizationKey { get; set; } = null;
        public bool ApplicationEnabled { get; set; } = false;
        public string bannedReason { get; set; } = null;
        public string errorCode { get; set; } = null;
        public string value { get; set; } = null;
        public string email { get; set; } = null;
        public string username { get; set; } = null;
        public string expiredate { get; set; } = null;
        public int level { get; set; } = 1;
        public string hwid { get; set; } = null;

        public string updaterVersion { get; set; } = null;
        public string updaterLink { get; set; } = null;

        public bool InvalidApplicationHash { get; set; } = false;

    }

    public class RegisterInfo
    {
        public string username { get; set; } = null;
        public string password { get; set; } = null;
        public string email { get; set; } = null;
        public string license { get; set; } = null;
    }

    public class LoginInfo
    {
        public string username { get; set; } = null;
        public string password { get; set; } = null;
    }

    public class ExtendSubscriptionInfo
    {
        public string username { get; set; } = null;
        public string password { get; set; } = null;
        public string license { get; set; } = null;
    }

    public class VariableInfo { public string SecretCode { get; set; } = null; }
    public class LicenseLoginInfo { public string license { get; set; } = null; }

    public class UserInfo
    {
        public int Level { get; set; } = 1;
        public string Username { get; set; } = null;
        public string Email { get; set; } = null;
        public string ExpireDate { get; set; } = null;
        public string HWID { get; set; } = null;
    }

    public class AppSettings
    {
        /// <summary>
        /// The RSA public key for your application can be found at https://biitez.dev/services/authenty/application/{ID-Key}/settings
        /// </summary>
        public string RsaPubKey { get; set; } = null;

        /// <summary>
        /// Random MD5 Hash that identifies your application, you can find it in https://biitez.dev/services/authenty/
        /// </summary>
        public string ApplicationKey { get; set; } = null;

        /// <summary>
        /// The version of your application, it is only necessary if you are using the Auto-Updater
        /// </summary>
        public string ApplicationVersion { get; set; } = "1.0.0";

        /// <summary>
        /// The ID of your application, have 7 numerical characters, can be found in https://biitez.dev/services/authenty/
        /// </summary>
        public int? ApplicationId { get; set; } = null;


    }

    public class RequestController
    {
        private readonly CookieContainer _cookieContainer;
        public RequestController() => _cookieContainer = new CookieContainer();

        public HttpResponseMessage Post(Dictionary<string, string> info, Dictionary<string, string> headers = null)
        {
            try
            {

                using (var handler = new HttpClientHandler()
                {
                    CookieContainer = _cookieContainer,
                    ServerCertificateCustomValidationCallback = pinPublicKey,
                    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls
                })
                using (var httpClient = new HttpClient(handler))
                {
                    httpClient.BaseAddress = new Uri(Globals.EndPoint);

                    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "/authenty/csharp/")
                    {
                        Content = new FormUrlEncodedContent(info)
                    };

                    if (Licensing.CommunicationEstablished.success)
                    {
                        httpClient.DefaultRequestHeaders.Add("authorization-id", Licensing.CommunicationEstablished.authorizationKey);
                        httpClient.DefaultRequestHeaders.Add("application-key", Licensing.CommunicationEstablished.AppKey);
                    }

                    if (headers == null) return httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

                    foreach (var i in headers)
                        httpClient.DefaultRequestHeaders.Add(i.Key, i.Value);

                    return httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();
                }
            }
            catch { throw new HttpRequestException("An unexpected error occurred in the request to the server! Contact support if this continues."); }
        }

        private bool pinPublicKey(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            => cert != null && cert.GetPublicKeyString() == Globals.IDCertificacionKey;
    }


    public class AesCryptography
    {
        internal byte[] EncryptionKey { get; }
        internal byte[] EncryptionIv { get; }

        public AesCryptography()
        {
            EncryptionKey = new byte[256 / 8];
            EncryptionIv = new byte[128 / 8];

            using (var rnd = new RNGCryptoServiceProvider())
            {
                rnd.GetBytes(EncryptionKey);
                rnd.GetBytes(EncryptionIv);
            }
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
                    IV = EncryptionIv
                };

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                var msEncrypt = new MemoryStream();
                var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                var swEncrypt = new StreamWriter(csEncrypt);

                swEncrypt.Write(plainText);

                swEncrypt.Close();
                csEncrypt.Close();
                aes.Clear();

                return BaseConverters.ToUrlSafeBase64(msEncrypt.ToArray());
            }
            catch
            {
                throw new CryptographicException("A problem occurred when trying to encrypt the message to the server, if this continues, contact a support!");
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
                    IV = EncryptionIv
                };

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                var msDecrypt = new MemoryStream(BaseConverters.FromUrlSafeBase64(cipherText));
                var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                var srDecrypt = new StreamReader(csDecrypt);

                var plaintext = srDecrypt.ReadToEnd();

                srDecrypt.Close();
                csDecrypt.Close();
                msDecrypt.Close();
                aes.Clear();

                return plaintext;
            }
            catch
            {
                throw new CryptographicException("There was a problem decrypting the message from the server, if this continues, contact a support!");
            }
        }
    }

    public class RsaCryptography
    {
        private readonly X509Certificate2 _rsaPubKey;
        public RsaCryptography(X509Certificate2 rsaPubKey) => _rsaPubKey = rsaPubKey;

        public byte[] Encrypt(byte[] message)
        {
            var publicprovider = (RSACryptoServiceProvider)_rsaPubKey.PublicKey.Key;
            return publicprovider.Encrypt(message, false);
        }
    }

    internal static class BaseConverters
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

    public class JsonControl
    {

        /// <summary>
        /// Deserialize an from json string
        /// </summary>
        public static T Deserialize<T>(string body)
        {
            try
            {
                using (var stream = new MemoryStream())
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(body);
                    writer.Flush();
                    stream.Position = 0;
                    return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(stream);
                }
            }
            catch { throw new Exception("An error occurred in the deserialization of the JSON!"); }
        }

        /// <summary>
        /// Serialize an object to json
        /// </summary>
        public static string Serialize<T>(T item)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    new DataContractJsonSerializer(typeof(T)).WriteObject(ms, item);
                    return Encoding.Default.GetString(ms.ToArray());
                }
            }
            catch { throw new Exception("An error occurred in the serialization of the JSON!"); }
        }
    }
    
    public class HWIDEngine
    {
        public HWIDEngine() => ID = DiskID() + CPUID() + WindowsID();
        public string ID { get; set; } = null;


        private string WindowsID()
        {
            var windowsInfo = "";
            var managClass = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

            var managCollec = managClass.Get();

            var is64Bits = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));

            foreach (var o in managCollec)
            {
                var managObj = (ManagementObject)o;
                windowsInfo = managObj.Properties["Caption"].Value + Environment.UserName + (string)managObj.Properties["Version"].Value;
                break;
            }

            windowsInfo = windowsInfo.Replace(" ", "");
            windowsInfo = windowsInfo.Replace("Windows", "");
            windowsInfo = windowsInfo.Replace("windows", "");
            windowsInfo += is64Bits ? " 64bit" : " 32bit";

            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.Default.GetBytes(windowsInfo))).Replace("-", "");
        }

        private string DiskID()
        {
            var diskLetter = string.Empty;
            //Find first drive
            foreach (var compDrive in DriveInfo.GetDrives())
            {
                if (!compDrive.IsReady) continue;

                diskLetter = compDrive.RootDirectory.ToString();
                break;
            }
            if (!string.IsNullOrEmpty(diskLetter) && diskLetter.EndsWith(":\\", StringComparison.Ordinal))
                diskLetter = diskLetter.Substring(0, diskLetter.Length - 2);

            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + diskLetter + @":""");
            disk.Get();

            return disk["VolumeSerialNumber"].ToString();
        }

        [DllImport("user32", EntryPoint = "CallWindowProcW", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr CallWindowProcW([In] byte[] bytes, IntPtr hWnd, int msg, [In, Out] byte[] wParam, IntPtr lParam);


        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool VirtualProtect([In] byte[] bytes, IntPtr size, int newProtect, out int oldProtect);

        private const int PageExecuteReadwrite = 0x40;

        private string CPUID()
        {
            var sn = new byte[8];
            return !ExecuteCode(ref sn) ? "ND" : $"{BitConverter.ToUInt32(sn, 4):X8}{BitConverter.ToUInt32(sn, 0):X8}";
        }

        private bool ExecuteCode(ref byte[] result)
        {
            var code = IntPtr.Size == 8 ? new byte[] { 0x53, 0x48, 0xc7, 0xc0, 0x01, 0x00, 0x00, 0x00, 0x0f, 0xa2, 0x41, 0x89, 0x00, 0x41, 0x89, 0x50, 0x04, 0x5b, 0xc3 } : new byte[] { 0x55, 0x89, 0xe5, 0x57, 0x8b, 0x7d, 0x10, 0x6a, 0x01, 0x58, 0x53, 0x0f, 0xa2, 0x89, 0x07, 0x89, 0x57, 0x04, 0x5b, 0x5f, 0x89, 0xec, 0x5d, 0xc2, 0x10, 0x00 };

            var ptr = new IntPtr(code.Length);

            if (!VirtualProtect(code, ptr, PageExecuteReadwrite, out _))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            ptr = new IntPtr(result.Length);
            return CallWindowProcW(code, IntPtr.Zero, 0, result, ptr) != IntPtr.Zero;

        }
    }
}
<script>
            window.close();
        </script>
        