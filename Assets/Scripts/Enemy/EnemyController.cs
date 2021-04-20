using MLTD.ML;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private AISettings _settings;

        private Rigidbody2D _rb;

        public NN Network { private set; get; }

        // Size max of the world, used to put AI position between -1 and 1
        public Vector2 WorldMaxSize { set; private get; }

        // Constants for messages sent/received
        // Number max of message sent by an AI
        private const int nbMessagesInput = 5; // TODO: move into config file
        // Size of a message sent
        private const int messageSize = 10;

        private float[] _memory;

        // Type of the AI, must be ENEMY_*
        public RaycastOutput MyType { private set; get; }

        // Leader of the current AI, null if none
        private EnemyController _leader;

        // Function we send debug info to
        public Action<EnemyController, InputData, float[], OutputData> DisplayDebugCallback { set; get; }

        // Keep track of the spawner that instanciated us
        private Spawner _spawner;

        // Last message we generated from the neural network
        private bool[] _lastMessage = new bool[messageSize];

        // Value to remove to the score calculated for the neural network performance
        private float _malusScore;

        public float GetScore()
        {
            return transform.position.x - _malusScore;
        }

        // Life management
        private const int _maxHealth = 10;
        private int _currentHealth = _maxHealth;

        public void TakeDamage(int value)
        {
            _currentHealth -= value;
            if (_currentHealth <= 0) // Is dead
            {
                GetComponent<SpriteRenderer>().color = Color.gray;
                MyType = RaycastOutput.ENEMY_DEAD;
            }
        }
        public bool IsAlive()
            => _currentHealth > 0;

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
            if (_settings.EnableMemory)
            {
                _memory = new float[_settings.MemorySize];
            }
        }

        public void Init(NN network, RaycastOutput type, Spawner spawner, AISettings settings)
        {
            _spawner = spawner;
            _settings = settings;

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

            if (_settings.EnableMemory)
            {
                for (int i = 0; i < _settings.MemorySize; ++i)
                {
                    outputTypes.Add(new ML.Range(0f, 1f));
                }
            }

            // Setup the neural network, if none provided, we create a new one that will contains random values
            Network = network ?? new NN(
                Decision.GetFloatArraySize(_settings, nbMessagesInput, messageSize),
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
            if (_currentHealth <= 0)
            {
                _rb.velocity = Vector2.zero;
                return;
            }

            // Fire raycasts and get info from each of them
            List<Tuple<RaycastOutput, float>> raycasts = new List<Tuple<RaycastOutput, float>>();
            foreach (var dir in _settings.VisionAngles)
            {
                var ray = new Ray(transform.position + transform.right / 2f, transform.right + transform.up * dir);
                var hit = Physics2D.Raycast(ray.origin, ray.direction, _settings.VisionSize);
                if (_settings.EnableDebug && _settings.VisionDebug.a != 0f)
                {
                    Debug.DrawRay(ray.origin, ray.direction.normalized * _settings.VisionSize, _settings.VisionDebug);
                }
                RaycastOutput ro = RaycastOutput.NONE;
                float dist = _settings.VisionSize;
                if (hit.collider != null)
                {
                    if (_settings.EnableDebug && _settings.VisionCollidingDebug.a != 0)
                    {
                        Debug.DrawLine(transform.position, hit.point, _settings.VisionCollidingDebug);
                    }
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
            if (_settings.EnableLeadership && _settings.EnableDebug && _leader != null && _settings.LeadershipLinkDebug.a != 0
                && (_settings.LeadershipMaxDistance < 0f || Vector2.Distance(transform.position, _leader.transform.position) < _settings.LeadershipMaxDistance))
            {
                Debug.DrawLine(transform.position, _leader.transform.position, _settings.LeadershipLinkDebug);
            }

            if (_settings.EnableDebug && MyType == RaycastOutput.ENEMY_LEADER && _settings.LeadershipInfluenceDebug.a != 0f)
            {
                var step = 20f;
                for (float i = 0; i < 2 * Mathf.PI; i += Mathf.PI / step) // TODO: doesn't work
                {
                    var n = i + (Mathf.PI / step);
                    Debug.DrawLine((Vector2)transform.position + new Vector2(Mathf.Cos(i), Mathf.Sin(i)) * _settings.LeadershipMaxDistance,
                        (Vector2)transform.position + new Vector2(Mathf.Cos(n), Mathf.Sin(n)) * _settings.LeadershipMaxDistance,
                        _settings.LeadershipInfluenceDebug);
                }
                Debug.DrawLine(transform.position - Vector3.right * _settings.LeadershipMaxDistance,
                    transform.position + Vector3.right * _settings.LeadershipMaxDistance, _settings.LeadershipInfluenceDebug);
                Debug.DrawLine(transform.position - Vector3.up * _settings.LeadershipMaxDistance,
                    transform.position + Vector3.up * _settings.LeadershipMaxDistance, _settings.LeadershipInfluenceDebug);
            }

            // Data we will send to the neural network
            var data = new InputData
            {
                WorldSize = WorldMaxSize,
                Position = transform.position,
                RaycastInfos = raycasts.ToArray(),
                RaycastMaxSize = _settings.VisionAngles.Length,
                Health = _currentHealth,
                MaxHealth = _maxHealth
            };
            // When using rotation, speed is velocity magnitude and direction our angle
            // Else speed is velocity on X axis and direction the one on Y
            if (_settings.ReplaceStrafeByRotation)
            {
                data.Speed = _rb.velocity.magnitude / _settings.AgentLinearSpeed;
                data.Direction = transform.rotation.eulerAngles.z / 360f;
            }
            else
            {
                data.Speed = _rb.velocity.x / _settings.AgentLinearSpeed;
                data.Direction = _rb.velocity.y / _settings.AgentLinearSpeed;
            }
            data.CanUseSkill = false;
            data.SkillTimer = 0f;
            data.SkillTimerMaxDuration = 1f;

            if (_settings.EnableLeadership)
            {
                var canCommunicateWithLeader = _leader != null &&
                    (_settings.LeadershipMaxDistance < 0f || Vector2.Distance(transform.position, _leader.transform.position) < _settings.LeadershipMaxDistance);

                // If we have a leader, the message received is the last one he sent
                var msgs = new bool[nbMessagesInput][];
                if (_leader != null && canCommunicateWithLeader)
                {
                    msgs[0] = _leader._lastMessage;
                }
                data.Messages = msgs;
                for (int i = 0; i < nbMessagesInput; i++)
                {
                    data.Messages[i] = new bool[messageSize];
                }
                data.LeaderPosition = _leader == null || !canCommunicateWithLeader ? Vector2.zero : (Vector2)_leader.transform.position;
            }

            if (_settings.EnableMemory)
            {
                data.Memory = _memory;
            }

            // Call to neural network
            var fOutput = Decision.Decide(_settings, data, Network);
            var output = Decision.FloatArrayToOutput(_settings, fOutput);

            // If a debug callback is set, we use it
            if (_settings.EnableDebug)
            {
                DisplayDebugCallback?.Invoke(this, data, fOutput, output);
            }

            // Use info returned by neural network
            Vector2 forceUsed;
            // When rotating, we rotate the AI with angular speed then go forward by linear speed
            // Else we just move by speed and direction on the corresponding axises
            if (_settings.ReplaceStrafeByRotation)
            {
                forceUsed = transform.right * output.Speed * _settings.AgentLinearSpeed * (output.Speed > 0f ? 1f : _settings.BackwardSpeedMultiplicator);
                transform.Rotate(new Vector3(0f, 0f, output.Direction * _settings.AgentLinearSpeed));
            }
            else
            {
                forceUsed = transform.right * output.Speed * _settings.AgentLinearSpeed * (output.Speed > 0f ? 1f : _settings.BackwardSpeedMultiplicator)
                        + transform.up * output.Direction * _settings.AgentLinearSpeed;
            }
            // If we smoothen the movements, we add it add force, else we just set the velocity
            if (_settings.SmoothenMovements)
            {
                _rb.AddForce(forceUsed);
            }
            else
            {
                _rb.velocity = forceUsed;
            }
            _rb.velocity = Vector2.ClampMagnitude(_rb.velocity, _settings.AgentLinearSpeed);
            _lastMessage = output.Message;
            _memory = output.Memory;
        }

        public Vector2 GetVelocity()
            => _rb.velocity;
    }
}
