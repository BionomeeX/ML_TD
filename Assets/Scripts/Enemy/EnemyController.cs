using MLTD.ML;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace MLTD.Enemy
{
    public class EnemyController : Agent
    {
        private AISettings _settings;

        private Rigidbody2D _rb;

        // Type of the AI, must be ENEMY_*
        public RaycastOutput MyType { private set; get; }

        // Leader of the current AI, null if none
        private EnemyController _leader;

        // Keep track of the spawner that instanciated us
        private Spawner _spawner;


        // Life management
        private const int _maxHealth = 10;
        private int _currentHealth = _maxHealth;

        private float _lifeTimer;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Victory"))
            {
                SetReward(1100f);
                TakeDamage(1000);
            }
        }

        public void TakeDamage(int value)
        {
            _currentHealth -= value;
            SetReward(-10f);
            if (_currentHealth <= 0) // Is dead
            {
                SetReward(-100f);
                GetComponent<SpriteRenderer>().color = Color.gray;
                MyType = RaycastOutput.ENEMY_DEAD;
                EndEpisode();
            }
        }
        public bool IsAlive()
            => _currentHealth > 0;

        /// <summary>
        /// When clicking on an AI, we begin to track its debug info
        /// </summary>
        private void OnMouseDown()
        {
            //_spawner.SetDebug(this, true);
        }

        private Vector3 _basePos;
        public void Init(Spawner spawner, AISettings settings)
        {
            _spawner = spawner;
            _settings = settings;

            _basePos = transform.position;
        }

        private void Update()
        {
            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                EndEpisode();
            }
        }

        public override void OnEpisodeBegin()
        {
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
                _raySensor = GetComponentInChildren<RayPerceptionSensorComponent2D>();
            }

            _lifeTimer = 20f;

            transform.position = _basePos;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = 0f;

            _currentHealth = _maxHealth;

            _spawner.RemoveAgentFromTurrets(this);

            // Set the color of an AI depending of its type
            var rand = UnityEngine.Random.Range(0, 100);
            if (_settings.EnableLeadership && rand < _settings.LeadershipChance) MyType = RaycastOutput.ENEMY_LEADER;
            else MyType = RaycastOutput.ENEMY_SCOUT;
            if (MyType == RaycastOutput.ENEMY_LEADER)
            {
                GetComponent<SpriteRenderer>().color = Color.yellow;
                tag = "EnemyLeader";
            }
            else
            {
                GetComponent<SpriteRenderer>().color = Color.red;
                tag = "Enemy";
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
        }


        // When using rotation, speed is velocity magnitude and direction our angle
        // Else speed is velocity on X axis and direction the one on Y

        private RayPerceptionSensorComponent2D _raySensor;

        public float Speed
        {
            get
            {
                return _settings.ReplaceStrafeByRotation ?
                    _rb.velocity.magnitude / _settings.AgentLinearSpeed :
                    _rb.velocity.x / _settings.AgentLinearSpeed;
            }
        }

        public float Direction
        {
            get
            {
                return _settings.ReplaceStrafeByRotation ?
                    transform.rotation.eulerAngles.z / 360f :
                    _rb.velocity.y / _settings.AgentLinearSpeed;
            }
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(Speed);
            sensor.AddObservation(Direction);
            sensor.AddObservation(_raySensor);
        }

        public override void OnActionReceived(ActionBuffers vectorAction)
        {
            var speed = vectorAction.ContinuousActions[0];
            var direction = vectorAction.ContinuousActions[1];

            var lastPos = transform.position;

            // Use info returned by neural network
            Vector2 forceUsed;
            // When rotating, we rotate the AI with angular speed then go forward by linear speed
            // Else we just move by speed and direction on the corresponding axises
            if (_settings.ReplaceStrafeByRotation)
            {
                forceUsed = transform.right * speed * _settings.AgentLinearSpeed * (speed > 0f ? 1f : _settings.BackwardSpeedMultiplicator);
                transform.Rotate(new Vector3(0f, 0f, direction * _settings.AgentLinearSpeed));
            }
            else
            {
                forceUsed = transform.right * speed * _settings.AgentLinearSpeed * (speed > 0f ? 1f : _settings.BackwardSpeedMultiplicator)
                        + transform.up * direction * _settings.AgentLinearSpeed;
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

            //SetReward(transform.position.x - lastPos.x);
            SetReward(transform.position.x - lastPos.x > 0f ? 3f : -2f);

        }

        public Vector2 GetVelocity()
            => _rb.velocity;
    }
}
