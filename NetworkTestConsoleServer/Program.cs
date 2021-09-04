using System;
using NetworkTest;
using NetworkTest.Messages;

namespace NetworkTestConsoleServer
{
    class MainClass
    {
        //This could be another object...
        private static float latitude = 0.1f;
        private static float longitude = 0.2f;
        private static float altitude = 0.3f;
        private static float pitch = 0.4f;
        private static float roll = 0.5f;
        private static float yaw = 0.6f;

        public static void Main(string[] args)
        {
            NetworkHandler handler = new NetworkHandler();
            handler.RegisterConnect(HandleConnect);
            handler.RegisterDisconnect(HandleDisconnect);
            handler.RegisterReceive(MessageType.HEARTBEAT, HeartbeatMessage);
            handler.RegisterReceive(MessageType.SET_RATE, SetRateMessage);
            handler.RegisterSend(MessageType.HEARTBEAT, SendHeartbeat);
            handler.RegisterSend(MessageType.POSITION, SendPosition);
            handler.RegisterSend(MessageType.ATTITUDE, SendAttitude);
            handler.StartServer(Console.WriteLine);
            bool running = true;
            while (running)
            {
                bool processed = true;
                string currentLine = Console.ReadLine();
                string[] split = currentLine.Split(' ');
                if (split.Length == 4)
                {
                    if (split[0] == "pos")
                    {
                        processed = true;
                        latitude = float.Parse(split[1]);
                        longitude = float.Parse(split[2]);
                        altitude = float.Parse(split[3]);
                    }
                    if (split[0] == "att")
                    {
                        processed = true;
                        pitch = float.Parse(split[1]);
                        roll = float.Parse(split[2]);
                        yaw = float.Parse(split[3]);
                    }
                }
                if (currentLine == "quit")
                {
                    processed = true;
                    running = false;
                }
                if (!processed)
                {
                    Console.WriteLine("Unknown command");
                }
            }
            handler.Stop();
        }

        private static void HandleConnect(ClientObject client)
        {
            Console.WriteLine("CONNECT");
        }

        private static void HandleDisconnect(ClientObject client)
        {
            Console.WriteLine("DISCONNECT");
        }

        private static void HeartbeatMessage(ClientObject client, IMessage messageRaw)
        {
            Console.WriteLine("HEARTBEAT");
        }

        private static void SetRateMessage(ClientObject client, IMessage messageRaw)
        {
            SetRate message = messageRaw as SetRate;
            Console.WriteLine($"SET_RATE {message.messageType} {message.rate}");
            client.requestedRates[message.messageType] = message.rate;
            Ack ackMessage = new Ack();
            ackMessage.messageType = message.messageType;
            client.SendMessage(ackMessage);
        }

        private static IMessage SendHeartbeat(ClientObject client)
        {
            Heartbeat message = new Heartbeat();
            return message;
        }

        private static IMessage SendAttitude(ClientObject client)
        {
            Attitude message = new Attitude();
            message.pitch = pitch;
            message.roll = roll;
            message.yaw = yaw;
            return message;
        }

        private static IMessage SendPosition(ClientObject client)
        {
            Position message = new Position();
            message.latitude = latitude;
            message.longitude = longitude;
            message.altitude = altitude;
            return message;
        }
    }
}
