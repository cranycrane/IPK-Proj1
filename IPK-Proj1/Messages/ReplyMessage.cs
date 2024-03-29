using System.Text;


namespace IPK_Proj1.Messages
{
	public class ReplyMessage : IMessage
	{

		public string Content { get; set; }
		
		public string IsOk { get; set; }
		
		public ushort? MessageId { get; set; }
		public ushort? RefMessageId { get; set; }
		public bool IsAwaitingReply { get; set; } = false;


		public ReplyMessage(string content, string isOk, ushort? messageId = null, ushort? refMessageId = null)
		{
			Content = content;
			IsOk = isOk;
			MessageId = messageId;
			RefMessageId = refMessageId;
		}

        public byte[] ToUdpBytes(ushort messageId)
        {
	        if (!RefMessageId.HasValue || !MessageId.HasValue)
	        {
		        throw new Exception("Interni chyba klienta");
	        }
	        
	        List<byte> bytesList = new List<byte>();

	        bytesList.Add(0x01);

	        bytesList.AddRange(BitConverter.GetBytes(MessageId.Value));
	        bytesList.AddRange(BitConverter.GetBytes(IsOk == "OK" ? 1 : 0));
	        bytesList.AddRange(BitConverter.GetBytes(RefMessageId.Value));
	        bytesList.AddRange(Encoding.UTF8.GetBytes(Content + "\0"));

	        return bytesList.ToArray();
        }

        public string ToTcpString()
        {
	        return $"REPLY {IsOk} IS {Content}\r\n";
        }
	}
}
