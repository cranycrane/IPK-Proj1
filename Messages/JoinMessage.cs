using System.Net;
using System.Text;


namespace IPK_Proj1.Messages
{
	public class JoinMessage : IMessage
	{
		public string ChannelId { get; set; }
		public string DisplayName { get; set; }
		public ushort? RefMessageId { get; set; }
		public bool IsAwaitingReply { get; set; } = true;

		public JoinMessage(string channelId, string displayName, ushort? refMessageId = null)
		{
			ChannelId = channelId;
			DisplayName = displayName;
			RefMessageId = refMessageId;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x03);

	        messageId = (ushort)IPAddress.HostToNetworkOrder((short)messageId);
	        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
	        bytesList.AddRange(messageIdBytes);

	        bytesList.AddRange(Encoding.UTF8.GetBytes(ChannelId + "\0"));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(DisplayName + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        return $"JOIN {ChannelId} AS {DisplayName}\r\n";
        }
	}
}
