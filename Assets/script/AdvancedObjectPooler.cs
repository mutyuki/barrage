// Assets/Scripts/Pooling/AdvancedObjectPooler.cs (または任意のフォルダ構成)
using System.Collections.Generic; // Dictionary, Queue, List を使うため
using UnityEngine;

public class AdvancedObjectPooler : MonoBehaviour
{
    // --- シングルトンインスタンス ---
    public static AdvancedObjectPooler Instance { get; private set; }

    // --- インスペクター設定 ---
    [Header("Pool Configuration")]
    [Tooltip("プールしたいオブジェクトの情報リスト (Prefabと数を設定)")]
    public List<PoolInfo> poolsToCreate = new List<PoolInfo>();

    [Header("Dynamic Instantiation")]
    [Tooltip("プールが空の時に動的に新しいオブジェクトを生成するか")]
    public bool allowDynamicInstantiation = true;

    // --- プールの実体 (内部データ) ---
    // Key (PoolInfo.GetKey()の結果) と GameObjectキュー の辞書
    private Dictionary<string, Queue<GameObject>> pooledObjects =
        new Dictionary<string, Queue<GameObject>>();

    // Key と 元のプレハブ の辞書 (動的生成用)
    private Dictionary<string, GameObject> keyToPrefab = new Dictionary<string, GameObject>();

    // --- Unityライフサイクルメソッド ---

    void Awake()
    {
        // シングルトン設定
        if (Instance == null)
        {
            Instance = this;
            // (任意) DontDestroyOnLoad(gameObject); // シーンを越えて使う場合
        }
        else
        {
            Debug.LogWarning(
                $"複数の AdvancedObjectPooler が検出されました。'{gameObject.name}' を破棄します。",
                this
            );
            Destroy(gameObject); // 自分を破棄
            return;
        }
    }

    void Start()
    {
        // Start でプールを初期化 (Awakeの後で参照が設定されるのを期待)
        InitializePools();
    }

    void OnDestroy()
    {
        // シングルトン参照解除
        if (Instance == this)
        {
            Instance = null;
        }
        // 必要であればプール内オブジェクトを破棄するなどの後処理
        // CleanupPools();
    }

    // --- 初期化 ---

    private void InitializePools()
    {
        pooledObjects.Clear(); // 再初期化の場合を考慮
        keyToPrefab.Clear();

        foreach (PoolInfo poolInfo in poolsToCreate)
        {
            // キーとプレハブの有効性をチェック
            string key = poolInfo.GetKey();
            if (string.IsNullOrEmpty(key) || poolInfo.prefab == null)
            {
                Debug.LogError(
                    $"PoolInfo has invalid settings (Key: {key ?? "null"}, Prefab: {poolInfo.prefab?.name ?? "null"}). Ignoring pool.",
                    this
                );
                continue;
            }

            // キーの重複チェック
            if (pooledObjects.ContainsKey(key))
            {
                Debug.LogWarning(
                    $"Pool key '{key}' (Prefab: {poolInfo.prefab.name}) is duplicated. Ignoring subsequent definition.",
                    this
                );
                continue;
            }

            // プールとプレハブ情報を登録
            pooledObjects.Add(key, new Queue<GameObject>());
            keyToPrefab.Add(key, poolInfo.prefab);

            // Debug.Log($"Initializing pool '{key}' for '{poolInfo.prefab.name}', Size: {poolInfo.initialSize}");

            // 初期オブジェクト生成
            for (int i = 0; i < poolInfo.initialSize; i++)
            {
                InstantiateAndPoolObject(key, poolInfo.prefab);
            }
        }
    }

    // 1つのオブジェクトを生成し、情報を設定してプールに非アクティブで追加する内部メソッド
    private GameObject InstantiateAndPoolObject(string key, GameObject prefab)
    {
        if (prefab == null)
            return null;

        GameObject newObj = Instantiate(prefab);
        if (newObj != null)
        {
            // PooledObjectコンポーネントを準備
            PooledObject pooledComp = newObj.GetComponent<PooledObject>();
            if (pooledComp == null)
            {
                Debug.LogWarning(
                    $"Prefab '{prefab.name}' is missing PooledObject component. Adding it automatically.",
                    newObj
                );
                pooledComp = newObj.AddComponent<PooledObject>();
            }

            // 情報を設定
            pooledComp.poolKey = key;
            pooledComp.ownerPooler = this;

            // プールに追加
            newObj.SetActive(false);
            newObj.transform.SetParent(this.transform, false); // 親を設定 (ワールド座標維持)
            if (pooledObjects.TryGetValue(key, out var queue))
            {
                queue.Enqueue(newObj);
            }
            else
            {
                Debug.LogError(
                    $"Internal error: Queue for key '{key}' not found during Instantiate!",
                    this
                );
                Destroy(newObj); // 戻せないなら破棄
                return null;
            }
        }
        else
        {
            Debug.LogError($"Failed to instantiate prefab '{prefab.name}' for key '{key}'.", this);
        }
        return newObj;
    }

    // --- 公開メソッド: オブジェクト取得 ---

    // 文字列キーでオブジェクトを取得
    public GameObject GetObject(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("GetObject key is null or empty.", this);
            return null;
        }

        if (!pooledObjects.TryGetValue(key, out Queue<GameObject> queue))
        {
            Debug.LogError($"Pool with key '{key}' not found.", this);
            return null;
        }

