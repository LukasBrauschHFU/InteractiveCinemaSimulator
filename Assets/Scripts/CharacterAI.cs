using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterAI : MonoBehaviour
{
    private NavMeshAgent _agent;
    [SerializeField]
    private GameObject _goal;
   // NavMeshAgent myNavMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.Log("No NavMesh Component found");
                }
        _agent.SetDestination(_goal.transform.position);

    }



    // Update is called once per frame
    void Update()
    {
      /*  if (Input.GetMouseButtonDown(0))
        {
            SetDestinationToMousePosition();
        }*/
    }

  /*  void SetDestinationToMousePosition()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            myNavMeshAgent.SetDestination(hit.point);
        }
    }*/
}
