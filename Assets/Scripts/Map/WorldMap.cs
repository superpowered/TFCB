using System.Collections.Generic;
using System.Linq;

namespace TFCB
{
    public class WorldMap
    {
        public int Size { get; private set; }
        public int Width => 2 * Size + 1;
        public int Area => Width * Width;

        public List<Cell> Cells;

        public WorldMap(int size)
        {
            Size = size;
            Cells = new List<Cell>(Area);
        }
    }
}