        if (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            // Reset before activation? (Optional)
            // ResetObjectState(obj); // 必要なら状態リセットメソッドを呼ぶ
            // 呼び出し元で SetActive(true) する前提
            return obj;
        }
        else if (allowDynamicInstantiation)
        {
            // プールが空で動的生成許可
            if (!keyToPrefab.TryGetValue(key, out GameObject prefabToInstantiate))
            {
                Debug.LogError(
                    $"Cannot dynamically instantiate. Prefab for key '{key}' not found.",
                    this
                );
                return null;
            }

            Debug.LogWarning($"Pool '{key}' is empty. Instantiating new object dynamically.", this);
            // 動的に生成する場合はキューには追加せず、直接返す
            // PooledObjectの情報は付ける
            GameObject newObj = Instantiate(prefabToInstantiate);
            if (newObj != null)
            {
                PooledObject pooledComp = newObj.GetComponent<PooledObject>();
                if (pooledComp == null)
                    pooledComp = newObj.AddComponent<PooledObject>();
                pooledComp.poolKey = key;
                pooledComp.ownerPooler = this;
                newObj.transform.SetParent(this.transform, false);
                // ここでは SetActive(false) にはしない (呼び出し元が使うため)
                return newObj;
            }
            else
            {
                Debug.LogError($"Failed to dynamically instantiate prefab for key '{key}'.", this);
                return null;
            }
        }
        else
        {
            // プール空＆動的生成不許可
            Debug.LogWarning($"Pool '{key}' is empty and dynamic instantiation is disabled.", this);
            return null;
        }
    }

    // プレハブ指定でオブジェクトを取得 (キーはInstanceIDを使う)
    public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("GetObject with null prefab.", this);
            return null;
        }

        // ★ プレハブからキーを生成 (常にInstanceIDベース)
        string key = prefab.GetInstanceID().ToString();

        // このキーが登録されているか確認
        if (!pooledObjects.ContainsKey(key)) // keyToPrefabではなくpooledObjectsで確認
        {
            // 初回のみ、このプレハブのプールを動的に作成する、という選択肢も考えられる
            Debug.LogError(
                $"Pool for prefab '{prefab.name}' (Key: {key}) not found. Please register it in the Inspector.",
                this
            );
            return null;
        }

        return GetObject(key); // 文字列キー版のGetObjectを呼ぶ
    }

    // --- 公開メソッド: オブジェクト返却 ---

    // GameObjectをプールに戻す (推奨される方法)
    public void ReturnObject(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("Tried to return a null object to the pool.", this);
            return;
        }

        PooledObject pooledComp = obj.GetComponent<PooledObject>();

        // プーラー管理下のオブジェクトか、情報があるかを確認
        if (
            pooledComp == null
            || string.IsNullOrEmpty(pooledComp.poolKey)
            || pooledComp.ownerPooler != this
        )
        {
            Debug.LogWarning(
                $"Object '{obj.name}' is not managed by this pooler or missing info. Destroying.",
                obj
            );
            Destroy(obj);
            return;
        }

        // 対応するキューが存在するか確認
        if (pooledObjects.TryGetValue(pooledComp.poolKey, out Queue<GameObject> queue))
        {
            // 状態をリセットしてキューに戻す
            ResetObjectState(obj); // リセット処理を分離
            queue.Enqueue(obj);
        }
        else
        {
            // 本来ここには来ないはず (キーがあればキューもあるはず)
            Debug.LogError(
                $"Internal Error: Queue not found for existing key '{pooledComp.poolKey}'. Destroying '{obj.name}'.",
                obj
            );
            Destroy(obj);
        }
    }

    // 文字列キーを指定してプールに戻す (互換性・特殊用途)
    public void ReturnObject(GameObject obj, string key)
    {
        if (obj == null || string.IsNullOrEmpty(key))
            return;

        if (pooledObjects.TryGetValue(key, out Queue<GameObject> queue))
        {
            // 検証は省略されている点に注意
            ResetObjectState(obj);
            queue.Enqueue(obj);
        }
        else
        {
            Debug.LogError(
                $"Cannot return '{obj.name}': Pool with key '{key}' not found. Destroying.",
                obj
            );
            Destroy(obj);
        }
    }

    // --- ヘルパーメソッド ---

    // オブジェクトをプールに戻す前の状態リセット処理
    private void ResetObjectState(GameObject obj)
    {
        obj.SetActive(false); // 必ず非アクティブ化
        obj.transform.SetParent(this.transform, false); // 親をプーラーに
        obj.transform.position = Vector3.zero; // 位置リセット (必要なら)
        obj.transform.rotation = Quaternion.identity; // 回転リセット (推奨)

        // Rigidbodyのリセット (推奨)
        Rigidbody2D rb2D = obj.GetComponent<Rigidbody2D>();
        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
        // Rigidbody (3D) のリセットも必要なら追加
        Rigidbody rb3D = obj.GetComponent<Rigidbody>();
        if (rb3D != null)
        {
            rb3D.linearVelocity = Vector3.zero;
            rb3D.angularVelocity = Vector3.zero;
        }

        // ★ 他のコンポーネントの状態リセットが必要な場合、ここに追加する
        // 例: TrailRenderer をクリアする
        TrailRenderer tr = obj.GetComponentInChildren<TrailRenderer>(); // 子も含めて探す場合
        if (tr != null)
            tr.Clear();
        // 例: ParticleSystem を停止・クリアする
        ParticleSystem ps = obj.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    // (任意) プールをクリーンアップするメソッド
    // private void CleanupPools() { ... }
}
