using UnityEngine;

namespace MGJ
{
    public class SystemController : MonoBehaviour
    {
        [SerializeField] protected string _systemName = "System";
        public string SystemName => _systemName;

        protected bool _isBroken = false;
        public bool IsBroken => _isBroken;

        protected float _damages = 0;
        protected bool _isRepaired = false;

        public void takeDamage(float damage) => _damages += damage;
    }
}