using UnityEngine;

public class TriggerYourself : MonoBehaviour {
    public void Trigger() {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}