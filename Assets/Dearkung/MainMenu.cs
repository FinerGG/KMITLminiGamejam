using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MainMenu : MonoBehaviour
{
    [Header("UI & Video Elements")]
    public GameObject menuUI;         // ใส่ GameObject ที่รวม UI และ BG ทั้งหมด
    public VideoPlayer cutsceneVideo; // ใส่ VideoPlayer Component

    private bool isPlayingCutscene = false;

    public void PlayGame()
    {
        // 1. ซ่อนภาพ BG และปุ่มกดทั้งหมดทันที
        if (menuUI != null)
        {
            menuUI.SetActive(false);
        }

        // 2. เล่นวิดีโอคัทซีน
        if (cutsceneVideo != null)
        {
            isPlayingCutscene = true;
            cutsceneVideo.Play();

            // รอ 12 วินาทีแล้วค่อยเปลี่ยน Scene
            StartCoroutine(WaitAndLoadScene(12f));
        }
        else
        {
            LoadNextScene();
        }
    }

    void Update()
    {
        // แป้นพิมพ์ข้ามคัทซีน (กด Spacebar หรือ Esc เพื่อข้ามได้)
        if (isPlayingCutscene && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape)))
        {
            StopAllCoroutines();
            LoadNextScene();
        }
    }

    private IEnumerator WaitAndLoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited");
        Application.Quit();
    }

    
}