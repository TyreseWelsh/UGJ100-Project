using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    [SerializeField] private string level;
    [SerializeField] private AnimationClip fadeIn;
    [SerializeField] private GameObject fadeinUI;
    private bool goUp = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (goUp)
        {
            gameObject.transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("Entering new level");
            StartCoroutine(levelTransition());
        }
    }

    IEnumerator levelTransition()
    {
        goUp = true;
        gameObject.transform.Translate(Vector3.up * Time.deltaTime, Space.World);
        Instantiate(fadeinUI);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(level);
    }
}
