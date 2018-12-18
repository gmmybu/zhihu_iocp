using System;
using System.Net.Sockets;
using System.Threading;

namespace TestSlowReceiveServer
{
    class Program
    {
        class SocketAndBuffer
        {
            public Socket s;
            public byte[] buffer;
        }


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
                        BeginReceive(c);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        static void BeginReceive(Socket s)
        {
            byte[] buffer = new byte[8 * 1024];

            var state = new SocketAndBuffer();
            state.s = s;
            state.buffer = buffer;

            s.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None,
                AsyncReceiveCallback, state);
        }

        static UInt32 GetBufferIndex(byte[] bb, int length)
        {
            if (bb == null || length < 4)
                return 0;

            UInt32 a = bb[0];
            UInt32 b = bb[1];
            UInt32 c = bb[2];
            UInt32 d = bb[3];
            return (a << 24) | (b << 16) | (c << 8) | d;
        }

        static void AsyncReceiveCallback(IAsyncResult result)
        {
            SocketAndBuffer c = result.AsyncState as SocketAndBuffer;
            try
            {
                int length = c.s.EndReceive(result);
                if (length != 0)
                {
                    Console.WriteLine(DateTime.Now + " receive buffer " + GetBufferIndex(c.buffer, length) + ", length " + length);
                    Thread.Sleep(20);

                    BeginReceive(c.s);
                }
                else
                {
                    c.s.Close();
                }
            }
            catch (Exception ex)
            {
                c.s.Close();
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
