using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TestSlowReceiveClient
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    //改成接收端电脑的IP,也可以使用本机地址
                    s.Connect(new System.Net.IPEndPoint(IPAddress.Parse("172.16.4.98"), 3333));

                    while (true)
                    {
                        byte[] buffer = new byte[1024 * 1024];
                        s.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, AsyncSendCallback, s);
                        Thread.Sleep(1000);
                        if (Console.KeyAvailable)
                            break;
                    }

                    Console.WriteLine("Stop Fast Send");
                    Console.ReadKey();

                    Console.WriteLine("Begin Slow Send");
                    while (true)
                    {
                        byte[] buffer = new byte[1024];
                        s.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, AsyncSendCallback, s);
                        Thread.Sleep(1000);
                        if (Console.KeyAvailable)
                            break;
                    }

                    Console.WriteLine("Stop Slow Send");
                    Console.ReadKey();

                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void AsyncSendCallback(IAsyncResult result)
        {
            Socket s = result.AsyncState as Socket;
            try
            {
                Console.WriteLine(DateTime.Now + " send buffer");
                s.EndSend(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                s.Close();
            }
        }
    }
}
