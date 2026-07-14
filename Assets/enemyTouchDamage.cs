using UnityEngine;
using UnityEngine.SceneManagement;

// Touching the player ends the game: "CAUGHT!" screen, R restarts
// (there is no health system — one touch is enough).
public class EnemyTouchDamage : MonoBehaviour
{
    static bool caught = false;     // shared: only one enemy shows the screen

    void Start()
    {
        caught = false;             // reset after a scene reload
    }

    void OnCollisionStay(Collision collision)
    {
        TryCatch(collision.collider);
    }

    void OnTriggerStay(Collider other)
    {
        TryCatch(other);
    }

    void TryCatch(Collider c)
    {
        if (caught || !c.CompareTag("Player")) return;

        caught = true;
        Time.timeScale = 0f;    // freeze the game
        Debug.Log("💀 An enemy caught you — game over!");
    }

    void Update()
    {
        if (caught && Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        if (!caught) return;

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
        title.normal.textColor = new Color(1f, 0.25f, 0.2f);

        var subtitle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            alignment = TextAnchor.MiddleCenter
        };
        subtitle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, Screen.height / 2f - 100, Screen.width, 100), "CAUGHT! 💀", title);
        GUI.Label(new Rect(0, Screen.height / 2f + 10, Screen.width, 50), "Press R to try again", subtitle);
    }
}
