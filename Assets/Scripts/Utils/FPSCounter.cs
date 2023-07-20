using System;
using UnityEngine;
using UnityEngine.UI;
using Utils.Singleton;

namespace UnityStandardAssets.Utility
{
    [RequireComponent(typeof (TMPro.TextMeshProUGUI))]
    public class FPSCounter : MonoBehaviour
    {
        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0;
        private int m_CurrentFps;
        const string display = "{0} FPS";
        
        [SerializeField]
        private TMPro.TextMeshProUGUI m_Text;

        private void Start()
        {
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        private void Update()
        {
            // measure average frames per second
            m_FpsAccumulator++;
            if (Time.realtimeSinceStartup > m_FpsNextPeriod)
            {
                m_CurrentFps = (int) (m_FpsAccumulator / fpsMeasurePeriod);
                m_FpsAccumulator = 0;
                m_FpsNextPeriod += fpsMeasurePeriod;
                m_Text.text = string.Format(display, m_CurrentFps);
            }
        }

        public int GetFPS()
        {
            return m_CurrentFps;
        }

        public void HideFPSCounter()
        {
            if (m_Text != null)
            {
                m_Text.enabled = false;
            }
        }
    }
}
