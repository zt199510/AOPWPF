using Library;
using System;
using System.Collections.Generic;
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

        public override void TryDisconnect()
        {
            Disconnect();
        }

        public override void Disconnect()
        {
            base.Disconnect();

            if (CEnvir.Connection == this)
            {
                CEnvir.Connection = null;
                Console.WriteLine("与服务器断开连接\n原因：连接超时", "已断开连接");
            }
        }

        public override void Process()
        {
            Enqueue(new G.Connected());
            ServerConnected = true;
        }

        public override void SendDisconnect(Packet p)
        {
            base.SendDisconnect(p);
        }

      

      

        public override void TrySendDisconnect(Packet p)
        {
           
        }
    }
}
