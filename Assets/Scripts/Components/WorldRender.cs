using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TFCB
{
    public enum CitizenAnimationType
    {
        Idle,
        Walk,
    }

    public class WorldRender : MonoBehaviour
    {
        private RenderSettings _renderSettings;

        private Grid _grid;

        private Tilemap _groundTilemap;
        private Tilemap _structureTilemap;
        private Tilemap _overlayTilemap;

        private Dictionary<GroundType, Tile> _groundTiles;
        private Dictionary<StructureType, Tile> _structureTiles;
        private Dictionary<OverlayType, Tile> _overlayTiles;
        private Dictionary<int, CitizenRenderData> _citizenRenderData;
        private Dictionary<Nation, GameObject> _nationPrefabs;

        private GameObject _citizensGameObject;

        // TODO: this maybe should be in the map system?
        private int _rotation = 0;

        private void Awake()
        {
            _renderSettings = Resources.Load<RenderSettings>("Settings/RenderSettings");

            SetupEvents();

            SetupEntityResources();

            SetupTilemapResources();
        }

        private void SetupEvents()
        {
            MapSystem.OnUpdateMapRender += UpdateMapRender;
            EntitySystem.OnCreateCitizen += CreateCitizenRenderData;
            User.OnMainStart += Handle;
            User.OnRotate += HandleRot;
            //User.OnMouseHold += Handle;
            //User.OnMouseUp += Handle;
        }

        private void SetupEntityResources()
        {
            _citizensGameObject = GameObject.Find("World/Entities/Citizens");
            _citizenRenderData = new Dictionary<int, CitizenRenderData>();
            _nationPrefabs = new Dictionary<Nation, GameObject>
            {
                [Nation.Guys] = Resources.Load<GameObject>("Prefabs/Entities/Citizen/GuysPrefab"),
                [Nation.Kailt] = Resources.Load<GameObject>("Prefabs/Entities/Citizen/KailtPrefab"),
                [Nation.Taylor] = Resources.Load<GameObject>("Prefabs/Entities/Citizen/TaylorPrefab"),
            };

            Animator guysAnimator = _nationPrefabs[Nation.Guys].GetComponent<Animator>();
            Animator kailtAnimator = _nationPrefabs[Nation.Kailt].GetComponent<Animator>();
            Animator taylorAnimator = _nationPrefabs[Nation.Taylor].GetComponent<Animator>();

            foreach (AnimationClip clip in guysAnimator.runtimeAnimatorController.animationClips)
            {
                clip.frameRate = 12;
            }
            foreach (AnimationClip clip in kailtAnimator.runtimeAnimatorController.animationClips)
            {
                clip.frameRate = 12;
            }
            foreach (AnimationClip clip in taylorAnimator.runtimeAnimatorController.animationClips)
            {
                clip.frameRate = 12;
            }
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

        private void TearDownEvents()
        {
            MapSystem.OnUpdateMapRender -= UpdateMapRender;
            EntitySystem.OnCreateCitizen -= CreateCitizenRenderData;
            User.OnMainStart -= Handle;
            User.OnRotate -= HandleRot;
        }

        private void OnDisable()
        {
            TearDownEvents();
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

        private void CreateCitizenRenderData(object sender, OnCitizenEventArgs eventArgs)
        {
            Citizen citizen = eventArgs.Citizen;
            CitizenRenderData citizenRenderData = new CitizenRenderData();

            Vector3 position = GridToWorld(citizen.Position);
            position.z = citizen.Id * _renderSettings.EntitySpacing;

            citizenRenderData.WorldGameObject = Instantiate(
                _nationPrefabs[citizen.Nation],
                position,
                Quaternion.identity,
                _citizensGameObject.transform
            );

            citizenRenderData.Animator = citizenRenderData.WorldGameObject.GetComponent<Animator>();

            _citizenRenderData[citizen.Id] = citizenRenderData;

            PlayAnimation(citizen, CitizenAnimationType.Idle);
        }

        private void PlayAnimation(Citizen citizen, CitizenAnimationType citizenAnimationType)
        {
            CitizenRenderData citizenRenderData = _citizenRenderData[citizen.Id];
            citizenRenderData.Animator.Play($"Base Layer.{citizen.Nation}-{citizenAnimationType}-{citizen.Direction}");
        }

        private Vector3 GridToWorld(int x, int y)
        {
            Vector3 worldPosition = _grid.CellToWorld(new Vector3Int(x, y, 0));
            worldPosition.y += 1 / 4f;

            return worldPosition;
        }

        private Vector3 GridToWorld(int2 position)
        {
            return GridToWorld(position.x, position.y);
        }

        // TODO: Debug, remvoe?
        private void Handle(object sender, OnMainClickArgs eventArgs)
        {
            Vector3Int test1 = _grid.WorldToCell(eventArgs.CameraPos);
            Vector2Int test = new Vector2Int(test1.x, test1.y);
        }

        // TODO: Debug, remvoe?
        private void HandleRot(object sender, OnRotateArgs eventArgs)
        {
            Vector3Int test1 = _grid.WorldToCell(eventArgs.CameraPosition);
            Vector2Int test = new Vector2Int(test1.x, test1.y);
        }
    }
}
