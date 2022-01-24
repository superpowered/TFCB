using System;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

namespace TFCB
{
    public class MapSystem : SimulationSystem
    {
        public static event EventHandler<OnMapEventArgs> OnUpdateMapRender;
        private WorldMap _worldMap;
        private int _rotation = 0;

        private void SetupEvents()
        {
            SimulationManager.OnTick += Tick;
            User.OnRotate += HandleRotate;
        }

        private void GenerateWorldMap()
        {
            _worldMap = new WorldMap(MapInfo.WorldMapSize);

            for (int id = 0; id < _worldMap.Area; id++)
            {
                Cell cell = new Cell
                {
                    OriginId = id,
                    Id = id,
                    Solid = false,
                    Position = IdToPosition(id),
                    OriginPosition = IdToPosition(id),
                    GroundType = GroundType.Floor1,
                    StructureType = StructureType.None,
                    OverlayType = OverlayType.None,
                };

                _worldMap.Cells.Add(cell);
            }

            testStructures();

            OnUpdateMapRender?.Invoke(this, new OnMapEventArgs { WorldMap = _worldMap });
        }

        private void testStructures()
        {
            SetCell(0, 0, GroundType.Floor2);

            SetCell(0, 1, OverlayType.Outline1);
            SetCell(-1, 0, OverlayType.Outline2);

            SetCell(7, 7, StructureType.Wall1);
            SetCell(7, 8, StructureType.Wall1);
            SetCell(8, 7, StructureType.Wall1);
            SetCell(8, 8, StructureType.Wall1);

            SetCell(4, 4, StructureType.Wall1);
            SetCell(4, -4, StructureType.Wall1);
            SetCell(-4, -4, StructureType.Wall1);
            SetCell(-6, -6, StructureType.Wall1);

            SetCell(5, 5, StructureType.Wall2);
            SetCell(-5, 5, StructureType.Wall2);
            SetCell(-5, -5, StructureType.Wall2);
            SetCell(-6, 6, StructureType.Wall2);
        }

        private Cell GetCell(int id)
        {
            if (id < 0 || id > _worldMap.Area)
            {
                return null;
            }
            else
            {
                return _worldMap.Cells[id];
            }
        }

        private Cell GetCell(int x, int y)
        {
            int cellId = PositionToId(x, y);

            return GetCell(cellId);
        }

        private Cell GetCell(int2 position)
        {
            return GetCell(position.x, position.y);
        }

        private void SetCell(int x, int y, GroundType groundType)
        {
            if (OnMap(x, y))
            {
                Cell cell = GetCell(x, y);

                cell.GroundType = groundType;
            }
        }

        private void SetCell(int x, int y, StructureType structureType)
        {
            if (OnMap(x, y))
            {
                Cell cell = GetCell(x, y);

                cell.Solid = true;
                cell.StructureType = structureType;
            }
        }

        private void SetCell(int x, int y, OverlayType overlayType)
        {
            if (OnMap(x, y))
            {
                Cell cell = GetCell(x, y);

                cell.OverlayType = overlayType;
            }
        }

        private bool OnMap(int x, int y)
        {
            bool insideHorizontalBounds = x >= -_worldMap.Size && x <= _worldMap.Size;
            bool insideVerticalBounds = y >= -_worldMap.Size && y <= _worldMap.Size;

            return insideHorizontalBounds && insideVerticalBounds;
        }

        private bool OnMap(int2 position)
        {
            return OnMap(position.x, position.y);
        }

        private int2 IdToPosition(int id)
        {
            int x = id % _worldMap.Width - _worldMap.Size;
            int y = id / _worldMap.Width - _worldMap.Size;
            return new int2(x, y);
        }

        private int PositionToId(int x, int y)
        {
            return (x + _worldMap.Size) + _worldMap.Width * (y + _worldMap.Size);
        }

        private int PositionToId(int2 position)
        {
            return PositionToId(position.x, position.y);
        }

        private void HandleRotate(object sender, OnRotateArgs eventArgs)
        {
            _worldMap.Cells = _worldMap.Cells.ConvertAll(cell => rotateCell(cell, eventArgs.Direction));
            updateCurrentRotation(eventArgs.Direction);
            OnUpdateMapRender?.Invoke(this, new OnMapEventArgs { WorldMap = _worldMap });
        }

        private void updateCurrentRotation(string direction)
        {
            // adjust world rotation and keep it within 0,90,180,270
            _rotation = direction == "left" ? _rotation + 90 : _rotation - 90;
            // TODO: function for this? V
            if (_rotation == 360)
                _rotation = 0;
            if (_rotation == -90)
                _rotation = 270;
        }

        // TODO: This function is a duplicate. Make a util?
        /// <summary>
        /// Returns tile's rotated position
        /// </summary>
        /// <param name="direction"> "left" = ⟲, "right" = ⟳</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Rotated position</returns>
        private int2 rotateTile(string direction, int x, int y)
        {
            return direction == "right" ? new int2(y, -x) : new int2(-y, x);
        }

        private Cell rotateCell(Cell cell, string direction)
        {
            return new Cell
            {
                OriginId = cell.OriginId,
                Id = PositionToId(cell.Position),
                Solid = cell.Solid,
                Position = rotateTile(direction, cell.Position.x, cell.Position.y),
                OriginPosition = cell.OriginPosition,
                GroundType = cell.GroundType,
                StructureType = cell.StructureType,
                OverlayType = cell.OverlayType,
            };
        }

        public override void Init()
        {
            SetupEvents();
            GenerateWorldMap();
        }

        public override void Quit()
        {
            SimulationManager.OnTick -= Tick;
        }

        public bool IsSolid(int x, int y)
        {
            if (OnMap(x, y))
            {
                Cell cell = GetCell(x, y);

                return cell.Solid;
            }
            else
            {
                return true;
            }
        }

        public bool IsSolid(int2 position)
        {
            return IsSolid(position.x, position.y);
        }

        // TODO: remove or refactor
        public int2? GetOpenPosition()
        {
            int2 cellPosition;
            int timeTried = 0;

            do
            {
                if (timeTried > _worldMap.Size * 100)
                {
                    return null;
                }

                cellPosition = new int2(
                    Utils.RandomRange(-_worldMap.Size, _worldMap.Size),
                    Utils.RandomRange(-_worldMap.Size, _worldMap.Size)
                );
                timeTried++;
            } while (IsSolid(cellPosition));

            return cellPosition;
        }
    }
}

