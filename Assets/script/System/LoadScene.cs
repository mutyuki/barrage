// Assets/Scripts/System/SceneLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
  // シングルトンにする場合 (よりアクセスしやすく)
  public static SceneLoader Instance { get; private set; }

  void Awake()
  {
    // シングルトン設定
    if (Instance == null)
    {
      Instance = this;
      // DontDestroyOnLoad(gameObject); // シーン遷移後も残す場合
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public void ReloadCurrentScene()
  {
    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    LoadSceneByIndex(currentSceneIndex);
  }

  public void LoadSceneByName(string sceneName)
  {
    Debug.Log($"Loading scene: {sceneName}");
    // ここでロード前の処理（フェードアウトなど）を挟むことも可能
    SceneManager.LoadScene(sceneName);
    Time.timeScale = 1f; // 念のためタイムスケールリセット
  }

  public void LoadSceneByIndex(int sceneIndex)
  {
    Debug.Log($"Loading scene by index: {sceneIndex}");
    SceneManager.LoadScene(sceneIndex);
    Time.timeScale = 1f;
  }

  // 他、非同期ロードなどのメソッドを追加可能
}
