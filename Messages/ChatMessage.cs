using System.Text;


namespace IPK_Proj1.Messages
{
	public class ChatMessage : IMessage
	{
		public string DisplayName { get; set; }
		
		public string Content { get; set; }

		public ChatMessage(string displayName, string content)
		{
			DisplayName = displayName;
			Content = content;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x04);

	        bytesList.AddRange(BitConverter.GetBytes(messageId));

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

