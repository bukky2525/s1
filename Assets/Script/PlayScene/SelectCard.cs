using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/*
 カードの選択機能（シンプルなタッチ対応版）
 ダブルタップ、ドラッグ＆ドロップ
 PC・スマホ・タブレット対応
 軽い視覚フィードバック付き
 */

public class SelectCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private RectTransform rectTransform; // カードのRectTransformコンポーネント
    private Canvas canvas; // カードが配置されているキャンバス
    private Image image; // カードの画像コンポーネント
    private CardSendRPC cardSender; // CardSendRPCの参照

    private Vector2 initialPosition; // 初期の位置を記録
    private Vector3 initialScale; // 初期のスケールを記録
    private float lastClickTime = 0f; // 最後にクリックされた時間
    private const float doubleClickThreshold = 0.4f; // ダブルタップと判定する時間間隔
    private bool isDragging = false; // ドラッグ中かどうかのフラグ
    
    [Header("タッチ操作設定")]
    [SerializeField] private float touchScaleMultiplier = 1.1f; // タッチ時のスケール倍率
    [SerializeField] private float animationDuration = 0.15f; // アニメーションの時間
    [SerializeField] private float deleteThreshold = 150f; // 削除判定のY座標閾値
    [SerializeField] private Color pressedColor = new Color(1f, 1f, 1f, 0.8f); // 押下時の色
    
    public int cardIndex; // カードのインデックス

    // カードの初期位置を設定するメソッド
    public void SetInitialPosition(Vector2 position)
    {
        initialPosition = position; // 初期位置を記録
        transform.localPosition = position; // カードの位置を設定
    }

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // RectTransformコンポーネントを取得
        image = GetComponent<Image>(); // Imageコンポーネントを取得
        canvas = GetComponentInParent<Canvas>(); // 親のCanvasコンポーネントを取得
        initialPosition = rectTransform.anchoredPosition; // 初期位置を現在の位置に設定
        initialScale = rectTransform.localScale; // 初期スケールを記録
        cardSender = FindObjectOfType<CardSendRPC>(); // CardSendRPCコンポーネントを取得
        
        // タッチエリアを有効化
        if (image != null)
        {
            image.raycastTarget = true;
        }
    }

    // マウスボタン/タッチが押されたときに呼ばれる
    public void OnPointerDown(PointerEventData eventData)
    {
        // 軽い視覚フィードバック
        rectTransform.DOScale(initialScale * touchScaleMultiplier, animationDuration);
        if (image != null)
        {
            image.DOColor(pressedColor, animationDuration);
        }
        
        // ダブルタップの判定
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            TryDelete("DoubleTap"); // ダブルタップで削除を試みる
            return;
        }
        lastClickTime = Time.time; // タップ時間を更新
    }

    // ドラッグ中に呼ばれる
    public void OnDrag(PointerEventData eventData)
    {
        isDragging = true; // ドラッグ中フラグを立てる

        Vector2 localPoint;
        // スクリーン座標をローカル座標に変換
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        rectTransform.anchoredPosition = localPoint; // カードの位置を更新
        
        // ドラッグ中の視覚フィードバック（距離に応じて透明度変更）
        float dragDistance = Vector2.Distance(localPoint, initialPosition);
        float alpha = Mathf.Clamp01(1f - (dragDistance / 400f)); // 距離に応じて透明度を変更
        if (image != null)
        {
            Color currentColor = image.color;
            currentColor.a = Mathf.Max(alpha, 0.4f); // 最低でも40%の透明度を保持
            image.color = currentColor;
        }
    }

    // マウスボタン/タッチが離されたときに呼ばれる
    public void OnPointerUp(PointerEventData eventData)
    {
        // 視覚フィードバックをリセット
        if (!isDragging)
        {
            // タップのみの場合
            rectTransform.DOScale(initialScale, animationDuration);
            if (image != null)
            {
                image.DOColor(Color.white, animationDuration);
            }
        }
        
        if (isDragging)
        {
            float y = rectTransform.anchoredPosition.y; // 現在のY座標を取得

            if (y > deleteThreshold)
            {
                TryDelete($"DragDrop Y={y}"); // 一定の高さを超えたら削除を試みる
            }
            else
            {
                // 元の位置に戻すアニメーション
                rectTransform.DOAnchorPos(initialPosition, animationDuration * 1.5f).SetEase(Ease.OutBack);
                rectTransform.DOScale(initialScale, animationDuration);
                if (image != null)
                {
                    image.DOColor(Color.white, animationDuration);
                }
                Debug.Log($"[戻す] Y={y} の {image.sprite.name}");
            }
        }

        isDragging = false; // ドラッグ中フラグを解除
    }

    // カードを削除するメソッド
    private void TryDelete(string reason)
    {
        if (image != null && image.sprite != null)
        {
            string spriteName = image.sprite.name; // スプライト名を取得
            Debug.Log($"[{reason}] 削除: {spriteName}");

            // 削除アニメーション
            rectTransform.DOScale(Vector3.zero, animationDuration * 1.5f).SetEase(Ease.InBack);
            if (image != null)
            {
                image.DOFade(0f, animationDuration * 1.5f);
            }

            // 削除を通知
            if (cardSender != null)
            {
                cardSender.SendCardImageName(spriteName); // スプライト名を送信
            }
            else
            {
                Debug.LogWarning("[SelectCard] CardSendRPCコンポーネントが見つかりません。カードの送信ができません。");
            }
            
            // アニメーション完了後にオブジェクトを削除
            Destroy(gameObject, animationDuration * 1.5f);
        }
        else
        {
            Debug.Log($"[{reason}] 削除: 不明な画像");
            Destroy(gameObject);
        }
    }

    // 初期位置を取得するメソッド
    public Vector2 GetInitialPosition()
    {
        return initialPosition;
    }
}
