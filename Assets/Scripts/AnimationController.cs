using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public int currentAgentID = -1;
    public bool agentHitByCeiling = false;
    public bool agentCollision = false;
    public bool agentIsPaniced = false;
    private bool isCollisionTimerRunning = false;
    public float waitTime = 0f; //Set waittime here so it can be transfered to the 2D simulator   
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(string triggeredAnimation, int agentID)
    {
        switch (triggeredAnimation)
        {
            case "Collision":
                animator.SetTrigger("Collision");
                agentCollision = true;
                currentAgentID = agentID;
                if (!isCollisionTimerRunning)
                {
                    StartCoroutine(HandleCollisionRecovery());
                }
                break;
            case "HitByCeiling":
                animator.SetTrigger("HitByCeiling");
                agentHitByCeiling = true;
                currentAgentID = agentID;
                break;
            case "Panic":
                animator.SetTrigger("IsPaniced");
                agentIsPaniced = true;
                currentAgentID = agentID;
                break;
        }
    }
    //Start a timer after which the character gets up again
    private IEnumerator HandleCollisionRecovery()
    {
        isCollisionTimerRunning = true;

        waitTime = Random.Range(5f, 10f);
        yield return new WaitForSeconds(waitTime);

        agentCollision = false;
        animator.SetTrigger("StandsUp");
        isCollisionTimerRunning = false;
    }
    void Update()
    {
    }
}