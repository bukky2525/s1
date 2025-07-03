using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class To_Field : MonoBehaviour
{
    private bool isTransitioning = false;
    private PhotonView photonView;
    private float waitTime = 20f; // 20秒待つ
    private float timer = 0f;
    [SerializeField] private string backSceneName = "FieldScene"; // 戻るシーン名（必要に応じて変更）

    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // スペースキーが押されたらFieldSceneへ移行
        if (Input.GetKeyDown(KeyCode.Space) && !isTransitioning)
        {
            Debug.Log("スペースキーが押されました - FieldSceneへ移行します");
            isTransitioning = true;
            
            // PhotonViewを破棄
            if (photonView != null)
            {
                PhotonNetwork.Destroy(photonView);
                photonView = null;
            }
            
            // シーン遷移前に少し待機
            StartCoroutine(LoadFieldScene());
        }

        // タイマーを進める
        if (!isTransitioning)
        {
            timer += Time.deltaTime;
            if (timer >= waitTime)
            {
                Debug.Log($"{waitTime}秒間待機したのでFieldSceneへ移行します");
                isTransitioning = true;
                
                // PhotonViewを破棄
                if (photonView != null)
                {
                    PhotonNetwork.Destroy(photonView);
                    photonView = null;
                }
                
                // シーン遷移前に少し待機
                StartCoroutine(LoadFieldScene());
            }
        }
    }

    private IEnumerator LoadFieldScene()
    {
        // 1フレーム待機してPhotonViewの破棄を確実にする
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene("FieldScene");
    }

    private IEnumerator BackToPrevScene()
    {
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(backSceneName);
    }
}
