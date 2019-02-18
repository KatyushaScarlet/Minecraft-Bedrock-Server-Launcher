using System;
using System.IO;
using System.Xml.Linq;

namespace MCServant
{
    class XMLConfig
    {
        private XDocument document = null;
        private XElement eleRoot = null;

        private XElement elePort = null;
        private XElement elePassword = null;
        private XElement eleRemote = null;
        private XElement eleWindows = null;
        private XElement eleLinux = null;

        public int port => int.Parse(elePort.Value);
        public string password => elePassword.Value;
        public bool remote => Convert.ToBoolean(eleRemote.Value);
        public string windows => eleWindows.Value;
        public string linux => eleLinux.Value;

        public XMLConfig(string path)
        {
            if (File.Exists(path))
            {
                //配置文件存在，则读取
                document = XDocument.Load(path);
                eleRoot = document.Root;

                //读取属性
                elePort = eleRoot.Element("port");
                elePassword = eleRoot.Element("password");
                eleRemote = eleRoot.Element("remote");
                eleWindows = eleRoot.Element("windows");
                eleLinux = eleRoot.Element("linux");
            }
            else
            {
                //配置文件不存在，则创建
                eleRoot = new XElement("config");

                //默认端口
                elePort = new XElement("port");
                elePort.Value = "666";

                //默认远程密码
                elePassword = new XElement("password");
                elePassword.Value = "default_password";

                //默认不开启远程管理功能
                eleRemote = new XElement("remote");
                eleRemote.Value = false.ToString();

                //Windows下默认启动命令
                eleWindows = new XElement("windows");
                eleWindows.Value = "bedrock_server.exe";

                //Linux下默认启动命令
                eleLinux = new XElement("linux");
                eleLinux.Value = "LD_LIBRARY_PATH=. ./bedrock_server";

                //保存到文件
                eleRoot.Add(elePort);
                eleRoot.Add(elePassword);
                eleRoot.Add(eleRemote);
                eleRoot.Add(eleWindows);
                eleRoot.Add(eleLinux);
                eleRoot.Save(path);
            }
        }
    }
}
