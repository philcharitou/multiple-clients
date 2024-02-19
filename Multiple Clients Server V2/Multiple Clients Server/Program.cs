using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Multiple_Clients_Server
{
    class Program
    {
        static List<Socket> clientSockets = new List<Socket>();

        static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        static List<string> clientNames = new List<string>();

        static bool[] clientNameAdded = new bool[9] { false, false, false, false, false, false, false, false, false };

        static byte[] buffer = new byte[1024];

        static void Main(string[] args)
        {
            Console.Title = "Server";

            Console.WriteLine("The server's IP is: " + GetLocalIP());

            SetupServer();

            Console.ReadLine();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");

            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));

            serverSocket.Listen(1);

            serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);
        }

        private static void AcceptCallBack(IAsyncResult AR)
        {
            Socket socket = serverSocket.EndAccept(AR);

            Console.WriteLine("Connected");

            clientSockets.Add(socket);

            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallBack), socket);

            serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

        }

        private static string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "127.0.0.1";
        }

        private static int ClientIndexValue(Socket socket)
        {
            for (int i = 0; i < clientSockets.Count; i++)
            {
                if (socket == clientSockets[i])
                {
                    return i;
                }
            }
            return 0;
        }

        private static void RecieveCallBack(IAsyncResult AR)
        {
            try
            {
                Socket socket = (Socket)AR.AsyncState;

                int recievedSize = socket.EndReceive(AR);

                byte[] recievedBuff = new byte[recievedSize];

                Array.Copy(buffer, recievedBuff, recievedSize);

                string textRecieved = Encoding.ASCII.GetString(recievedBuff);

                if (clientNameAdded[ClientIndexValue(socket)] == false)
                {
                    Console.WriteLine("Text recieved: " + textRecieved + ". From: unknown");

                    clientNames.Add(textRecieved);
                    clientNameAdded[ClientIndexValue(socket)] = true;

                    byte[] sentBuff = Encoding.ASCII.GetBytes(clientNames[ClientIndexValue(socket)] + " client added.");

                    socket.BeginSend(sentBuff, 0, sentBuff.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
                }
                else
                {
                    Console.WriteLine("Text recieved: " + textRecieved + ". From: " + clientNames[ClientIndexValue(socket)]);

                    if (textRecieved == "get time")
                    {
                        byte[] sentBuff = Encoding.ASCII.GetBytes(clientNames[ClientIndexValue(socket)] + ", " + DateTime.Now.ToLongTimeString());

                        socket.BeginSend(sentBuff, 0, sentBuff.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
                    }
                    else
                    {
                        byte[] sentBuff = Encoding.ASCII.GetBytes(clientNames[ClientIndexValue(socket)] + ": Invalid response... Please try again");

                        socket.BeginSend(sentBuff, 0, sentBuff.Length, SocketFlags.None, new AsyncCallback(SendCallBack), socket);
                    }
                }

                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(RecieveCallBack), socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void SendCallBack(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            socket.EndSend(AR);
        }
    }
}
