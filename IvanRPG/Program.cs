using System;
using System.Text.RegularExpressions;
using System.Linq;
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
        public static readonly long?[] Admins = new long?[] { 559144282, 334913416 };
        public static Cell[,] Map = new Cell[5, 5];
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
                        long? peerID = a.MessageNew.Message.PeerId;
                        long? userID = a.MessageNew.Message.FromId;
                        City user = City.Cities.Find(_ => _.Owner == userID);
                        string respond = "";
                        try
                        {
                            switch (UserMessage)
                            {
                                case "тест":
                                    respond = "тесто?";
                                    break;
                                case "!начать":
                                    if (!Admins.Contains(userID))
                                        respond = "Не тебе решать когда начинать игру";
                                    else
                                    {
                                        //Начало игры
                                    }
                                    break;
                                default:
                                    if (Regex.IsMatch(UserMessage, "^!играть (.+)$"))
                                    {
                                        if (user != null)
                                        {
                                            respond = "Ты уже в очереди, не переживай";
                                            break;
                                        }
                                        Regex regex = new("^!играть (.+)$");
                                        string name = regex.Match(UserMessage).Groups[1].Value;
                                        City.Cities.Add(new City(name, userID.GetValueOrDefault()));
                                        respond = $"Добавлен город \"{name}\"\nИгроков в ожидании: {City.Cities.Count}";
                                    }
                                    else if (Regex.IsMatch(UserMessage, "^!нанять ((?:дд)|(?:танк))(?: (\\d+))?$"))
                                    {
                                        if (user == null)
                                        {
                                            respond = "Да ты даже не играешь!";
                                            break;
                                        }
                                        Regex regex = new Regex("^!нанять ((?:дд)|(?:танк))(?: (\\d+))?$");
                                        string name = regex.Match(UserMessage).Groups[1].Value;
                                        int amount = 1;
                                        string temp_string = regex.Match(UserMessage).Groups[2].Value;
                                        if (temp_string != "")
                                            amount = Convert.ToInt32(temp_string);
                                        if (user.Gold < 2 * amount)
                                        {
                                            respond = "Недостаточно золота для найма";
                                            break;
                                        }
                                        else if (user.Units.Count >= 5)
                                        {
                                            respond = "Нет места в казармах";
                                            break;
                                        }
                                        Unit unit;
                                        if (name == "дд")
                                        {
                                            unit = new Unit(1);
                                        }
                                        else
                                        {
                                            unit = new Unit(2);
                                        }
                                        user.Gold -= 2 * amount;
                                        for (int i = 0; i < amount; i++)
                                        {
                                            user.Units.Add(unit);
                                        }
                                        respond = $"Нанят {name}";
                                        if (amount > 1)
                                            respond += $" {amount} штуки";
                                    }
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            respond = "Ой-ой! Что-то я запуталась... Может, перезапустите меня?\n\n" + e.Message;
                        }
                        if (respond == "") continue;
                        api.Messages.Send(new MessagesSendParams
                        {
                            Message = respond,
                            PeerId = peerID,
                            RandomId = rnd.Next()
                        });
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
