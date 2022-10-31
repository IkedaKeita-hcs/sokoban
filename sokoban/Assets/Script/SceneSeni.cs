using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSeni : MonoBehaviour
{
    private void Start()
    {
        //コルーチンの実行
        StartCoroutine(TitleDelay());
    }


    float Delay = 5.0f;//ディレイ時間
    private IEnumerator TitleDelay()
    {
        //2秒待つ
        yield return new WaitForSeconds(Delay);

    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene("MainScene");
        }

    }

}
  
