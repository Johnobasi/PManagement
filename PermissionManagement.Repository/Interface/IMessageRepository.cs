using PermissionManagement.Model;
using System;
using System.Collections.Generic;

namespace PermissionManagement.Repository
{
    public interface IMessageRepository
    {
        void SendMessageToQueue(MessageItem mailMessage);
        void UpdateMessageStatus(MessageStateInfo message);
        IList<MessageItemWithState> GetMessagesToSend(bool allAvailable, TimeSpan lastExecuteTime);
    }

}
