using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
  public static class CEnvir
    {
        public static CConnection Connection;
        private static TcpClient ConnectingClient;
        public static void StartClientServer()
        {
            ConnectingClient?.Close();
            ConnectingClient = new TcpClient();
           
                ConnectingClient.BeginConnect("127.0.0.1", 7000, Connecting, ConnectingClient);
            

        }
        public static DateTime Now;
        private static DateTime ConnectionTime, StartTime;
        private static void Connecting(IAsyncResult result)
        {
            try
            {
                TcpClient client = (TcpClient)result.AsyncState;
                client.EndConnect(result);

                if (!client.Connected) return;

                if (client != ConnectingClient)
                {
                    ConnectingClient = null;
                    client.Close();
                    return;
                }

                ConnectionTime = Now.AddSeconds(5); //Add 5 more seconds to timeout for delayed HandShake
                ConnectingClient = null;

                Connection = new CConnection(client);
            }
            catch { }
        }
    }
}
