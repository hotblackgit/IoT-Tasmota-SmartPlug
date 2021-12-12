
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.Net.Http;
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
        private HttpClientRequest httpRequest { get; set; }
        private HttpClient httpClient { get; set; }
        //private HttpClientResponse httpResponse;
        public Int16 PowerOnState { get; set; }
        public Int16 PowerOffState { get; set; }
        public string DeviceName { get; set; }
        private bool IsInitialized { get; set; }


        public Tasmota_Smart_Plug()
        {
            httpClient = new HttpClient();
            httpRequest = new HttpClientRequest();
            //HttpClientResponse httpResponse = new HttpClientResponse();
                
        }

        public void Initialize(SimplSharpString username, SimplSharpString password, SimplSharpString ipAddress)
        {
            IpAddress = ipAddress.ToString();
            Password = password.ToString();
            Username = username.ToString();

            //TasmotaClient.AuthenticationMethod = AuthMethod.BASIC;
            httpClient.UserName = username.ToString();
            httpClient.Password = password.ToString();

            //TasmotaClient.PeerVerification = false;
            //TasmotaClient.HostVerification = false;
            //bool b = string.IsNullOrEmpty(Password);
            Get_Status();
            IsInitialized = true;
            //return IsInitialized;
        }


        public void Turn_On()
        {
           if(IsInitialized.Equals(true))
            {
                HttpClientResponse httpClientResponse = Send_Command("Power On");
                Parse_Power_Response(httpClientResponse);
            }
           else
            {
                Call_Module_Not_Initialized_Message();
            }
            
        }

        public void Turn_Off()
        {
            if(IsInitialized.Equals(true))
            {
                HttpClientResponse httpClientResponse = Send_Command("Power Off");
                Parse_Power_Response(httpClientResponse);
            }
            else
            {
                Call_Module_Not_Initialized_Message();
            }

        }

        public void Power_Toggle()
        { 
            if(IsInitialized.Equals(true))
            {
                HttpClientResponse httpClientResponse = Send_Command("Power Toggle");
                Parse_Power_Response(httpClientResponse);
            }
            else
            {
                Call_Module_Not_Initialized_Message();
            }

        }

        public void Get_Status()
        {
           if(IsInitialized.Equals(true))
            {
                HttpClientResponse httpClientResponse = Send_Command("status");
                Parse_Status_Response(httpClientResponse);
            }
            else
            {
                Call_Module_Not_Initialized_Message();
            }

        }

        private HttpClientResponse Send_Command(string command)
        {
            httpClient.KeepAlive = false;
            httpRequest.RequestType = Crestron.SimplSharp.Net.Http.RequestType.Get;
            string url;

            if(string.IsNullOrEmpty(Password).Equals(true))
            {
                url = $"http://{IpAddress}/cm?cmnd={command}";
            }
            else if(string.IsNullOrEmpty(Password).Equals(false) & string.IsNullOrWhiteSpace(Password).Equals(false))
            {
                url = $"http://{IpAddress}/cm?user={Username}&password={Password}&cmnd={command}";
            }
            else
            {
                url = $"http://{IpAddress}/cm?cmnd={command}";
            }
            
            
            

            try
            {
                httpRequest.Url = new UrlParser(String.Format(url));
                CrestronConsole.PrintLine($"Trying to send command -- {command} -- the Smart Plug at {IpAddress} -- Using Url = {url}");
                //httpClient.Get(httpRequest.Url.ToString());
                //if (httpRequest.IsCompleted.Equals(true))
                //{
                    HttpClientResponse httpResponse = httpClient.Dispatch(httpRequest);
                    return httpResponse;
                //}
                //else
                //{
                //    return null;
                //}
                //Root root = JsonConvert.DeserializeObject<Root>(httpResponse.ContentString);
                //Parse_Response(httpResponse);
                
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine($"---Tasmota Module for {IpAddress} --------An Exception Thrown While Trying To send command {command} --------- : {e} --- {e.Message} --- {e.InnerException}");
                return null;
            }
            finally
            {

            }

        }

        private void Parse_Power_Response(HttpClientResponse r)
        {
            //CrestronConsole.PrintLine("HttpClientResponse = {0}", r.ContentString);


            //Root root = JsonConvert.DeserializeObject<Root>(r.ContentString);
            Status status = JsonConvert.DeserializeObject<Status>(r.ContentString);

            string powerStatusGive = status.POWER.ToLower();

            CrestronConsole.PrintLine($"--- The {DeviceName} SmartPlug has a Power State Value of {status.POWER} from the Power ON/OFF/TOGGLE command ---");


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


        private void Parse_Status_Response(HttpClientResponse r)
        {
            //CrestronConsole.PrintLine("HttpClientResponse = {0}", r.ContentString);


            Root root = JsonConvert.DeserializeObject<Root>(r.ContentString);
            int powerStatusGet = root.Status.Power;


            CrestronConsole.PrintLine($"--- The {root.Status.DeviceName} SmartPlug has a Power State Value of {root.Status.Power} from the Get_Status command ---");

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

            DeviceName = root.Status.DeviceName;
        }


        private void Call_Module_Not_Initialized_Message()
        {
            CrestronConsole.PrintLine($"--- The SmartPlug Module using the Tasmota Module has not been initialized. Please trigger the initialize digital input before using module. ---");

        }

    }
}
