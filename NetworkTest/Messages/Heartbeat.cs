using System;
namespace NetworkTest.Messages
{
    [MessageAttribute(MessageType.HEARTBEAT)]
    public class Heartbeat : IMessage
    {
        public byte[] Serialize()
        {
            return null;
        }

        public void Deserialize(byte[] bytes)
        {
        }
    }
}
