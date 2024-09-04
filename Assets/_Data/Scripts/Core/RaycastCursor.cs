using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CuaHang
{
    /// <summary> Dùng raycast và drag các item </summary>
    public class RaycastCursor : Singleton<RaycastCursor>
    {
        [Header("RaycastCursor")]
        public ItemDrag _objectDrag;
        Camera _cam;

        [Space]
        public bool _enableSnapping; // bật chế độ snapping
        public bool _enableRaycast; // bat raycast
        public bool _enableOutline;
        public Transform _itemFocus;
        public float _rotationSpeed;// Tốc độ xoay
        public float _snapDistance = 6f; // Khoảng cách cho phép đặt 
        public float _tileSize = 1; // ô snap tỷ lệ snap
        public Vector3 _tileOffset = Vector3.zero; // tỷ lệ snap + sai số này
        public LayerMask _layerMask;

        public RaycastHit _hit;
        public RaycastHit[] _hits;
        InputImprove _input;

        protected override void Awake()
        {
            base.Awake();
            _input = new();
            _cam = Camera.main;
            _enableRaycast = true;
            _enableOutline = true;

        }

        private void Start()
        {
            _objectDrag = SingleModuleManager.Instance._itemDrag;
        }

        private void OnEnable()
        {
            _input.DragPerformed += SetItemDrag;
            _input.SnapPerformed += SetSnap;
            _input.Click += SetItemFocus;
            _input.Cancel += CancelFocus;
        }

        private void OnDisable()
        {
            _input.DragPerformed -= SetItemDrag;
            _input.SnapPerformed -= SetSnap;
            _input.Click -= SetItemFocus;
            _input.Cancel -= CancelFocus;
        }

        void FixedUpdate()
        {
            CanNotPlant();
        }

        void Update()
        {
            SetRayHit();
            MoveItemDrag();
            RotationItemDrag();
        }

        private void SetSnap(InputAction.CallbackContext context)
        {
            _enableSnapping = !_enableSnapping;
        }

        /// <summary> Chiếu tia raycast lấy dữ liệu cho _Hit </summary>
        private void SetRayHit()
        {
            if (_enableRaycast == false) return;

            Ray ray = _cam.ScreenPointToRay(_input.MousePosition());
            Physics.Raycast(ray, out _hit, 100, _layerMask);
            _hits = Physics.RaycastAll(ray, 100f, _layerMask);
            In($"You Hit {_hit.transform}");
        }

        /// <summary> Tạo viền khi click vào đối tượng để nó focus </summary>
        private void SetItemFocus(InputAction.CallbackContext context)
        {
            if (_hit.transform && !_objectDrag._itemDragging && !EventSystem.current.IsPointerOverGameObject())
            {
                // chuyển đói tượng focus
                if (_itemFocus != _hit.transform && _itemFocus != null)
                {
                    SetOutlines(_itemFocus, false);
                }

                _itemFocus = _hit.transform;
                SetOutlines(_itemFocus, true);
            }
        }

        /// <summary> Thoát không muốn cam tập trung nhìn tối tượng item này nữa </summary>
        private void CancelFocus(InputAction.CallbackContext context)
        {
            if (_itemFocus)
            {
                SetOutlines(_itemFocus, false);
                _itemFocus = null;
            }
        }

        /// <summary> Tìm outline trong đối tượng và bật tắt viền của nó </summary>
        private void SetOutlines(Transform target, bool value)
        {
            foreach (Outline outline in target.GetComponentsInChildren<Outline>())
            {
                if (_enableOutline && value) outline.enabled = true;
                else outline.enabled = false;
            }
        }

        /// <summary> Set thông số trường hợp cho không thể đặt </summary>
        private void CanNotPlant()
        {
            if (!_objectDrag) return;

            // khoảng cách bị quá dài
            if (Vector3.Distance(_cam.transform.position, _hit.point) < _snapDistance)
            {
                _objectDrag.GetComponent<ItemDrag>()._isDistance = true;
            }
            else
            {
                _objectDrag.GetComponent<ItemDrag>()._isDistance = false;
            }
        }

        /// <summary> Bật item drag với item được _Hit chiếu</summary>
        private void SetItemDrag(InputAction.CallbackContext context)
        {
            if (!_itemFocus || _objectDrag._isDragging) return;

            Item item = _itemFocus.transform.GetComponent<Item>();

            if (item && item._isCanDrag)
            {
                item.DragItem();
                _objectDrag.PickUpItem(item);
            }
        }

        /// <summary> Di chuyen item </summary>
        private void MoveItemDrag()
        {
            //  Làm tròn vị trí temp để nó giống snap
            if (_enableSnapping)
            {
                Vector3 hitPoint = _hit.point;

                float sX = Mathf.Round(hitPoint.x / _tileSize) * _tileSize + _tileOffset.x;
                float sZ = Mathf.Round(hitPoint.z / _tileSize) * _tileSize + _tileOffset.z;
                float sY = Mathf.Round(hitPoint.y / _tileSize) * _tileSize + _tileOffset.y;

                Vector3 snappedPosition = new Vector3(sX, sY, sZ);
                _objectDrag.transform.position = snappedPosition;
            }
            else
            {
                _objectDrag.transform.position = _hit.point;
            }
        }

        /// <summary> Xoay item </summary>
        private void RotationItemDrag()
        {
            if (_objectDrag && _objectDrag._modelsHolding)
            {
                // để đối tượng vuông góc với bề mặt va chạm
                _objectDrag.transform.rotation = Quaternion.FromToRotation(Vector3.up, _hit.normal);

                // xoay theo roll chuột giữa 
                // Lấy góc xoay hiện tại của vật thể
                float currentAngle = _objectDrag._modelsHolding.eulerAngles.y;

                // Làm tròn góc xoay hiện tại về hàng chục gần nhất
                float roundedAngle = Mathf.Round(currentAngle / 10.0f) * 10.0f;

                // Tính toán góc xoay mới dựa trên giá trị cuộn chuột
                float rotationAngle = Mathf.Round(_input.MouseScroll() * _rotationSpeed / 10.0f) * 10.0f;

                // Cộng góc xoay mới vào góc xoay đã làm tròn
                float newAngle = roundedAngle + rotationAngle;

                // Áp dụng góc xoay mới cho vật thể
                _objectDrag._modelsHolding.rotation = Quaternion.Euler(0, newAngle, 0);
            }
        }
    }

}
