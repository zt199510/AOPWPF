using Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
            Console.WriteLine($"当前Ping值{Ping}");
        }

        /// <summary>
        /// 验证客户端版本
        /// </summary>
        /// <param name="p"></param>
        public void Process(G.CheckVersion p)
        {
            byte[] clientHash;
            using (MD5 md5 = MD5.Create())
            {
                using (FileStream stream = File.OpenRead(Application.ExecutablePath))
                    clientHash = md5.ComputeHash(stream);
            }

            Enqueue(new G.Version { ClientHash = clientHash });
        }


        public void Process(G.GoodVersion p)
        {

        }
    }
}
