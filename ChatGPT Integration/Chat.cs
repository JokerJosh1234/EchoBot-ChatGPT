using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatGPT_Integration.Commands
{
    public class Chat : Event
    {
        // when the bot is pinged or replied to, it will initiate chatgpt responses
        public override async Task MessageReceived(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            using (message.Channel.EnterTypingState())
            {
                string previousContent = null;
                if (message.Reference != null)
                {
                    var referencedMessage = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
                    previousContent = referencedMessage?.Content;
                }

                var response = await ChatGPT.Chat(message.Content, previousContent);
                await message.Channel.SendMessageAsync(response.choices[0].message.content, messageReference: new MessageReference(message.Id));
            }
        }
    }
}

