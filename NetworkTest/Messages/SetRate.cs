using System;
namespace NetworkTest.Messages
{
    [MessageAttribute(MessageType.SET_RATE)]
    public class SetRate : IMessage
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
        public float rate;


        public byte[] Serialize()
        {
            byte[] retBytes = new byte[5];
            retBytes[0] = messageTypeByte;
            BitConverter.GetBytes(rate).CopyTo(retBytes, 1);
            return retBytes;
        }

        public void Deserialize(byte[] bytes)
        {
            messageTypeByte = bytes[0];
            rate = BitConverter.ToSingle(bytes, 1);
        }
    }
}
