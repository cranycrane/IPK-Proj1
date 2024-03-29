using System.Net;
using System.Text;


namespace IPK_Proj1.Messages
{
	public class ChatMessage : IMessage
	{
		public string DisplayName { get; set; }
		
		public string Content { get; set; }
		
		public ushort? RefMessageId { get; set; }
		
		public bool IsAwaitingReply { get; set; }


		public ChatMessage(string displayName, string content, ushort? refMessageId = null)
		{
			DisplayName = displayName;
			Content = content;
			RefMessageId = refMessageId;
			IsAwaitingReply = false;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x04);

	        messageId = (ushort)IPAddress.HostToNetworkOrder((short)messageId);
	        byte[] messageIdBytes = BitConverter.GetBytes(messageId);
	        bytesList.AddRange(messageIdBytes);

	        bytesList.AddRange(Encoding.UTF8.GetBytes(DisplayName + "\0"));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(Content + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        return $"MSG FROM {DisplayName} IS {Content}\r\n";
        }
	}
}

