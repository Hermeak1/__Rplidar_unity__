using System;
using System.Threading;
using UnityEngine;

namespace RpSensor
{
    public class RpEachSensor
    {
        const int LIDAR_POINT = 720;

        SSensorItem sensor;

        struct SLidarData
        {
            public float a;
            public float d;
        }
        struct SLidarClickData
        {
            public float x;
            public float y;
            public long tick;
        }

        public int _isConnect;
        public bool _isStartMotor;
        public bool _isOnScan;
        public bool _isSetMotorPWM;


        private LidarData[] m_data;
        private SLidarData[] m_arsLidarData;

        private Thread m_thread;
        private bool m_datachanged = false;
        private bool is_sensor_inited = false;
        private bool is_stop_getdata = false;
        private bool is_stop_mouse_pos = true;

        public void Init(SSensorItem config)
        {
            sensor = config;
            SetRPLidar();
        }
        public void SetSensorAcitve(bool isActive)
        {
            Console.WriteLine("센서 모니터 : " + sensor.monitor);
            if (isActive == true)
            {
                _isStartMotor = RplidarBinding.StartMotor(sensor.monitor);
                _isOnScan = RplidarBinding.StartScan(sensor.monitor, "Sensitivity");
                _isSetMotorPWM = RplidarBinding.OnSetMotorPwm(sensor.monitor, sensor.pwm);

                if (_isOnScan == true && m_thread == null)
                {
                    m_thread = new Thread(GetData);
                    m_thread.Start();
                    is_sensor_inited = true;
                }

                Constants.LogMessage("SENSOR", $"START SENSOR");
            }
            else
            {
                RplidarBinding.EndScan(sensor.monitor);
                RplidarBinding.EndMotor(sensor.monitor); 
                Constants.LogMessage("\n");
                Constants.LogMessage("SENSOR", $"STOP SENSOR");
            }
        }
        void SetRPLidar()
        {
            m_data = new LidarData[LIDAR_POINT];
            m_arsLidarData = new SLidarData[LIDAR_POINT];

            _isConnect = RplidarBinding.OnConnect(sensor.monitor, sensor.port, 256000);
            SetSensorAcitve(true);
            //_isStartMotor = RplidarBinding.StartMotor(sensor.monitor);
            //_isOnScan = RplidarBinding.StartScan(sensor.monitor, "Sensitivity");
            //_isSetMotorPWM = RplidarBinding.OnSetMotorPwm(sensor.monitor, sensor.pwm);

            Constants.LogMessage("CONFIG", $"IS CONNECT:{_isConnect}, IS START MOTOR:{_isStartMotor}, SCAN MODE: Sensitiviy, Is ON SCAN: {_isOnScan}, IS SET MOTOR PWM: {_isSetMotorPWM}");
            Constants.LogMessage("CONFIG", $"MARGIN LEFT: {sensor.margin_left}, RIGHT: {sensor.margin_right} TOP: {sensor.margin_top}, BOTTOM: {sensor.margin_bottom}");

            //if (_isOnScan == true)
            //{
            //    m_thread = new Thread(GetData);
            //    m_thread.Start();
            //    is_sensor_inited = true;
            //}
        }
        bool IsSimilarPosition(int idx)
        {
            foreach (SLidarData s in m_arsLidarData)
            {
                /*
                 * 같은 점이 계속하여 클릭되는 현상을 막기 위한 로직임.
                 * 이전점과 현재점의 위치를 비교하여, 위치가 비슷하면 그 아래 로직은 수행하지 않도록 하는 것
                 */

                if (sensor.sensibility < Math.Abs(m_data[idx].theta - s.a)) // 센시빌리티가 작을수록 한곳에 점이 계속 찍히는 것을 허용하지 않을 것이다 라는 뜻
                    continue;
                if (sensor.sensibility < Math.Abs(m_data[idx].distant - s.d))
                    continue;

                return true;
            }

            return false;
        }

        public void AcceptChangedData()
        {
            if (is_sensor_inited == false)
                return;

            if (m_datachanged)
            {
                for (int i = 0; i < LIDAR_POINT; i++)
                {
                    if (IsSimilarPosition(i) == true)
                        continue;

                    if (0 == m_data[i].distant)
                        continue;
                                        
                    Vector3 v3Position = UnityQuaternion.Euler(0, 0, m_data[i].theta) * Vector3.right * m_data[i].distant;
                    v3Position = UnityQuaternion.Euler(0, 0, sensor.rotation) * v3Position;

                    float fNormalizedXFactor = sensor.normalize.r - sensor.normalize.l;
                    float fNormalizedYFactor = sensor.normalize.b - sensor.normalize.t;

                    float fXRatio = .5f + (v3Position.x + sensor.normalize.l) / fNormalizedXFactor;
                    float fYRatio = .5f + (v3Position.y - sensor.normalize.t) / fNormalizedYFactor;

                    v3Position.x = fXRatio * Screen.width;
                    v3Position.y = fYRatio * Screen.height;
                    v3Position.x *= -1;

                    SetMousePos(v3Position);
                }

                for (int i = 0; i < LIDAR_POINT; i++)
                {
                    m_arsLidarData[i].a = m_data[i].theta;
                    m_arsLidarData[i].d = m_data[i].distant;
                }

                m_datachanged = false;
            }
        }

        public void ToggleMouseStop()
        {
            Constants.LogMessage($"MOUSE INDEX:{sensor.monitor}, STATE: {is_stop_mouse_pos}");
            is_stop_mouse_pos = !is_stop_mouse_pos;
        }

        void SetMousePos(Vector3 v3)
        {
            if (sensor.margin_left * Screen.width > v3.x || v3.x > Screen.width * (1 - sensor.margin_right))
            {
                return;
            }

            if (sensor.margin_bottom * Screen.height > v3.y || v3.y > (1 - sensor.margin_top) * Screen.height)
            {
                return;
            }

            v3.y = Math.Abs(Screen.height - v3.y);
            if(((v3.x > Screen.width - 50) || v3.x < 50 ) || ((v3.y > (Screen.height-50)) || v3.y < 50) )
            {
                return;
            }
            v3.x += (Screen.width * sensor.screenfactor);

            //Constants.LogMessage($"물체 인식: X: {v3.x}, Y:{v3.y}");

            
            RpMouseOperations.SetCursorPosition((int)v3.x, (int)v3.y);
            
            RpMouseOperations.MouseEvent(RpMouseOperations.MouseEventFlags.LeftUp | RpMouseOperations.MouseEventFlags.LeftDown);
            
        }

        void GetData()
        {
            while (true)
            {
                if (is_stop_getdata == true)
                    break;

                int datacount = RplidarBinding.GetData(sensor.monitor, ref m_data);

                if (datacount == 0)
                {
                    Thread.Sleep(20);
                }
                else
                {
                    m_datachanged = true;
                }
                Thread.Sleep(1);
            }
        }
        public void DestroySensor()
        {
            //m_thread.Abort(); // Flag 를 false 로 해서 while 을 탈출시켜주도록, Abort 사용하지 않음
            is_stop_getdata = true;

            RplidarBinding.EndScan(sensor.monitor);
            RplidarBinding.EndMotor(sensor.monitor);
            RplidarBinding.OnDisconnect(sensor.monitor); // 이 함수 안에 이미 EndScan, EndMotor 수행함.
            RplidarBinding.ReleaseDrive(sensor.monitor); // 여기서 최종적으로 lidar_drv[idx] = nullptr; 해줌
        }
    }
}