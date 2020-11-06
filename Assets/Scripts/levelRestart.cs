using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class levelRestart : MonoBehaviour
{
    public Text restartTimerText;
    public float maxRestartTimer;
    private float currentRestartTimer;
    // Start is called before the first frame update
    void Start()
    {
        currentRestartTimer = maxRestartTimer;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerStats.health <= 0) // check the players health
        {
            currentRestartTimer -= Time.deltaTime; 
            string timeLeft = currentRestartTimer.ToString("F2");
            restartTimerText.text = "Respawning in: " + timeLeft;
        }
     
        if (currentRestartTimer < 0)//reloads current scene
        {
            Time.timeScale = 1.0f;//remove pause
            playerStats.health = playerStats.maxHealth;
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name == "TutorialLevel")
            { 
                SceneManager.LoadScene(0); 
            }
            else
            {
                SceneManager.LoadScene(1);
            }
        }
        
    }


 }
