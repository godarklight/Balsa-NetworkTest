using System;
namespace NetworkTest.Messages
{
    [MessageAttribute(MessageType.POSITION)]
    public class Position : IMessage
    {
        public float latitude;
        public float longitude;
        public float altitude;

        public byte[] Serialize()
        {
            byte[] retBytes = new byte[12];
            BitConverter.GetBytes(latitude).CopyTo(retBytes, 0);
            BitConverter.GetBytes(longitude).CopyTo(retBytes, 4);
            BitConverter.GetBytes(altitude).CopyTo(retBytes, 8);
            return retBytes;
        }

        public void Deserialize(byte[] bytes)
        {
            latitude = BitConverter.ToSingle(bytes, 0);
            longitude = BitConverter.ToSingle(bytes, 4);
            altitude = BitConverter.ToSingle(bytes, 8);
        }
    }
}
