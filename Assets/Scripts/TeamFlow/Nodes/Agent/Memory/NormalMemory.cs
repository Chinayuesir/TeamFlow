using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenAI.Chat;
using QFramework;

namespace TeamFlow.Nodes
{
    [Serializable]
    public class NormalMemory:IMemory
    {
        public List<SimpleMessage> Messages { get; set; } = new List<SimpleMessage>();
        [JsonIgnore]
        public EasyEvent Changed = new EasyEvent();

        [JsonIgnore]
        private int mWindowSize = 17; //最好为奇数
        
        public void AddMessage(Message message)
        {
            if (Messages.Count >= mWindowSize)
            {
                Messages.RemoveAt(1);
                Messages.RemoveAt(1);
            }
            Messages.Add(new SimpleMessage(message.Role,message.Content.ToString()));
        }
        

        public void AddMessage(SimpleMessage message)
        {
            if (Messages.Count >= mWindowSize)
            {
                Messages.RemoveAt(1);
                Messages.RemoveAt(2);
            }
            Messages.Add(message);
        }
    }
}