using UnityEngine;

namespace MLTD.ML
{
    [CreateAssetMenu(menuName = "ScriptableObject/AISettings", fileName = "AISettings")]
    public class AISettings : ScriptableObject
    {
        [Header("Movement settings")]

        [Tooltip("Linear speed of the agent")]
        [Range(1, 50)]
        public float AgentLinearSpeed = 5f;

        [Tooltip("Angular speed of the agent")]
        [Range(1, 50)]
        public float AgentAngularSpeed = 10f;

        [Tooltip("If enabled, movements will be smoothen by adding force to the AI instead of setting its velocity")]
        public bool SmoothenMovements = true;

        [Tooltip("If enabled, the AI will rotate instead of strafing")]
        public bool ReplaceStrafeByRotation = true;

        [Header("Vision settings")]

        [Tooltip("Maximum size of the raycast done by the agent to see in front of it")]
        [Range(1, 50)]
        public float VisionSize = 20f;

        [Tooltip("Angles used by the vision raycast")]
        public float[] VisionAngles = new[]
        {
            .1f,
            .2f,
            0f,
            -.1f,
            -.2f
        };


        [Header("Physics settings")]

        [Tooltip("Can the AIs collide between each others")]
        public bool EnableAICollision = false;


        [Header("Leadership settings")]

        [Tooltip("Enable leadership, a random amount of leader will spawn, each soldiers are assigned to a leader")]
        public bool EnableLeadership = true;

        [Tooltip("Change that an AI is a leader")]
        [Range(1, 100)]
        public int LeadershipChance = 5;

        [Tooltip("Maximum distance allowed before the AI will loose its leader information, -1 to disable")]
        [Range(-1, 30)]
        public float LeadershipMaxDistance = 15f;


        [Header("Memory settings")]

        [Tooltip("Enable memory: AI will remember what it saw recently")]
        public bool EnableMemory = false;

        [Tooltip("How many data the AI will remember, really costly performance wise")]
        [Range(1, 100)]
        public int MemorySize = 5;

        [Tooltip("How much time will past between 2 data acquisition")]
        [Range(0.02f, 5f)]
        public float TimeBetweenMemoryAcquisition = 1f;


        [Header("Debug settings")]

        [Tooltip("Enable display of debug information with gizmos")]
        public bool EnableDebug = true;

        [Tooltip("Display vision raycasts")]
        public Color VisionDebug = Color.clear;

        [Tooltip("Display vision raycasts when colliding with something")]
        public Color VisionCollidingDebug = Color.blue;

        [Tooltip("Display connections between leader and subalternes")]
        public Color LeadershipLinkDebug = Color.green;

        [Tooltip("Display leadership influence zones")]
        public Color LeadershipInfluenceDebug = Color.yellow;
    }
}
