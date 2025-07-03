using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectionManager : MonoBehaviour
{
    [SerializeField] private Text autoSelectText; // Inspectorでアタッチ必須
    [SerializeField] private int startBlinkAtSeconds = 10; // 表示を開始する残り秒数（10秒前）
    
    private CountDown countDown;
    
    // Start is called before the first frame update
    void Start()
    {
        if (autoSelectText == null)
        {
            Debug.LogError("[CardSelectionManager] autoSelectTextがInspectorでセットされていません！");
            enabled = false;
            return;
        }
        
        // CountDownコンポーネントを取得
        countDown = FindObjectOfType<CountDown>();
        if (countDown == null)
        {
            Debug.LogWarning("[CardSelectionManager] CountDownコンポーネントが見つかりません");
        }
        
        // 初期状態では非表示
        autoSelectText.gameObject.SetActive(false);
    }

    // 外部からテキストを非表示にするメソッド
    public void StopBlinkingExternal()
    {
        Debug.Log("[CardSelectionManager] 外部からテキスト非表示が呼ばれました");
        
        // テキストを非表示
        if (autoSelectText != null)
        {
            autoSelectText.gameObject.SetActive(false);
        }
    }
    
    // 新しいカード生成時に初期状態にリセット
    public void ResetForNewCards()
    {
        // テキストをリセット
        if (autoSelectText != null)
        {
            autoSelectText.gameObject.SetActive(false);
        }
    }
    
    // CountDownから呼ばれるメソッド（残り時間をチェック）
    public void CheckBlinkCondition(float remainingTime)
    {
        // タイマーが0の場合はテキストを非表示
        if (remainingTime <= 0f)
        {
            if (autoSelectText != null)
            {
                autoSelectText.gameObject.SetActive(false);
            }
            return;
        }
        
        // 残り10秒になったらテキスト表示
        if (remainingTime <= startBlinkAtSeconds)
        {
            // テキストを表示・更新（10秒のカウントダウン）
            if (autoSelectText != null)
            {
                autoSelectText.gameObject.SetActive(true);
                int countdownSeconds = Mathf.CeilToInt(remainingTime);
                autoSelectText.text = $"カードを選択してください\n残り{countdownSeconds}秒で自動選択されます";
            }
        }
        // 10秒を超えたらテキスト非表示
        else if (remainingTime > startBlinkAtSeconds)
        {
            // テキストを非表示
            if (autoSelectText != null)
            {
                autoSelectText.gameObject.SetActive(false);
            }
        }
    }
}
