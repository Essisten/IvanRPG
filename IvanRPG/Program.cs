using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections;
using System.Text.Json;
using System.Timers;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Keyboard;
using System.Collections.Generic;

namespace IvanRPG
{
    class Program
    {
        private const string api_key = "vk1.a.aXxURvxpJe_O7zUDUv2Lv4H4wKrwK6h3Qn9hKwpssAwMSvy20ynHaCJD1IXKYewRrS2LpokPVrud7JhKzJ4XHm0HAPXpDspC7k7gyD_CrDk0YbmFHRf465nER7YqZvlFUoI62wN1uwzRkJv3ms66ggoje_wOCzTwee0gin3qM_hNIn_4EHIPoLJo54CL6bbRtELTHhTnw7a-CBPF7iJc8Q";
        private const int group_id = 199833644;
        public static readonly VkApi api = new();
        public static Timer Timer = new(1000);
        public static KeyboardBuilder key = new();
        public static MessageKeyboard keyboard_current = key.Build();
        static void Main(string[] args)
        {
            Console.WriteLine("Авторизация...");
            Auth();
            Random rnd = new();
            Console.WriteLine("Авторизация прошла успешно");
            Timer.Elapsed += Timer_Second;
            Timer.Enabled = true;
            Timer.Start();
            BotCommandHandler.InitKeyboards();
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
                keyboard_current = key.Clear().Build();
                if (BotCommandHandler.Started)
                    keyboard_current = BotCommandHandler.keyboard_game;
                else
                    keyboard_current = BotCommandHandler.keyboard_start;
                foreach (var a in poll.Updates)
                {
                    if (a.Type == GroupUpdateType.MessageNew)
                    {
                        string UserMessage = a.MessageNew.Message.Text.ToLower();
                        long? peerID = a.MessageNew.Message.PeerId;
                        long? userID = a.MessageNew.Message.FromId;
                        City user = City.Cities.Find(_ => _.Owner == userID);
                        UserVK vk_user = UserVK.AddUser(userID.Value);
                        string respond = "";
                        if (user != null || UserMessage == "!клава" || UserMessage.Contains("!играть ") || vk_user.cache[0] == "join" ||
                                 (CanUse(ref respond, userID.Value) && UserMessage == "!начать"))
                        {
                            //try
                            //{
                            switch (UserMessage)
                            {
                                case "тест":
                                    respond = "тесто?";
                                    break;
                                case "!начать":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Игра уже идёт"))
                                        break;
                                    BotCommandHandler.StartGameCommand(ref respond, userID);
                                    break;
                                case "!инфо":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                        break;
                                    BotCommandHandler.GetUserInfoCommand(ref respond, user);
                                    break;
                                case "!дерево":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                        break;
                                    if (BotCommandHandler.CollectWoodCommand(ref respond, user))
                                        break;
                                    break;
                                case "!камень":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                        break;
                                    if (BotCommandHandler.CollectStoneCommand(ref respond, user))
                                        break;
                                    break;
                                case "!золото":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                        break;
                                    if (BotCommandHandler.CollectGoldCommand(ref respond, user))
                                        break;
                                    break;
                                case "!клава":
                                    BotCommandHandler.ToggleKeyboardCommand(ref respond);
                                    break;
                                default:
                                    if (Regex.IsMatch(UserMessage, "^!играть (.+)$"))
                                    {
                                        if (BotCommandHandler.GameIsStarted(ref respond, "А вот раньше надо было"))
                                            break;
                                        BotCommandHandler.JoinTheGameCommand(ref respond, user, userID, UserMessage);
                                    }
                                    else if (Regex.IsMatch(UserMessage, "^!нанять ((?:дд)|(?:танк))(?: (\\d+))?$"))
                                    {
                                        if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                            break;
                                        if (BotCommandHandler.HireUnitCommand(ref respond, user, UserMessage))
                                            break;
                                    }
                                    else if (Regex.IsMatch(UserMessage, "^!отправить (\\d+)(\\w) (\\d+) (\\d+)$"))
                                    {
                                        if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                            break;
                                        if (BotCommandHandler.SendGroupCommand(ref respond, user, UserMessage, peerID))
                                            break;
                                    }
                                    else
                                    {
                                        switch (vk_user.cache[0])
                                        {
                                            case "join":
                                                if (BotCommandHandler.GameIsStarted(ref respond, "А вот раньше надо было"))
                                                    break;
                                                BotCommandHandler.JoinTheGameCommand(ref respond, user, userID, UserMessage);
                                                vk_user.cache[0] = "";
                                                break;
                                        }
                                    }
                                    break;
                            }
                            /*}
                            catch (Exception e)
                            {
                                respond = "Ой-ой! Что-то я запуталась...\n\n" + e.Message;
                            }*/
                        }
                        if (respond == "") continue;
                        MessagesSendParams param = new()
                        {
                            Message = respond,
                            PeerId = peerID,
                            RandomId = rnd.Next()
                        };
                        if (BotCommandHandler.keyboard_enabled || (!BotCommandHandler.keyboard_enabled && UserMessage == "!клава"))
                            param.Keyboard = keyboard_current;
                        api.Messages.Send(param);
                    }
                    else if (a.Type == GroupUpdateType.MessageEvent)
                    {
                        MessagePayloadBody payload = JsonSerializer.Deserialize<MessagePayloadBody>(a.MessageEvent.Payload);
                        long? peerID = a.MessageEvent.PeerId;
                        long? userID = a.MessageEvent.UserId;
                        City user = City.Cities.Find(_ => _.Owner == userID);
                        UserVK vk_user = UserVK.AddUser(userID.Value);
                        string respond = "";
                        if (user != null || payload.button == "-keyboard" || payload.button == "join" ||
                                        (CanUse(ref respond, userID.Value) && payload.button == "start"))
                        {
                            switch (payload.button)
                            {
                                case "info":
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Дождись начала игры", true))
                                        break;
                                    BotCommandHandler.GetUserInfoCommand(ref respond, user);
                                    break;
                                case "-keyboard":
                                    BotCommandHandler.ToggleKeyboardCommand(ref respond);
                                    break;
                                case "join":
                                    BotCommandHandler.JoinTheGameCommand(ref respond, user, userID);
                                    break;
                                case "start":
                                    if (!CanUse(ref respond, userID.Value))
                                        break;
                                    if (BotCommandHandler.GameIsStarted(ref respond, "Игра уже идёт"))
                                        break;
                                    BotCommandHandler.StartGameCommand(ref respond, userID);
                                    break;
                            }
                        }
                        if (respond == "") continue;
                        MessagesSendParams param = new()
                        {
                            Message = respond,
                            PeerId = peerID,
                            RandomId = rnd.Next()
                        };
                        if (BotCommandHandler.keyboard_enabled || payload.button == "-keyboard")
                            param.Keyboard = keyboard_current;
                        api.Messages.Send(param);
                        api.Messages.SendMessageEventAnswer(a.MessageEvent.EventId, userID.Value, peerID.Value);
                    }
                }
            }
        }

        private static void Timer_Second(object sender, ElapsedEventArgs e)
        {
            foreach (City city in City.Cities)
            {
                if (city.WoodFarm_CD > 0)
                    city.WoodFarm_CD--;
                if (city.StoneFarm_CD > 0)
                    city.StoneFarm_CD--;
                if (city.GoldFarm_CD > 0)
                    city.GoldFarm_CD--;
            }
        }

        public static void Auth()
        {
            api.Authorize(new ApiAuthParams()
            {
                AccessToken = api_key
            });

        }

        /// <summary>
        /// Проверяет, является ли отправитель админом
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user_id">идентификатор пользователя</param>
        /// <returns></returns>
        public static bool CanUse(ref string respond, long user_id)
        {
            if (BotCommandHandler.Admins.Contains(user_id))
            {
                return true;
            }
            else
            {
                respond = "Пускай админ запустит игру";
                return false;
            }
        }
    }
}