using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace TFCB
{
    public class Citizen
    {
        private static int _nextCitizenId = 1;

        public int2 Position { get; set; }
        public Direction Direction { get; set; }
        public Direction Nation { get; set; }

        public int Id { get; private set; }

        public Citizen()
        {
            Id = _nextCitizenId++;
        }

        public void Tick()
        {

        }
    }
}
