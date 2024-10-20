using System;
using System.Collections.Generic;

namespace IvanRPG
{
    class Cell
    {
        /// <summary>
        /// 0 - взрыба
        /// 1 - Дерево
        /// 2 - Камень
        /// 3 - Золото
        /// 4 - Город
        /// 5 - Деревня
        /// </summary>
        private static readonly string[] icons = new string[] { "🐟", "🌲", "🍙", "🌕", "🏠", "?" };
        public int Type { get; set; }
        public string GetIcon()
        {
            return icons[Type];
        }
    }
}
