using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IvanRPG
{
    class City : Cell
    {
        public static List<City> Cities = new List<City>();

        public List<Unit> Units = new List<Unit>();
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
            Wood = 10;
            Stone = 10;
            Gold = 10;
            Type = 1;
        }
    }
}
