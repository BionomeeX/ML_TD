using MLTD.ML;
using MLTD.Turret;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MLTD.Enemy
{
    public class Spawner : MonoBehaviour
    {
        [Tooltip("Prefab for new ennemies")]
        [SerializeField]
        private GameObject _enemyPrefab;

        [Tooltip("Text UI that display time remainding")]
        [SerializeField]
        private Text _timeRemainding;

        [Tooltip("Pannel that will contains debug info")]
        [SerializeField]
        private GameObject _debugDisplay;
        private Text _debugText;
        private bool _isDebugSetManually = false;

        [SerializeField]
        private AISettings _settings;

        // List of all instanciated ennemies
        private readonly List<EnemyController> _instancied = new List<EnemyController>();

        // Spawn zone
        [SerializeField]
        private int _x = 3;
        [SerializeField]
        private int _y = 5;

        public void RemoveAgentFromTurrets(EnemyController ec)
        {
            TurretZones.ForEach(x => x.RemoveAgent(ec));
        }

        // Current wave
        private int _waveCount = 1;

        // Current enemy we display debug information about
        private EnemyController _currentDebugFollowed;

        public List<TurretZone> TurretZones { private set; get; } = new List<TurretZone>();

        public static Spawner S;

        private void Awake()
        {
            S = this;
        }

        private void Start()
        {
            if (!_settings.EnableDebug)
            {
                _debugDisplay = null;
            }
            int enemyLayerCollision = 6;
            Physics2D.IgnoreLayerCollision(enemyLayerCollision, enemyLayerCollision, !_settings.EnableAICollision);
            SpawnAll();
            if (_debugDisplay != null)
            {
                StartCoroutine(KeepDebugUpdated());
            }
        }

        public bool EndGame()
        {
            return _instancied.All(x => !x.IsAlive());
        }

        /// <summary>
        /// Coroutine that manage the spawing and training of AI
        /// </summary>
        private void SpawnAll()
        {
            // Reset debug pannel
            if (_debugDisplay.activeInHierarchy)
            {
                _currentDebugFollowed = null;
                _isDebugSetManually = false;
            }

            int count = 0;

            foreach (var zone in TurretZones)
            {
                zone.Regenerate();
            }

            // Keep list of all leaders currently spawned
            List<EnemyController> leaders = new List<EnemyController>();

            // Spawn AI on each tile of spawn zone
            for (int x = -_x; x <= _x; x++)
            {
                for (int y = -_y; y <= _y; y++)
                {
                    // Spawn AI
                    var go = Instantiate(_enemyPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
                    go.transform.parent = transform;

                    // Set AI type
                    var ec = go.GetComponent<EnemyController>();
                    ec.Init(this, _settings);
                    ec.name = "AI " + count;

                    // We keep track of leaders
                    /*if (type == RaycastOutput.ENEMY_LEADER)
                    {
                        leaders.Add(ec);
                    }*/
                    _instancied.Add(ec);
                    count++;
                }
            }

            // Set the leader of each AI (leader doesn't have another leader on top of them)
            if (_settings.EnableLeadership && leaders.Count > 0)
            {
                foreach (var e in _instancied)
                {
                    e.SetLeader(e.MyType == RaycastOutput.ENEMY_LEADER ? null : leaders[Random.Range(0, leaders.Count)]);
                }
            }

            _timeRemainding.text = $"Current wave: {_waveCount}";
        }

        // Update the debug with the element the further on the X axis, is disabled if the user manually select an element
        private IEnumerator KeepDebugUpdated()
        {
            while (true)
            {
                if (!_isDebugSetManually)
                {
                    lock(_instancied)
                    {
                        if (_instancied.Count > 0)
                        {
                            //SetDebug(_instancied.OrderByDescending(x => x.transform.position.x).First(), false);
                        }
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Draw line to show spawn bounds
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var mid = transform.position;
            var up = transform.up * _y;
            var right = transform.right * _x;
            Gizmos.DrawLine(mid + up + right, mid - up + right);
            Gizmos.DrawLine(mid + up + right, mid + up - right);
            Gizmos.DrawLine(mid - up - right, mid + up - right);
            Gizmos.DrawLine(mid - up + right, mid - up - right);
        }

        /// <summary>
        /// Display all debug information for one AI
        /// </summary>
        /// <param name="input">Last infos the AI sent to the neural network</param>
        /// <param name="output">Last infos the AI received from the neural network</param>
        private void DisplayDebug(EnemyController ec)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("<b>GENERAL</b>");
            str.AppendLine(ec.name + " - " + ec.MyType.ToString());
            var v = ec.GetVelocity();
            str.AppendLine($"Velocity: ({v.x:0.00};{v.y:0.00})");

            _debugText.text = str.ToString();
        }
    }
}
