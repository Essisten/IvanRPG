using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using VkNet.Model.RequestParams;
using static System.Math;

namespace IvanRPG
{
    class Group
    {
        public List<Unit> Team = new();
        private int[] destination = new int[2];
        public City GroupOwner;
        private long? chatID_response;
        Timer Timer;
        public Group(City own, int y, int x, long? chat)
        {
            GroupOwner = own;
            chatID_response = chat;
            destination[0] = y;
            destination[1] = x;
            int c_x = 0, c_y = 0;
            own.GetCoords(ref c_x, ref c_y);
            int distance = Convert.ToInt32(Ceiling(Sqrt(Sqrt(Abs(x - c_x)) + Sqrt(Abs(y - c_y)))));
            Timer = new(60000 * distance);
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
            Timer.AutoReset = false;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Cell target = Program.Map[destination[0], destination[1]];
            int gold = 0, wood = 0, stone = 0;
            string msg = "";
            Random rnd = new();
            bool success = false;
            switch (target.Type)
            {
                case 1:
                    City target_city = (City)target;
                    if (target_city.Owner == GroupOwner.Owner)
                        break;
                    target_city.Defend(ref Team);
                    if (Team.Count > 0)
                    {
                        int total_slots = Team.Sum(_ => _.Slots);
                        success = true;
                        while (total_slots > 0)
                        {
                            if (target_city.Wood > 0)
                            {
                                wood++;
                                target_city.Wood--;
                                total_slots--;
                            }
                            if (total_slots <= 0)
                                break;
                            if (target_city.Stone > 0)
                            {
                                stone++;
                                target_city.Stone--;
                                total_slots--;
                            }
                            if (total_slots <= 0)
                                break;
                            if (target_city.Gold > 0)
                            {
                                gold++;
                                target_city.Gold--;
                                total_slots--;
                            }
                            else if (target_city.Wood == 0 && target_city.Stone == 0)
                                break;
                        }
                    }
                    break;
                case 2:
                case 3:
                case 4:
                    break;
                case 5:
                    msg = "Нападение на деревню ещё не реализовано";
                    break;
            }
            if (msg == "")
            {
                if (success)
                {
                    msg = "Поход удался.\nПолучено:\n";
                    if (gold > 0)
                        msg += $"{gold} золота\n";
                    if (wood > 0)
                        msg += $"{wood} древесины\n";
                    if (stone > 0)
                        msg += $"{stone} камня";
                }
                else
                    msg = "Никто из солдат не вернулся домой...";
            }
            GroupOwner.Units.AddRange(Team);
            Program.api.Messages.Send(new MessagesSendParams
            {
                Message = msg,
                PeerId = chatID_response,
                RandomId = rnd.Next()
            });
        }
    }
}