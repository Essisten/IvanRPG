using System;
using System.Collections.Generic;
using System.Linq;

namespace IvanRPG
{
    class Unit
    {
        public int HP { get; set; }
        public int Block { get; set; }
        public int Attack { get; set; }
        public int Slots { get; set; }
        /// <summary>
        /// 1 - танк
        /// 2 - ДД
        /// </summary>
        public int Type { get; set; }
        public Unit(int c)
        {
            switch (c)
            {
                case 1:
                    Attack = 1;
                    HP = 10;
                    Block = 2;
                    Slots = 25;
                    break;
                case 2:
                    Attack = 5;
                    HP = 2;
                    Block = 0;
                    Slots = 10;
                    break;
            }
            Type = c;
        }
    }
}
