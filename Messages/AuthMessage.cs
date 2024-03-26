using System;
using System.Text;


namespace IPK_Proj1.Messages
{
	public class AuthMessage : IMessage
	{
		private string Username { get; set; }
		private string DisplayName { get; set; }
		private string Secret { get; set; }
		public bool IsAwaitingReply { get; set; }
		
		public AuthMessage(string username, string secret, string displayName, ushort? messageId = null)
		{
			Username = username;
			Secret = secret;
			DisplayName = displayName;
			IsAwaitingReply = true;
		}
		

        public byte[] ToUdpBytes(ushort messageId)
        {
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x02);

	        bytesList.AddRange(BitConverter.GetBytes(messageId));

	        bytesList.AddRange(Encoding.UTF8.GetBytes(Username + "\0"));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(DisplayName + "\0"));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(Secret + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        return $"AUTH {Username} AS {DisplayName} USING {Secret}\r\n";
        }
	}
}
