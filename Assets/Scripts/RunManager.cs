using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;

        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            StartRun();
        }
    }

    public void StartRun()
    {
        SceneManager.LoadScene("Run");
    }
}
