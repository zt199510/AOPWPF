using Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using G = Library.GeneralPackets;
namespace ServerTest
{
    public sealed class SConnection : BaseConnection
    {
        
        private static int SessionCount;
        protected override TimeSpan TimeOutDelay => TimeSpan.FromSeconds(20);

        private DateTime PingTime;

        private bool PingSent;

        public int Ping { get; private set; }
        public string IPAddress { get; }
        public int SessionID { get; }

        public SConnection Observed;

        public List<SConnection> Observers = new List<SConnection>();

        public SConnection(TcpClient client) : base(client)
        {
            IPAddress = client.Client.RemoteEndPoint.ToString().Split(':')[0];
            SessionID = ++SessionCount;


            UpdateTimeOut();
            BeginReceive();

        }



        public override void Disconnect()
        {
            if (!Connected) return;

            base.Disconnect();

            SEnvir.Connections.Remove(this);
            SEnvir.IPCount[IPAddress]--;
            SEnvir.DBytesSent += TotalBytesSent;
            SEnvir.DBytesReceived += TotalBytesReceived;
        }

        public override void Enqueue(Packet p)
        {
            base.Enqueue(p);
            if (p == null || !p.ObserverPacket) return;

            foreach (SConnection observer in Observers)
                observer.Enqueue(p);
        }
        public void Process(G.Ping p)
        {
            

            int ping = (int)(SEnvir.Now - PingTime).TotalMilliseconds / 2;
            PingSent = false;
            PingTime = SEnvir.Now + TimeSpan.FromSeconds(2);

            Ping = ping;
            Enqueue(new G.PingResponse { Ping = Ping, ObserverPacket = false });
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void Process()
        {
            if (SEnvir.Now >= PingTime && !PingSent )
            {
                PingTime = SEnvir.Now;
                PingSent = true;
                Enqueue(new G.Ping { ObserverPacket = false });
            }

            if (ReceiveList.Count > 50)
            {
                TryDisconnect();
                SEnvir.IPBlocks[IPAddress] = SEnvir.Now.Add(TimeSpan.FromMinutes(5));

                for (int i = SEnvir.Connections.Count - 1; i >= 0; i--)
                    if (SEnvir.Connections[i].IPAddress == IPAddress)
                        SEnvir.Connections[i].TryDisconnect();

                return;
            }


            base.Process();
        }

        public override void SendDisconnect(Packet p)
        {
            base.SendDisconnect(p);
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public override void TryDisconnect()
        {
           
          
                if (!Disconnecting)
                {
                    Disconnecting = true;
                    TimeOutTime = Time.Now.AddSeconds(10);
                }

                if (SEnvir.Now <= TimeOutTime) return;
            

            Disconnect();
        }

        public override void TrySendDisconnect(Packet p)
        {

                if (!Disconnecting)
                {
                    base.SendDisconnect(p);

                    TimeOutTime = Time.Now.AddSeconds(10);
                }

                if (SEnvir.Now <= TimeOutTime) return;

            SendDisconnect(p);
        }
    }
}
