using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Text用

public class Text_Tenmetu : MonoBehaviour
{
    // アタッチするテキスト
    [SerializeField] private Text targetText; // UI Text用（旧UGUI）
    [SerializeField] private float blinkInterval = 0.5f; // 点滅間隔（秒）

    private float timer = 0f;
    private bool isVisible = true;

    // Start is called before the first frame update
    void Start()
    {
        if (targetText == null)
        {
            targetText = GetComponent<Text>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (targetText == null) return;

        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            isVisible = !isVisible;
            targetText.enabled = isVisible;
            timer = 0f;
        }
    }
}
