using MLTD.ML;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MLTD.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private readonly Vector2[] _directions = new[]
        {
            new Vector2(1f, .1f).normalized,
            new Vector2(1f, .2f).normalized,
            new Vector2(1f, 0f).normalized,
            new Vector2(1f, -.1f).normalized,
            new Vector2(1f, -.2f).normalized
        };

        private const float _distance = 20f;
        private const float _speed = 5f;

        private Rigidbody2D _rb;

        public NN Network { private set; get; }

        public Vector2 WorldMaxSize { set; private get; }

        private const int nbMessagesInput = 5;
        private const int messageSize = 10;

        public RaycastOutput MyType { private set; get; }

        private Transform _leaderPos;

        public Action<InputData, OutputData> DisplayDebugCallback { set; get; }

        private Spawner _spawner;

        private void OnMouseDown()
        {
            _spawner.SetDebug(this);
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Init(NN network, RaycastOutput type, Spawner spawner)
        {
            List<ADataType> outputTypes = new List<ADataType>();
            outputTypes.Add(new Range(-1f, 1f));
            outputTypes.Add(new Range(-1f, 1f));
            outputTypes.Add(new MLTD.ML.Boolean());
            for (int i = 0; i < messageSize; i++)
            {
                outputTypes.Add(new MLTD.ML.Boolean());
            }
            _spawner = spawner;
            Network = network ?? new NN(
                Decision.GetFloatArraySize(_directions.Length, nbMessagesInput, messageSize),
                outputTypes,
                new List<int> { 30 }
                );

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

        public void SetLeader(Transform leaderPos)
        {
            _leaderPos = leaderPos;
        }

        private void FixedUpdate()
        {
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
            if (_leaderPos != null)
            {
                Debug.DrawLine(transform.position, _leaderPos.position, Color.green);
            }

            InputData data = new InputData();
            data.WorldSize = WorldMaxSize;
            data.Position = transform.position;
            data.Direction = _rb.velocity.y / _speed;
            data.Speed = _rb.velocity.x / _speed;
            data.RaycastInfos = raycasts.ToArray();
            data.RaycastMaxSize = _directions.Length;
            data.Messages = new bool[nbMessagesInput][];
            for (int i = 0; i < nbMessagesInput; i++)
            {
                data.Messages[i] = new bool[messageSize];
            }
            data.CanUseSkill = false;
            data.SkillTimer = 0f;
            data.SkillTimerMaxDuration = 1f;
            data.LeaderPosition = _leaderPos?.position ?? Vector2.zero;

            var output = Decision.Decide(data, Network);

            DisplayDebugCallback?.Invoke(data, output);

            _rb.velocity = new Vector2(output.Speed, output.Direction) * _speed;
        }
    }
}
