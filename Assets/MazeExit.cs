using UnityEngine;
using UnityEngine.SceneManagement;

// Exit portal: touching it wins the game.
// Press R to restart (a brand new maze is generated).
public class MazeExit : MonoBehaviour
{
    bool won = false;

    void OnTriggerEnter(Collider other)
    {
        if (!won && other.CompareTag("Player"))
        {
            won = true;
            Time.timeScale = 0f;    // freeze the game
            Debug.Log("🎉 You escaped the maze — you win!");
        }
    }

    void Update()
    {
        if (won && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        if (!won) return;

        // Semi-transparent black backdrop
        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var title = new GUIStyle(GUI.skin.label)
        {
            fontSize = 64,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        title.normal.textColor = new Color(0.3f, 1f, 0.6f);

        var subtitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
        subtitle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height / 2f - 100, Screen.width, 100), "YOU WIN! 🎉", title);
        GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 50), "Press R to play again", subtitle);
    }
}
