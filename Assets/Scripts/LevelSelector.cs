using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public void LoadLevel() {
            SceneManager.LoadScene(gameObject.name);
    }
}
