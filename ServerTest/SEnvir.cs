using Library;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerTest
{
    public static class SEnvir
    {
        public static DateTime Now, StartTime, LastWarTime;

        public static Dictionary<string, DateTime> IPBlocks = new Dictionary<string, DateTime>();
        public static List<SConnection> Connections = new List<SConnection>();
        public static Dictionary<string, int> IPCount = new Dictionary<string, int>();
        public static ConcurrentQueue<SConnection> NewConnections;
        private static TcpListener _listener, _userCountListener;
        public static bool NetworkStarted { get; set; }
        public static bool Started { get; set; }

        public static long DBytesSent, DBytesReceived;

        public static long TotalBytesSent, TotalBytesReceived;

        public static long DownloadSpeed, UploadSpeed;

        public static int EMailsSent;
        public static Thread EnvirThread { get; private set; }
        public static void StartServer()
        {
            if (Started || EnvirThread != null) return;

            EnvirThread = new Thread(StartNetwork) { IsBackground = true };
            EnvirThread.Start();
        }

        public static void StartNetwork()
        {

            try
            {
            
                Now = Time.Now;
                DateTime DBTime = Now + TimeSpan.FromMinutes(5);
                NewConnections = new ConcurrentQueue<SConnection>();
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7000);
                _listener.Start();
                _listener.BeginAcceptTcpClient(Connection, null);
                _userCountListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
                _userCountListener.Start();
                _userCountListener.BeginAcceptTcpClient(CountConnection, null);

                NetworkStarted = true;

                int count = 0, loopCount = 0;

                DateTime nextCount = Now.AddSeconds(1), UserCountTime = Now.AddMinutes(5), saveTime;

                long previousTotalSent = 0, previousTotalReceived = 0;
                int lastindex = 0;
                long conDelay = 0;

                Console.WriteLine("服务端启动");
                Console.WriteLine("等待玩家连接....");
                LastWarTime = Now;
                while (NetworkStarted)
                {
                    Now = Time.Now;
                    loopCount++;
                    try
                    {
                        SConnection connection;
                        while (!NewConnections.IsEmpty)
                        {
                            if (!NewConnections.TryDequeue(out connection)) break;

                            IPCount.TryGetValue(connection.IPAddress, out var ipCount);
                            IPCount[connection.IPAddress] = ipCount + 1;
                            Console.WriteLine($"玩家{connection.IPAddress}进入游戏");
                            Connections.Add(connection);
                        }
                        long bytesSent = 0;
                        long bytesReceived = 0;
                        for (int i = Connections.Count - 1; i >= 0; i--)
                        {
                            if (i >= Connections.Count) break;
                            connection = Connections[i];
                            connection.Process();
                            bytesSent += connection.TotalBytesSent;
                            bytesReceived += connection.TotalBytesReceived;
                        }
                        long delay = (Time.Now - Now).Ticks / TimeSpan.TicksPerMillisecond;
                        if (delay > conDelay)
                            conDelay = delay;
                    }
                    catch (Exception)
                    {

                       
                    }

                }
            }
            catch (Exception ex)
            {
                Started = false;
               
            }
        }

        private static void CountConnection(IAsyncResult result)
        {
            try
            {
                if (_userCountListener == null || !_userCountListener.Server.IsBound) return;

                TcpClient client = _userCountListener.EndAcceptTcpClient(result);

                byte[] data = Encoding.ASCII.GetBytes(string.Format("c;/Zircon/{0}/;", Connections.Count));
                
                client.Client.BeginSend(data, 0, data.Length, SocketFlags.None, CountConnectionEnd, client);
            }
            catch { }
            finally
            {
                if (_userCountListener != null && _userCountListener.Server.IsBound)
                    _userCountListener.BeginAcceptTcpClient(CountConnection, null);
            }


        }

        private static void CountConnectionEnd(IAsyncResult result)
        {
            try
            {
                TcpClient client = result.AsyncState as TcpClient;

                if (client == null) return;

                client.Client.EndSend(result);

                client.Client.Dispose();
            }
            catch { }
        }

        private static void Connection(IAsyncResult result)
        {
            try
            {
                if (_listener == null || !_listener.Server.IsBound) return;
                TcpClient client = _listener.EndAcceptTcpClient(result);
                string ipAddress = client.Client.RemoteEndPoint.ToString().Split(':')[0];
                if (!IPBlocks.TryGetValue(ipAddress, out DateTime banDate) || banDate < Now)
                {
                    SConnection Connection = new SConnection(client);

                    if (Connection.Connected)
                        NewConnections?.Enqueue(Connection);
                }
            }
            catch (Exception)
            {

               
            }
            finally
            {
                while (NewConnections?.Count >= 15)
                    Thread.Sleep(1);

                if (_listener != null && _listener.Server.IsBound)
                    _listener.BeginAcceptTcpClient(Connection, null);
            }
        }
    }
}
