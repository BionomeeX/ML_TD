using MLTD.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretManager : MonoBehaviour
    {
        [Tooltip("Turret prefab")]
        [SerializeField]
        private GameObject _turret;

        // List of all turrets instanciated
        private Dictionary<Vector2Int, GameObject> _turrets = new Dictionary<Vector2Int, GameObject>();

        public void Update()
        {
            // When we click somewhere on the map
            if (Input.GetMouseButtonDown(0))
            {
                // Set world position from mouse and convert it to grid position
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var dictPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

                // If there is no turret in this position
                if (!_turrets.Any(x => x.Key.x == dictPos.x && x.Key.y == dictPos.y)
                    && PlayerController.S.GoldAmount >= 0) // Turret cost 5 gold
                {
                    PlayerController.S.GoldAmount -= 0;
                    PlayerController.S.UpdateGoldText();
                    var finalPos = new Vector2(dictPos.x + .5f, dictPos.y + .5f); // .5f is to put the turret on the right grid position
                    _turrets.Add(dictPos, Instantiate(_turret, finalPos, Quaternion.identity));
                }
            }
        }
    }
}
