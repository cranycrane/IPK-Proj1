using System;

namespace IPK_Proj1.Messages
{
    public interface IMessage
    {
        public abstract byte[] ToUdpBytes(ushort messageId);
        public abstract string ToTcpString();
        //@todo asi dat do tovarny
        //public static abstract IMessage FromUdpBytes();
        //public static abstract IMessage FromTcpString();
    }
}
