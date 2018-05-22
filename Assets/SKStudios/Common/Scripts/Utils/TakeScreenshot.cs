using System;
using UnityEngine;

namespace SKStudios.Common.Utils {
    /// <summary>
    ///     Utility class for screenshot capturing
    /// </summary>
    public class TakeScreenshot : MonoBehaviour {
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space))
                ScreenCapture.CaptureScreenshot(Application.dataPath + "/Screenshots/" +
                                              DateTime.Now.ToString("yyyy-MM-dd-HH-mm") + ".png");
        }
    }
}