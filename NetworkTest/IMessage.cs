using System;
namespace NetworkTest
{
    public interface IMessage
    {
        void Deserialize(byte[] bytes);
        byte[] Serialize();
    }
}
