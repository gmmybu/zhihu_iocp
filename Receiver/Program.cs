using System;
using System.Net.Sockets;
using System.Threading;

namespace TestSlowReceiveServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 3333));
                    s.Listen(5);

                    while (true)
                    {
                        Socket c = s.Accept();

                        byte[] buffer = new byte[8 * 1024];
                        c.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                            AsyncReceiveCallback, c);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void AsyncReceiveCallback(IAsyncResult result)
        {
            Socket c = result.AsyncState as Socket;
            try
            {
                if (c.EndReceive(result) != 0)
                {
                    Console.WriteLine(DateTime.Now + " receive buffer");
                    Thread.Sleep(20);

                    byte[] buffer = new byte[8 * 1024];
                    c.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                        AsyncReceiveCallback, c);
                }
                else
                {
                    c.Close();
                }
            }
            catch (Exception ex)
            {
                c.Close();
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
