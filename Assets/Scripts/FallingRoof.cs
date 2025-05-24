using UnityEngine;
using UnityEngine.AI;

public class FallingRoof : MonoBehaviour
{
    public float chanceOfFalling = 1f / 1000f; // 1 in 1000 chance
    public float acceleration = 9.8f;          // Gravity
    public float maxFallSpeed = 50f;

    private float fallSpeed = 0f;
    public bool isFalling = false; //Prevents the roof from falling multiple times
    private Vector3 targetPosition;
    private float checkTimer = 0f;
    private float checkInterval = 1f; // in seconds

    void Update()
    {
        if (!isFalling)
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                if (Random.value <= chanceOfFalling)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(transform.position, out hit, 100.0f, NavMesh.AllAreas))
                    {
                        targetPosition = new Vector3(transform.position.x, hit.position.y, transform.position.z);
                        isFalling = true;
                    }
                    else
                    {
                        Debug.LogWarning("No NavMesh found under object.");
                    }
                }
            }
        }

        if (!isFalling) return;

        fallSpeed += acceleration * Time.deltaTime;
        fallSpeed = Mathf.Min(fallSpeed, maxFallSpeed);
        transform.position -= new Vector3(0, fallSpeed * Time.deltaTime, 0);
        if (transform.position.y <= targetPosition.y)
        {
            transform.position = targetPosition;
            isFalling = false;
            fallSpeed = 0f;
            Debug.Log($"{gameObject.name} landed on NavMesh.");
        }
    }
}
