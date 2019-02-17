using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace MCServant
{
    class Program
    {
        private static Process p = new Process();
        private static string shell = "";
        private static string startCommand = "";
        private static string password = "";

        private static string output = "";
    
        static void Main(string[] args)
        {
            //读取配置文件
            XMLConfig config = new XMLConfig("config.xml");
            int port = config.port;
            string password = config.password;
            string windows = config.windows;
            string linux = config.linux;


            //根据系统环境切换命令
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                shell = "cmd";
                startCommand = config.windows;
            }
            else
            {
                shell = "bash";
                startCommand = config.linux;
            }

            //启动Web API
            string url = string.Format("http://127.0.0.1:{0}/", port);
            WebServer webServer = new WebServer(SendResponse, url);
            webServer.Run();
            Console.WriteLine(string.Format("[Web API]Running at port[{0}], password[{1}]", port, password));

            //捕获Ctrl+C事件
            Console.CancelKeyPress += new ConsoleCancelEventHandler(ConsoleExit);
            //进程退出事件
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ConsoleExit);

            //进程信息
            p.StartInfo.FileName = shell;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            //接收消息事件
            p.OutputDataReceived += new DataReceivedEventHandler(P_OutputDataReceived);

            //启动进程
            p.Start();
            p.BeginOutputReadLine();
            p.StandardInput.WriteLine(startCommand);

            //读取输入
            while (true)
            {
                string input = Console.ReadLine();
                if (input != "")
                {
                    p.StandardInput.WriteLine(input);

                    if (input == "stop")
                    {
                        ConsoleExit(null, null);
                    }
                }
            }
        }

        //控制台输出
        private static void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output += e.Data+"\n";
                Console.WriteLine(e.Data);
            }
        }

        //退出事件
        private static void ConsoleExit(object a , object b )
        {
            p.StandardInput.WriteLine("stop");
            p.Close();
            Environment.Exit(0);
        }

        //Web API
        private static string SendResponse(HttpListenerRequest request)
        {
            output = "";
            string command = WebUtility.UrlDecode(request.RawUrl).Substring(1);
            switch (command)
            {
                case "":
                case null:
                case "favicon.ico":
                    Console.WriteLine(string.Format("[Web API]Get command:{0},ignore", command));
                    break;
                default:
                    Console.WriteLine(string.Format("[Web API]Get command:{0}", command));
                    p.StandardInput.WriteLine(command);
                    System.Threading.Thread.Sleep(500);
                    break;
            }
            return output;
        }
    }
}
