using System.Text;


namespace IPK_Proj1.Messages
{
	public class ReplyMessage : IMessage
	{
		private string DisplayName { get; set; }

		private string Content { get; set; }
		
		private bool IsOk { get; set; }
		
		private ushort? RefMessageId { get; set; }

		public ReplyMessage(string displayName, string content, bool isOk, ushort? refMessageId = null)
		{
			DisplayName = displayName;
			Content = content;
			IsOk = isOk;
			RefMessageId = refMessageId;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        if (!RefMessageId.HasValue)
	        {
		        throw new Exception("Interni chyba klienta");
	        }
	        
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x01);

	        bytesList.AddRange(BitConverter.GetBytes(messageId));
	        bytesList.AddRange(BitConverter.GetBytes(IsOk ? 1 : 0));
	        bytesList.AddRange(BitConverter.GetBytes(RefMessageId.Value));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(Content + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        var isOk = IsOk ? "OK" : "NOK";
	        return $"REPLY {isOk} IS {Content}\r\n";
        }
	}
}
