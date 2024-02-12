using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace ChatGPT_Integration
{
    public static class ChatGPT
    {
        // found at https://platform.openai.com/api-keys
        public const string apiKey = "";

        // made this way for better readability
        static string Context()
        {
            string context = "";

            // Who is the bot?
            context += $"Your name is {Hub.client.CurrentUser.Username}. ";
            context += "You are a bot in a discord server. ";


            // Personality [nice]
            context += "You are a friendly and easy going. Always be polite, positive, and ready to help. ";

            /* Personality [rude]
            context += "You are not friendly or easy going. Never be polite, positive, or ready to help. ";
            */

            // Define Moderators
            context += "This discord server has moderators who are JokerJosh, JokerJosh2, and JokerJosh3. ";

            return context;
        }


        /// BORING STUFF UP AHEAD
        
        public class Message { public string role { get; set; } public string content { get; set; } }
        public class ChatResponse
        {
            public List<Choice> choices { get; set; }
            public class Choice
            {
                public Message message { get; set; }
            }
        }

        public class Response
        {
            public string id { get; set; }
            public string model { get; set; }
            public string system_fingerprint { get; set; }
            public List<Choice> choices { get; set; }
            public class Choice
            {
                public int index;
                public Message message { get; set; }
                public string logprobs { get; set; }
                public string finish_reason { get; set; }
            }
            public class Usage
            {
                public int promy_tokens { get; set; }
                public int completion_tokens { get; set; }
                public int total_tokens { get; set; }
            }
        }

        public static async Task<Response> Chat(string message, string previousMessage = "", int? maxTokens = 100)
        {
            List<Message> messages = new List<Message>
            {
                // this determines how the bot will act
                new Message { role = "system", content = Context() },
            };

            // let the ai know that this was its own message
            if (!string.IsNullOrEmpty(previousMessage))
                messages.Add(new Message { role = "assistant", content = previousMessage });

            messages.Add(new Message { role = "user", content = message });

            object data = new
            {
                model = "gpt-3.5-turbo",
                messages = messages,
                max_tokens = maxTokens
            };

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error: {response.StatusCode}. Details: {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Response Content = JsonConvert.DeserializeObject<Response>(responseContent);

                // prevent pinging @everyone or any other role/user (yes the bot can ping everyone if someone asks it to, i learnt the hard way in a server with 20k people :( )
                Content.choices[0].message.content.Replace("@", "");

                return Content;
            }
        }
    }
}