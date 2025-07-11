using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CountDown : MonoBehaviour
{
    [SerializeField] private int startSeconds = 30; // カウントダウン秒数（30秒）
    private float timer;
    private bool isCounting = false;
    private bool isPaused = false; // 一時停止フラグ
    private Auto_Select autoSelect;
    private SettingUI settingUI; // SettingUIの参照
    private RoomKick roomKick; // RoomKickの参照
    
    // タッチ検知用パネル
    private GameObject touchDetector;
    private Canvas canvas;

    public System.Action OnCountDownEnd; // 終了時のコールバック

    // Start is called before the first frame update
    void Start()
    {
        // Auto_Selectコンポーネントを取得
        autoSelect = FindObjectOfType<Auto_Select>();
        if (autoSelect == null)
        {
            Debug.LogWarning("[CountDown] Auto_Selectコンポーネントが見つかりません");
        }
        
        // SettingUIコンポーネントを取得
        settingUI = FindObjectOfType<SettingUI>();
        if (settingUI == null)
        {
            Debug.LogWarning("[CountDown] SettingUIコンポーネントが見つかりません");
        }
        
        // RoomKickコンポーネントを取得
        roomKick = FindObjectOfType<RoomKick>();
        if (roomKick == null)
        {
            Debug.LogWarning("[CountDown] RoomKickコンポーネントが見つかりません");
        }
        
        // キャンバスを取得
        canvas = FindObjectOfType<Canvas>();
        
        // タッチ検知パネルを作成
        CreateTouchDetector();
        
        timer = startSeconds;
        
        StartCountDown();
    }

    /// <summary>
    /// タッチ検知用の透明パネルを作成
    /// </summary>
    private void CreateTouchDetector()
    {
        if (canvas == null)
        {
            Debug.LogWarning("[CountDown] Canvasが見つからないため、タッチ検知パネルを作成できません");
            return;
        }

        // タッチ検知用のGameObjectを作成
        touchDetector = new GameObject("TouchDetector");
        touchDetector.transform.SetParent(canvas.transform, false);
        
        // RectTransformを追加してCanvas全体をカバー
        RectTransform rectTransform = touchDetector.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        
        // 最背面に配置（他のUIの邪魔をしないように）
        touchDetector.transform.SetAsFirstSibling();
        
        // 透明なImageを追加（raycastTarget用）
        Image image = touchDetector.AddComponent<Image>();
        image.color = Color.clear; // 完全に透明
        image.raycastTarget = true; // タッチ検知を有効化
        
        // EventTriggerを追加してタッチイベントを検知
        EventTrigger eventTrigger = touchDetector.AddComponent<EventTrigger>();
        
        // PointerDownイベント（タッチ/クリック開始）
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => OnTouchDetected("Touch"));
        eventTrigger.triggers.Add(pointerDownEntry);
        
        Debug.Log("[CountDown] タッチ検知パネルを作成しました（Canvas全体をカバー）");
    }
    
    /// <summary>
    /// タッチが検知された時の処理
    /// </summary>
    private void OnTouchDetected(string source)
    {
        Debug.Log($"[CountDown] {source}を検知 - タイマーをリセット");
        ResetCountDownTimer();
        ResetRoomKickTimer();
    }

    public void StartCountDown()
    {
        timer = startSeconds;
        isCounting = true;
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCounting) return;

        // ヘルプパネルが開いている場合は一時停止
        if (settingUI != null && settingUI.IsAnyHelpPanelOpen())
        {
            if (!isPaused)
            {
                isPaused = true;
                Debug.Log("[CountDown] ヘルプパネルが開かれました - カウントダウンを一時停止");
            }
            return; // カウントダウンを停止
        }
        else
        {
            if (isPaused)
            {
                isPaused = false;
                Debug.Log("[CountDown] ヘルプパネルが閉じられました - カウントダウンを再開");
            }
        }

        // キーボード入力をチェック（追加の入力検知として残す）
        if (CheckKeyboardInput())
        {
            OnTouchDetected("Keyboard");
        }

        timer -= Time.deltaTime;
        
        // CardSelectionManagerに残り時間を通知
        CardSelectionManager cardSelectionManager = FindObjectOfType<CardSelectionManager>();
        if (cardSelectionManager != null)
        {
            cardSelectionManager.CheckBlinkCondition(timer);
        }
        
        if (timer > 0)
        {
            // タイマーが動いている間は何もしない
        }
        else
        {
            timer = 0;
            isCounting = false;
            if (OnCountDownEnd != null) OnCountDownEnd.Invoke();
        }
    }
    
    /// <summary>
    /// キーボード入力をチェック（補助的な入力検知）
    /// </summary>
    private bool CheckKeyboardInput()
    {
        // キーボード入力（特定のキーは除外）
        if (Input.anyKeyDown)
        {
            // ESCキーやF1-F12などのシステムキーは除外
            if (!Input.GetKeyDown(KeyCode.Escape) && 
                !Input.GetKeyDown(KeyCode.F1) && !Input.GetKeyDown(KeyCode.F2) && 
                !Input.GetKeyDown(KeyCode.F3) && !Input.GetKeyDown(KeyCode.F4) &&
                !Input.GetKeyDown(KeyCode.F5) && !Input.GetKeyDown(KeyCode.F6) &&
                !Input.GetKeyDown(KeyCode.F7) && !Input.GetKeyDown(KeyCode.F8) &&
                !Input.GetKeyDown(KeyCode.F9) && !Input.GetKeyDown(KeyCode.F10) &&
                !Input.GetKeyDown(KeyCode.F11) && !Input.GetKeyDown(KeyCode.F12))
            {
                return true;
            }
        }
        
        // マウスホイール
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// カウントダウンタイマーをリセット（ユーザーアクション時）
    /// </summary>
    public void ResetCountDownTimer()
    {
        if (isCounting && !isPaused)
        {
            timer = startSeconds;
            Debug.Log("[CountDown] ユーザーアクションによりタイマーをリセットしました");
        }
    }
    
    /// <summary>
    /// RoomKickタイマーをリセット（ユーザーアクション時）
    /// </summary>
    private void ResetRoomKickTimer()
    {
        if (roomKick != null)
        {
            roomKick.ResetTimer();
            Debug.Log("[CountDown] ユーザーアクションによりRoomKickタイマーをリセットしました");
        }
    }
    
    /// <summary>
    /// 外部からタイマーリセットを呼び出す（SettingUIのボタン操作など）
    /// </summary>
    public void ResetTimerFromExternalAction()
    {
        ResetCountDownTimer();
        ResetRoomKickTimer(); // RoomKickタイマーもリセット
    }
    
    /// <summary>
    /// カウントダウンを一時停止
    /// </summary>
    public void PauseCountDown()
    {
        isPaused = true;
        Debug.Log("[CountDown] カウントダウンを一時停止しました");
    }
    
    /// <summary>
    /// カウントダウンを再開
    /// </summary>
    public void ResumeCountDown()
    {
        isPaused = false;
        Debug.Log("[CountDown] カウントダウンを再開しました");
    }
    
    /// <summary>
    /// 現在一時停止中かどうかを取得
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    // カード送信後にタイマーを非表示にする
    public void HideTimer()
    {
        isCounting = false;
        timer = 0f; // タイマーを0にリセット
        isPaused = false;
        Debug.Log("[CountDown] タイマーを完全に停止しました");
    }
    
    // 新しいカード生成時にタイマーを再開する
    public void RestartTimerForNewCards()
    {
        StartCountDown();
        
        // Auto_Selectも再開
        if (autoSelect != null)
        {
            autoSelect.SetAutoSelectEnabled(true);
        }
        
        // CardSelectionManagerも再開
        CardSelectionManager cardSelectionManager = FindObjectOfType<CardSelectionManager>();
        if (cardSelectionManager != null)
        {
            cardSelectionManager.ResetForNewCards(); // 初期状態にリセット
        }
    }
    
    // カード送信後にテキストを非表示にする
    public void HideBlink()
    {
        CardSelectionManager cardSelectionManager = FindObjectOfType<CardSelectionManager>();
        if (cardSelectionManager != null)
        {
            cardSelectionManager.StopBlinkingExternal();
            Debug.Log("[CountDown] テキストを非表示にしました");
        }
        else
        {
            Debug.LogWarning("[CountDown] CardSelectionManagerが見つからないため、テキストを非表示にできません");
        }
    }
    
    // オブジェクト破棄時の処理
    void OnDestroy()
    {
        // タッチ検知パネルを削除
        if (touchDetector != null)
        {
            Destroy(touchDetector);
        }
    }
}
