using UnityEngine;
using UnityEngine.SceneManagement;

public class XButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

}
