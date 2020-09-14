using UnityEngine;
using UnityEngine.UI;

public class EnemiesLeftCounter : MonoBehaviour
{
    private Text counterText;
    private float enemyCounter;
    [Header("UIS")]
    [Tooltip("level complete UI")]
    public Canvas completeCanvas;
    [Tooltip("gameplay UI")]
    public Canvas gamePlayScreen;
    [Header("PLAYER BLOCKING OBJECT")]
    [Tooltip("a object to get disabled when there are no more enemies")]
    public GameObject playerBlockingWall;

    private GameObject[] torches;

    private void Awake()
    {
        counterText = gameObject.GetComponent<Text>();
        torches = GameObject.FindGameObjectsWithTag("torch");
        //turn the torches off
        foreach (GameObject fire in torches)
        {
            fire.SetActive(false);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        completeCanvas.enabled = false;
    }

    public void subtract()
    {
        enemyCounter--;
        counterText.text = "Enemies left: " + enemyCounter;
        if(enemyCounter <= 0)
        {
            if(playerBlockingWall != null)
            {
                playerBlockingWall.SetActive(false);
                counterText.text = "all enemies have been slain";
            }
            setTorchesOnFire();
        }
    }

    public void add()
    {
        enemyCounter++;
        counterText.text = "Enemies left: " + enemyCounter;
    }
    public void getNumberOfEnemies()
    {
        GameObject[] list = GameObject.FindGameObjectsWithTag("enemy");
        enemyCounter = list.Length;
        counterText.text = "Enemies left: " + enemyCounter;
    }

    public float EnemiesLeft
    {
        get { return enemyCounter; }
    }

    private void setTorchesOnFire()
    {
        foreach(GameObject fire in torches)
        {
            fire.SetActive(true);
        }
    }
}
