using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PermissionManagement.Model;

namespace PermissionManagement.Repository
{
    public interface IMessageRepository
    {
        void SendMessageToQueue(MessageItem mailMessage);
        void UpdateMessageStatus(MessageStateInfo message);
        IList<MessageItemWithState> GetMessagesToSend(bool allAvailable, TimeSpan lastExecuteTime);
    }

}
