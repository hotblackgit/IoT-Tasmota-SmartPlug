using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT_Tasmota_SmartPlug
{
    public class Status
    {
        public int Module { get; set; }
        public string DeviceName { get; set; }
        public List<string> FriendlyName { get; set; }
        public string Topic { get; set; }
        public string ButtonTopic { get; set; }
        //[JsonProperty("powerStatus")]
        public int Power { get; set; }
        public String POWER { get; set; }
        public int PowerOnState { get; set; }
        public int LedState { get; set; }
        public string LedMask { get; set; }
        public int SaveData { get; set; }
        public int SaveState { get; set; }
        public string SwitchTopic { get; set; }
        public List<int> SwitchMode { get; set; }
        public int ButtonRetain { get; set; }
        public int SwitchRetain { get; set; }
        public int SensorRetain { get; set; }
        public int PowerRetain { get; set; }
    }

    public class Root
    {
        public Status Status { get; set; }
    }


}
