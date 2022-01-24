using System;
using UnityEngine;
using Unity.Mathematics;

namespace TFCB
{
    public class OnMainClickArgs : EventArgs
    {
        public int2 Position;
        public Vector2Int PositionVectorInt;
        public Vector3 CameraPos;
    }
}
