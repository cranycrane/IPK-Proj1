using System;


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
	        throw new NotImplementedException();
        }

        public string ToTcpString()
        {
	        return $"JOIN {ChannelId} AS {DisplayName}\r\n";
        }
	}
}
