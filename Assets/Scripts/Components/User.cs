using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;

namespace TFCB
{
    public class User : MonoBehaviour
    {
        public static event EventHandler<OnMainClickArgs> OnMainStart;
        public static event EventHandler<OnMainClickArgs> OnMainHold;
        public static event EventHandler<OnMainClickArgs> OnMainEnd;

        public static event EventHandler<OnRotateArgs> OnRotate;


        private RenderSettings _renderSettings;
        private Camera _camera;

        private Grid _grid;

        private float _panSpeed;
        private float _zoomSpeed;
        private float _defaultZoom;
        private float _zoomScrollClamp;
        private float _minZoom;
        private float _maxZoom;

        private UserInputActions _userInputActions;

        private InputAction _panAction;
        private InputAction _zoomAction;
        private InputAction _mainClickAction;
        private InputAction _pointerPositionAction;
        private InputAction _rotateGlobalAction;

        private bool _mainClickHold = false;
        private LayerMask _groundLayer;
        private LayerMask _structureLayer;


        private void Awake()
        {

            initRenderSettings();

            initLayerMask();

            initInputActions();

            initCamera();
        }

        private void initRenderSettings()
        {
            _renderSettings = Resources.Load<RenderSettings>("Settings/RenderSettings");
            _panSpeed = _renderSettings.PanSpeed;
            _zoomSpeed = _renderSettings.ZoomSpeed;
            _defaultZoom = _renderSettings.DefaultZoom;
            _zoomScrollClamp = _renderSettings.ZoomScrollClamp;
            _minZoom = _renderSettings.MinZoom;
            _maxZoom = _renderSettings.MaxZoom;
        }

        private void initLayerMask()
        {
            _grid = GameObject.Find("Grid").GetComponent<Grid>();
            _groundLayer = LayerMask.NameToLayer("Ground");
            _structureLayer = LayerMask.NameToLayer("Structure");
        }
        private void initInputActions()
        {
            _userInputActions = new UserInputActions();
            _panAction = _userInputActions.UserActionMap.Pan;
            _zoomAction = _userInputActions.UserActionMap.Zoom;
            _mainClickAction = _userInputActions.UserActionMap.MainClick;
            _pointerPositionAction = _userInputActions.UserActionMap.PointerPosition;
            _rotateGlobalAction = _userInputActions.UserActionMap.RotateGlobal;

            _rotateGlobalAction.performed += OnGlobalRotate;
            // _panAction.performed += MoveByOne;
        }

        private void initCamera()
        {
            _camera = GameObject.Find("User").GetComponentInChildren<Camera>();
            _camera.transform.position = new Vector3(0, 0, -10);
            _camera.orthographicSize = _defaultZoom;
        }

        private void OnEnable()
        {
            _panAction.Enable();
            _zoomAction.Enable();
            _mainClickAction.Enable();
            _pointerPositionAction.Enable();
            _rotateGlobalAction.Enable();
        }

        private void Start()
        {

        }


        private void Update()
        {
            UpdatePan();
            UpdateZoom();
            UpdateMainClick();
        }

        private void UpdatePan()
        {
            Vector2 panValue = _panAction.ReadValue<Vector2>();
            Vector3 panDisplacement = _panSpeed * panValue;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                _camera.transform.position + panDisplacement,
                Time.deltaTime
            );
        }

        private void UpdateZoom()
        {
            // Zoom is Vector2 due to scroll wheel only working on axis controls,
            // Have to flip and clamp the value since we can't control the mouse zoom axis value returns
            Vector2 zoomScrollValue = _zoomAction.ReadValue<Vector2>();
            float zoomValue = Mathf.Clamp(
               -zoomScrollValue.y,
                -_zoomScrollClamp,
                _zoomScrollClamp
            );
            float zoomDisplacement = _zoomSpeed * zoomValue;

            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize,
                _camera.orthographicSize + zoomDisplacement,
                Time.deltaTime
            );
            _camera.orthographicSize = Mathf.Clamp(
                _camera.orthographicSize,
                _minZoom,
                _maxZoom
            );
        }

        private void UpdateMainClick()
        {
            float click = _mainClickAction.ReadValue<float>();
            if (click == 1)
            {
                Vector2 position = _pointerPositionAction.ReadValue<Vector2>();
                Vector3 cameraPos = _camera.ScreenToWorldPoint(position);

                if (!_mainClickHold)
                {
                    _mainClickHold = true;
                    OnMainStart?.Invoke(this, new OnMainClickArgs { CameraPos = cameraPos });
                }
                else
                {
                    OnMainHold?.Invoke(this, new OnMainClickArgs { CameraPos = cameraPos });
                }
            }
            else if (_mainClickHold && click == 0)
            {
                Vector2 position = _pointerPositionAction.ReadValue<Vector2>();
                Vector3 cameraPos = _camera.ScreenToWorldPoint(position);
                _mainClickHold = false;
                OnMainEnd?.Invoke(this, new OnMainClickArgs { CameraPos = cameraPos });
            }
        }

        // TODO: RM debug code
        private void MoveByOne(InputAction.CallbackContext action)
        {
            Vector2 panValue = action.ReadValue<Vector2>();
            Vector3 panDisplacement = _panSpeed * panValue;
            _camera.transform.position = new Vector3(
                _camera.transform.position.x + panValue.x,
                _camera.transform.position.y + panValue.y,
                _camera.transform.position.z
            );

            Vector3Int worldPosition = _grid.WorldToCell(_camera.transform.position);
        }

        private void OnGlobalRotate(InputAction.CallbackContext action)
        {
            string direction = action.ReadValue<float>() == 1 ? "left" : "right";

            Vector3Int previousWorldPosition = _grid.WorldToCell(_camera.transform.position);
            int2 rotatedWorldPosition = rotateTile(direction, previousWorldPosition.x, previousWorldPosition.y);
            Vector3 previousCameraPosition = _camera.transform.position;
            int cx = (rotatedWorldPosition.x - rotatedWorldPosition.y) / 2;
            int cy = (rotatedWorldPosition.y + cx) / 2;
            _camera.transform.position = new Vector3(
                cx,
                cy,
                _camera.transform.position.z
            );

            OnRotate?.Invoke(this, new OnRotateArgs
            {
                Direction = direction,
                CameraPosition = _camera.transform.position,
                WorldPosition = new Vector2Int(rotatedWorldPosition.x, rotatedWorldPosition.y),
                PreviousCameraPosition = previousCameraPosition,
                PreviousWorldPosition = new Vector2Int(previousWorldPosition.x, previousWorldPosition.y),
            });
        }

        private int2 rotateTile(string direction, int x, int y)
        {
            return direction == "right" ? new int2(y, -x) : new int2(-y, x);
        }
        private void OnDisable()
        {
            _panAction.Disable();
            _zoomAction.Disable();
            _mainClickAction.Disable();
            _pointerPositionAction.Disable();
            _rotateGlobalAction.Disable();
        }
    }
}
