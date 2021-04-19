using MLTD.ML;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        // Directions of the raycast done by the AI to get information
        private readonly Vector2[] _directions = new[]
        {
            new Vector2(1f, .1f).normalized,
            new Vector2(1f, .2f).normalized,
            new Vector2(1f, 0f).normalized,
            new Vector2(1f, -.1f).normalized,
            new Vector2(1f, -.2f).normalized
        };

        // Constant variable for AI movements
        private const float _distance = 20f;
        private const float _speed = 5f;

        private Rigidbody2D _rb;

        public NN Network { private set; get; }

        // Size max of the world, used to put AI position between -1 and 1
        public Vector2 WorldMaxSize { set; private get; }

        // Constants for messages sent/received
        // Number max of message sent by an AI
        private const int nbMessagesInput = 5;
        // Size of a message sent
        private const int messageSize = 10;

        // Type of the AI, must be ENEMY_*
        public RaycastOutput MyType { private set; get; }

        // Leader of the current AI, null if none
        private EnemyController _leader;

        // Function we send debug info to
        public Action<EnemyController, InputData, OutputData> DisplayDebugCallback { set; get; }

        // Keep track of the spawner that instanciated us
        private Spawner _spawner;

        // Last message we generated from the neural network
        private bool[] _lastMessage = new bool[messageSize];

        /// <summary>
        /// When clicking on an AI, we begin to track its debug info
        /// </summary>
        private void OnMouseDown()
        {
            _spawner.SetDebug(this, true);
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Init(NN network, RaycastOutput type, Spawner spawner)
        {
            _spawner = spawner;

            // Setup the output types for the neural network
            List<ADataType> outputTypes = new List<ADataType>
            {
                new Range(-1f, 1f),
                new Range(-1f, 1f),
                new ML.Boolean()
            };
            for (int i = 0; i < messageSize; i++)
            {
                outputTypes.Add(new ML.Boolean());
            }
            // Setup the neural network, if none provided, we create a new one that will contains random values
            Network = network ?? new NN(
                Decision.GetFloatArraySize(_directions.Length, nbMessagesInput, messageSize),
                outputTypes,
                new List<int> { 30 }
                );

            // Set the color of an AI depending of its type
            MyType = type;
            if (MyType == RaycastOutput.ENEMY_LEADER)
            {
                GetComponent<SpriteRenderer>().color = Color.yellow;
            }
            else if (MyType == RaycastOutput.ENEMY_SHIELD)
            {
                GetComponent<SpriteRenderer>().color = Color.gray;
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        public void SetLeader(EnemyController leader)
        {
            _leader = leader;
        }

        private void FixedUpdate()
        {
            // Fire raycasts and get info from each of them
            List<Tuple<RaycastOutput, float>> raycasts = new List<Tuple<RaycastOutput, float>>();
            foreach (var dir in _directions)
            {
                var hit = Physics2D.Raycast(transform.position + transform.right, dir, _distance);
                RaycastOutput ro = RaycastOutput.NONE;
                float dist = _distance;
                if (hit.collider != null)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.blue);
                    dist = hit.distance;
                    ro = hit.collider.tag switch
                    {
                        "Player" => RaycastOutput.PLAYER,
                        "Enemy" => hit.collider.GetComponent<EnemyController>().MyType,
                        "Wall" => RaycastOutput.WALL,
                        "Turret" => RaycastOutput.TURRET_NORMAL,
                        _ => RaycastOutput.UNKNOWN,
                    };
                }
                raycasts.Add(new Tuple<RaycastOutput, float>(ro, dist));
            }

            // If we have a leader, display debug green line between AI and him
            if (_leader != null)
            {
                Debug.DrawLine(transform.position, _leader.transform.position, Color.green);
            }

            // Data we will send to the neural network
            var data = new InputData
            {
                WorldSize = WorldMaxSize,
                Position = transform.position,
                Direction = _rb.velocity.y / _speed,
                Speed = _rb.velocity.x / _speed,
                RaycastInfos = raycasts.ToArray(),
                RaycastMaxSize = _directions.Length
            };
            // If we have a leader, the message received is the last one he sent
            var msgs = new bool[nbMessagesInput][];
            if (_leader != null)
            {
                msgs[0] = _leader._lastMessage;
            }
            data.Messages = msgs;
            for (int i = 0; i < nbMessagesInput; i++)
            {
                data.Messages[i] = new bool[messageSize];
            }
            data.CanUseSkill = false;
            data.SkillTimer = 0f;
            data.SkillTimerMaxDuration = 1f;
            data.LeaderPosition = _leader == null ? Vector2.zero : (Vector2)_leader.transform.position;

            // Call to neural network
            var output = Decision.Decide(data, Network);

            // If a debug callback is set, we use it
            DisplayDebugCallback?.Invoke(this, data, output);

            // Use info returned by neural network
            _rb.AddForce(new Vector2(output.Speed, output.Direction) * _speed);
            _rb.velocity = Vector2.ClampMagnitude(_rb.velocity, _speed);
            _lastMessage = output.Message;
        }

        public Vector2 GetVelocity()
            => _rb.velocity;
    }
}
