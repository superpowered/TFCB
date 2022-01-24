using Unity.Mathematics;

namespace TFCB
{
    public class Cell
    {
        public int Id;
        public int OriginId;
        public bool Solid;
        public int2 Position;
        public int2 OriginPosition;

        public GroundType GroundType;
        public StructureType StructureType;
        public OverlayType OverlayType;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is Cell)
            {
                Cell p = obj as Cell;
                return this.Position.x == p.Position.x && this.Position.y == p.Position.y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 6949;
                hash = hash * 7907 + Position.x.GetHashCode();
                hash = hash * 7907 + Position.y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return "P(" + this.Position.x + ", " + this.Position.y + ")";
        }
    }
}
