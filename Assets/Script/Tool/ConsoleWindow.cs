using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using UnityEngine;

namespace Thunder.Tool
{
    public class ConsoleWindow
    {
        private const int STD_OUTPUT_HANDLE = -11;
        private TextWriter _OldOutput;

        public void Initialize()
        {
            if (!AttachConsole(0x0ffffffff)) AllocConsole();

            _OldOutput = Console.Out;

            try
            {
                var stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                var safeFileHandle = new SafeFileHandle(stdHandle, true);
                var fileStream = new FileStream(safeFileHandle, FileAccess.Write);
                var encoding = Encoding.ASCII;
                var standardOutput = new StreamWriter(fileStream, encoding);
                standardOutput.AutoFlush = true;
                Console.SetOut(standardOutput);

                Application.logMessageReceived += Say;
            }
            catch (Exception e)
            {
                Debug.Log("Couldn't redirect output: " + e.Message);
            }
        }

        public void Say(string condition, string stackTrace, LogType type)
        {
            var sb = new StringBuilder();
            sb.Append("UnityEngine:>");
            sb.Append(type.ToString());
            sb.Append("\n");
            sb.Append(condition);
            sb.Append("\n");
            sb.Append(stackTrace);
            sb.Append("\n");

            Console.WriteLine(sb.ToString());
        }

        public void Shutdown()
        {
            Console.SetOut(_OldOutput);
            FreeConsole();
        }

        public void SetTitle(string strName)
        {
            SetConsoleTitle(strName);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleTitle(string lpConsoleTitle);
    }
}