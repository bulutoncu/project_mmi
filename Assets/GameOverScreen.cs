using UnityEngine;
using UnityEngine.SceneManagement;

// Freezes the game and shows a "YOU DIED" screen when health runs out.
// Press R to restart. MazeGenerator adds this to the player automatically.
public class GameOverScreen : MonoBehaviour
{
    PlayerHealth health;
    bool dead = false;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!dead && health != null && health.currentHealth <= 0f)
        {
            dead = true;
            Time.timeScale = 0f;    // freeze the game
            Debug.Log("💀 Out of health — game over!");
        }

        if (dead && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        if (!dead) return;

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 64,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(1f, 0.25f, 0.2f);

        var subtitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
        subtitle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height / 2f - 100, Screen.width, 100), "YOU DIED 💀", title);
        GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 50), "Press R to try again", subtitle);
    }
}
