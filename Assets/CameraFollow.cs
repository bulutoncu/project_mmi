using UnityEngine;

// Makes the camera smoothly follow the player from behind.
// MazeGenerator adds this to the Main Camera automatically (FollowBehind mode).
public class CameraFollow : MonoBehaviour
{
    public Transform target;                            // the player to follow
    public Vector3 offset = new Vector3(0f, 11f, -7f);  // view from above and behind
    public float smoothness = 5f;

    void LateUpdate()
    {
        if (target == null)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) target = pm.transform;
            else return;
        }

        Vector3 goal = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, goal, smoothness * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up);
    }
}
