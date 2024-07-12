using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


static public class Screen
{
    static public float width { get; set; }
    static public float height { get; set; }
}

static internal class Constants
{
    static public readonly string SENSOR_JSON_PATH = "C:/Noricube/Config";
    static private Dictionary<int, SSensorItem> _SensorConfigData = new Dictionary<int, SSensorItem>();
    static public Dictionary<int, SSensorItem> GetSensorConfigList()
    {
        return _SensorConfigData;
    }
    static public SSensorItem GetSensorConfig(int moniterIndex)
    {
        if (_SensorConfigData.ContainsKey(moniterIndex) == true)
        {
            return _SensorConfigData[moniterIndex];
        }
        return null;
    }
    static public bool RemoveAtSensorConfig(int moniterIndex)
    {
        if (_SensorConfigData.ContainsKey(moniterIndex) == true)
        {
            return _SensorConfigData.Remove(moniterIndex);
        }
        return false;
    }
    static public bool AddSensorConfig(int moniterIndex, SSensorItem config)
    {
        if (_SensorConfigData.ContainsKey(moniterIndex) == true)
        {
            _SensorConfigData[moniterIndex] = config;
        }
        else
        {
            _SensorConfigData.Add(moniterIndex, config);
        }
        return true;
    }


    static public bool SaveSensorConfig()
    {
        var configs = GetSensorConfigList();
        SSensorJson data = new SSensorJson()
        {
            sensor = configs.Values.ToArray()
        };

        JsonFile.Write<SSensorJson>(data, $"{SENSOR_JSON_PATH}/sensor.json");
        return true;
    }

    static public void LogMessage(string title, string msg)
    {
        Console.WriteLine($"[{title}] - {msg}");
    }
    static public void LogMessage(string msg)
    {
        Console.WriteLine($"    {msg}");
    }

}
