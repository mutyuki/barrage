// Assets/Scripts/UI/RetryButtonHandler.cs
using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager を使うために必要
using UnityEngine.UI; // Buttonコンポーネントを使うため(任意)

[RequireComponent(typeof(Button))] // ボタンへのアタッチを前提とする(任意)
public class RetryButtonHandler : MonoBehaviour
{
  private Button retryButton;

  void Awake()
  {
    retryButton = GetComponent<Button>();
    if (retryButton != null)
    {
      // ボタンがクリックされたら RetryGame メソッドを呼び出すように設定
      retryButton.onClick.AddListener(RetryGame);
    }
    else
    {
      Debug.LogError("Button component not found on this GameObject!", this);
    }
  }

  /// <summary>
  /// ゲームをリトライする（現在のシーンを再読み込み）
  /// </summary>
  public void RetryGame()
  {
    Debug.Log("Retry button clicked!");

    // --- シンプルなリトライ実装 ---
    // 現在アクティブなシーンのビルドインデックスを取得
    int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    // そのインデックスを使ってシーンを再読み込み
    SceneManager.LoadScene(currentSceneIndex);

    // --- (より良い設計案) シーン管理クラスを使う場合 ---
    // SceneLoader sceneLoader = FindObjectOfType<SceneLoader>(); // またはシングルトンで取得
    // if (sceneLoader != null)
    // {
    //     sceneLoader.ReloadCurrentScene();
    // }
    // else
    // {
    //      Debug.LogError("SceneLoader not found!");
    //      // フォールバックとして直接ロード
    //      int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
    //      SceneManager.LoadScene(currentSceneIndex);
    // }

    // (任意) タイムスケールをリセット（ポーズ中にゲームオーバーした場合など）
    Time.timeScale = 1f;
  }

  // (任意) オブジェクトが無効になったらリスナーを削除
  void OnDestroy()
  {
    if (retryButton != null)
    {
      retryButton.onClick.RemoveListener(RetryGame);
    }
  }
}
