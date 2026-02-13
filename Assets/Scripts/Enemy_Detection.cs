using UnityEngine;

public class Enemy_Detection : MonoBehaviour
{
    public int health = 3;

    public Transform player;
    public float detectionRadius = 5f;
    public float moveSpeed = 5f;

    private Vector3 originalPosition;
    private bool isPlayerDetected;
    public float stop_distance = 1.5f;
    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        DetectPlayer();

        Vector3 targetPosition = isPlayerDetected ? player.position : originalPosition;
        MoveToPosition(targetPosition);
    }


    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            detectionRadius,
            LayerMask.GetMask("Player") 
        );

        isPlayerDetected = hit != null;
    }

   void MoveToPosition(Vector3 target)
{
    float dist = Vector3.Distance(transform.position, target);

    if (dist > stop_distance)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );
    }
}

    // BUAT DEBUG
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stop_distance);

    }
}
