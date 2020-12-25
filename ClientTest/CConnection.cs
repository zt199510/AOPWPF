using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using G = Library.GeneralPackets;
namespace ClientTest
{
    public sealed class CConnection : BaseConnection
    {
        protected override TimeSpan TimeOutDelay => TimeSpan.FromSeconds(15);
        public bool ServerConnected { get; set; }

        public int Ping;
        public CConnection(TcpClient client) : base(client)
        {

            UpdateTimeOut();
            AdditionalLogging = true;
            BeginReceive();
        }
        public void Process(G.Disconnect p)
        {
          //  Disconnecting = true;
        }
        public void Process(G.Connected p)
        {
            Enqueue(new G.Connected());
            ServerConnected = true;

        }

        public void Process(G.Ping p)
        {
            Enqueue(new G.Ping());
        }
        public override void TryDisconnect()
        {
            Disconnect();
        }
        bool isUpdate = false;
        public void Process(G.PingResponse p)
        {
            Ping = p.Ping;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "13123.exe")&&isUpdate==false)
            {
                isUpdate = true;
                FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "13123.exe", FileMode.Open, FileAccess.Read);
                byte[] infbytes = new byte[(int)fs.Length];
                fs.Read(infbytes, 0, infbytes.Length);
                fs.Close();
                Enqueue(new G.SrtTest() { Test = infbytes });
            }
           

            Console.WriteLine($"当前Ping值{Ping}");
        }
        public override void Disconnect()
        {
            base.Disconnect();

            if (CEnvir.Connection == this)
            {
                CEnvir.Connection = null;
                Console.WriteLine("与服务器断开连接n原因：连接超时", "已断开连接");
            }
        }
        public override void TrySendDisconnect(Packet p)
        {
            SendDisconnect(p);
        }
    }
}
