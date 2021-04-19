using MLTD.Player;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MLTD.Turret
{
    public class TurretManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _turret;

        private Dictionary<Vector2Int, GameObject> _turrets = new Dictionary<Vector2Int, GameObject>();

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var dictPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
                if (!_turrets.Any(x => x.Key.x == dictPos.x && x.Key.y == dictPos.y)
                    && PlayerController.S.GoldAmount >= 5)
                {
                    PlayerController.S.GoldAmount -= 5;
                    PlayerController.S.UpdateGoldText();
                    var finalPos = new Vector2(dictPos.x + .5f, dictPos.y + .5f);
                    _turrets.Add(dictPos, Instantiate(_turret, finalPos, Quaternion.identity));
                }
            }
        }
    }
}
