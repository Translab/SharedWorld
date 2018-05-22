using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace SKStudios.Common.Utils
{
    [RequireComponent(typeof(Text))]
    public class SKFPSCounter : MonoBehaviour
    {
        private const uint AvgFrames = 100;
        private uint _currentFrame = 0;

        private Text _text;
        private float[] _timeBuffer = new float[(uint)AvgFrames];

        void Start()
        {
            _text = GetComponent<Text>();
        }
        void Update()
        {
            _timeBuffer[_currentFrame] = Time.deltaTime;
            _currentFrame = (_currentFrame + 1) % AvgFrames;
            double msec = 0;
            for (int i = 0; i < AvgFrames; i++)
                msec += _timeBuffer[i];
            msec /= AvgFrames;
            msec *= 1000.0f;
            double fps = (1000.0f / (msec)) ;
            _text.text = string.Format("{0:00.0} ms ({1:000} fps)", msec, fps);
        }
    }
}