using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppleSculptureControl:Singleton<AppleSculptureControl> {

	protected AppleSculptureControl () {} // guarantee this will be always a singleton only - can't use the constructor!

	List<GameObject> apples = new List<GameObject>();
	List<GameObject> supporters = new List<GameObject>();

	public float rangeX = 10;
	public float rangeY = 10;

	public void addSculpture(){
		float locX = Random.Range (-rangeX/2,rangeX/2);
		float locY = Random.Range (-rangeY/2,rangeY/2);


		GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
		float supportLen = Random.Range (0.5f, 5.0f);
		support.transform.localScale = new Vector3(0.02f, 1*supportLen, 0.02f);
		support.transform.position = new Vector3(208f+locX, 0.5F*supportLen, 250.5f+locY);

		GameObject apple = Instantiate(Resources.Load("Apple", typeof(GameObject))) as GameObject;
		apple.transform.position = new Vector3(208f+locX, supportLen, 250.5f+locY);

		apples.Add(apple);
		supporters.Add(support);

//		print ("apples.Count = " + apples.Count);
//		print ("supporters.Count = " + supporters.Count);

	}

	public void removeAll(){
//		print ("apples.Count = " + apples.Count);
//		print ("supporters.Count = " + supporters.Count);

		for (int i = 0; i < apples.Count; i++) {
			Destroy(apples[i]);
		}

		for (int i = 0; i < supporters.Count; i++) {
			Destroy(supporters[i]);
		}
	}
}
