using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.Keyboard;
using VkNet.Model.RequestParams;

namespace IvanRPG
{
    static class BotCommandHandler
    {
        private const string alphabet = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЭЮЯ";
        public static bool keyboard_enabled = true;
        public static readonly long?[] Admins = new long?[] { 559144282, 334913416 };
        public static bool Started = false;
        public static Cell[,] Map = new Cell[5, 5];
        public static MessageKeyboard keyboard_start, keyboard_game;
        /// <summary>
        /// Команда при запросе информации о пользователе
        /// </summary>
        /// <param name="respond">возвращаемый ответ</param>
        /// <param name="user">пользователь</param>
        /// <returns>Статистика пользователя</returns>
        public static string GetUserInfoCommand(ref string respond, City user)
        {
            int x = 0, y = 0;
            user.GetCoords(ref x, ref y);
            StringBuilder str = new($"Название: {user.Name}\n");
            str.Append($"Координаты: {x + 1}{alphabet[y]}\n\n");
            str.Append($"Танки: {user.Units.Count(_ => _.Type == 1)}\n");
            str.Append($"Дамагеры: {user.Units.Count(_ => _.Type == 2)}\n\n");
            str.Append($"Древесина: {user.Wood}\n");
            str.Append($"Камни: {user.Stone}\n");
            str.Append($"Золото: {user.Gold}\n\n");
            str.Append($"Уровень стен: {user.Wall}\n");
            str.Append($"Уровень казарм: {user.Barracks}\n");
            str.Append($"Уровень рынка: {user.Market}\n");
            str.Append($"Уровень ратуши: {user.MainBuild}\n\n");
            str.Append($"Уровень лесопильни: {user.WoodFarm}\n");
            str.Append($"Уровень каменноломни: {user.StoneFarm}\n");
            str.Append($"Уровень золотого рудника: {user.GoldFarm}\n\n");
            str.Append($"Число жизней: {user.Lifes}\n");
            respond = str.ToString();
            return respond;
        }

        /// <summary>
        /// Команда для запуска игры
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="userID">идентификатор пользователя ВК</param>
        /// <returns>Строка с ответом</returns>
        public static string StartGameCommand(ref string respond, long? userID)
        {
            if (!Admins.Contains(userID))
                respond = "Не тебе решать когда начинать игру";
            else
            {
                StartGame();
                respond = "Игра начата!\n\n" + GetMap();
                Program.keyboard_current = keyboard_game;
            }
            return respond;
        }

        /// <summary>
        /// Присоединяет игрока к игре
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <param name="UserMessage">команда</param>
        /// <param name="userID">Идентификатор пользователя</param>
        /// <returns>Провалена ли попытка добавления?</returns>
        public static void JoinTheGameCommand(ref string respond, City user, long? userID, string UserMessage = "")
        {
            if (user != null)
            {
                respond = "Ты уже в очереди, не переживай";
                return;
            }
            if (City.Cities.Count >= Map.Rank * Map.Length)
            {
                respond = "Слишком много игроков! Они даже не поместятся на карте!";
                return;
            }
            string name;
            if (UserMessage == "")
            {
                respond = "Введи название города";
                UserVK.Users[userID.Value].cache[0] = "join";
                return;
            }
            else
            {
                if (UserVK.Users[userID.Value].cache[0] == "join")
                    name = UserMessage;
                else
                {
                    Regex regex = new("^!играть (.+)$");
                    name = regex.Match(UserMessage).Groups[1].Value;
                }
            }
            City.Cities.Add(new City(name, userID.GetValueOrDefault()));
            respond = $"Добавлен город \"{name}\"\nИгроков в ожидании: {City.Cities.Count}";
        }

        /// <summary>
        /// Команда найма юнита
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <param name="UserMessage">сообщение пользователя</param>
        /// <returns>Провалена ли попытка найма?</returns>
        public static bool HireUnitCommand(ref string respond, City user, string UserMessage)
        {
            Regex regex = new("^!нанять ((?:дд)|(?:танк))(?: (\\d+))?$");
            string name = regex.Match(UserMessage).Groups[1].Value;
            int amount = 1;
            string temp_string = regex.Match(UserMessage).Groups[2].Value;
            if (temp_string != "")
                amount = Convert.ToInt32(temp_string);
            amount = Math.Min(10 * user.Barracks - user.Units.Count, amount);
            if (user.Gold < 2 * amount)
            {
                respond = "Недостаточно золота для найма";
                return true;
            }
            else if (user.Units.Count >= 10 * user.Barracks)
            {
                respond = "Нет места в казармах";
                return true;
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
            return false;
        }

        /// <summary>
        /// Команда отправки группы по координатам
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <param name="UserMessage">сообщение пользователя</param>
        /// <param name="peerID">идентификатор диалога</param>
        /// <returns>Провалена ли отправка?</returns>
        public static bool SendGroupCommand(ref string respond, City user, string UserMessage, long? peerID)
        {
            Random rnd = new();
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
                return true;
            }
            Cell target = Map[y, x];
            if (target.Type == 1)
            {
                City c = (City)target;
                if (c.Owner == user.Owner)
                {
                    respond = "Нельзя нападать на себя";
                    return true;
                }
                Program.api.Messages.Send(new MessagesSendParams
                {
                    Message = $"На тебя собирается напасть игрок \"{user.Name}\"!",
                    PeerId = c.Owner,
                    RandomId = rnd.Next()
                });
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
                return true;
            }
            user.Groups.Add(g);
            return false;
        }

