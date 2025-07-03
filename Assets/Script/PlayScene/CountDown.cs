using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountDown : MonoBehaviour
{
    [SerializeField] private int startSeconds = 30; // カウントダウン秒数（30秒）
    private float timer;
    private bool isCounting = false;
    private Auto_Select autoSelect;

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
        
        timer = startSeconds;
        
        StartCountDown();
    }

    public void StartCountDown()
    {
        timer = startSeconds;
        isCounting = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCounting) return;

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
    
    // カード送信後にタイマーを非表示にする
    public void HideTimer()
    {
        isCounting = false;
        timer = 0f; // タイマーを0にリセット
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
}
