using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Model.Keyboard;

namespace IvanRPG
{
    class Program
    {
        public static readonly VkApi api = new();
        private static readonly string key = "vk1.a.aXxURvxpJe_O7zUDUv2Lv4H4wKrwK6h3Qn9hKwpssAwMSvy20ynHaCJD1IXKYewRrS2LpokPVrud7JhKzJ4XHm0HAPXpDspC7k7gyD_CrDk0YbmFHRf465nER7YqZvlFUoI62wN1uwzRkJv3ms66ggoje_wOCzTwee0gin3qM_hNIn_4EHIPoLJo54CL6bbRtELTHhTnw7a-CBPF7iJc8Q";
        private static readonly int group_id = 199833644;
        public static readonly long?[] Admins = new long?[] { 559144282, 334913416 };
        public static Cell[,] Map = new Cell[5, 5];
        private static bool started = false;
        private static readonly string alphabet = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШ";
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
                        //try
                        //{
                            switch (UserMessage)
                            {
                                case "тест":
                                    respond = "тесто?";
                                    break;
                                case "!начать":
                                    if (started)
                                    {
                                        respond = "Игра уже во всю идёт";
                                        break;
                                    }
                                    if (!Admins.Contains(userID))
                                        respond = "Не тебе решать когда начинать игру";
                                    else
                                    {
                                        StartGame();
                                        respond = "Игра начата!\n\n" + GetMap();
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
                                        else if (City.Cities.Count >= Map.Rank * Map.Length)
                                        {
                                            respond = "Слишком много игроков! Они даже не поместятся на карте!";
                                            break;
                                    }
                                    else if (started)
                                    {
                                        respond = "А вот раньше надо было";
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
                                        if (!started)
                                        {
                                            respond = "Дождись начала игры";
                                            break;
                                        }
                                        Regex regex = new Regex("^!нанять ((?:дд)|(?:танк))(?: (\\d+))?$");
                                        string name = regex.Match(UserMessage).Groups[1].Value;
                                        int amount = 1;
                                        string temp_string = regex.Match(UserMessage).Groups[2].Value;
                                        if (temp_string != "")
                                            amount = Convert.ToInt32(temp_string);
                                        amount = Math.Min(5 - user.Units.Count, amount);
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
                                            unit = new Unit(2);
                                        else
                                            unit = new Unit(1);
                                        user.Gold -= 2 * amount;
                                        for (int i = 0; i < amount; i++)
                                        {
                                            user.Units.Add(unit);
                                        }
                                        respond = $"Нанят {name}";
                                        if (amount > 1)
                                            respond += $" {amount} штуки";
                                    }
                                    else if (Regex.IsMatch(UserMessage, "^!отправить (\\d+)(\\w) (\\d+) (\\d+)$"))
                                    {
                                        if (!started)
                                        {
                                            respond = "Дождись начала игры";
                                            break;
                                        }
                                        Regex regex = new("^!отправить (\\d+)(\\w) (\\d+) (\\d+)$");
                                            string h_coord = regex.Match(UserMessage).Groups[1].Value,
                                                v_coord = regex.Match(UserMessage).Groups[2].Value.ToUpper(),
                                                dd_amount = regex.Match(UserMessage).Groups[3].Value,
                                                tank_amount = regex.Match(UserMessage).Groups[4].Value;
                                        int x = Convert.ToInt32(h_coord) - 1,
                                            y = alphabet.IndexOf(v_coord[0]),
                                            dd = Convert.ToInt32(dd_amount),
                                            tank = Convert.ToInt32(tank_amount);
                                        if (y == -1 || y > Map.GetLength(0) || x > Map.GetLength(1))
                                        {
                                            respond = "Неверные координаты";
                                            break;
                                        }
                                        Cell target = Map[y, x];
                                        if (target.Type == 1)
                                        {
                                            City c = (City)target;
                                            if (c.Owner == user.Owner)
                                            {
                                                respond = "Нельзя нападать на себя";
                                                break;
                                            }
                                        }
                                        Group g = new(user, y, x, peerID);
                                        for (int i = 0; i < user.Units.Count; i++)
                                        {
                                            Unit unit = user.Units[i];
                                            if (unit.Type == 1 && tank > 0)
                                                tank--;
                                            else if (unit.Type == 2 && dd > 0)
                                                dd--;
                                            else
                                                continue;
                                            g.Team.Add(unit);
                                            user.Units.Remove(unit);
                                            i--;
                                        }
                                        if (g.Team.Count == 0)
                                        {
                                            respond = "Фарш кончился";
                                            break;
                                        }
                                        user.Groups.Add(g);
                                    }
                                    else if (Regex.IsMatch(UserMessage, "^!инфо"))
                                    {
                                        if (!started)
                                        {
                                            respond = "Дождись начала игры";
                                            break;
                                        }
                                        respond = $"Название: {user.Name}\n";
                                        int x = 0, y = 0;
                                        for (int i = 0; i < Map.GetLength(0); i++)
                                        {
                                            for (int k = 0; k < Map.GetLength(1); k++)
                                            {
                                                if (Map[i, k].Type != 1)
                                                    continue;
                                                if (user.Owner == ((City)Map[i, k]).Owner)
                                                {
                                                    y = i;
                                                    x = k;
                                                }
                                            }
                                        }
                                        respond += $"Координаты: {x + 1}{alphabet[y]}\n\n";
                                        respond += $"Танки: {user.Units.Count(_ => _.Type == 1)}\n";
                                        respond += $"Дамагеры: {user.Units.Count(_ => _.Type == 2)}\n\n";
                                        respond += $"Древесина: {user.Wood}\n";
                                        respond += $"Камни: {user.Stone}\n";
                                        respond += $"Золото: {user.Gold}\n\n";
                                        respond += $"Уровень стен: {user.Wall}\n";
                                        respond += $"Уровень казарм: {user.Barracks}\n";
                                        respond += $"Уровень рынка: {user.Market}\n";
                                        respond += $"Уровень ратуши: {user.MainBuild}\n\n";
                                        respond += $"Уровень лесопильни: {user.WoodFarm}\n";
                                        respond += $"Уровень каменноломни: {user.StoneFarm}\n";
                                        respond += $"Уровень золотого рудника: {user.GoldFarm}\n\n";
                                        respond += $"Число жизней: {user.Lifes}\n";
                                    }
                                    break;
                            }
                        /*}
                        catch (Exception e)
                        {
                            respond = "Ой-ой! Что-то я запуталась...\n\n" + e.Message;
                        }*/
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
        public static void StartGame()
        {
            Random r = new();
            //Заполнение взрыбами
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int k = 0; k < Map.GetLength(1); k++)
                {
                    Map[i, k] = new Cell();
                }
            }
            //Заполнение игроками
            for (int i = 0; i < City.Cities.Count; i++)
            {
                int y = r.Next(Map.GetLength(0)),
                    x = r.Next(Map.GetLength(1));
                Cell place = Map[y, x];
                if (place.Type != 0)
                {
                    i--;
                    continue;
                }
                Map[y, x] = City.Cities[i];
            }
            //Заполнение всем остальным
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int k = 0; k < Map.GetLength(1); k++)
                {
                    Cell spot = Map[i, k];
                    if (spot.Type != 0)
                        continue;
                    spot.Type = r.Next(2, 6);
                }
            }
            started = true;
        }
        public static string GetMap()
        {
            string map = "__1️⃣2️⃣3️⃣4️⃣5️⃣\n";
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                map += $"{alphabet[i]} ";
                for (int k = 0; k < Map.GetLength(1); k++)
                {
                    map += Map[i, k].GetIcon();
                }
                map += "\n";
            }
            return map;
        }
    }
}
