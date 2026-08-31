using UnityEngine;

namespace MGJ 
{
    public class Interactable : MonoBehaviour
    {
        protected bool interact = false;
        protected bool active = true;
        public void Click() => interact = true;
        public void setActive(bool act) => active = act;
    }
}
