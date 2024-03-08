using System.Text;


namespace IPK_Proj1.Messages
{
	public class JoinMessage : IMessage
	{
		public string ChannelId { get; set; }
		public string DisplayName { get; set; }

		public JoinMessage(string channelId, string displayName)
		{
			ChannelId = channelId;
			DisplayName = displayName;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x03);

	        bytesList.AddRange(BitConverter.GetBytes(messageId));

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
