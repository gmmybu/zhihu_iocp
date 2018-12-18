using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace TestSlowReceiveClient
{
    class Program
    {

        class SocketAndIndex
        {
            public Socket s;
            public UInt32 index;
        }

        static void Main(string[] args)
        {
            try
            {
                using (Socket s = new Socket(SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Connect(new System.Net.IPEndPoint(IPAddress.Parse("172.16.4.98"), 3333));

                    while (true)
                    {
                        UInt32 index;
                        byte[] buffer = MakeBufferWithIndex(1024, out index);

                        var state = new SocketAndIndex();
                        state.s = s;
                        state.index = index;

                        s.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, AsyncSendCallback, state);
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

        static UInt32 buffer_index = 10000;

        static byte[] MakeBufferWithIndex(UInt32 kb, out UInt32 index)
        {
            byte[] buffer = new byte[1024 * kb];

            byte a = (byte)(buffer_index >> 24);
            byte b = (byte)(buffer_index >> 16);
            byte c = (byte)(buffer_index >> 8);
            byte d = (byte)(buffer_index);
            for (int i = 0; i < 1024 * kb; i += 4)
            {
                buffer[i] = a;
                buffer[i + 1] = b;
                buffer[i + 2] = c;
                buffer[i + 3] = d;
            }

            index = buffer_index;
            buffer_index++;

            return buffer;
        }

        static void AsyncSendCallback(IAsyncResult result)
        {
            SocketAndIndex state = result.AsyncState as SocketAndIndex;
            try
            {
                Console.WriteLine(DateTime.Now + " send buffer " + state.index);
                state.s.EndSend(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                state.s.Close();
            }
        }
    }
}
