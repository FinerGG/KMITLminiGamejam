using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MGJ
{
    public class SignalRow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI numberText;
        [SerializeField] private TextMeshProUGUI signalText;

        public void Setup(int number, string signal)
        {
            if (numberText != null)
                numberText.text = number.ToString("D2");

            if (signalText != null)
                signalText.text = signal;
        }
    }
}
