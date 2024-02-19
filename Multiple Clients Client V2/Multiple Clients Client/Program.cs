using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Multiple_Clients_Client
{
    class Program
    {
        static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static string clientName = "";
        static string serverIP;

        static void Main(string[] args)
        {
            Console.Title = "Client";

            Console.WriteLine("Provide the ip address of the server");
            serverIP = Console.ReadLine();

            Console.WriteLine("Please enter your name.");
            clientName = Console.ReadLine();

            ConnectLoop();

            SendLoop();

            Console.ReadLine();
        }

        private static void SendLoop()
        {
            bool firstResponseRecieved = false;
            while (true)
            {
                if (firstResponseRecieved == true)
                {
                    Console.WriteLine("Enter a request...");

                    string userResponse = Console.ReadLine();

                    byte[] sendBuffer = Encoding.ASCII.GetBytes(userResponse);

                    clientSocket.Send(sendBuffer);
                }

                byte[] recieveBuffer = new byte[1024];

                int rec = clientSocket.Receive(recieveBuffer);

                byte[] buffer = new byte[rec];

                Array.Copy(recieveBuffer, buffer, rec);

                Console.WriteLine("Recieved: " + Encoding.ASCII.GetString(buffer));

                firstResponseRecieved = true;
            }
        }

        private static void ConnectLoop()
        {
            int connectAttempts = 0;

            while (!clientSocket.Connected)
            {
                try
                {
                    connectAttempts++;
                    clientSocket.Connect(serverIP, 100);
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + connectAttempts);
                }
            }

            byte[] sendBuffer = Encoding.ASCII.GetBytes(clientName);
            clientSocket.Send(sendBuffer);

            Console.Clear();
            Console.WriteLine("Connected");
        }
    }
}
