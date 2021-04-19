using MLTD.ML;
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

        private const float _waveLength = 15f;

        // List of all instanciated ennemies
        private List<EnemyController> _instancied = new List<EnemyController>();

        // Spawn zone
        private const int _x = 3;
        private const int _y = 5;

        // Current wave
        private int _waveCount = 1;

        // Current enemy we display debug information about
        private EnemyController _currentDebugFollowed;

        private void Start()
        {
            if (!_settings.EnableDebug)
            {
                _debugDisplay = null;
            }
            int enemyLayerCollision = 6;
            Physics2D.IgnoreLayerCollision(6, 6, !_settings.EnableAICollision);
            StartCoroutine(SpawnAll());
            if (_debugDisplay != null)
            {
                StartCoroutine(KeepDebugUpdated());
            }
        }

        /// <summary>
        /// Coroutine that manage the spawing and training of AI
        /// </summary>
        private IEnumerator SpawnAll()
        {
            int bestOfMaxCount = 20;
            List<NN> networks = new List<NN>();
            List<(NN network, float score)> networks_BestOf = new List<(NN network, float score)>(bestOfMaxCount);

            while (true)
            {
                // Reset debug pannel
                if (_debugDisplay.activeInHierarchy)
                {
                    _currentDebugFollowed = null;
                    _isDebugSetManually = false;
                }

                var maxSize = new Vector2(-transform.position.x + _x, transform.position.y + _y);
                int count = 0;

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
                        var rand = Random.Range(0, 100);
                        RaycastOutput type;
                        if (_settings.EnableLeadership && rand < _settings.LeadershipChance) type = RaycastOutput.ENEMY_LEADER;
                        else type = RaycastOutput.ENEMY_SCOUT;
                        ec.Init(networks.Count == 0 ? null : new NN(networks[count]), type, this, _settings);
                        ec.WorldMaxSize = maxSize;
                        ec.name = "AI " + count;

                        // We keep track of leaders
                        if (type == RaycastOutput.ENEMY_LEADER)
                        {
                            leaders.Add(ec);
                        }
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

                // Display the timer on the game
                var timer = _waveLength;
                while (timer > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _timeRemainding.text = $"Wave {_waveCount} end in {timer} seconds";
                    timer--;
                }

                // Keep best AI and setup new neural networks for next generation
                List<(NN network, float score)> oldgen = _instancied.Select(ec => (ec.Network, ec.gameObject.transform.position.x - ec.MalusScore)).ToList();
                networks_BestOf.AddRange(oldgen);
                networks_BestOf.Sort(delegate
                ((NN network, float score) a, (NN network, float score) b)
                {
                    return b.score.CompareTo(a.score);
                });
                networks_BestOf = networks_BestOf.Take(bestOfMaxCount).ToList();
                networks = GeneticAlgorithm.GeneratePool(oldgen, networks_BestOf, _instancied.Count);

                // All AIs are killed
                foreach (var p in _instancied)
                {
                    Destroy(p.gameObject);
                }
                _instancied.RemoveAll(x => true);
                _waveCount++;
            }
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
                            SetDebug(_instancied.OrderByDescending(x => x.transform.position.x).First(), false);
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
        private void DisplayDebug(EnemyController ec, InputData input, OutputData output)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("<b>GENERAL</b>");
            str.AppendLine(ec.name + " - " + ec.MyType.ToString());
            var v = ec.GetVelocity();
            str.AppendLine($"Velocity: ({v.x:0.00};{v.y:0.00})");

            str.AppendLine("\n<b>INPUT</b>");
            str.AppendLine($"Position: ({input.Position.x:0.00};{input.Position.y:0.00})");
            if (_settings.EnableLeadership)
            {
                str.AppendLine($"Leader Position: ({input.LeaderPosition.x:0.00};{input.LeaderPosition.y:0.00})");
            }
            str.AppendLine($"Direction: {input.Direction:0.00}");
            str.AppendLine($"Speed: {input.Speed:0.00}");
            int i = 1;
            foreach (var ray in input.RaycastInfos)
            {
                str.AppendLine($"Raycast {i}: {ray.Item1} (Distance {ray.Item2:0.00})");
                i++;
            }

            if (_settings.EnableMemory)
            {
                i = 1;
                foreach (var ray in input.Memory)
                {
                    str.AppendLine($"Memory raycast {i}: {ray.Item1} at position ({ray.Item2.x:0.00};{ray.Item2.y:0.00})");
                    i++;
                }
            }

            str.AppendLine("\n<b>RAW INPUT</b>");
            str.AppendLine(string.Join(", ", Decision.InputToFloatArray(_settings, input).Select(x => x.ToString("0.00"))));

            str.AppendLine("\n<b>OUTPUT</b>");
            str.AppendLine($"Direction: {output.Direction:0.00}");
            str.AppendLine($"Speed: {output.Speed:0.00}");
            str.AppendLine("Skill state: " + output.SkillState);
            str.AppendLine("Message: " + string.Join("", output.Message.Select(x => x ? "1" : "0")));

            _debugText.text = str.ToString();
        }

        /// <summary>
        /// Set callback method so an AI can send debug info
        /// </summary>
        /// <param name="ec">AI we will track</param>
        public void SetDebug(EnemyController ec, bool isDebugSetManually)
        {
            if (_currentDebugFollowed != null)
            {
                _currentDebugFollowed.DisplayDebugCallback = null;
            }
            _isDebugSetManually = isDebugSetManually;
            ec.DisplayDebugCallback = DisplayDebug;
            _currentDebugFollowed = ec;
            _debugDisplay.SetActive(true);
            _debugText = _debugDisplay.GetComponentInChildren<Text>();
        }
    }
}
