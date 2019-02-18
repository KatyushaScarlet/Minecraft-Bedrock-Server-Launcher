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
        private static bool remote = false;

        private static string output = "";
    
        static void Main(string[] args)
        {
            //读取配置文件
            XMLConfig config = new XMLConfig("config.xml");
            int port = config.port;
            password = config.password;
            remote = config.remote;
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
            string url = "";
            if (remote)
            {
                url = string.Format("http://+:{0}/", port);
            }
            else
            {
                url = string.Format("http://127.0.0.1:{0}/", port);
            }
            WebServer webServer = new WebServer(SendResponse, url);
            webServer.Run();
            Console.WriteLine(string.Format("[Web API]Running at [{0}], remote manage:[{1}]", url, remote));

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
                    SendCommand(input);
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
            string ip = request.RemoteEndPoint.Address.ToString();
            string command = WebUtility.UrlDecode(request.RawUrl).Substring(1);

            if (remote && password != "")
            {
                //开启远程管理，且密码不为空时
                if (command.Contains('|'))
                {
                    //命令中附带密码时
                    string[] commands = command.Split('|');
                    if (commands[1] == password)
                    {
                        command = commands[0];
                    }
                    else
                    {
                        Console.WriteLine(string.Format("[Web API]Wrong password, command:[{0}], from:[{1}], ignore", command, ip));
                        return "error";
                    }
                }
                else
                {
                    Console.WriteLine(string.Format("[Web API]Empty password, command:[{0}], from:[{1}], ignore", command, ip));
                    return "error";
                }
            }

            //执行命令
            Console.WriteLine(string.Format("[Web API]Get command:{0}, from[{1}]", command,ip));
            SendCommand(command);
            System.Threading.Thread.Sleep(500);
            return output;
        }

        //发送命令
        private static void SendCommand(string command)
        {
            p.StandardInput.WriteLine(command);

            if (command == "stop")
            {
                ConsoleExit(null, null);
            }
        }
    }
}
