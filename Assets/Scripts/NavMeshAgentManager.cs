using UnityEngine;
using UnityEngine.AI;
using System.IO;
using System.Text.RegularExpressions;

public class NavMeshAgentsManager : MonoBehaviour
{
    public GameObject agentPrefab;  //How the agents are appearing
    public GameObject goalPrefab;  // Assigning the look of the object the character will move to
    public string coordinatesFilePath = "Assets/coordinates.txt"; // Path to the coordinates file
    public float updateInterval = 0.2f;  // Update every 0.2s

    private GameObject[] goals;  // Goal objects for each agent
    private NavMeshAgent[] agents;   // The NavMeshAgents
    private float timeSinceLastUpdate = 0f;
    private int frameIndex = 1; // Start from frame 1
    public int agentCount = 60; //Agent amount to be spawned
    private Vector3[] goalsPositionBuffer; //Creates a buffer array to store all possible positions of the goals
    private Vector3[] agentsPositionBuffer; //Creates a buffer array to store all possible positions of the agents
    //Debug


    void Start()
    {
        if (agentPrefab == null || goalPrefab == null)
        {
            Debug.LogError("Please assign the Agent Prefab and Goal Prefab in the editor.");
            return;
        }
        goalsPositionBuffer = new Vector3[agentCount];
        agentsPositionBuffer = new Vector3[agentCount];
        agents = new NavMeshAgent[agentCount];
        goals = new GameObject[agentCount];
        InitiateGoalsAndAgents();
        LoadCoordinatesAndAssignGoals(frameIndex);
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        if (timeSinceLastUpdate >= updateInterval)
        {
            frameIndex++; // Increase to the next frame
            LoadCoordinatesAndAssignGoals(frameIndex);
            timeSinceLastUpdate = 0f;
        }
        //Every frame to be independent of 2D Sim
        CheckNavMeshAgentProximity();
    }

  

