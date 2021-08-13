using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace server_example
{
    class Program
    {
        #region Varables
        
        const int serverPORT = 1000; // Selected Port, Open port in firewall!
        int MaxClients = 8;
        bool Quit = false;

        #endregion Varables
        
        static void Main(string[] args)
        {
            IPEndPoint serverEP;
            serverEP = AutoGetServerIP(serverPORT);            
            TcpListener TcpServer;
            TcpServer = new TcpListener(serverEP);
            TcpServer.Start();
            Socket[] TcpClients = new Socket[MaxClients];
            //

            while (true)
            {
                if (Quit) // stop loop condition
                {
                    break;
                }

                if (TcpServer.Pending())
                {
                    for (int i = 0; i < MaxClients; i++)
                    {
                        if (TcpClients[i] == null)
                        {
                            TcpClients[i] = TcpServer.AcceptSocket();
                        }
                    }
                }

                for (int i = 0; i < MaxClients; i++)
                {
                    if (TcpClients[i] != null)
                    {
                        if (TcpClients[i].Connected)
                        {
                            if (TcpClients[i].Available > 0)
                            {
                                Thread.Sleep(100);

                                int msgSize = TcpClients[i].Available;
                                byte[] msg = new byte[msgSize];

                                TcpClients[i].Receive(msg, msgSize, SocketFlags.None);

                                string msgASCII = Encoding.ASCII.GetString(msg);
                                //string msgUTF8 = Encoding.UTF8.GetString(msg);
                                //string msgUNICODE = Encoding.Unicode.GetString(msg);

                                //Server Response:
                                string MyResponse = "Got it!";

                                //Set the required reponse encoding:
                                byte[] responseASCII = Encoding.ASCII.GetBytes(MyResponse);
                                //byte[] responseUTF8 = Encoding.UTF8.GetBytes(MyResponse);
                                //byte[] responseUNICODE = Encoding.Unicode.GetBytes(MyResponse);

                                //Choose one and send it:
                                TcpClients[i].Send(responseASCII);
                                //Clients[i].Send(responseUTF8);
                                //Clients[i].Send(responseUNICODE);
                            }


                            if (TcpClients[i].Poll(1, SelectMode.SelectRead) && TcpClients[i].Available == 0)
                            {
                                TcpClients[i] = null; // Disconnect client
                            }
                        }
                        else
                        {
                            TcpClients[i] = null; // Disconnect client
                        }
                    }
                }

                //Remember to Create a Quit option:
                //Quit = true;

                Thread.Sleep(100);
            }




            TcpServer.Stop();
      
        }

        public static IPEndPoint AutoGetServerIP(int port)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return new IPEndPoint(ip, port); // Selected Server IP
                }
            }
            //Try to avoid this (No LAN link):
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"), port); //Default localhost
        }
    }
}