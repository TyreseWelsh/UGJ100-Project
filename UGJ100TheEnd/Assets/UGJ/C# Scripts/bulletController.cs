using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletController : MonoBehaviour
{
    private Rigidbody bulletRB;
    private GameObject player;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        bulletRB = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            other.gameObject.GetComponent<IDamageable>().Damaged(10, gameObject);

        }
        Destroy(this.gameObject);
    }

    IEnumerator lifespan()
    {
        yield return new WaitForSeconds(2);
        Destroy(this.gameObject);
    }
}
