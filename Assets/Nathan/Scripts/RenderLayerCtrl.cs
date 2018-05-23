using UnityEngine;
using System.Collections;

public class RenderLayerCtrl : MonoBehaviour
{

		public void toggleLayer (string layerName)
		{
				int layerMaskInt = LayerMask.NameToLayer (layerName);
				bool layerOn = (Camera.main.cullingMask & (1 << layerMaskInt)) != 0;

				if (layerOn) {
						//Turn Layer Off
						Camera.main.cullingMask &= ~(1 << layerMaskInt);
				} else {
						//Turn Layer On
						Camera.main.cullingMask |= (1 << layerMaskInt);
				}
		}

		public void toggleLayer (bool b)
		{
				int layerMaskInt = LayerMask.NameToLayer ("ExtraComponents");

				if (!b) {
						//Turn Layer Off
						Camera.main.cullingMask &= ~(1 << layerMaskInt);
				} else {
						//Turn Layer On
						Camera.main.cullingMask |= (1 << layerMaskInt);
				}
		}
}
