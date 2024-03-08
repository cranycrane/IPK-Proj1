using System;
using System.Text;


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
	        List<byte> bytesList = new List<byte>();

	        // Přidání typu zprávy (předpokládáme, že 0x02 je AUTH)
	        bytesList.Add(0x02);

	        // Přidání messageId
	        bytesList.AddRange(BitConverter.GetBytes(messageId));

	        // Přidání username, secret a displayName, každý ukončený nulovým bajtem
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
