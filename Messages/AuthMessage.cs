using System;


namespace IPK_Proj1.Messages
{
	public class AuthMessage : IMessage
	{
		private string Username { get; set; }
		private string DisplayName { get; set; }
		private string Secret { get; set; }

		public AuthMessage(string username, string secret, string displayName)
		{
			Username = username;
			Secret = secret;
			DisplayName = displayName;
		}
		

        public byte[] ToUdpBytes(ushort messageId)
        {
	        return [];
	        throw new NotImplementedException();
        }

        public string ToTcpString()
        {
	        return $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
        }
	}
}
