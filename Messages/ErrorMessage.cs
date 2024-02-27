using System;


namespace IPK_Proj1.Messages
{
	public class ErrorMessage : IMessage
	{
		private string DisplayName { get; set; }

		private string Content { get; set; }

		public ErrorMessage(string displayName, string content)
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
	        return $"ERROR FROM {DisplayName} IS {Content}\r\n";
        }
	}
}
