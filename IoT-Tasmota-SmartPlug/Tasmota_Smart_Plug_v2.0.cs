
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
//using Crestron.SimplSharp.Net.Https;
//using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronXml;
using Newtonsoft.Json;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace IoT_Tasmota_SmartPlug
{
    public class Tasmota_Smart_Plug
    {
        private string Password { get; set; }
        private string Username { get; set; }
        private string IpAddress { get; set; }
        //private HttpClientRequest httpRequest { get; set; }
        private HttpClient myHttpClient { get; set; }
        //private HttpClientResponse httpResponse;
        public Int16 PowerOnState { get; set; }
        public Int16 PowerOffState { get; set; }
        public string DeviceName { get; set; }
        private bool IsInitialized { get; set; }
        //private SimplSharpProConsoleCmdFunction SimplSharpProConsoleCmdFunction;
        public int DebugPrintLine { get; set; }
        public Int16 NotInitialized { get; set; }


        public Tasmota_Smart_Plug()
        {
            myHttpClient = new HttpClient();
            NotInitialized = 0;
        }

        public void Initialize(SimplSharpString username, SimplSharpString password, SimplSharpString ipAddress)
        {
            IpAddress = ipAddress.ToString();
            Password = password.ToString();
            Username = username.ToString();

            
            IsInitialized = true;
            NotInitialized = 1;
            Get_Status();
        }


        public void Turn_On()
        {
           if(IsInitialized.Equals(true))
            {
                Send_Command("Power On");
            }
           else
            {
                Alert_Module_Not_Initialized();
            }
            
        }

        public void Turn_Off()
        {
            if(IsInitialized.Equals(true))
            {
                Send_Command("Power Off");
            }
            else
            {
                Alert_Module_Not_Initialized();
            }

        }

        public void Power_Toggle()
        { 
            if(IsInitialized.Equals(true))
            {
                Send_Command("Power Toggle");
            }
            else
            {
                Alert_Module_Not_Initialized();
            }

        }

        public void Get_Status()
        {
           if(IsInitialized.Equals(true))
            {
                Send_Command("status");
            }
            else
            {
                Alert_Module_Not_Initialized();
            }

        }

        private void Send_Command(string command)
        {
            string url;

            if(string.IsNullOrWhiteSpace(Password))
            {
                url = $"http://{IpAddress}/cm?cmnd={command}";
            }
            else if(!string.IsNullOrWhiteSpace(Password))
            {
                url = $"http://{IpAddress}/cm?user={Username}&password={Password}&cmnd={command}";
            }
            else
            {
                url = $"http://{IpAddress}/cm?cmnd={command}";

                if (DebugPrintLine.Equals(1))
                {
                    CrestronConsole.PrintLine($"Check password syntax for module at {IpAddress} - Sending URL without username and password");
                }
            }

            if (DebugPrintLine.Equals(1))
            {
                CrestronConsole.PrintLine($"--- Sending  URL {url} for smartplug {IpAddress} ---");
            }

            using (HttpResponseMessage responseMessage = myHttpClient.GetAsync(url).Result)
            {
                if (responseMessage.IsSuccessStatusCode)
                {
                    if (command.Contains("status"))
                    {
                        string Data = responseMessage.Content.ReadAsStringAsync().Result;
                        Root root = JsonConvert.DeserializeObject<Root>(Data);
                        //return root;
                        Parse_Status_Response(root);
                    }else
                    {
                        string Data = responseMessage.Content.ReadAsStringAsync().Result;
                        Status status = JsonConvert.DeserializeObject<Status>(Data);
                        //return status;
                        Parse_Power_Response(status);
                    }

                }else
                {
                    CrestronConsole.PrintLine($" URL request for SmartPlug at {IpAddress} was not successful, Error Code = {(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
                    //return null;
                }
            }

        }

        private void Parse_Power_Response(Status s)
        {
            string powerStatusGive = s.POWER.ToLower();

            if (DebugPrintLine.Equals(0))
            {
                CrestronConsole.PrintLine($"--- The {DeviceName} SmartPlug has a Power State Value of {s.POWER} from the Power ON/OFF/TOGGLE command ---");
            }

            if (powerStatusGive.Equals("on"))
            {
                PowerOffState = 0;
                PowerOnState = 1;
            }
            else if (powerStatusGive.Equals("off"))
            {
                PowerOnState = 0;
                PowerOffState = 1;
            }

        }


        private void Parse_Status_Response(Root r)
        {

            int powerStatusGet = r.Status.Power;

            if (DebugPrintLine.Equals(1))
            { 
                CrestronConsole.PrintLine($"--- The {r.Status.DeviceName} SmartPlug has a Power State Value of {r.Status.Power} from the Get_Status command ---");
            }
            if (powerStatusGet.Equals(1))
            {
                PowerOffState = 0;
                PowerOnState = 1;
            }
            else if (powerStatusGet.Equals(0))
            {
                PowerOnState = 0;
                PowerOffState = 1;
            }

            DeviceName = r.Status.DeviceName;
        }


        private void Alert_Module_Not_Initialized()
        {
            CrestronConsole.PrintLine($"--- The SmartPlug Module using the Tasmota Module has not been initialized. Please trigger the initialize digital input before using module. ---");
            NotInitialized = 0;

        }

        public void Debug_Vlaue(int i)
        {
            DebugPrintLine = i;
        }

    }
}
