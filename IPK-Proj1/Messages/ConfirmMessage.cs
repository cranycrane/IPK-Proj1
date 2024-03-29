using System.Text;


namespace IPK_Proj1.Messages
{
    public class ConfirmMessage : IMessage
    {
        public bool IsAwaitingReply { get; set; } = false;

        public byte[] ToUdpBytes(ushort messageId)
        {
            List<byte> bytesList = new List<byte>();

            bytesList.Add(0x00);
            bytesList.AddRange(BitConverter.GetBytes(messageId));

            return bytesList.ToArray();
        }

        public string ToTcpString()
        {
            return "";
        }
    }
}