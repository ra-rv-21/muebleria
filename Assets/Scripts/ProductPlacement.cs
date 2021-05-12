using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Vuforia
{
    public class ProductPlacement : MonoBehaviour
    {
        public Transform product;
        public bool IsPlaced;
        [Space]
        public GameObject planeFinder;

        private Touch[] _touches;
        private int _lastTouchCount;
        private bool _isFirstFrameWithTwoTouches;
        private Vector3 _lookAtPosition;
        private Quaternion _rotation;

        // Plane Finder Components
        private PlaneFinderBehaviour _planeFinderBehaviour;
        private ContentPositioningBehaviour _contentPositionBehaviour;
        private AnchorBehaviour _anchorBehaviour;

        // Detect UI References
        private GraphicRaycaster _graphicRayCaster;
        private PointerEventData _pointerEventData;

        // Chached References of Input Translate
        private string _floorName;
        private Camera _camera;
        private Ray _ray;
        private RaycastHit _hit;
        private List<RaycastResult> _raycastResults;

        // Chached References of Input Rotate
        private float _cachedTouchDistance;
        private float _cachedTouchAngle;
        private Vector3 _cachedAugmentationRotation;
        private float _chachedCurrentTouchDistance;
        private float _chacheddiffY;
        private float _chacheddiffX;
        private float _chachedCurrentTouchAngle;

        private void Awake()
        {
            _planeFinderBehaviour = planeFinder.GetComponent<PlaneFinderBehaviour>(); // ADD EVENTS
            _contentPositionBehaviour = planeFinder.GetComponent<ContentPositioningBehaviour>();
            _anchorBehaviour = _contentPositionBehaviour.AnchorStage;
        }

        private void Start()
        {
            _camera = Camera.main;
            _cachedAugmentationRotation = product.localEulerAngles;

            _planeFinderBehaviour.OnInteractiveHitTest.AddListener(HandleInteractiveHitTest);
            _planeFinderBehaviour.OnAutomaticHitTest.AddListener(HandleAutomaticHitTest);

            _raycastResults = new List<RaycastResult>();

            SetupFloor();
        }

        private void Update()
        {
            if (!IsPlaced)
            {
                RotateTowardCamera();
                return;
            }

            InputTranslate();

            InputRotate();
        }

        private void InputTranslate()
        {
            if (IsPointerOverUIObject())return;

            if (IsPlaced)
            {
                if (IsSingleFingerDragging || (VuforiaRuntimeUtilities.IsPlayMode() && Input.GetMouseButton(0)))
                {
                    _ray = _camera.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(_ray, out _hit))
                    {
                        if (_hit.collider.gameObject.name == _floorName)
                        {
                            product.gameObject.PositionAt(_hit.point);
                        }
                    }
                }
            }
        }

        private void InputRotate()
        {

            _touches = Input.touches;

            if (Input.touchCount == 2)
            {
                _chachedCurrentTouchDistance = Vector2.Distance(_touches[0].position, _touches[1].position);
                _chacheddiffY = _touches[0].position.y - _touches[1].position.y;
                _chacheddiffX = _touches[0].position.x - _touches[1].position.x;
                _chachedCurrentTouchAngle = Mathf.Atan2(_chacheddiffY, _chacheddiffX) * Mathf.Rad2Deg;

                if (_isFirstFrameWithTwoTouches)
                {
                    _cachedTouchDistance = _chachedCurrentTouchDistance;
                    _cachedTouchAngle = _chachedCurrentTouchAngle;
                    _isFirstFrameWithTwoTouches = false;
                }

                product.localEulerAngles = _cachedAugmentationRotation - new Vector3(0, (_chachedCurrentTouchAngle - _cachedTouchAngle) * 3f, 0);
            }
            else if (Input.touchCount < 2)
            {
                _cachedAugmentationRotation = product.localEulerAngles;
                _isFirstFrameWithTwoTouches = true;
            }
        }

        private void SetupFloor()
        {
            if (VuforiaRuntimeUtilities.IsPlayMode())
            {
                _floorName = "Emulator Ground Plane";
            }
            else
            {
                _floorName = "Floor";
                GameObject floor = new GameObject(_floorName, typeof(BoxCollider));
                floor.transform.SetParent(product.parent);
                floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                floor.transform.localScale = Vector3.one;
                floor.GetComponent<BoxCollider>().size = new Vector3(100f, 0, 100f);
            }
        }

        private void SetProductAnchor(Transform transform)
        {
            if (transform)
            {
                IsPlaced = true;
                product.localPosition = Vector3.zero;
                RotateTowardCamera();
            }
            else
            {
                IsPlaced = false;
            }
        }
        #region Plane Finder Hit Test

        private void HandleAutomaticHitTest(HitTestResult result)
        {
            if (!IsPlaced)
            {
                SetProductAnchor(null);
                product.gameObject.PositionAt(result.Position);
            }
        }

        private void HandleInteractiveHitTest(HitTestResult result)
        {
            if (result == null)return;

            if (!IsPlaced || DoubleTap)
            {
                _contentPositionBehaviour.AnchorStage = _anchorBehaviour;
                _contentPositionBehaviour.PositionContentAtPlaneAnchor(result);
            }

            if (!IsPlaced)
            {
                SetProductAnchor(_anchorBehaviour.transform);
            }
        }

        #endregion

        #region Utility Helpers

        private bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning($"<color=yellow><b>[WARNING]</b></color> Without EventSystem, IsPointerOverUIObject will not work!");
                return false;
            }

            _pointerEventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            EventSystem.current.RaycastAll(_pointerEventData, _raycastResults);
            return _raycastResults.Count > 0;
        }

        private void RotateTowardCamera()
        {
            if (Vuforia.VuforiaManager.Instance.ARCameraTransform != null)
            {
                _lookAtPosition = Vuforia.VuforiaManager.Instance.ARCameraTransform.position - product.position;
                _lookAtPosition.y = 0;
                _rotation = Quaternion.LookRotation(_lookAtPosition);
                product.rotation = _rotation;
            }
        }

        private bool DoubleTap
        {
            get { return (Input.touchSupported) && Input.touches[0].tapCount == 2; }
        }

        private bool IsSingleFingerDragging
        {
            get { return IsSingleFingerDown() && (Input.touches[0].phase == TouchPhase.Moved); }
        }

        private bool IsSingleFingerDown()
        {
            if (Input.touchCount == 0 || Input.touchCount >= 2)
                _lastTouchCount = Input.touchCount;

            return (
                Input.touchCount == 1 &&
                Input.touches[0].fingerId == 0 &&
                _lastTouchCount == 0);
        }

        #endregion

    }
}