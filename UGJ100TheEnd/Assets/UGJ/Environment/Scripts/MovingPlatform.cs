using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Serializable]
    public class PlatformPoint
    {
        public GameObject point;
        public float waitTime;
    }
    
    [SerializeField] private float movementRate = 0.1f;
    public List<PlatformPoint> platformPoints;
    [SerializeField] private int startingPoint;
    [SerializeField] private bool startForwards = true;
    [SerializeField] private bool loop;

    private List<GameObject> objectsOnPlatform = new List<GameObject>();
    
    public Coroutine movementCoroutine;
    
    // Start is called before the first frame update
    void Start()
    {
        if (platformPoints.Count > 0)
        {
            transform.position = platformPoints[startingPoint].point.transform.position;

            if (startForwards)
            {
                movementCoroutine = StartCoroutine(Move(startingPoint, startingPoint + 1, true));
            }
            else
            {
                movementCoroutine = StartCoroutine(Move(startingPoint, startingPoint - 1, false));
            }
        }
    }

    IEnumerator Move(int startPoint, int targetPoint, bool forwards)
    {
        yield return new WaitForSeconds(platformPoints[startPoint].waitTime);
        
        //Vector3 startPosition = platformPoints[startPoint].point.
        Vector3 directionNormal = platformPoints[targetPoint].point.transform.position - platformPoints[startPoint].point.transform.position;
        directionNormal.Normalize();
        float distanceToTarget = Vector3.Distance(transform.position, platformPoints[targetPoint].point.transform.position);
        
        while (distanceToTarget > 0.25f)
        {
            Vector3 movementToAdd = movementRate * Time.deltaTime * directionNormal;
            transform.position += movementToAdd;
            foreach (GameObject platformObject in objectsOnPlatform)
            {
                print(platformObject.name + " on platform");

                if (platformObject != null)
                {
                    Debug.Log("Adding:" + movementToAdd);
                    platformObject.transform.position += movementToAdd * 2.25f;
                }
            }
            distanceToTarget = Vector3.Distance(transform.position, platformPoints[targetPoint].point.transform.position);
            yield return null;
        }
        
        // Next point would be out of range of list
        if (targetPoint + 1 >= platformPoints.Count || targetPoint - 1 < 0)
        {
            // 
            Debug.Log("Target not in range - reversing!");
            movementCoroutine = StartCoroutine(Move(targetPoint, startPoint, targetPoint - 1 < 0));
        }
        // Next point is inside of list
        else
        {
            Debug.Log("Successfully found next target!");
            if (forwards)
            {
                movementCoroutine = StartCoroutine(Move(targetPoint, targetPoint + 1, true));
            }
            else
            {
                movementCoroutine = StartCoroutine(Move(targetPoint, targetPoint - 1, false));
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject == null)
            {
                print("Object null");
            }
            else
            {
                print(other.gameObject.name + " has entered");
                objectsOnPlatform.Add(other.gameObject);
            }
            //other.gameObject.transform.SetParent(gameObject.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            print(other.gameObject.name + " has exited");
            objectsOnPlatform.Remove(other.gameObject);
            //other.gameObject.transform.SetParent(null);
        }
    }
}
