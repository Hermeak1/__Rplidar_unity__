using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using ConsoleApp1;

namespace RpSensor
{
    public class RpSensorManager : IDisposable
    {
        private static RpSensorManager instance = new RpSensorManager();
        public static RpSensorManager Instance { get { return instance; }  }

        private Dictionary<int, RpEachSensor> eachSensorDic = new Dictionary<int, RpEachSensor>();
        
        RpSensorManager()
        {
            InitEachSensor();
        }
        /// 유니티 PLAYER PREFS
        void InitEachSensor()
        {
            var sensorJsonData = JsonFile.Load<SSensorJson>($"{Constants.SENSOR_JSON_PATH}/sensor.json");
            foreach (var config in sensorJsonData.sensor)
            {
                Constants.AddSensorConfig(config.monitor, config);

                if (eachSensorDic.ContainsKey(config.monitor) == false)
                {
                    eachSensorDic.Add(config.monitor, new RpEachSensor());
                }
                eachSensorDic[config.monitor].Init(config);
            }
        }

        public RpEachSensor GetSensor(int idx)
        {
            if (idx < eachSensorDic.Count)
            {
                return eachSensorDic[idx];
            }
            return null;
        }
        public int GetSensorCount()
        {
            return eachSensorDic.Count;
        }


        public void FixedUpdate()
        {
            foreach(var sensor in eachSensorDic.Values)
            {
                sensor.AcceptChangedData();
            }
        }

        public void KeyInputEvent(ConsoleKey key)
        {
            if (key == ConsoleKey.M)
            {
                Constants.LogMessage("[KET-INPUT]", "Mouse Toggle!");
                foreach(var sensor in eachSensorDic.Values)
                {
                    sensor.ToggleMouseStop();
                }
            }
            if( key == ConsoleKey.S)
            {
                // App Exit
                Program.Exit();
            }else if(key == ConsoleKey.E)
            {
                // RPLidar Stop
                foreach (var sensor in eachSensorDic.Values)
                {
                    sensor.SetSensorAcitve(false);
                }
            }
            else if(key == ConsoleKey.R)
            {
                // RPLidar Start
                foreach (var sensor in eachSensorDic.Values)
                {
                    sensor.SetSensorAcitve(true);
                }
            }
        }
        
        public void Dispose()
        {
            foreach (var sensor in eachSensorDic.Values)
            {
                sensor.DestroySensor();
            }
            eachSensorDic.Clear();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}