using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using NetworkTest.Messages;

namespace NetworkTest
{
    public class NetworkHandler
    {
        private Action<string> log;

        //Network components
        private ConcurrentBag<ClientObject> clients = new ConcurrentBag<ClientObject>();
        private Thread sendThread;
        private Thread receiveThread;
        private AutoResetEvent sendEvent = new AutoResetEvent(false);
        private TcpListener listener;

        //Auto serialization/deserialization
        private Dictionary<MessageType, Type> messageTypes = new Dictionary<MessageType, Type>();
        private Dictionary<Type, MessageType> messageTypesRev = new Dictionary<Type, MessageType>();

        //Callbacks
        private Action<ClientObject> connectCallback;
        private Dictionary<MessageType, Action<ClientObject, IMessage>> receiveCallbacks = new Dictionary<MessageType, Action<ClientObject, IMessage>>();
        private Dictionary<MessageType, Func<ClientObject, IMessage>> sendCallbacks = new Dictionary<MessageType, Func<ClientObject, IMessage>>();
        private Action<ClientObject> disconnectCallback;

        private bool running = true;

        public void StartServer(Action<string> log)
        {
            this.log = log;
            PopulateMessageTypes();
            listener = new TcpListener(new IPEndPoint(IPAddress.Any, 12345));
            listener.Start();
            listener.BeginAcceptSocket(HandleConnect, listener);
            Start();
        }

        public ClientObject StartClient(Action<string> log, TcpClient client)
        {
            this.log = log;
            PopulateMessageTypes();
            ClientObject clientObject = new ClientObject(this, sendEvent, log, RequestMessage);
            clientObject.client = client;
            clients.Add(clientObject);
            Start();
            if (connectCallback != null)
            {
                connectCallback(clientObject);
            }
            return clientObject;
        }

        private void Start()
        {
            receiveThread = new Thread(new ThreadStart(ReceiveMain));
            receiveThread.Start();
            sendThread = new Thread(new ThreadStart(SendMain));
            sendThread.Start();
        }

        public void Stop()
        {
            running = false;
            if (listener != null)
            {
                listener.Stop();
            }
            sendThread.Join();
            receiveThread.Join();
        }

        public void RegisterConnect(Action<ClientObject> callback)
        {
            connectCallback = callback;
        }

        public void RegisterDisconnect(Action<ClientObject> callback)
        {
            disconnectCallback = callback;
        }

        public void RegisterReceive(MessageType messageType, Action<ClientObject, IMessage> callback)
        {
            receiveCallbacks[messageType] = callback;
        }

        public void RegisterSend(MessageType messageType, Func<ClientObject, IMessage> callback)
        {
            sendCallbacks[messageType] = callback;
        }

        private IMessage RequestMessage(ClientObject client, MessageType messageType)
        {
            if (sendCallbacks.ContainsKey(messageType))
            {
                return sendCallbacks[messageType](client);
            }
            return null;
        }

        private void PopulateMessageTypes()
        {
            messageTypes.Clear();
            messageTypesRev.Clear();
            Assembly networkTest = Assembly.GetExecutingAssembly();
            foreach (Type t in networkTest.GetExportedTypes())
            {
                MessageAttribute messageAttribute = t.GetCustomAttribute<MessageAttribute>();
                if (messageAttribute != null)
                {
                    messageTypes.Add(messageAttribute.type, t);
                    messageTypesRev.Add(t, messageAttribute.type);
                }
            }
        }

        private void HandleConnect(IAsyncResult ar)
        {
            try
            {
                ClientObject client = new ClientObject(this, sendEvent, log, RequestMessage);
                client.client = listener.EndAcceptTcpClient(ar);
                client.requestedRates[MessageType.HEARTBEAT] = 1f;
                clients.Add(client);
                if (connectCallback != null)
                {
                    connectCallback(client);
                }
            }
            catch(ObjectDisposedException ode)
            {
                if (running)
                {
                    log($"Failed to accept client: {ode.Message}");
                }
            }
            catch (SocketException se)
            {
                if (running)
                {
                    log($"Failed to accept client: {se.Message}");
                }
            }
            if (running)
            {
                listener.BeginAcceptSocket(HandleConnect, null);
            }
        }