        /// <summary>
        /// Команда сбора древесины
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <returns>Провалена ли попытка сбора?</returns>
        public static bool CollectWoodCommand(ref string respond, City user)
        {
            if (user.WoodFarm_CD > 0)
            {
                respond = $"Сбор ресурса будет доступень лишь через {user.WoodFarm_CD} секунд";
                return true;
            }
            int collected = 10 * user.WoodFarm;
            user.Wood += collected;
            user.WoodFarm_CD = 30;
            respond = $"Собрано {collected} древесины";
            return false;
        }
        /// <summary>
        /// Команда сбора камня
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <returns>Провалена ли попытка сбора?</returns>
        public static bool CollectStoneCommand(ref string respond, City user)
        {
            if (user.StoneFarm_CD > 0)
            {
                respond = $"Сбор ресурса будет доступень лишь через {user.StoneFarm_CD} секунд";
                return true;
            }
            int collected = 10 * user.StoneFarm;
            user.Stone += collected;
            user.StoneFarm_CD = 30;
            respond = $"Собрано {collected} камня";
            return false;
        }
        /// <summary>
        /// Команда сбора золота
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="user">пользователь</param>
        /// <returns>Провалена ли попытка сбора?</returns>
        public static bool CollectGoldCommand(ref string respond, City user)
        {
            if (user.GoldFarm_CD > 0)
            {
                respond = $"Сбор ресурса будет доступень лишь через {user.GoldFarm_CD} секунд";
                return true;
            }
            int collected = 10 * user.GoldFarm;
            user.Gold += collected;
            user.GoldFarm_CD = 30;
            respond = $"Собрано {collected} золота";
            return false;
        }

        /// <summary>
        /// Команда переключения клавиатуры
        /// </summary>
        /// <param name="respond">ответ</param>
        /// <param name="keyboard_enabled">включена ли клавиатура</param>
        public static void ToggleKeyboardCommand(ref string respond)
        {
            keyboard_enabled = !keyboard_enabled;
            if (keyboard_enabled)
            {
                respond = "Нравятся кнопочки?";
            }
            else
            {
                respond = "Клавиатура скрыта";
                Program.keyboard_current = Program.key.Clear().Build();
            }
        }

        /// <summary>
        /// Размещает объекты на карте мира
        /// </summary>
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
            Started = true;
        }


        /// <summary>
        /// Формирует карту мира
        /// </summary>
        /// <returns>Строка, содержащая карту</returns>
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

        /// <summary>
        /// Возвращает ответ в случае, если игра начата
        /// </summary>
        /// <param name="respond">Возвращаемая строка</param>
        /// <param name="answer">Предполагаемый ответ</param>
        /// <param name="invert">Инверсия вопроса</param>
        /// <returns>ответ</returns>
        public static bool GameIsStarted(ref string respond, string answer, bool invert = false)
        {
            bool s = Started;
            if (invert)
                s = !s;
            if (s)
                respond = answer;
            return s;
        }

        public static void InitKeyboards()
        {
            KeyboardBuilder key = new();
            key.AddButton(new()
            {
                Label = "Присоединиться",
                ActionType = KeyboardButtonActionType.Callback,
                Extra = "join"
            });
            key.AddButton(new()
            {
                Label = "Начать",
                ActionType = KeyboardButtonActionType.Callback,
                Extra = "start"
            });
            key.AddButton(new()
            {
                Label = "Скрыть клавиатуру",
                ActionType = KeyboardButtonActionType.Callback,
                Extra = "-keyboard"
            });
            keyboard_start = key.Build();
            key.Clear();


            key.AddButton(new()
            {
                Label = "Мой город",
                ActionType = KeyboardButtonActionType.Callback,
                Extra = "info"
            });
            key.AddButton(new()
            {
                Label = "Скрыть клавиатуру",
                ActionType = KeyboardButtonActionType.Callback,
                Extra = "-keyboard"
            });
            keyboard_game = key.Build();
        }

    }
}
