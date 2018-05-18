using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBud : MonoBehaviour {
    public Rigidbody[] moons;
    public GameObject moonpref;
    public int moonNumber;
    public float moonspeed = 5f;

    public float range =100f;

    public List<GameObject> interestList;
    public List<Vector3> interestPosition;
    public List<float> interestRate;

    public GameObject target;
    public float agility = 0.1f;
    private Vector3 atten = Vector3.zero;

    private float timer;
    private float attentime;
    private float attentimer;
    private AudioSource aud;
    // Use this for initialization
    void Start () {

        aud = GetComponent<AudioSource>();

        transform.position = transform.position+ new Vector3(Random.Range(-1f, 1f), Random.Range(0.0f, 0.0f), Random.Range(-1f, 1f)) * 100;
        atten = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 1000f;
        attentime = Random.Range(3f, 20f);
        attentimer = Random.Range(0f, 2f);

        moons = new Rigidbody[moonNumber];
        for (int i = 0; i < moonNumber; i++)
        {
            GameObject mn = Instantiate(moonpref, transform.position+new Vector3(Random.Range(-1f,1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)), transform.rotation);
            mn.SetActive(true);
            moons[i] = mn.GetComponent<Rigidbody>();
        }


    }
	
	// Update is called once per frame
	void Update () {
        timer += Time.deltaTime;
        attentimer += Time.deltaTime;
        if (timer > attentime)
        {
            GrabAttention();
            timer = 0f;
            attentime = Random.Range(3f, 10f);
            aud.time = Random.Range(0, aud.clip.length);
            aud.Play();
        }

        if (attentimer > 3f)
        {
            attentimer = 0f;
            atten = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 1000f;
        }

		foreach (Rigidbody m in moons)
        {
            //m.AddForce((transform.position - m.transform.position) * 10f);
            Vector3 dist = (transform.position - m.transform.position);
            m.velocity = Vector3.Cross(dist, m.transform.right)* moonspeed + dist*(Mathf.Clamp(dist.magnitude-1f,0,100f));
            m.transform.LookAt(transform,m.transform.up);
        }

        if (target)
        {
            Vector3 dist = (target.transform.position -transform.position);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dist, Vector3.up), agility);

            GetComponent<Rigidbody>().velocity = Vector3.Cross(dist, transform.up).normalized * Mathf.Sin(Time.time * 2f) * 0.3f + // left right osciclation
                dist.normalized * (Mathf.Clamp(dist.magnitude - 3f, 0, 1000f * agility)) + // moving to object
                Vector3.up * Mathf.Cos(Time.time * 1.5f) * 0.2f + // up down osi
                Vector3.up * (3f -transform.position.y )*5f;

        }else
        {
            GetComponent<Rigidbody>().velocity = Vector3.Lerp(GetComponent<Rigidbody>().velocity, Vector3.zero, 0.001f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(atten, Vector3.up), agility);
        }

    }


    void GrabAttention()
    {
        

        interestList = new List<GameObject>();
        interestPosition = new List<Vector3>();
        interestRate = new List<float>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        int i = 0;
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].gameObject.tag != "self"&& hitColliders[i].gameObject.activeSelf)
            {
                interestList.Add(hitColliders[i].gameObject);
                interestPosition.Add(hitColliders[i].transform.position);
                interestRate.Add(0f);
            }
            i++;
        }

        Invoke("CalcAttention", 0.1f);
    }

    void CalcAttention()
    {

        for (int j = 0; j < interestList.Count; j++)
        {
            if (!interestList[j])
            {
                interestList.RemoveAt(j);
                interestPosition.RemoveAt(j);
                interestRate.RemoveAt(j);
            }
        }

            float maxInt = 0f;
        int maxintindex = -1;
        for (int i = 0; i < interestList.Count; i++)
        {
            if (!interestList[i])
            {
                return;
            }

            interestRate[i] = (interestPosition[i] - interestList[i].transform.position).magnitude;
            if (interestRate[i] > maxInt)
            {
                maxInt = interestRate[i];
                maxintindex = i;
            }
        }

        if (maxintindex >= 0)
        {
            target = interestList[maxintindex];
        }else
        {
            target = interestList[Random.Range(0, interestList.Count)];
        }
    }
}
