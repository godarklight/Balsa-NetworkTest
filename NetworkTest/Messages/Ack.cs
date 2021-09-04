using System;
namespace NetworkTest.Messages
{
    [MessageAttribute(MessageType.ACK)]
    public class Ack : IMessage
    {
        public MessageType messageType
        {
            get
            {
                return (MessageType)messageTypeByte;
            }
            set
            {
                messageTypeByte = (byte)value;
            }
        }
        private byte messageTypeByte;

        public byte[] Serialize()
        {
            byte[] retVal = new byte[1];
            retVal[0] = messageTypeByte;
            return retVal;
        }

        public void Deserialize(byte[] bytes)
        {
            messageTypeByte = bytes[0];
        }
    }
}
