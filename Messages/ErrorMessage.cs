using System.Text;


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
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0xFE);

	        bytesList.AddRange(BitConverter.GetBytes(messageId));

	        bytesList.AddRange(Encoding.UTF8.GetBytes(DisplayName + "\0"));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(Content + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        return $"ERROR FROM {DisplayName} IS {Content}\r\n";
        }
	}
}
