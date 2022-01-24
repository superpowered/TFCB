using System;
using UnityEngine;

namespace TFCB
{
    public class OnRotateArgs : EventArgs
    {
        public string Direction;
        public Vector3 CameraPosition;
        public Vector2Int WorldPosition;

        public Vector3 PreviousCameraPosition;
        public Vector2Int PreviousWorldPosition;
    }
}
