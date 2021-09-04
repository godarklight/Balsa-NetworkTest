using System;
namespace NetworkTest.Messages
{
    public class MessageAttribute : Attribute
    {
        public MessageType type;
        public MessageAttribute(MessageType type)
        {
            this.type = type;
        }
    }
}
