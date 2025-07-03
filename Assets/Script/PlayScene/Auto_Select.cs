using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Auto_Select : MonoBehaviour
{
    private HandManager handManager;
    private CountDown countDown;
    private bool isAutoSelectEnabled = true;
    
    // カードの座標位置（HandManagerから取得）
    private Vector2[] cardPositions = new Vector2[]
    {
        new Vector2(-600f, -315f),
        new Vector2(-300f, -315f),
        new Vector2(   0f, -315f),
        new Vector2( 300f, -315f),
        new Vector2( 600f, -315f)
    };

    // Start is called before the first frame update
    void Start()
    {
        // 必要なコンポーネントを取得
        handManager = FindObjectOfType<HandManager>();
        countDown = FindObjectOfType<CountDown>();
        
        if (handManager == null)
        {
            Debug.LogError("[Auto_Select] HandManagerが見つかりません");
        }
        
        if (countDown == null)
        {
            Debug.LogWarning("[Auto_Select] CountDownが見つかりません");
        }
        else
        {
            // CountDownの終了イベントに登録
            countDown.OnCountDownEnd += OnCountDownEnd;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // オート選択が有効でない場合は何もしない
        if (!isAutoSelectEnabled) return;
        
        // CountDownが存在する場合は、CountDownに任せる
        if (countDown != null)
        {
            return;
        }
        
        // CountDownがない場合は従来の動作（バックアップ用）
        // プレイヤーのアクションを検出（より細かい検出）
        if (CheckPlayerAction())
        {
            ResetAutoSelectTimer();
        }
        
        // バックアップ機能は削除（CountDownに完全に依存）
        // タイマーチェック
        // if (Time.time - lastActionTime >= autoSelectDelay)
        // {
        //     PerformAutoSelect();
        // }
    }
    
    // CountDown終了時のコールバック
    private void OnCountDownEnd()
    {
        Debug.Log("[Auto_Select] CountDown終了、カードを自動選択します");
        PerformAutoSelect();
    }
    
    // プレイヤーのアクションをチェック（CountDownがない場合のみ使用）
    private bool CheckPlayerAction()
    {
        // マウスボタンのクリック
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
        {
            return true;
        }
        
        // キーボード入力
        if (Input.anyKeyDown)
        {
            return true;
        }
        
        // マウスホイール
        if (Input.GetAxis("Mouse ScrollWheel") != 0f)
        {
            return true;
        }
        
        return false;
    }
    
    // オート選択タイマーをリセット（CountDown用）
    public void ResetAutoSelectTimer()
    {
        // CountDownのタイマーを使用するため、ここでは何もしない
    }
    
    // オート選択を実行（カード選択のみ）
    private void PerformAutoSelect()
    {
        if (handManager == null)
        {
            Debug.LogWarning("[Auto_Select] HandManagerが見つかりません");
            return;
        }
        
        // 現在表示されているカードを取得（Card1(Clone)オブジェクト）
        GameObject[] allCardClones = GameObject.FindGameObjectsWithTag("Card");
        List<GameObject> availableCards = new List<GameObject>();
        
        // 実際に表示されているカードのみをフィルタリング
        foreach (GameObject cardClone in allCardClones)
        {
            // カードが実際に表示されているかチェック（非アクティブでない）
            if (cardClone.activeInHierarchy && cardClone.GetComponent<SelectCard>() != null)
            {
                availableCards.Add(cardClone);
            }
        }
        
        if (availableCards.Count == 0)
        {
            Debug.Log("[Auto_Select] 選択可能なカードが見つかりません");
            return;
        }
        
        // ランダムでカードを選択（一枚だけ）
        int randomIndex = Random.Range(0, availableCards.Count);
        GameObject selectedCard = availableCards[randomIndex];
        
        Debug.Log($"[Auto_Select] ランダムでカードを選択しました: {randomIndex + 1}番目のカード (合計{availableCards.Count}枚中)");
        
        // 選択されたカードのSelectCardコンポーネントを取得
        SelectCard selectCardComponent = selectedCard.GetComponent<SelectCard>();
        if (selectCardComponent != null)
        {
            // SelectCardのTryDeleteメソッドを呼び出してカード選択を実行
            // リフレクションを使ってprivateメソッドにアクセス
            var method = typeof(SelectCard).GetMethod("TryDelete", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(selectCardComponent, new object[] { "AutoSelect" });
                Debug.Log("[Auto_Select] カード選択を実行しました");
            }
            else
            {
                Debug.LogError("[Auto_Select] SelectCardのTryDeleteメソッドが見つかりません");
            }
        }
        else
        {
            Debug.LogError("[Auto_Select] 選択されたカードにSelectCardコンポーネントがありません");
        }
        
        // オート選択を一時的に無効化（連続実行を防ぐ）
        isAutoSelectEnabled = false;
    }
    
    // オート選択の有効/無効を切り替え
    public void SetAutoSelectEnabled(bool enabled)
    {
        isAutoSelectEnabled = enabled;
    }
    
    void OnDestroy()
    {
        // イベントの登録解除
        if (countDown != null)
        {
            countDown.OnCountDownEnd -= OnCountDownEnd;
        }
    }
}
