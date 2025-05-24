using UnityEngine;

public class AnimationRandomizer : MonoBehaviour
{
    private Animator animator;
    public int walkingAnimationsAmount;
    public int runningAnimationsAmount;
    public int fallingAnimationsAmount;
    public int IDLEAnimationsAmount;
    public bool randomizeAnimationsOnStatechange = false;
    private bool isTransitioning;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetRandomWalkingAnimation();
        SetRandomRunningAnimation();
        SetRandomFallingAnimation();
        SetRandomIDLEAnimation();
        //Debug until the 2D simulator is implemented to start the animations of
        animator.SetTrigger("StandsUp");

    }

    public void SetRandomWalkingAnimation()
    {
        int randomIndex = Random.Range(0, walkingAnimationsAmount);
        animator.SetInteger("SelectedWalkingAnimation", randomIndex);
    }

    public void SetRandomRunningAnimation()
    {
        int randomIndex = Random.Range(0, runningAnimationsAmount);
        animator.SetInteger("SelectedRunningAnimation", randomIndex);
    }

    public void SetRandomFallingAnimation()
    {
        int randomIndex = Random.Range(0, fallingAnimationsAmount);
        animator.SetInteger("SelectedFallingAnimation", randomIndex);
    }

    public void SetRandomIDLEAnimation()
    {
        int randomIndex = Random.Range(0, IDLEAnimationsAmount);
        animator.SetInteger("SelectedIDLEAnimation", randomIndex);
    }


    void Update()
    {   //Only call when a transition is happening and randomizeAnimationsOnStatechange == true
        if (randomizeAnimationsOnStatechange) 
        {
            if (animator.IsInTransition(0) && !isTransitioning)
            {
                isTransitioning = true; // Set flag to prevent multiple triggers in the same transition
                SetRandomWalkingAnimation();
                SetRandomRunningAnimation();
                SetRandomFallingAnimation();
                SetRandomIDLEAnimation();
            }
            else if (!animator.IsInTransition(0) && isTransitioning)
            {
                isTransitioning = false; // Reset the flag once the transition is done
            }
            
        }
    }
}
