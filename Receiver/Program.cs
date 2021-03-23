using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Receiver
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2) return;

            String ip = args[0];
            String port = args[1];

            new Program(ip, port);
        }

        public Program(String _ip, String port)
        {
            IPAddress ip = _ip.Equals("*") ? IPAddress.Any : IPAddress.Parse(_ip);

            IPEndPoint ipep = new IPEndPoint(ip, Int32.Parse(port));
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(20);

            WriteLog(String.Format("[{0}] Server start, Listen IP : {1}, Port {2}",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ip, port));
            while(true)
            {
                Socket client = server.Accept();
                IPEndPoint clientIp = (IPEndPoint)client.RemoteEndPoint;

                WriteLog(String.Format("[{0}] IP {1} 에서 접속",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), clientIp.Address));

                client.Send(Encoding.Default.GetBytes("connect"));

                Byte[] _data = new Byte[1024];
                client.Receive(_data);
                String target = Encoding.Default.GetString(_data).TrimEnd('\0');

                WriteLog(String.Format("[{0}] {1} 프로그램 실행",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), target));

                client.Send(StartProcess(target) ? Encoding.Default.GetBytes("실행 성공") : 
                    Encoding.Default.GetBytes("실행 실패") );

                client.Close();
            }

            server.Close();
        }

        public String WriteLog(String msg)
        {
            String result = "로그 작성";
            String folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            String name = DateTime.Now.ToString("yyyyMMdd");
            String pathString = Path.Combine(folder, name);

            try
            {
                if (!File.Exists(pathString))
                {
                    File.CreateText(pathString);
                }
                StreamWriter writer = File.AppendText(pathString);
                writer.WriteLine(msg);
                writer.Close();
            }
            catch (Exception e)
            {
                result = "작성 실패";
                result += e.Message;
            }

            return result;
        }

        public bool StartProcess(String path)
        {
            bool result = true;
            var processInfo = new ProcessStartInfo();

            processInfo.CreateNoWindow = true;
            processInfo.FileName = @"cmd.exe";
            processInfo.Arguments = "/C" + path;

            try
            {
                var process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                WriteLog(String.Format("[{0}] 프로그램 실행 오류 : {1}",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), e.Message));
                result = false;
            }

            return result;
        }
    }
}
