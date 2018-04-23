using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfReplication : MonoBehaviour {

    public GameObject obj;
    public Transform father;
    private float speed;
    private bool highSpeed;
    private bool stayed;
    private bool givenbirth1st = true;
    public float maxAge = 5f;
    private float timer = 0f;

    public float maXspeed = 10f;
    public Material mat;
    private Material mymat;
    private bool untouched = true;
    // Use this for initialization
    IEnumerator Start()
    {
        GetComponent<Renderer>().material = new Material(mat);
        mymat = GetComponent<Renderer>().material;

        yield return new WaitForSeconds(Random.Range(0.1f, 0.2f));
        givenbirth1st = false;


    }
	
	// Update is called once per frame
	void Update () {

        Vector3 velocity = GetComponent<Rigidbody>().velocity;

        timer += Time.deltaTime;
        if (timer > maxAge)
        {
            Destroy(gameObject);
        }

        if (!givenbirth1st)
        {
            if (velocity.magnitude < 0.8f * transform.localScale.x)
            {
                
                GiveBirth();
                givenbirth1st = true;
            }
        }

        if (transform.position.y < 0)
        {
            Destroy(gameObject);
        }

        if (velocity.magnitude > 2.0f * transform.localScale.x)
        {
            highSpeed = true;
        }

        if (highSpeed && velocity.magnitude < 0.001f * transform.localScale.x)
        {
            highSpeed = false;
            StartCoroutine(Start());
        }

        if (velocity.magnitude > 5f)
        {
            //Destroy(gameObject);
        }

        mymat.SetFloat("_Level", velocity.magnitude / maXspeed);


    }

    void GiveBirth()
    {
        if (untouched)
        {
            Vector3 newpos = transform.position + new Vector3(Random.Range(-0.05f, 0.05f) * transform.localScale.x, transform.localScale.y * 1.05f, Random.Range(-0.05f, 0.05f) * transform.localScale.x);
            var o = Instantiate(obj, newpos, transform.rotation);
            o.transform.Rotate(Vector3.up, 10f);
            o.name = "cb";
            o.GetComponent<SelfReplication>().father = father;
            if (father)
            {
                o.transform.SetParent(father);
            }
        }

    }

    
    void OnCollisionStay(Collision col)
    {
        if (!col.transform.CompareTag("generator"))
        {
            untouched = false;
        }
    }

    void OnDestroy()
    {
        Destroy(mymat);
    }

}
