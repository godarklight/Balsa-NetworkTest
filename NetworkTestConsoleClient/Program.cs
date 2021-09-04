using System;
using System.Net;
using System.Net.Sockets;
using NetworkTest;
using NetworkTest.Messages;
using System.Threading;

namespace NetworkTestConsoleClient
{
    class MainClass
    {
        //Client emulating a receiver
        private static bool running = true;

        public static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect(new IPEndPoint(IPAddress.Loopback, 12345));

            NetworkHandler handler = new NetworkHandler();
            handler.RegisterConnect(HandleConnect);
            handler.RegisterDisconnect(HandleDisconnect);
            handler.RegisterSend(MessageType.HEARTBEAT, SendHeartbeat);
            handler.RegisterReceive(MessageType.HEARTBEAT, HeartbeatMessage);
            handler.RegisterReceive(MessageType.ACK, AckMessage);
            handler.RegisterReceive(MessageType.ATTITUDE, AttitudeMessage);
            handler.RegisterReceive(MessageType.POSITION, PositionMessage);

            //Initial setup
            ClientObject clientObject = handler.StartClient(Console.WriteLine, client);
            clientObject.requestedRates[MessageType.HEARTBEAT] = 1;
            SetRate sr1 = new SetRate();
            sr1.messageType = MessageType.POSITION;
            sr1.rate = 0.5f;
            clientObject.SendMessage(sr1);
            SetRate sr2 = new SetRate();
            sr2.messageType = MessageType.ATTITUDE;
            sr2.rate = 0.5f;
            clientObject.SendMessage(sr2);


            while (running)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    if (cki.KeyChar == 'q')
                    {
                        running = false;
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

            handler.Stop();
        }

        private static void HandleConnect(ClientObject client)
        {
            Console.WriteLine("CLIENT CONNECTED");
        }

        private static void HandleDisconnect(ClientObject client)
        {
            Console.WriteLine("CLIENT DISCONNECTED");
            running = false;
        }

        private static void HeartbeatMessage(ClientObject client, IMessage messageRaw)
        {
            Console.WriteLine("HEARTBEAT");
        }

        private static void AckMessage(ClientObject client, IMessage messageRaw)
        {
            Ack message = messageRaw as Ack;
            Console.WriteLine($"ACK {message.messageType}");
        }

        private static void PositionMessage(ClientObject client, IMessage messageRaw)
        {
            Position message = messageRaw as Position;
            Console.WriteLine($"POSITION: {message.latitude},{message.longitude},{message.altitude}");
        }

        private static void AttitudeMessage(ClientObject client, IMessage messageRaw)
        {
            Attitude message = messageRaw as Attitude;
            Console.WriteLine($"ATTITUDE: {message.pitch},{message.roll},{message.yaw}");
        }

        private static IMessage SendHeartbeat(ClientObject client)
        {
            return new Heartbeat();
        }
    }
}
