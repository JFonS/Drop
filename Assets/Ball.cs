using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    public static readonly float Gravity = 0.2f;
    public bool upwardsGravity = false;
    public bool holding = false;

    public float speedX = 0.0f, speedY = 0.0f;
    public float accX = 1.0f;

    void Start() { }

    void FixedUpdate()
    {
        if (!holding) speedY += Gravity * Time.deltaTime;
    }

    void Update()
    {
        if(holding)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, GetWorldPoint(Input.mousePosition), Time.deltaTime * accX);
            transform.position = new Vector3(newPos.x, newPos.y, 0.0f);
        }

        transform.position += Vector3.up * speedY * (upwardsGravity ? 1.0f : -1.0f);

        if(Input.GetMouseButton(0))
        {
            Vector3 worldTouch = GetWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast ( new Vector2( worldTouch.x, worldTouch.y ), Vector2.zero, Mathf.Infinity);
            if (hit && hit.collider && hit.collider.gameObject == gameObject)
            {
                OnHold();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (holding) OnDrop();
        }

        foreach(Touch t in Input.touches)
        {
            Vector3 worldTouch = GetWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(worldTouch.x, worldTouch.y), Vector2.zero, Mathf.Infinity);
            if (hit && hit.collider && hit.collider.gameObject == gameObject)
            {
                switch(t.phase)
                {
                    case TouchPhase.Canceled:
                    case TouchPhase.Ended:
                        hit.collider.SendMessage("OnDrop", null, SendMessageOptions.DontRequireReceiver);
                        break;

                    default:
                        OnHold();
                        break;
                }
            }
        }
    }

    Vector3 GetWorldPoint(Vector3 screenPos)
    {
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    bool IsScreenUp()
    {
        return transform.position.y >= Camera.main.ScreenToWorldPoint( new Vector3(0.0f, Screen.height / 2, 0.0f) ).y;
    }

    void OnHold()
    {
        holding = true;
        speedY = 0.0f;
    }

    void OnDrop()
    {
        holding = false;
        if (IsScreenUp()) upwardsGravity = false;
        else upwardsGravity = true;
    }
}
