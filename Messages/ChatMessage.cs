using System;


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
	        throw new NotImplementedException();
        }

        public string ToTcpString()
        {
	        return $"MSG FROM {DisplayName} IS {Content}\r\n";
        }
	}
}
