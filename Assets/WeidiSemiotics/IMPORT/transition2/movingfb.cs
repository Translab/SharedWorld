using UnityEngine;
using System.Collections;

public class movingfb : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float countdown = 20f;


    void Update()
    {
        countdown -= Time.deltaTime * 10;
        if (countdown >= 0.0f)
        {
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime); ;
        }
        if (countdown <= -20.0f)
            countdown = 20.0f;
        print(countdown);
    }
}
