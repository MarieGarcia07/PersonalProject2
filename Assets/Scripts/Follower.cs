using UnityEngine;
using UnityEngine.AI;

public class Follower : MonoBehaviour
{
    public Transform player;
    public float walkDistance = 3f;   // walk if within this range
    public float runDistance = 6f;    // run if farther
    public float stopDistance = 1.5f; // stop when this close

    private bool isFollowing = false;
    private Animator animator;
    private NavMeshAgent agent;

    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (isFollowing && player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer > stopDistance)
            {
                // Set NavMesh destination
                agent.SetDestination(player.position);

                // changing speed based on distance
                if (distanceToPlayer > runDistance)
                    agent.speed = 5f; // run
                else
                    agent.speed = 2f; // walk
            }
            else
            {
                agent.ResetPath(); // stop moving
                isFollowing = false;
            }

            // updating animations
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        else
        {
            agent.ResetPath();
            animator.SetFloat("Speed", 0f);
        }
    }

    public void ToggleFollow()
    {
        isFollowing = !isFollowing;
        Debug.Log("Following set to: " + isFollowing);
    }
}

