﻿using System.Text;


namespace IPK_Proj1.Messages
{
    public class ByeMessage : IMessage
    {
        public bool IsAwaitingReply { get; set; }

        public ByeMessage(ushort? messageId = null)
        {
            IsAwaitingReply = false;
        }
        public byte[] ToUdpBytes(ushort messageId)
        {
            List<byte> bytesList = new List<byte>();

            bytesList.Add(0xFF);

            bytesList.AddRange(BitConverter.GetBytes(messageId));

            return bytesList.ToArray();
        }

        public string ToTcpString()
        {
            return "BYE\r\n";
        }
    }
}