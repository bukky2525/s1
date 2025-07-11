using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public GameObject circlePrefab; // 白丸プレファブをInspectorでセット
    [SerializeField] private Text loadingText; // 接続中テキストをInspectorでアタッチ
    private const float DURATION = 1f; // アニメーション全体の基準時間
    private const int CIRCLE_COUNT = 3; // 丸の数
    public float spacing = 120f; // 丸同士の間隔（100サイズに合わせて大きめに）

    // 追加：Tweenを管理する配列
    private Sequence[] sequences = new Sequence[CIRCLE_COUNT];
    private GameObject[] circles; // 丸オブジェクトの参照を保持

    // オブジェクト破棄時にDOTweenアニメーションをキル
    void OnDestroy()
    {
        // 全てのSequenceを確実にKill
        if (sequences != null)
        {
            foreach (var seq in sequences)
            {
                if (seq != null && seq.IsActive())
                {
                    seq.Kill();
                }
            }
        }

        // 各円オブジェクトのTransformのTweenもKill
        if (circles != null)
        {
            foreach (var circle in circles)
            {
                if (circle != null && circle.transform != null)
                {
                    circle.transform.DOKill();
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 接続中テキストの初期化
        if (loadingText != null)
        {
            loadingText.text = "Connecting";
            loadingText.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[Loading] loadingTextがInspectorでセットされていません");
        }

        circles = new GameObject[CIRCLE_COUNT]; // 丸オブジェクトを格納する配列
        for (int i = 0; i < CIRCLE_COUNT; i++)
        {
            circles[i] = Instantiate(circlePrefab, this.transform);
            circles[i].transform.localPosition = new Vector3((i - (CIRCLE_COUNT - 1) / 2f) * spacing, 0, 0);
            circles[i].transform.localScale = new Vector3(100f, 100f, 1f);
            
            // 表示を確実にするための設定
            var sr = circles[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingOrder = 1000; // 最前面に表示
                sr.color = Color.white; // 白色を確実に設定
            }
            
            sequences[i] = DOTween.Sequence()
                .SetLoops(-1, LoopType.Restart)
                .SetDelay((DURATION / 2) * ((float)i / CIRCLE_COUNT))
                .Append(circles[i].transform.DOScale(new Vector3(150f, 150f, 1f), DURATION / 4))
                .Append(circles[i].transform.DOScale(new Vector3(100f, 100f, 1f), DURATION / 4))
                .AppendInterval((DURATION / 2) * ((float)(1 - i) / CIRCLE_COUNT));
            sequences[i].Play(); // アニメーション開始
        }
    }

    public void HideLoading()
    {
        // 1. Tweenを全てKill
        foreach (var seq in sequences)
        {
            if (seq != null && seq.IsActive()) seq.Kill();
        }

        // 各円オブジェクトのTransformのTweenもKill
        if (circles != null)
        {
            foreach (var circle in circles)
            {
                if (circle != null && circle.transform != null)
                {
                    circle.transform.DOKill();
                }
            }
        }

        // 2. 接続中テキストを非表示
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        // 3. 全ての白丸（子オブジェクト）を非表示にする
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        // 0.1秒後にカード配りを呼ぶ
        Invoke(nameof(CallHandManagerCreateHand), 0.1f);

        // 2秒後に親（Loading）を削除
        Invoke(nameof(DeleteSelf), 2f);
    }

    private void CallHandManagerCreateHand()
    {
        var handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.CreateHand();
        }
        else
        {
            Debug.LogWarning("HandManagerが見つかりませんでした");
        }
    }

    private void DeleteSelf()
    {
        Destroy(this.gameObject);
    }

    // 接続完了時に呼ぶ
    public void HideLoadingAndDeal()
    {
        StartCoroutine(HideAndDeal());
    }

    private System.Collections.IEnumerator HideAndDeal()
    {
        // 1. Tweenを全てKill
        foreach (var seq in sequences)
        {
            if (seq != null && seq.IsActive()) seq.Kill();
        }

        // 各円オブジェクトのTransformのTweenもKill
        if (circles != null)
        {
            foreach (var circle in circles)
            {
                if (circle != null && circle.transform != null)
                {
                    circle.transform.DOKill();
                }
            }
        }

        // 2. 接続中テキストを非表示
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        // 3. 白丸（子オブジェクト）のSpriteRendererを無効化
        foreach (Transform child in transform)
        {
            //Debug.Log("Loadingの子: " + child.gameObject.name);
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
                Debug.Log("SpriteRendererを無効化しました");
            }
            else
            {
                Debug.LogWarning("SpriteRendererが見つかりませんでした");
            }
        }

        // 4. 0.1秒待つ
        yield return new WaitForSeconds(0.1f);

        // 5. HandManagerのカード配りを呼ぶ
        var handManager = FindObjectOfType<HandManager>();
        if (handManager != null)
        {
            handManager.CreateHand();
        }
        else
        {
            Debug.LogWarning("HandManagerが見つかりませんでした");
        }

        // 6. さらに1.9秒待って（合計2秒後）親（Loading）を削除
        yield return new WaitForSeconds(1.9f);
        Destroy(this.gameObject);
    }
}
