using UnityEngine;

public class FigureEightMovement : MonoBehaviour
{
    public float speed = 1.0f;   
    public float width = 3.0f; 
    public float height = 1.5f; 

    private Vector3 startPos;  

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float time = Time.time * speed;
        float x = Mathf.Sin(time) * width;
        float y = Mathf.Sin(time * 2.0f) * height; 

        transform.position = startPos + new Vector3(x, y, 0);
    }
}