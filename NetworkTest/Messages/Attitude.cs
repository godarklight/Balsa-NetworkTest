using System;
namespace NetworkTest.Messages
{
    [MessageAttribute(MessageType.ATTITUDE)]
    public class Attitude : IMessage
    {
        public float pitch;
        public float roll;
        public float yaw;

        public byte[] Serialize()
        {
            byte[] retBytes = new byte[12];
            BitConverter.GetBytes(pitch).CopyTo(retBytes, 0);
            BitConverter.GetBytes(roll).CopyTo(retBytes, 4);
            BitConverter.GetBytes(yaw).CopyTo(retBytes, 8);
            return retBytes;
        }

        public void Deserialize(byte[] bytes)
        {
            pitch = BitConverter.ToSingle(bytes, 0);
            roll = BitConverter.ToSingle(bytes, 4);
            yaw = BitConverter.ToSingle(bytes, 8);
        }
    }
}
