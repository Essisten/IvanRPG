using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace IvanRPG
{
    class City : Cell
    {
        public static List<City> Cities = new List<City>();

        public List<Unit> Units = new();
        public List<Group> Groups = new();
        public string Name { get; set; }
        public long Owner { get; set; }

        public int MainBuild { get; set; }
        public int Barracks { get; set; }
        public int Wall { get; set; }
        public int Market { get; set; }
        public int WoodFarm { get; set; }
        public int StoneFarm { get; set; }
        public int GoldFarm { get; set; }

        public int Lifes { get; set; }
        public int Wood { get; set; }
        public int Stone { get; set; }
        public int Gold { get; set; }
        public int WoodFarm_CD { get; set; }
        public int StoneFarm_CD { get; set; }
        public int GoldFarm_CD { get; set; }

        public City(string name, long owner)
        {
            Name = name;
            Owner = owner;
            MainBuild = 1;
            Barracks = 1;
            Wall = 1;
            Market = 1;
            WoodFarm = 1;
            StoneFarm = 1;
            GoldFarm = 1;
            Lifes = 1;
            Wood = 50;
            Stone = 50;
            Gold = 1000;
            Type = 1;
        }

        public void Defend(ref List<Unit> attackers)
        {
            while (attackers.Count > 0 && Units.Count > 0)
            {
                FightTurn(ref attackers, ref Units);
                FightTurn(ref Units, ref attackers);
            }
        }
        private void FightTurn(ref List<Unit> attackers, ref List<Unit> defenders)
        {
            for (int attacker_id = 0; attacker_id < attackers.Count; attacker_id++)
            {
                defenders = defenders.OrderBy(_ => _.Type).ToList();
                attackers = attackers.OrderBy(_ => _.GetHashCode()).ToList();
                foreach (Unit def_unit in defenders)
                {
                    Unit attacker_unit = attackers[attacker_id];
                    def_unit.HP -= Math.Max(attacker_unit.Attack - def_unit.Block, 1);
                    attacker_id++;
                    if (attacker_id >= attackers.Count)
                        break;
                }
                defenders = defenders.Where(_ => _.HP > 0).ToList();
            }
        }
        public void GetCoords(ref int x, ref int y)
        {
            for (int i = 0; i < BotCommandHandler.Map.GetLength(0); i++)
            {
                for (int k = 0; k < BotCommandHandler.Map.GetLength(1); k++)
                {
                    if (BotCommandHandler.Map[i, k].Type != 1)
                        continue;
                    if (Owner == ((City)BotCommandHandler.Map[i, k]).Owner)
                    {
                        y = i;
                        x = k;
                    }
                }
            }
        }
    }
}