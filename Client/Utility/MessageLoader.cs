using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Client.Utility
{
    public static class MessageLoader
    {
        public static async Task<List<Models.SourceMessage>> LoadMessage(Models.Chat chat, List<Models.SourceMessage> sourceMessages, int clientId, int count, int _countLeft)
        {
            ChatService.UnitClient unitClient = new ChatService.UnitClient();
            ChatService.ServiceMessage[] serviceMessages = await unitClient.MessagesFromOneChatAsync(chat.SqlId, clientId, chat._messageOffset, count, chat._offsetDate);

            if (serviceMessages == null)
            {
                sourceMessages.Insert(0, Models.SystemMessage.ShiftDate(chat._offsetDate));
                chat._messageOffset = 0;
                chat._offsetDate = chat._offsetDate.AddDays(-1);
                if (_countLeft > 0)
                    await LoadMessage(chat, sourceMessages, clientId, count, _countLeft);
                return sourceMessages;
            }

            if (serviceMessages.First().DateTime == DateTime.MinValue)
            {
                return sourceMessages;
            }

            if (serviceMessages.First().DateTime == DateTime.MaxValue)
            {
                sourceMessages.Insert(0, Models.SystemMessage.ShiftDate(chat._offsetDate));
                chat._messageOffset = 0;
                chat._offsetDate = chat._offsetDate.AddDays(-1);
                await LoadMessage(chat, sourceMessages, clientId, count, _countLeft);
                return sourceMessages;
            }

            if (serviceMessages != null)
            {
                System.Collections.ObjectModel.ObservableCollection<Models.SourceMessage> messages =
                new System.Collections.ObjectModel.ObservableCollection<Models.SourceMessage>(await System.Threading.Tasks.Task.Run(() =>
                {
                    List<Models.SourceMessage> messagesFromChat = new List<Models.SourceMessage>();
                    foreach (var message in serviceMessages)
                    {
                        if (message is ChatService.ServiceMessageText)
                        {
                            var textMessage = message as ChatService.ServiceMessageText;
                            messagesFromChat.Add(chat.GetMessageType(clientId, textMessage.UserId, new Models.TextMessage(textMessage.Text, textMessage.DateTime)));
                        }
                        else if (message is ChatService.ServiceMessageFile)
                        {
                            var fileMessage = message as ChatService.ServiceMessageFile;
                            messagesFromChat.Add(chat.GetMessageType(clientId, fileMessage.UserId, new Models.FileMessage(fileMessage.FileName, fileMessage.DateTime, fileMessage.StreamId) { IsLoaded = true }));
                        }
                        else
                        {
                            var systemMessage = message as ChatService.ServiceMessageManage;
                            switch (systemMessage.RulingMessage)
                            {
                                case ChatService.RulingMessage.UserJoined:
                                    messagesFromChat.Add(Models.SystemMessage.UserAdded(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                                case ChatService.RulingMessage.UserLeft:
                                    messagesFromChat.Add(Models.SystemMessage.UserLeftChat(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                                case ChatService.RulingMessage.UserRemoved:
                                    messagesFromChat.Add(Models.SystemMessage.UserRemoved(systemMessage.DateTime, systemMessage.UserNickname));
                                    break;
                                case ChatService.RulingMessage.ChatroomCreate:
                                    messagesFromChat.Add(Models.SystemMessage.ChatroomCreated(systemMessage.DateTime));
                                    break;
                            }
                        }
                    }
                    return messagesFromChat;
                }));

                chat._messageOffset += messages.Count;
                _countLeft -= messages.Count;

                foreach (var message in messages)
                    sourceMessages.Insert(0, message);
                 if(sourceMessages.FirstOrDefault() != null)
                 {
                    chat._offsetDate = sourceMessages.First().Message.Date;
                 }

                if (_countLeft > 0)
                {
                    await LoadMessage(chat, sourceMessages, clientId, count, _countLeft);
                    return sourceMessages;
                }

            }

            return sourceMessages;
        }
    }
}
