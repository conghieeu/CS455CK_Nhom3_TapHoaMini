using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CuaHang.Pooler
{
    public class EntityPooler : GameBehavior, ISaveData
    {
        [Header("Entity Pooler")]
        [SerializeField] protected List<Transform> _prefabs;
        [SerializeField] private List<Entity> objectPools;
        
        public UnityAction OnPoolChange;

        public List<Entity> ListEntity { get => ObjectPools; private set => ObjectPools = value; }
        protected List<Entity> ObjectPools
        {
            get => objectPools;
            set
            {
                objectPools = value;
                OnPoolChange?.Invoke();
            }
        }
        
        private void OnValidate()
        {
            Init();
        }

        private void Init()
        {
            ObjectPools.Clear();
            foreach (Transform child in transform)
            {
                ObjectPools.Add(child.GetComponent<Entity>());
            }
        }

        /// <summary> Xoá object khỏi pool và đánh dấu là có thể tái sử dụng  </summary>
        public virtual void RemoveEntityFromPool(Entity entity)
        {
            entity.RemoveThis();
        }

        /// <summary> Kiểm tra xem pool có chứa object với ID cụ thể hay không  </summary>
        public virtual bool ContainsID(string id)
        {
            foreach (var obj in ListEntity)
            {
                if (obj.ID == id) return true;
            }
            return false;
        }

        /// <summary> Lấy object từ pool theo ID </summary>
        public virtual Entity GetObjectByID(string id)
        {
            if (id == "") return null;

            foreach (var obj in ListEntity)
            {
                if (obj.ID == id) return obj;
            }
            return null;
        }

        /// <summary> Tái sử dụng object nhàn rỗi hoặc tạo mới object từ pool, default(Vector3) là mặc định sẽ là Vector.Zero </summary>
        public Entity GetOrCreateObjectPool(TypeID typeID, Vector3 spawnPosition = default(Vector3), Quaternion rotation = default(Quaternion))
        {
            Entity objectPool = GetDisabledObject(typeID);

            if (objectPool) // tái sử dụng
            {
                objectPool.transform.rotation = rotation;
                objectPool.transform.position = spawnPosition;
                objectPool.IsRecyclable = false;
                objectPool.gameObject.SetActive(true);
            }
            else // Create New 
            {
                foreach (var prefab in _prefabs)
                {
                    Entity entity = prefab.GetComponent<Entity>();

                    if (entity && entity.TypeID == typeID)
                    {
                        objectPool = Instantiate(entity, spawnPosition, rotation, transform);
                        ObjectPools.Add(objectPool);
                        break;
                    }
                }
            }

            if (objectPool)
            {
                objectPool.GenerateIdentifier();
                return objectPool;
            }
            else
            {
                Debug.LogWarning($"Item {typeID} Này Tạo từ pool không thành công");
                return null;
            }
        }

        /// <summary> Tìm object nhàn rỗi trong pool theo typeID  </summary>
        private Entity GetDisabledObject(TypeID typeID)
        {
            foreach (var objectPool in ObjectPools)
            {
                if (objectPool.TypeID == typeID && objectPool.IsRecyclable)
                {
                    return objectPool;
                }
            }
            return null;
        }

        #region SaveData
        /// <summary> Entity sẽ truyền loại data vào đây set dữ liệu từ data, T: Kiểu dữ liệu trả về, V: kiểu dữ liệu muốn lấy </summary>
        public virtual void SetVariables<T, V>(T data)
        {
            // T: là list<V>
            if (data is List<V> dataList == false) return;

            foreach (var iData in dataList)
            {
                if (iData is EntityData entityData)
                {
                    Entity entity = GetObjectByID(entityData.Id);
                    if (entity) // set value thực thể đã tồn tại sẵn
                    {
                        entity.GetComponent<ISaveData>().SetVariables<V, object>(iData);
                    }
                    else
                    {
                        if (entityData.IsDestroyed == false) // không tạo những đối tuọng bị phá huỷ
                        {
                            entity = GetOrCreateObjectPool(entityData.TypeID, entityData.Position, entityData.Rotation);
                            entity.GetComponent<ISaveData>().SetVariables<V, object>(iData);
                        }
                    }
                }
            }
        }

        public virtual void LoadVariables()
        {
            foreach (var entity in ListEntity)
            {
                entity.GetComponent<ISaveData>().LoadVariables();
            }
        }

        public T GetData<T, D>()
        {
            List<D> listData = new List<D>();

            foreach (var entity in ListEntity)
            {
                if (entity)
                {
                    listData.Add(entity.GetComponent<ISaveData>().GetData<D, object>());
                }
            }
            return (T)(object)listData;
        }

        public virtual void SaveData()
        {
            // for override
        }
        #endregion
    }
}