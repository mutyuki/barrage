using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPooler : MonoBehaviour
{
    public static SimpleObjectPooler Instance;

    [System.Serializable]
    public class PoolInfo
    {
        public GameObject prefab;
        public int initialSize = 10;
    }

    public List<PoolInfo> poolsInfo;
    private Dictionary<GameObject, Queue<GameObject>> pooledObjects;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        pooledObjects = new Dictionary<GameObject, Queue<GameObject>>();

        foreach (PoolInfo info in poolsInfo)
        {
            if (info.prefab == null) continue;

            // --- プレハブが IBulletBehavior を持っているか確認 ---
            if (info.prefab.GetComponent<IBulletBehavior>() == null)
            {
                Debug.LogError($"Prefab '{info.prefab.name}' must have a component implementing IBulletBehavior!", info.prefab);
                continue; // IBulletBehavior がないプレハブはプールできない
            }
            // --------------------------------------------------

            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < info.initialSize; i++)
            {
                GameObject obj = Instantiate(info.prefab);
                // --- IBulletBehavior を介して OriginalPrefab を設定 ---
                IBulletBehavior bulletBehavior = obj.GetComponent<IBulletBehavior>();
                if (bulletBehavior != null) // GetComponentはnullチェックが望ましい
                {
                    bulletBehavior.OriginalPrefab = info.prefab;
                }
                else
                {
                    // 通常ここには来ないはず（上のチェックがあるため）
                    Debug.LogError($"Instantiated object from {info.prefab.name} is missing IBulletBehavior somehow!", obj);
                }
                // -----------------------------------------------
                obj.SetActive(false);
                obj.transform.SetParent(this.transform);
                objectQueue.Enqueue(obj);
            }
            pooledObjects.Add(info.prefab, objectQueue);
            Debug.Log($"Pool created for {info.prefab.name} with size {info.initialSize}");
        }
    }

    public GameObject GetBullet(GameObject prefab)
    {
        if (pooledObjects.TryGetValue(prefab, out Queue<GameObject> objectQueue))
        {
            GameObject obj;
            IBulletBehavior bulletBehavior; // 後で使うので宣言しておく

            if (objectQueue.Count > 0)
            {
                obj = objectQueue.Dequeue();
                // OriginalPrefabが設定されているはずだが、念のため再設定しても良い
                // bulletBehavior = obj.GetComponent<IBulletBehavior>();
                // if(bulletBehavior != null) bulletBehavior.OriginalPrefab = prefab;

            }
            else
            {
                Debug.LogWarning($"Pool for {prefab.name} is empty. Instantiating new one.");
                obj = Instantiate(prefab);
                // --- 新規生成時も OriginalPrefab を設定 ---
                bulletBehavior = obj.GetComponent<IBulletBehavior>();
                if (bulletBehavior != null)
                {
                    bulletBehavior.OriginalPrefab = prefab;
                }
                else
                {
                    Debug.LogError($"Instantiated object from {prefab.name} is missing IBulletBehavior!", obj);
                    // 本来プールできないオブジェクトなので、即時破棄なども検討
                    Destroy(obj);
                    return null;
                }
                // --------------------------------------
                obj.transform.SetParent(this.transform);
                // obj.SetActive(false); // 初期状態は非アクティブ
            }
            // obj.SetActive(true); // SetActiveは Setup メソッドで行う想定
            return obj;
        }
        else
        {
            Debug.LogError($"No pool found for prefab: {prefab.name}. Add it to Pools Info list.", this);
            return null;
        }
    }

    public void ReturnBullet(GameObject obj)
    {
        obj.SetActive(false);
        // Transformリセットなど (任意)

        // --- IBulletBehavior を使って OriginalPrefab を取得 ---
        IBulletBehavior bulletBehavior = obj.GetComponent<IBulletBehavior>();
        if (bulletBehavior != null && bulletBehavior.OriginalPrefab != null)
        {
            if (pooledObjects.TryGetValue(bulletBehavior.OriginalPrefab, out Queue<GameObject> objectQueue))
            {
                objectQueue.Enqueue(obj);
            }
            else
            {
                // 返却先キューが見つからない (通常発生しないはず)
                Debug.LogWarning($"Tried to return object {obj.name} to a non-existent pool (Prefab: {bulletBehavior.OriginalPrefab.name}). Destroying instead.", obj);
                Destroy(obj);
            }
        }
        else
        {
            // IBulletBehavior がないか、OriginalPrefab が設定されていない場合
            Debug.LogWarning($"Cannot determine the original prefab for {obj.name} (IBulletBehavior missing or OriginalPrefab not set). Destroying object.", obj);
            Destroy(obj);
        }
        // -------------------------------------------------
    }
}
