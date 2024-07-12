using RpSensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Security;
using System.Numerics;
using System.Windows.Forms;
using System.Drawing;
//using UnityEngine;

namespace ConsoleApp1
{
    public class Program
    {
#if false // RpMouseOperation에서 구현되어있다.
        [DllImport("user32.dll")]
        static extern void mouse_event(int flag, int dx, int dy, int buttons, int extra);
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point point);
        [DllImport("user32.dll")]
        static extern int SetCursorPos(int x, int y);
        [DllImport("User32.dll")]
        static extern void keybd_event(byte vk, byte scan, int flags, int extra);

        /// <summary>
        /// 키 누름(DOWN) 이벤트를 발생시키는 메서드
        /// </summary>
        public static void KeyDown(int keycode)
        {
            keybd_event((byte)keycode, 0, (int)KeyFlag.KE_DOWN, 0);
        }
        /// <summary>
        /// 키 뗌(UP) 이벤트를 발생시키는 메서드
        /// </summary>
        public static void KeyUp(int keycode)
        {
            keybd_event((byte)keycode, 0, (int)KeyFlag.KE_UP, 0);
        }
        /// <summary>
        /// 마우스 좌표를 바꾸는 메서드
        /// </summary>
        public static void Move(int x, int y)
        {
            SetCursorPos(x, y);
        }
        /// <summary>
        /// 마우스 좌표를 바꾸는 메서드
        /// </summary>
        public static void Move(Point pt)
        {
            Move((int)pt.X, (int)pt.Y);
        }
        /// <summary>
        /// 프로그램 방식으로 마우스 왼쪽 버튼 누름 이벤트 발생시키는 메서드
        /// </summary>
        public static void LeftDown()
        {
            mouse_event((int)MouseFlag.ME_LEFTDOWN, 0, 0, 0, 0);
        }
        /// <summary>
        /// 프로그램 방식으로 마우스 왼쪽 버튼 뗌 이벤트 발생시키는 메서드
        /// </summary>
        public static void LeftUp()
        {
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);
        }
#endif

        /////////////////////////////////////////////////////////////////////////////////////////////
        #region 윈도우종료 버튼 비활성 처리하기 위한 DLL
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr windowHandle, bool revert);
        [DllImport("user32.dll")]
        private static extern bool EnableMenuItem(IntPtr menuHandle, uint menuItemID, uint enabled);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const uint SC_CLOSE = 0xf060;
        private const uint MF_ENABLED = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        #endregion
        /////////////////////////////////////////////////////////////////////////////////////////////
        #region 트레이 아이콘 사용하기 위한 변수
        public static ContextMenu menu;
        public static MenuItem mnuExit = null;
        public static NotifyIcon notificationIcon;
        #endregion

        /////////////////////////////////////////////////////////////////////////////
        ///
        static public bool isApplicitonQuit = false;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Screen.width = 1920;
            Screen.height = 1200;

            //NotifyIcon trayIcon = new NotifyIcon();

            var handl = GetConsoleWindow();
            EnableMenuItem(GetSystemMenu(handl, false), SC_CLOSE, MF_GRAYED);
            ShowWindow(handl, SW_SHOW);

            // 라이다 매니저 생성
            while (RpSensorManager.Instance == null)
            {
                Thread.Sleep(1);
            }

            Thread notifyThread = new Thread(delegate ()
            {
                menu = new System.Windows.Forms.ContextMenu();
                menu.MenuItems.Add(0, new MenuItem("Exit"));
                menu.MenuItems.Add(1, new MenuItem("Show"));

                notificationIcon = new NotifyIcon()
                {
                    Icon = new Icon(SystemIcons.Application, 40, 40),
                    ContextMenu = menu,
                    Text = "GAME SERVER"
                };
                menu.MenuItems[0].Click += new EventHandler(mnuExit_Click);
                menu.MenuItems[1].Click += new EventHandler(mnuShow_Click);
                notificationIcon.Visible = true;


                Task.Factory.StartNew(Run);
                Application.Run();
            });
            notifyThread.Start();





            //// 키 인풋 테스트
            //do
            //{
            //    var keyinfo = Console.ReadKey(true);
            //    // 마우스 사용 유.무
            //    RpSensorManager.Instance.KeyInputEvent(keyinfo.Key);

            //} while (!isApplicitonQuit);
        }


        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Exit();
        }

        static public void Exit()
        {
            var cnt = RpSensorManager.Instance.GetSensorCount();
            for (int i = 0; i < cnt; i++)
            {
                var sensor = RpSensorManager.Instance.GetSensor(i);
                if (sensor != null)
                {
                    sensor.DestroySensor();
                }
            }
            isApplicitonQuit = true;
            Constants.LogMessage("RPLIDAR", "SENSOR STOP");
        }

        static void Run()
        {
            // 라이다 체크 업데이트
            Update();
            do
            {
                var keyinfo = Console.ReadKey(true);
                // 마우스 사용 유.무
                RpSensorManager.Instance.KeyInputEvent(keyinfo.Key);

            } while (!isApplicitonQuit);
        }
        /////////////////////////////////////////////////////////////////////////////
        ///
        static public void Update()
        {
            Task.Run(async () =>
            {
                Constants.LogMessage("RPLIDAR THREAD", "MAIN THREAD START");
                DateTime start = DateTime.Now;
                int cnt = 0;
                do
                {
#if _TEST_
                    if ((DateTime.Now - start).Ticks > 10000000)
                    {
                        Constants.LogMessage("[TICK]", $"FRAME: {cnt}");
                        cnt = 0;
                        start = DateTime.Now;
                    }
                    cnt++;
#endif
                    RpSensorManager.Instance.FixedUpdate();

                    await Task.Delay(10);
                } while (!isApplicitonQuit);

                Constants.LogMessage("PRLIDAR THREAD", "MAIN THREAD END");
                Environment.Exit(0);
            });
        }

        static void mnuExit_Click(object sender, EventArgs e = null)
        {
            Exit();
        }
        static void mnuShow_Click(object sender, EventArgs e)
        {
            var handl = GetConsoleWindow();
            ShowWindow(handl, SW_SHOW);
        }
    }
}
