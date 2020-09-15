using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelRestart : MonoBehaviour
{
    public Text restartTimerText;
    public float maxRestartTimer;
    private float currentRestartTimer;
    private GameObject player;
    private playerController1 pc;
    // Start is called before the first frame update
    void Start()
    {
        currentRestartTimer = maxRestartTimer;
        player = GameObject.FindGameObjectWithTag("player");
        pc = player.GetComponent<playerController1>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pc.currentHealth <= 0) // check the players health
        {
            currentRestartTimer -= Time.deltaTime; 
            string timeLeft = currentRestartTimer.ToString("F2");
            restartTimerText.text = "Level restarting in: " + timeLeft;
        }
     
        if (currentRestartTimer < 0)//reloads current scene
        {
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }
        
    }


 }
