using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using NetworkTest.Messages;

namespace NetworkTest
{
    public class ClientObject
    {
        private NetworkHandler networkHandler;
        private AutoResetEvent sendEvent;
        private Func<ClientObject, MessageType, IMessage> requestMessage;
        public Action<string> log;

        public TcpClient client;
        //Rates are stored in seconds
        public ConcurrentDictionary<MessageType, float> requestedRates = new ConcurrentDictionary<MessageType, float>();
        //Send time stored in ticks
        public ConcurrentDictionary<MessageType, long> nextSendTime = new ConcurrentDictionary<MessageType, long>();
        public ConcurrentQueue<IMessage> outgoingMessages = new ConcurrentQueue<IMessage>();

        public byte[] buffer = new byte[263];
        public bool readingHeader = true;
        public int readPos = 0;
        public int bytesLeft = 8;

        public ClientObject(NetworkHandler networkHandler, AutoResetEvent sendEvent, Action<string> log, Func<ClientObject, MessageType, IMessage> requestMessage)
        {
            this.networkHandler = networkHandler;
            this.sendEvent = sendEvent;
            this.log = log;
            this.requestMessage = requestMessage;
        }

        public void Update()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            foreach (KeyValuePair<MessageType, float> kvp in requestedRates)
            {
                bool sendType = false;
                if (nextSendTime.TryGetValue(kvp.Key, out long nextTime))
                {
                    if (currentTime > nextTime)
                    {
                        sendType = true;
                        nextSendTime[kvp.Key] = currentTime + (long)(kvp.Value * TimeSpan.TicksPerSecond);
                    }
                }
                else
                {
                    sendType = true;
                    nextSendTime[kvp.Key] = currentTime + (long)(kvp.Value * TimeSpan.TicksPerSecond);
                }
                if (sendType)
                {
                    IMessage requestedMessage = requestMessage(this, kvp.Key);
                    if (requestedMessage != null)
                    {
                        SendMessage(requestedMessage);
                    }
                }
            }
        }

        public void SendMessage(IMessage message)
        {
            outgoingMessages.Enqueue(message);
            sendEvent.Set();
        }
    }
}
