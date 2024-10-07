using System;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace IvanRPG
{
    class Program
    {
        private static readonly VkApi api = new();
        private static readonly string key = "vk1.a.aXxURvxpJe_O7zUDUv2Lv4H4wKrwK6h3Qn9hKwpssAwMSvy20ynHaCJD1IXKYewRrS2LpokPVrud7JhKzJ4XHm0HAPXpDspC7k7gyD_CrDk0YbmFHRf465nER7YqZvlFUoI62wN1uwzRkJv3ms66ggoje_wOCzTwee0gin3qM_hNIn_4EHIPoLJo54CL6bbRtELTHhTnw7a-CBPF7iJc8Q";
        private static readonly int group_id = 199833644;
        static void Main(string[] args)
        {
            Console.WriteLine("Авторизация...");
            Auth();
            Random rnd = new();
            Console.WriteLine("Авторизация прошла успешно");
            while (true)
            {
                var s = api.Groups.GetLongPollServer(ulong.Parse(group_id.ToString()));
                var poll = api.Groups.GetBotsLongPollHistory(new BotsLongPollHistoryParams()
                {
                    Server = s.Server,
                    Ts = s.Ts,
                    Key = s.Key,
                    Wait = 25

                });
                if (poll?.Updates == null) continue;
                foreach (var a in poll.Updates)
                {
                    if (a.Type == GroupUpdateType.MessageNew)
                    {
                        string UserMessage = a.MessageNew.Message.Text.ToLower();
                        long? userID = a.MessageNew.Message.PeerId;
                        if (UserMessage == "тест")
                        {
                            api.Messages.Send(new MessagesSendParams
                            {
                                Message = "тесто?",
                                PeerId = userID,
                                RandomId = rnd.Next()
                            });
                        }
                    }

                }
            }
        }
        public static void Auth()
        {
            api.Authorize(new ApiAuthParams()
            {
                AccessToken = key
            });

        }
    }
}
