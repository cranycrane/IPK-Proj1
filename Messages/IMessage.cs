namespace IPK_Proj1.Messages
{
    public interface IMessage
    {
        public bool IsAwaitingReply { get; set; }
        public abstract byte[] ToUdpBytes(ushort messageId);
        public abstract string ToTcpString();
    }
}
