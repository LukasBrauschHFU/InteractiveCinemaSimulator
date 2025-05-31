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
    public float collisionThreshold = 1.0f; // Set this to your desired collision distance

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
        //Call every frame to be independent of 2d sim
        CheckNavMeshAgentProximity();
        CheckCeilingCollision();
    }
    //Initialize the agents and goals
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

            // Create goal objects for each agent (start at the same spawn position as the agents initially)
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
                    if (match.Success && !agents[i].GetComponent<AnimationController>().agentCollision && !agents[i].GetComponent<AnimationController>().agentHitByCeiling)
                    {
                        float x = float.Parse(match.Groups[1].Value);
                        float z = float.Parse(match.Groups[2].Value);
                        Vector3 rawGoalPosition = new Vector3(x / 200, 0f, z / 200); //Math added because of scale

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
                        //Add the small posibility of an agent getting paniced, and preventing it from entering again if they are already paniced
                        if (Random.Range(0f, 1000f) > 999f && agents[i].GetComponent<AnimationController>().agentIsPaniced == false)
                        {
                            agents[i].GetComponent<AnimationController>().ChangeAnimation("Panic", i);
                            Debug.Log($"Agent {i+1} is paniced");
                            //Make agent move faster when paniced
                            agents[i].speed = agents[i].speed*1.5f;
                        }
                        //Send data to 2D simulator if the ID of the Animation Controller currentAgentID int is not -1
                    }
                    
                    //Stop the agent from moving if a collision of agents or the ceiling is detected
                    else if (match.Success && (agents[i].GetComponent<AnimationController>().agentCollision || agents[i].GetComponent<AnimationController>().agentHitByCeiling))
                    {
                        //agents[i].ResetPath();
                        agents[i].velocity = Vector3.zero;
                        // agents[i].enabled = false;
                        agents[i].isStopped = true;
                        //Only reset the goal since the agent is already stopped
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

        // Perform a downward raycast to find the closest NavMesh point within the range
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
                // Calculate distance between agents
                float distance = Vector3.Distance(positionA, positionB);
                if (distance <= collisionThreshold)
                {
                    Debug.Log($"Agents {agents[i].name} and {agents[j].name} are colliding. Distance: {distance}");
                    // Call animation change on both agents
                    var animA = agents[i].GetComponent<AnimationController>();
                    var animB = agents[j].GetComponent<AnimationController>();
                    if (animA != null) animA.ChangeAnimation("Collision", i);
                    if (animB != null) animB.ChangeAnimation("Collision", j);
                    // Store positions in the buffer
                    agentsPositionBuffer[i] = agents[i].transform.position;
                    agentsPositionBuffer[j] = agents[j].transform.position;
                    goalsPositionBuffer[i] = goals[i].transform.position;
                    goalsPositionBuffer[j] = goals[j].transform.position;
                    // TODO: Send signal to 2D simulator to stop sending/moving characte and send position of collision + send collision buffer
                }
            }
        }
    }

    //Check for ceiling collision only with the object tagged as "Debris" and only if the object moves downward while it hits the agent
    void CheckCeilingCollision()
    {
        GameObject ceiling = GameObject.FindWithTag("Debris");
        if (ceiling != null)
        {
            CapsuleCollider ceilingCollider = ceiling.GetComponent<CapsuleCollider>();
            if (ceiling.GetComponent<FallingRoof>().isFalling == false)
            {
                return;
            }
            if (ceilingCollider == null)
            {
                Debug.LogWarning("Ceiling object does not have a CapsuleCollider.");
                return;
            }
            for (int i = 0; i < agents.Length; i++)
            {
                if (agents[i] == null) continue;

                Vector3 agentPos = agents[i].transform.position;

                // Get the closest point on the collider to the agent's world position
                Vector3 closestPoint = ceilingCollider.ClosestPoint(agentPos);
                float distanceToCollider = Vector3.Distance(agentPos, closestPoint);

                if (distanceToCollider < 0.1f && agents[i].GetComponent<AnimationController>().agentHitByCeiling == false)
                {
                    agents[i].GetComponent<AnimationController>().ChangeAnimation("HitByCeiling", i);
                    agentsPositionBuffer[i] = agents[i].transform.position;
                    goalsPositionBuffer[i] = goals[i].transform.position;
                    Debug.Log($"Agent {i} is hit by ceiling");
                }
            }
        }
    }


/*       //Todo Check for jump over chair
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

