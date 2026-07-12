using UnityEngine;
using UnityEngine.AI;

// Patrolling enemy: wanders slowly between points in its own region.
// If it spots the player (close enough AND no wall in between) it speeds up
// and gives chase; once the player gets far enough away it gives up
// and returns to its patrol. A red light glows while it is chasing.
public class EnemyPatrol : MonoBehaviour
{
    public Transform player;
    public Vector3[] patrolPoints;
    public float sightRange = 11f;      // notices the player within this range if line of sight is clear
    public float giveUpRange = 16f;     // stops chasing once the player is this far away
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4.5f;

    NavMeshAgent agent;
    Light angerLight;
    int targetIndex = -1;
    bool chasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        // Red "anger" light that glows while chasing
        var lightObj = new GameObject("AngerLight");
        lightObj.transform.SetParent(transform, false);
        lightObj.transform.localPosition = Vector3.up;
        angerLight = lightObj.AddComponent<Light>();
        angerLight.type = LightType.Point;
        angerLight.color = new Color(1f, 0.1f, 0.05f);
        angerLight.range = 7f;
        angerLight.intensity = 3f;
        angerLight.enabled = false;

        NextPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (!chasing && distance < sightRange && CanSeePlayer())
        {
            chasing = true;
            agent.speed = chaseSpeed;
            angerLight.enabled = true;
        }
        else if (chasing && distance > giveUpRange)
        {
            chasing = false;
            agent.speed = patrolSpeed;
            angerLight.enabled = false;
            NextPoint();
        }

        if (chasing)
            agent.SetDestination(player.position);
        else if (!agent.pathPending && agent.remainingDistance < 0.7f)
            NextPoint();    // reached the patrol point, move to the next one
    }

    // Is there a wall between the enemy and the player?
    bool CanSeePlayer()
    {
        Vector3 from = transform.position + Vector3.up * 0.5f;
        Vector3 to = player.position + Vector3.up * 0.5f;

        if (Physics.Linecast(from, to, out RaycastHit hit,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            return hit.transform == player || hit.collider.CompareTag("Player");

        return true;
    }

    void NextPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        targetIndex = (targetIndex + 1) % patrolPoints.Length;
        agent.SetDestination(patrolPoints[targetIndex]);
    }
}
