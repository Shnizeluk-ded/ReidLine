using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class RunManager : MonoBehaviour
{
    public static RunManager Instance;
    private bool runStarted;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame && !runStarted)
        {
            StartRun();
            runStarted = true;
        }
    }

    public void StartRun()
    {
        SceneManager.LoadScene("Run");
    }

    public void EndRun()
    {
        SceneManager.LoadScene("Lobby");
        runStarted = false;
    }
}
