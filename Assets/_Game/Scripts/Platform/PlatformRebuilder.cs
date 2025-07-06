using UnityEngine;
using System.Collections;

public class PlatformRebuilder : MonoBehaviour
{
    [Header("Platform Building")]
    public GameObject[] chunks;
    public float delayBetweenChunks = 0.5f;

    [Header("Movement")]
    public Transform pointA;
    public Transform pointB;
    public Transform pointC;

    public float moveSpeed = 2f;

    private Vector3 moveTarget;
    public void StartBuilding()
{

    transform.position = pointA.position;
    moveTarget = pointB.position;

    StartCoroutine(BuildPlatform());
}


    void Start()
    {

        transform.position = pointA.position;
        moveTarget = pointB.position;


        foreach (GameObject chunk in chunks)
            chunk.SetActive(false);


    }

    void Update()
    {
        MovePlatform();
    }

    void MovePlatform()
{
    transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);

    if (Vector3.Distance(transform.position, moveTarget) < 0.05f)
    {
        // Move A → B → C → A (loop)
        if (moveTarget == pointA.position)
            moveTarget = pointB.position;
        else if (moveTarget == pointB.position)
            moveTarget = pointC.position;
        else
            moveTarget = pointA.position;
    }
}


    IEnumerator BuildPlatform()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            GameObject chunk = chunks[i];

            Vector3 localTargetPos = chunk.transform.localPosition;
            Vector3 localStartPos = localTargetPos + new Vector3(0, -1f, 0);
            chunk.transform.localPosition = localStartPos;

            chunk.SetActive(true);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                chunk.transform.localPosition = Vector3.Lerp(localStartPos, localTargetPos, t);
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenChunks);
        }

        // Enable colliders after all chunks are placed
        foreach (GameObject chunk in chunks)
        {
            var col = chunk.GetComponent<BoxCollider2D>();
            if (col != null)
                col.enabled = true;
        }
    }
}