        private void HandleDisconnect(ClientObject client)
        {
            lock (client)
            {
                if (client.client != null)
                {
                    client.client = null;
                    if (disconnectCallback != null)
                    {
                        disconnectCallback(client);
                    }
                }
            }
        }

        private void ReceiveMain()
        {
            while (running)
            {
                foreach (ClientObject client in clients)
                {
                    NetworkStream ns;
                    try
                    {
                        ns = client.client.GetStream();
                    }
                    catch
                    {
                        HandleDisconnect(client);
                        RebuildClients();
                        continue;
                    }
                    if (ns.DataAvailable)
                    {
                        int bytesRead = 0;
                        try
                        {
                            bytesRead = ns.Read(client.buffer, client.readPos, client.bytesLeft);
                        }
                        catch
                        {
                            HandleDisconnect(client);
                            RebuildClients();
                            continue;
                        }
                        client.readPos += bytesRead;
                        client.bytesLeft -= bytesRead;
                        if (bytesRead == 0)
                        {
                            HandleDisconnect(client);
                            RebuildClients();
                        }
                        if (client.bytesLeft == 0)
                        {
                            if (client.readingHeader)
                            {
                                //Process 0 byte messages
                                client.bytesLeft = client.buffer[1];
                                if (client.bytesLeft == 0)
                                {
                                    MessageType type = (MessageType)client.buffer[5];
                                    IMessage message = (IMessage)Activator.CreateInstance(messageTypes[type]);
                                    if (receiveCallbacks.ContainsKey(type))
                                    {
                                        receiveCallbacks[type](client, message);
                                    }
                                    client.readingHeader = true;
                                    client.readPos = 0;
                                    client.bytesLeft = 8;
                                }
                                else
                                {
                                    client.readingHeader = false;
                                }
                            }
                            else
                            {
                                //Process messages with payloads
                                MessageType type = (MessageType)client.buffer[5];
                                int length = client.buffer[1];
                                IMessage message = (IMessage)Activator.CreateInstance(messageTypes[type]);
                                byte[] payload = new byte[length];
                                Array.Copy(client.buffer, 6, payload, 0, length);
                                message.Deserialize(payload);
                                if (receiveCallbacks.ContainsKey(type))
                                {
                                    receiveCallbacks[type](client, message);
                                }
                                client.readingHeader = true;
                                client.readPos = 0;
                                client.bytesLeft = 8;
                            }
                        }
                    }
                }
            }
        }

        private void RebuildClients()
        {
            ConcurrentBag<ClientObject> newClients = new ConcurrentBag<ClientObject>();
            foreach (ClientObject client in clients)
            {
                if (client.client != null)
                {
                    newClients.Add(client);
                }
            }
            clients = newClients;
        }

        private void SendMain()
        {
            while (running)
            {
                sendEvent.WaitOne(50);
                foreach (ClientObject client in clients)
                {
                    if (!client.client.Connected)
                    {
                        HandleDisconnect(client);
                        RebuildClients();
                    }
                    else
                    {
                        while (client.outgoingMessages.TryDequeue(out IMessage sendMessage))
                        {
                            //Add header
                            byte[] sendMessageBytes;
                            byte[] payload = sendMessage.Serialize();
                            if (payload == null)
                            {
                                sendMessageBytes = new byte[8];
                            }
                            else
                            {
                                sendMessageBytes = new byte[8 + payload.Length];
                                sendMessageBytes[1] = (byte)payload.Length;
                                payload.CopyTo(sendMessageBytes, 6);
                            }
                            sendMessageBytes[5] = (byte)messageTypesRev[sendMessage.GetType()];
                            try
                            {
                                client.client.GetStream().Write(sendMessageBytes, 0, sendMessageBytes.Length);
                            }
                            catch
                            {
                                HandleDisconnect(client);
                                RebuildClients();
                            }
                        }
                    }
                    client.Update();
                }
            }
        }
    }
}