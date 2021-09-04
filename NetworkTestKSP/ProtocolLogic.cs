using System;
using NetworkTest;
using NetworkTest.Messages;
using DarkLog;

namespace NetworkTestKSP
{
    public class ProtocolLogic
    {
        private DataStore data;
        private ModLog log;

        public ProtocolLogic(DataStore data, ModLog log)
        {
            this.data = data;
            this.log = log;
        }

        public void ConnectEvent(ClientObject client)
        {
            client.requestedRates[MessageType.HEARTBEAT] = 1f;
            log.Log("Client connected");
        }

        public void DisconnectEvent(ClientObject client)
        {
            log.Log("Client disconnected");
        }

        public void ReceiveSetRate(ClientObject client, IMessage rawMessage)
        {
            SetRate message = rawMessage as SetRate;
            client.requestedRates[message.messageType] = message.rate;
        }

        public IMessage SendHeartbeat(ClientObject client)
        {
            Heartbeat message = new Heartbeat();
            return message;
        }

        public IMessage SendAttitude(ClientObject client)
        {
            Attitude message = new Attitude();
            message.pitch = data.pitch;
            message.roll = data.roll;
            message.yaw = data.yaw;
            return message;
        }

        public IMessage SendPosition(ClientObject client)
        {
            Position message = new Position();
            message.latitude = data.latitude;
            message.longitude = data.longitude;
            message.altitude = data.altitude;
            return message;
        }
    }
}
