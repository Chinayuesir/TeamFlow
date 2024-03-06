using System.Collections.Generic;
using OpenAI.Chat;

namespace TeamFlow.Nodes
{
    public interface IMemory
    {
        List<SimpleMessage> Messages { get; set; }
        public void AddMessage(Message message);
        public void AddMessage(SimpleMessage message);
    }
}