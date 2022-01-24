using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TFCB
{
    public class WorldRender : MonoBehaviour
    {
        private Grid _grid;
        private Tilemap _groundTilemap;
        private Tilemap _structureTilemap;
        private Tilemap _overlayTilemap;

        private Dictionary<GroundType, Tile> _groundTiles;
        private Dictionary<StructureType, Tile> _structureTiles;
        private Dictionary<OverlayType, Tile> _overlayTiles;

        private int _rotation = 0;

        private void Awake()
        {
            SetupEvents();

            SetupTilemapResources();
        }

        private void SetupEvents()
        {
            MapSystem.OnUpdateMapRender += UpdateMapRender;
            User.OnMainStart += Handle;
            User.OnRotate += HandleRot;
            //User.OnMouseHold += Handle;
            //User.OnMouseUp += Handle;
        }

        private void SetupTilemapResources()
        {
            _grid = GameObject.Find("Grid").GetComponent<Grid>();
            _groundTilemap = GameObject.Find("Ground").GetComponent<Tilemap>();
            _structureTilemap = GameObject.Find("Structure").GetComponent<Tilemap>();
            _overlayTilemap = GameObject.Find("Overlay").GetComponent<Tilemap>();

            _groundTiles = new Dictionary<GroundType, Tile>
            {
                [GroundType.None] = null,
                [GroundType.Floor1] = Resources.Load<Tile>("Tiles/floor-1"),
                [GroundType.Floor2] = Resources.Load<Tile>("Tiles/floor-2"),
            };
            _structureTiles = new Dictionary<StructureType, Tile>
            {
                [StructureType.None] = null,
                [StructureType.Wall1] = Resources.Load<Tile>("Tiles/wall-1"),
                [StructureType.Wall2] = Resources.Load<Tile>("Tiles/wall-2"),
            };
            _overlayTiles = new Dictionary<OverlayType, Tile>
            {
                [OverlayType.None] = null,
                [OverlayType.Outline1] = Resources.Load<Tile>("Tiles/outline-1"),
                [OverlayType.Outline2] = Resources.Load<Tile>("Tiles/outline-2"),
            };
        }

        private void Start()
        {

        }

        private void Update()
        {

        }

        private void OnDisable()
        {
            MapSystem.OnUpdateMapRender -= UpdateMapRender;
        }

        private void UpdateMapRender(object sender, OnMapEventArgs eventArgs)
        {
            foreach (Cell cell in eventArgs.WorldMap.Cells)
            {
                Vector3Int tilemapPosition = new Vector3Int(cell.Position.x, cell.Position.y, 0);

                _groundTilemap.SetTile(tilemapPosition, _groundTiles[cell.GroundType]);
                _structureTilemap.SetTile(tilemapPosition, _structureTiles[cell.StructureType]);
                _overlayTilemap.SetTile(tilemapPosition, _overlayTiles[cell.OverlayType]);
            }
        }

        private void Handle(object sender, OnMainClickArgs eventArgs)
        {
            Vector3Int test1 = _grid.WorldToCell(eventArgs.CameraPos);
            Vector2Int test = new Vector2Int(test1.x, test1.y);
        }

        private void HandleRot(object sender, OnRotateArgs eventArgs)
        {
            Vector3Int test1 = _grid.WorldToCell(eventArgs.CameraPosition);
            Vector2Int test = new Vector2Int(test1.x, test1.y);
        }
    }
}