    void InitiateGoalsAndAgents()
    {
        for (int i = 0; i < agentCount; i++)
        {
            // Find a valid point on the NavMesh to spawn the agent
            Vector3 spawnPosition = FindClosestNavMeshPoint(Vector3.zero, 10.0f); // Search around Vector3.zero
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogError("Failed to find a valid NavMesh position to spawn agents.");
                return;
            }

            // Create the agent at the valid NavMesh position
            agents[i] = Instantiate(agentPrefab, spawnPosition, Quaternion.identity).GetComponent<NavMeshAgent>();
            agents[i].name = "Agent-" + (i + 1);

            // Create goal objects for each agent (start at the same spawn position)
            goals[i] = Instantiate(goalPrefab, spawnPosition, Quaternion.identity);
            goals[i].name = "Goal-" + (i + 1);
        }
    }

    Vector3 FindClosestNavMeshPoint(Vector3 origin, float searchRadius)
    {
        NavMeshHit hit;

        // Find a spawnpoint within the specified radius
        if (NavMesh.SamplePosition(origin, out hit, searchRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero; // Return zero if no valid point is found
    }


    void LoadCoordinatesAndAssignGoals(int frameIndex)
    {
        if (!File.Exists(coordinatesFilePath))
        {
            Debug.LogError("Coordinates file not found at: " + coordinatesFilePath);
            return;
        }
        //Start of Part written by ChatGPT
        string[] lines = File.ReadAllLines(coordinatesFilePath);
        string frameHeader = $"*** {frameIndex} ***";
        bool isFrameFound = false;
        //End of Part written by ChatGPT

        foreach (string line in lines)
        {
            if (line.Trim() == frameHeader)
            {
                isFrameFound = true;
                continue;
            }

            if (isFrameFound)
            {
                for (int i = 0; i < agents.Length; i++)
                {//Start of Part written by ChatGPT
                    string agentId = $"agent-{i + 1}";
                    Match match = Regex.Match(line, $@"{agentId}\(([-+]?[0-9]*\.?[0-9]+),([-+]?[0-9]*\.?[0-9]+)\)");
                    //End of Part written by ChatGPT
                    if (match.Success && agents[i].GetComponent<Animator>().GetBool("Collision") == false)
                    {
                        float x = float.Parse(match.Groups[1].Value);
                        float z = float.Parse(match.Groups[2].Value);
                        Vector3 rawGoalPosition = new Vector3(x / 200, 0f, z / 200);

                        // Use NavMesh.SamplePosition to find the nearest point on the NavMesh
                        Vector3 goalPosition = AdjustPositionToNavMesh(rawGoalPosition);
                        // Set the agents' initial position if at frame 1
                        if (frameIndex == 1)
                        {
                            agents[i].transform.position = goalPosition;
                        }
                        goals[i].transform.position = goalPosition;

                        if (agents[i] != null)
                        {
                            agents[i].SetDestination(goalPosition);
                        }
                    }
                    else if (match.Success && agents[i].GetComponent<Animator>().GetBool("Collision") == true)
                    {
                        // Debug.Log("Za Warudo");
                        //agents[i].ResetPath();
                        agents[i].velocity = Vector3.zero;
                        // agents[i].enabled = false;
                        agents[i].isStopped = true;
                       // agents[i].transform.position = agentsPositionBuffer[i];
                        goals[i].transform.position = goalsPositionBuffer[i];
                        
                    }
                }
                return;
            }
        }

        Debug.LogWarning($"Frame {frameIndex} not found in coordinates file.");
    }

    Vector3 AdjustPositionToNavMesh(Vector3 position)
    {
        NavMeshHit hit;

        // Perform a downward raycast to find the closest NavMesh point within a range
        if (NavMesh.SamplePosition(position, out hit, 10.0f, NavMesh.AllAreas))
        {
            return hit.position; // Return the closest point on the NavMesh
        }
        else
        {
            Debug.LogWarning($"Could not find NavMesh position near {position}");
            return position; // Return the original position as a fallback
        }
    }

    //Check for collision between NavMeshAgents
    //Maybe move this to the Agent itself ?
    void CheckNavMeshAgentProximity()
    {
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] == null) continue;

            Vector3 positionA = agents[i].transform.position;

            for (int j = i + 1; j < agents.Length; j++)
            {
                if (agents[j] == null) continue;

                Vector3 positionB = agents[j].transform.position;
                //AnimationController animationController = agents[i].GetComponent<AnimationController>();

                // Calculate distance between agents
                float distance = Vector3.Distance(positionA, positionB);

            //    if (distance < 0.5f && agents[i].GetComponent<Animator>().GetBool("Collision") == false)
                    if (Input.GetKeyDown("space"))
                    {
                    //  Debug.Log($"Agents {agents[i].name} and {agents[j].name} are colliding. Distance: {distance}");
                    //Call animation change function on the specified object
                    agents[i].GetComponent<AnimationController>().ChangeAnimation("Collision");
                    //Log collision position
                    //Change to see if both agents fall
                    agentsPositionBuffer[i] = agents[i].transform.position;
                    goalsPositionBuffer[i] = goals[i].transform.position; 
                    
                    //Send signal to 2D simulator to stop sending and moving the character and send position of collision

                }
            }
        }
    }
    //Todo check for falling ceiling
    /*   void CheckCeilingCollision()
       {
           for (int i = 0; i < agents.Length; i++)
           {
               if (agents[i] == null) continue;

               Vector3 positionA = agents[i].transform.position;

               for (int j = i + 1; j < agents.Length; j++)
               {
                   if (agents[j] == null) continue;

                   Vector3 positionB = agents[j].transform.position;

                   // Calculate distance between agents
                   float distance = Vector3.Distance(positionA, positionB);

                   if (distance < 0.1f)
                   {
                       Debug.Log($"Agents {agents[i].name} and {agents[j].name} are colliding. Distance: {distance}");
                       //Call animation change function on the specified object
                       AnimationController animationController = agents[i].GetComponent<AnimationController>();
                       animationController.ChangeAnimation("hitByCeiling");

                   }
               }
           }
       }
       //Todo Check for jump over chair
       void CheckJumpCondition()
       {
           for (int i = 0; i < agents.Length; i++)
           {
               if (agents[i] == null) continue;

               Vector3 positionA = agents[i].transform.position;

               for (int j = i + 1; j < agents.Length; j++)
               {
                   if (agents[j] == null) continue;

                   Vector3 positionB = agents[j].transform.position;

                   // Calculate distance between agents
                   float distance = Vector3.Distance(positionA, positionB);

                   if (distance < 1.0f)
                   {
                       Debug.Log($"Agents {agents[i].name} and {agents[j].name} are colliding. Distance: {distance}");
                       //Call animation change function on the specified object
                       AnimationController animationController = agents[i].GetComponent<AnimationController>();
                       animationController.ChangeAnimation("Collision");

                   }
               }
           }
      */
}

