using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class SettingUI : MonoBehaviourPunCallbacks
{
    [Header("UI パネル")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject helpPanel1;        // ルール用（旧HelpPanel）
    [SerializeField] private GameObject helpPanel2;        // 勝利条件用（新規追加）
    
    [Header("ボタン参照（オプション - 自動アタッチ用）")]
    [SerializeField] private Button settingButton;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button closeButton;          // SettingPanel内のCloseButton
    [SerializeField] private Button helpCloseButton;      // HelpPanel1内のCloseButton
    [SerializeField] private Button helpCloseButton2;     // HelpPanel2内のCloseButton
    [SerializeField] private Button nextButton;           // HelpPanel1内のNextButton
    [SerializeField] private Button backButton;           // HelpPanel2内のBackButton
    [SerializeField] private Button leaveButton;

    [Header("パネル開閉イベント")]
    [SerializeField] private UnityEvent onSettingsPanelOpened;
    [SerializeField] private UnityEvent onSettingsPanelClosed;
    [SerializeField] private UnityEvent onHelpPanel1Opened;
    [SerializeField] private UnityEvent onHelpPanel1Closed;
    [SerializeField] private UnityEvent onHelpPanel2Opened;
    [SerializeField] private UnityEvent onHelpPanel2Closed;
    
    [Header("ゲーム操作イベント")]
    [SerializeField] private UnityEvent onLeaveButtonPressed;
    
    // ルーム退出フラグ
    private bool isLeavingRoom = false;
    
    // CountDownの参照
    private CountDown countDown;

    void Start()
    {
        // CountDownコンポーネントを取得
        countDown = FindObjectOfType<CountDown>();
        if (countDown == null)
        {
            Debug.LogWarning("[SettingUI] CountDownコンポーネントが見つかりません");
        }

        // 初期状態でパネルを非表示にする
        InitializePanels();
        
        // ボタンの自動アタッチ（Inspector未設定の場合）
        AutoAttachButtons();
    }

    // パネルの初期化
    private void InitializePanels()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        if (helpPanel1 != null)
        {
            helpPanel1.SetActive(false);
        }
        if (helpPanel2 != null)
        {
            helpPanel2.SetActive(false);
        }
        Debug.Log("[SettingUI] パネルを初期化しました");
    }

    // ボタンの自動アタッチ
    private void AutoAttachButtons()
    {
        // SettingButton
        if (settingButton == null)
        {
            settingButton = GameObject.Find("SettingButton")?.GetComponent<Button>();
        }
        if (settingButton != null)
        {
            settingButton.onClick.RemoveAllListeners();
            settingButton.onClick.AddListener(OpenSettingsPanel);
            Debug.Log("[SettingUI] SettingButtonをアタッチしました");
        }

        // HelpButton
        if (helpButton == null)
        {
            helpButton = GameObject.Find("HelpButton")?.GetComponent<Button>();
        }
        if (helpButton != null)
        {
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(OpenHelpPanel1);
            Debug.Log("[SettingUI] HelpButtonをアタッチしました");
        }

        // CloseButton (SettingPanel内)
        if (closeButton == null)
        {
            closeButton = GameObject.Find("CloseButton")?.GetComponent<Button>();
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseSettingsPanel);
            Debug.Log("[SettingUI] CloseButton（設定パネル用）をアタッチしました");
        }

        // HelpCloseButton (HelpPanel1内)
        if (helpCloseButton == null)
        {
            helpCloseButton = GameObject.Find("HelpCloseButton")?.GetComponent<Button>();
        }
        if (helpCloseButton != null)
        {
            helpCloseButton.onClick.RemoveAllListeners();
            helpCloseButton.onClick.AddListener(CloseHelpPanel1);
            Debug.Log("[SettingUI] HelpCloseButton（ヘルプパネル1用）をアタッチしました");
        }

        // HelpCloseButton2 (HelpPanel2内)
        if (helpCloseButton2 == null)
        {
            helpCloseButton2 = GameObject.Find("HelpCloseButton2")?.GetComponent<Button>();
        }
        if (helpCloseButton2 != null)
        {
            helpCloseButton2.onClick.RemoveAllListeners();
            helpCloseButton2.onClick.AddListener(CloseHelpPanel2);
            Debug.Log("[SettingUI] HelpCloseButton2（ヘルプパネル2用）をアタッチしました");
        }

        // NextButton (HelpPanel1内)
        if (nextButton == null)
        {
            nextButton = GameObject.Find("NextButton")?.GetComponent<Button>();
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(GoToHelpPanel2);
            Debug.Log("[SettingUI] NextButton（ヘルプパネル1→2遷移用）をアタッチしました");
        }

        // BackButton (HelpPanel2内)
        if (backButton == null)
        {
            backButton = GameObject.Find("BackButton")?.GetComponent<Button>();
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToHelpPanel1);
            Debug.Log("[SettingUI] BackButton（ヘルプパネル2→1遷移用）をアタッチしました");
        }

        // LeaveButton
        if (leaveButton == null)
        {
            leaveButton = GameObject.Find("LeaveButton")?.GetComponent<Button>();
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(OnLeaveButtonPressed);
            Debug.Log("[SettingUI] LeaveButtonをアタッチしました");
        }
    }

    /// <summary>
    /// カウントダウンタイマーをリセット（ボタン操作時）
    /// </summary>
    private void ResetCountDownTimer()
    {
        if (countDown != null)
        {
            countDown.ResetTimerFromExternalAction();
        }
    }

    // --- オブザーバーパターン用のパブリックメソッド ---
    // これらのメソッドをInspectorのボタンイベントにアタッチします

    /// <summary>
    /// 設定パネルの表示/非表示を切り替える（ボタンのOnClickにアタッチ）
    /// </summary>
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            bool isCurrentlyActive = settingsPanel.activeSelf;
            
            if (isCurrentlyActive)
            {
                CloseSettingsPanel();
            }
            else
            {
                OpenSettingsPanel();
            }
        }
        else
        {
            Debug.LogWarning("[SettingUI] settingsPanelが設定されていません");
        }
    }

    /// <summary>
    /// 設定パネルを開く（SettingButtonのOnClickにアタッチ）
    /// </summary>
    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            // 他のパネルを閉じる
            CloseAllHelpPanels();
            
            settingsPanel.SetActive(true);
            Debug.Log("[SettingUI] 設定パネルを開きました");
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onSettingsPanelOpened?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] settingsPanelが設定されていません");
        }
    }

    /// <summary>
    /// 設定パネルを閉じる（SettingPanel内のCloseButtonのOnClickにアタッチ）
    /// </summary>
    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            
            // ヘルプパネルも非表示にする
            CloseAllHelpPanels();
            
            Debug.Log("[SettingUI] 設定パネルを閉じました（ヘルプパネルも一緒に非表示）");
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onSettingsPanelClosed?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] settingsPanelが設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル1の表示/非表示を切り替える（ボタンのOnClickにアタッチ）
    /// </summary>
    public void ToggleHelpPanel1()
    {
        if (helpPanel1 != null)
        {
            bool isCurrentlyActive = helpPanel1.activeSelf;
            
            if (isCurrentlyActive)
            {
                CloseHelpPanel1();
            }
            else
            {
                OpenHelpPanel1();
            }
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel1が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル1を開く（HelpButtonのOnClickにアタッチ）
    /// </summary>
    public void OpenHelpPanel1()
    {
        if (helpPanel1 != null)
        {
            // 他のヘルプパネルを閉じる
            CloseHelpPanel2();
            
            // ヘルプパネル1を表示
            helpPanel1.SetActive(true);
            
            // 設定パネルが開いている場合は非表示にする
            if (IsSettingsPanelOpen())
            {
                settingsPanel.SetActive(false);
                Debug.Log("[SettingUI] ヘルプパネル1を開き、設定パネルを一時的に非表示にしました");
            }
            else
            {
                Debug.Log("[SettingUI] ヘルプパネル1を開きました");
            }
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel1Opened?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel1が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル1を閉じる（HelpPanel1内のCloseButtonのOnClickにアタッチ）
    /// </summary>
    public void CloseHelpPanel1()
    {
        if (helpPanel1 != null)
        {
            // ヘルプパネル1を非表示
            helpPanel1.SetActive(false);
            
            // 設定パネルを再表示（ヘルプから戻る）
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                Debug.Log("[SettingUI] ヘルプパネル1を閉じ、設定パネルに戻りました");
            }
            else
            {
                Debug.Log("[SettingUI] ヘルプパネル1を閉じました");
            }
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel1Closed?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel1が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル2を開く（NextButtonのOnClickにアタッチ）
    /// </summary>
    public void OpenHelpPanel2()
    {
        if (helpPanel2 != null)
        {
            // 他のヘルプパネルを閉じる
            CloseHelpPanel1();
            
            // ヘルプパネル2を表示
            helpPanel2.SetActive(true);
            
            Debug.Log("[SettingUI] ヘルプパネル2を開きました");
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel2Opened?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel2が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル2を閉じる（HelpPanel2内のCloseButtonのOnClickにアタッチ）
    /// </summary>
    public void CloseHelpPanel2()
    {
        if (helpPanel2 != null)
        {
            // ヘルプパネル2を非表示
            helpPanel2.SetActive(false);
            
            // 設定パネルを再表示（ヘルプから戻る）
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
                Debug.Log("[SettingUI] ヘルプパネル2を閉じ、設定パネルに戻りました");
            }
            else
            {
                Debug.Log("[SettingUI] ヘルプパネル2を閉じました");
            }
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel2Closed?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel2が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル1からヘルプパネル2に遷移する（NextButtonのOnClickにアタッチ）
    /// </summary>
    public void GoToHelpPanel2()
    {
        if (helpPanel1 != null && helpPanel2 != null)
        {
            // ヘルプパネル1を非表示
            helpPanel1.SetActive(false);
            
            // ヘルプパネル2を表示
            helpPanel2.SetActive(true);
            
            Debug.Log("[SettingUI] ヘルプパネル1からヘルプパネル2に遷移しました");
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel1Closed?.Invoke();
            onHelpPanel2Opened?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel1またはhelpPanel2が設定されていません");
        }
    }

    /// <summary>
    /// ヘルプパネル2からヘルプパネル1に戻る（BackButtonのOnClickにアタッチ）
    /// </summary>
    public void BackToHelpPanel1()
    {
        if (helpPanel1 != null && helpPanel2 != null)
        {
            // ヘルプパネル2を非表示
            helpPanel2.SetActive(false);
            
            // ヘルプパネル1を表示
            helpPanel1.SetActive(true);
            
            Debug.Log("[SettingUI] ヘルプパネル2からヘルプパネル1に戻りました");
            
            // ボタン操作でタイマーリセット
            ResetCountDownTimer();
            
            // イベント通知
            onHelpPanel2Closed?.Invoke();
            onHelpPanel1Opened?.Invoke();
        }
        else
        {
            Debug.LogWarning("[SettingUI] helpPanel1またはhelpPanel2が設定されていません");
        }
    }

    /// <summary>
    /// 部屋を退出する（LeaveButtonのOnClickにアタッチ）
    /// </summary>
    public void OnLeaveButtonPressed()
    {
        if (isLeavingRoom) return; // 重複実行防止
        
        Debug.Log("[SettingUI] 部屋退出ボタンが押されました - FieldSceneへ遷移します");
        
        isLeavingRoom = true;
        
        // 全パネルを閉じる
        CloseAllPanels();
        
        // ボタン操作でタイマーリセット
        ResetCountDownTimer();
        
        // ルームの状態をチェック
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Debug.Log("[SettingUI] ルームに接続中です。RPCとキャッシュをクリアします。");
            // RPCとキャッシュをクリア
            try
            {
                PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
                PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SettingUI] RPCクリア中にエラーが発生しました: {e.Message}");
            }
            
            // ルームから退室
            try
            {
                PhotonNetwork.LeaveRoom();
                Debug.Log("[SettingUI] ルームから退出を試行しました");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SettingUI] ルーム退出中にエラーが発生しました: {e.Message}");
                // エラーが発生した場合も直接シーン遷移
                LoadFieldScene();
            }
        }
        else
        {
            Debug.Log("[SettingUI] ルームに接続されていないため、直接FieldSceneに遷移します");
            // 直接シーン遷移
            LoadFieldScene();
        }
        
        // 退出イベント通知
        onLeaveButtonPressed?.Invoke();
    }

    /// <summary>
    /// すべてのパネルを閉じる（ボタンのOnClickにアタッチ可能）
    /// </summary>
    public void CloseAllPanels()
    {
        CloseSettingsPanel();
        CloseAllHelpPanels();
        Debug.Log("[SettingUI] すべてのパネルを閉じました");
    }

    /// <summary>
    /// すべてのヘルプパネルを閉じる
    /// </summary>
    public void CloseAllHelpPanels()
    {
        if (helpPanel1 != null)
        {
            helpPanel1.SetActive(false);
        }
        if (helpPanel2 != null)
        {
            helpPanel2.SetActive(false);
        }
        Debug.Log("[SettingUI] すべてのヘルプパネルを閉じました");
    }

    // --- パネルの状態を取得するメソッド ---

    /// <summary>
    /// 設定パネルが開いているかどうかを取得
    /// </summary>
    public bool IsSettingsPanelOpen()
    {
        return settingsPanel != null && settingsPanel.activeSelf;
    }

    /// <summary>
    /// ヘルプパネル1が開いているかどうかを取得
    /// </summary>
    public bool IsHelpPanel1Open()
    {
        return helpPanel1 != null && helpPanel1.activeSelf;
    }

    /// <summary>
    /// ヘルプパネル2が開いているかどうかを取得
    /// </summary>
    public bool IsHelpPanel2Open()
    {
        return helpPanel2 != null && helpPanel2.activeSelf;
    }

    /// <summary>
    /// いずれかのヘルプパネルが開いているかどうかを取得
    /// </summary>
    public bool IsAnyHelpPanelOpen()
    {
        return IsHelpPanel1Open() || IsHelpPanel2Open();
    }

    /// <summary>
    /// いずれかのパネルが開いているかどうかを取得
    /// </summary>
    public bool IsAnyPanelOpen()
    {
        return IsSettingsPanelOpen() || IsAnyHelpPanelOpen();
    }

    /// <summary>
    /// ルーム退出完了時の処理（Photonコールバック）
    /// </summary>
    public override void OnLeftRoom()
    {
        Debug.Log("[SettingUI] ルームから退出完了 - FieldSceneに遷移します");
        LoadFieldScene();
    }
    
    /// <summary>
    /// FieldSceneに遷移
    /// </summary>
    private void LoadFieldScene()
    {
        StartCoroutine(LoadFieldSceneCoroutine());
    }
    
    /// <summary>
    /// FieldScene遷移のコルーチン
    /// </summary>
    private IEnumerator LoadFieldSceneCoroutine()
    {
        // 1フレーム待機して確実にPhotonの処理を完了させる
        yield return new WaitForEndOfFrame();
        
        Debug.Log("[SettingUI] FieldSceneをロードします");
        SceneManager.LoadScene("FieldScene");
    }

    // オブジェクト破棄時の処理
    void OnDestroy()
    {
        // ボタンのイベントを解除してメモリリークを防ぐ
        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OpenSettingsPanel);
        }
        if (helpButton != null)
        {
            helpButton.onClick.RemoveListener(OpenHelpPanel1);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettingsPanel);
        }
        if (helpCloseButton != null)
        {
            helpCloseButton.onClick.RemoveListener(CloseHelpPanel1);
        }
        if (helpCloseButton2 != null)
        {
            helpCloseButton2.onClick.RemoveListener(CloseHelpPanel2);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(GoToHelpPanel2);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(BackToHelpPanel1);
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveListener(OnLeaveButtonPressed);
        }
    }
}

