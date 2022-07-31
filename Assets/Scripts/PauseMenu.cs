using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace P90brush
{
    public class PauseMenu : MonoBehaviour
    {

        public static bool GameIsPaused = false;

        public GameObject pauseMenuUI;

        // Start is called before the first frame update
        void Start() {
        }

        // Update is called once per frame
        void Update() {
            // Cursor Lock Managment
            if (Input.GetKeyDown(KeyCode.Escape)) {
                if (GameIsPaused) {
                    Resume();
                } else {
                    Pause();
                }
            }
        }

        private void Pause() {
            GameIsPaused = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (pauseMenuUI) {
                pauseMenuUI.SetActive(true);
            }
            Time.timeScale = 0f;
        }

        public void Resume() {
            GameIsPaused = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (pauseMenuUI) {
                pauseMenuUI.SetActive(false);
            }
            Time.timeScale = 1f;
        }

        public void LevelSelectorMenu() {
            Resume();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene("level_selector");
        }

        public void Quit() {
            Application.Quit();
        }
    }
}