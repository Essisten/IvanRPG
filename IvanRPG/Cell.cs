using System;
using System.Collections.Generic;

namespace IvanRPG
{
    class Cell
    {
        /// <summary>
        /// 0 - взрыба
        /// 1 - Город
        /// 2 - Дерево
        /// 3 - Камень
        /// 4 - Золото
        /// 5 - Деревня
        /// </summary>
        private static readonly string[] icons = new string[] { "🐟", "🏠", "🌲", "🍙", "🌕", "🇺🇸" };
        public int Type { get; set; }
        public Cell()
        {
            Type = 0;
        }
        public string GetIcon()
        {
            return icons[Type];
        }
    }
}
