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
        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private Text _timeRemainding;

        [SerializeField]
        private GameObject _debugDisplay;

        private Text _debugText;

        private List<EnemyController> _instancied = new List<EnemyController>();

        private const int _x = 3;
        private const int _y = 5;

        private int _waveCount = 1;

        private EnemyController _currentDebugFollowed;

        private void Start()
        {
            _debugDisplay.SetActive(false);
            StartCoroutine(SpawnAll());
        }

        private IEnumerator SpawnAll()
        {
            int bestOfMaxCount = 20;
            List<NN> networks = new List<NN>();
            List<(NN network, float score)> networks_BestOf = new List<(NN network, float score)>(bestOfMaxCount);

            while (true)
            {
                _currentDebugFollowed = null;
                _debugDisplay.SetActive(false);
                var maxSize = new Vector2(-transform.position.x + _x, transform.position.y + _y);
                int count = 0;
                List<Transform> leaders = new List<Transform>();
                for (int x = -_x; x <= _x; x++)
                {
                    for (int y = -_y; y <= _y; y++)
                    {
                        var go = Instantiate(_enemyPrefab, transform.position + new Vector3(x, y), Quaternion.identity);
                        go.transform.parent = transform;
                        var ec = go.GetComponent<EnemyController>();
                        var rand = Random.Range(0, 100);
                        RaycastOutput type;
                        if (rand < 5) type = RaycastOutput.ENEMY_LEADER;
                        else if (rand < 20) type = RaycastOutput.ENEMY_SHIELD;
                        else type = RaycastOutput.ENEMY_SCOUT;
                        ec.Init(networks.Count == 0 ? null : new NN(networks[count]), type, this);
                        if (type == RaycastOutput.ENEMY_LEADER)
                        {
                            leaders.Add(ec.transform);
                        }
                        ec.WorldMaxSize = maxSize;
                        _instancied.Add(ec);
                        count++;
                    }
                }
                foreach (var e in _instancied)
                {
                    e.SetLeader(e.MyType == RaycastOutput.ENEMY_LEADER ? null : leaders[Random.Range(0, leaders.Count)]);
                }
                var timer = 10f;
                while (timer > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _timeRemainding.text = $"Wave {_waveCount} end in {timer} seconds";
                    timer--;
                }

                List<(NN network, float score)> oldgen = _instancied.Select(ec => (ec.Network, ec.gameObject.transform.position.x)).ToList();
                networks_BestOf.AddRange(oldgen);
                networks_BestOf.Sort(delegate
                ((NN network, float score) a, (NN network, float score) b)
                {
                    return b.score.CompareTo(a.score);
                });
                networks_BestOf = networks_BestOf.Take(bestOfMaxCount).ToList();

                networks = GeneticAlgorithm.GeneratePool(oldgen, networks_BestOf, _instancied.Count);

                foreach (var p in _instancied)
                {
                    Destroy(p.gameObject);
                }
                _instancied.RemoveAll(x => true);
                _waveCount++;
            }
        }

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

        private void DisplayDebug(InputData input, OutputData output)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine("<b>INPUT</b>");
            str.AppendLine($"Position: ({input.Position.x:0.00};{input.Position.y:0.00})");
            str.AppendLine($"Leader Position: ({input.LeaderPosition.x:0.00};{input.LeaderPosition.y:0.00})");
            str.AppendLine($"Direction: {input.Direction:0.00}");
            str.AppendLine($"Speed: {input.Speed:0.00}");
            int i = 1;
            foreach (var ray in input.RaycastInfos)
            {
                str.AppendLine($"Raycast {i}: {ray.Item1} (Distance {ray.Item2:0.00})");
                i++;
            }
            str.AppendLine("\n<b>RAW INPUT</b>");
            str.AppendLine(string.Join(", ", Decision.InputToFloatArray(input).Select(x => x.ToString("0.00"))));
            str.AppendLine("\n<b>OUTPUT</b>");
            str.AppendLine($"Direction: {output.Direction:0.00}");
            str.AppendLine($"Speed: {output.Speed:0.00}");
            str.AppendLine("Skill state: " + output.SkillState);
            str.AppendLine("Message: " + string.Join("", output.Message.Select(x => x ? "1" : "0")));

            _debugText.text = str.ToString();
        }

        public void SetDebug(EnemyController ec)
        {
            if (_currentDebugFollowed != null)
            {
                _currentDebugFollowed.DisplayDebugCallback = null;
            }
            ec.DisplayDebugCallback = DisplayDebug;
            _currentDebugFollowed = ec;
            _debugDisplay.SetActive(true);
            _debugText = _debugDisplay.GetComponentInChildren<Text>();
        }
    }
}
