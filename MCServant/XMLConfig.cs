using System.IO;
using System.Xml.Linq;

namespace MCServant
{
    class XMLConfig
    {
        private XDocument document = null;
        private XElement eleRoot = null;
        private XElement elePassword = null;
        private XElement elePort = null;
        private XElement eleWindows = null;
        private XElement eleLinux = null;

        public string password { get { return elePassword.Value; } }
        public int port { get { return int.Parse(elePort.Value); } }
        public string windows { get { return eleWindows.Value; } }
        public string linux { get { return eleLinux.Value; } }

        public XMLConfig(string path)
        {
            if (File.Exists(path))
            {
                //配置文件存在，则读取
                document = XDocument.Load(path);
                eleRoot = document.Root;

                //读取属性
                elePassword = eleRoot.Element("password");
                elePort = eleRoot.Element("port");
                eleWindows = eleRoot.Element("windows");
                eleLinux = eleRoot.Element("linux");
            }
            else
            {
                //配置文件不存在，则创建
                eleRoot = new XElement("config");

                //默认密码
                elePassword = new XElement("password");
                elePassword.Value = "default_password";

                //默认端口
                elePort = new XElement("port");
                elePort.Value = "6666";

                //Windows下默认启动命令
                eleWindows = new XElement("windows");
                eleWindows.Value = "bedrock_server.exe";

                //Linux下默认启动命令
                eleLinux = new XElement("linux");
                eleLinux.Value = "LD_LIBRARY_PATH=. ./bedrock_server";

                //保存到文件
                eleRoot.Add(elePassword);
                eleRoot.Add(elePort);
                eleRoot.Add(eleWindows);
                eleRoot.Add(eleLinux);
                eleRoot.Save(path);
            }
        }
    }
}
