using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ChangeAnimation(string triggeredAnimation)
    {
        switch (triggeredAnimation)
        {
            case "Collision":
            animator.SetTrigger("Collision");
               // animator.ResetTrigger("Collision");
                break;
        }
    }

/*
    public void TriggerCollisionAnimation()
    {
        animator.SetTrigger("Collision");
        Debug.Log("Nya");
    }*/

    void Update()
    {
    }
}