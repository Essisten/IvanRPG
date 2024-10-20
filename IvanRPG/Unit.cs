using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IvanRPG
{
    class Unit
    {
        public int HP { get; set; }
        public int Block { get; set; }
        public int Attack { get; set; }
        public int Slots { get; set; }
        public int Class { get; set; }
        public Unit(int c)
        {
            switch (c)
            {
                case 1:
                    Attack = 5;
                    HP = 2;
                    Block = 0;
                    break;
                case 2:
                    Attack = 1;
                    HP = 10;
                    Block = 2;
                    break;
            }
            Class = c;
        }
    }
}
