using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour
{
    public static Vector3 initialBallPosition, initialBallScale;
    public bool gameStarted = false;

    public static readonly float Gravity = 0.2f, MaxSpeedX = 10.0f, MaxSpeedY = 10.0f;
    public bool upwardsGravity = false;
    public bool topTurn = true;
    public bool holding = false;

    public float decreaseRate = 0.05f; //The rate at which the scale of the ball decreases
    public float speedX = 0.0f, speedY = 0.0f;
    public float accX = 1.0f, dampingX = 0.98f;
    public float disaccelerationY = 100.0f; //The amount of disacceleration (speed of the Lerp) when the ball is thrown very fast on the y axis

    void Start() 
    {
        topTurn = IsScreenUp();
        initialBallPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        initialBallScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
        
        Reset();
    }

    void FixedUpdate()
    {
        if (!holding) speedY += Gravity * (upwardsGravity ? 1.0f : -1.0f);
    }

    void Update()
    {
        if (topTurn && !IsScreenUp() || !topTurn && IsScreenUp()) OnDrop();

        if (IsUpOut() || IsDownOut()) Reset();

        Vector3 touchWorldPoint = GetWorldPoint(Input.mousePosition);
        if(holding)
        {
            //Move the ball to the touch position
            Vector3 newPos = Vector3.Lerp(transform.position, touchWorldPoint, Time.deltaTime * accX);
            transform.position = new Vector3(newPos.x, newPos.y, 0.0f);

            //Set the speedX based on the finger movement
            speedX = (touchWorldPoint.x - transform.position.x) / Time.deltaTime * 0.05f;
            speedX = Mathf.Clamp(speedX, -MaxSpeedX, MaxSpeedX);

            speedY = (touchWorldPoint.y - transform.position.y) / Time.deltaTime * 0.05f;

            if (IsRightOut() || IsLeftOut()) speedX = 0.0f;
            CorrectXPosition();
        }

        if(!holding && gameStarted)
        {
            transform.position += Vector3.right * speedX * Time.deltaTime;

            if (speedY < -MaxSpeedY) speedY = Mathf.Lerp(speedY, -MaxSpeedY, Time.deltaTime * Mathf.Abs(speedY / MaxSpeedY) * disaccelerationY);
            else if (speedY > MaxSpeedY) speedY = Mathf.Lerp(speedY, MaxSpeedY, Time.deltaTime * Mathf.Abs(speedY / MaxSpeedY) * disaccelerationY);

            transform.position += Vector3.up * speedY * Time.deltaTime; //move with the gravity

            if (IsRightOut() || IsLeftOut()) speedX *= -1.0f; //Bounce on side walls
            else
            {
                speedX *= dampingX;
            }
            CorrectXPosition();
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast ( new Vector2( touchWorldPoint.x, touchWorldPoint.y ), Vector2.zero, Mathf.Infinity);
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
            touchWorldPoint = GetWorldPoint(t.position);
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(touchWorldPoint.x, touchWorldPoint.y), Vector2.zero, Mathf.Infinity);
            if (hit && hit.collider && hit.collider.gameObject == gameObject)
            {
                if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended) OnDrop();
                else if(t.phase == TouchPhase.Began) OnHold();
            }
        }
    }

    void OnHold()
    {
        bool lastTopTurn = topTurn;
        if (IsScreenUp() && !topTurn) topTurn = true;
        else if (!IsScreenUp() && topTurn) topTurn = false;
        bool changedTurn = (lastTopTurn != topTurn);
        if (changedTurn) OnChangeTurn();
        speedY = 0.0f;

        gameStarted = true;
        holding = true;
        upwardsGravity = !topTurn;
    }

    void OnChangeTurn()
    {
        transform.localScale -= new Vector3(decreaseRate, decreaseRate, decreaseRate);
    }

    void OnDrop()
    {
        holding = false;
        if (IsScreenUp()) upwardsGravity = false;
        else upwardsGravity = true;
    }

    void Reset()
    {
        gameStarted = false;
        holding = false;
        transform.localScale = initialBallScale;
        if(!topTurn) transform.position = initialBallPosition;
        else
        {
            float initY = GetScreenDown() + (GetScreenUp() - initialBallPosition.y);
            transform.position = new Vector3(initialBallPosition.x, initY, initialBallPosition.z);
        }
        topTurn = !topTurn;
        upwardsGravity = !topTurn;
    }
    
    //If out of the X boundaries(left or right), puts the ball inside the screen
    void CorrectXPosition()
    {
        if (IsRightOut()) transform.position = new Vector3(GetScreenRight() - GetBallWidth() / 2 * 1.05f, transform.position.y, transform.position.z);
        else if (IsLeftOut()) transform.position = new Vector3(GetScreenLeft() + GetBallWidth() / 2 * 1.05f, transform.position.y, transform.position.z);
    }
    
    //Gets and Isses //////////////////////////////////////////////////////////////////////////////////////////////////
    Vector3 GetWorldPoint(Vector3 screenPos)
    {
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    bool IsScreenUp()
    {
        return transform.position.y >= Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height / 2, 0.0f)).y;
    }

    float GetBallWidth()
    {
        return GetComponent<SpriteRenderer>().bounds.size.x;
    }

    float GetBallHeight()
    {
        return GetComponent<SpriteRenderer>().bounds.size.y;
    }

    float GetScreenUp()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)).y;
    }

    float GetScreenDown()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)).y;
    }

    float GetScreenRight()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)).x;
    }

    float GetScreenLeft()
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)).x;
    }

    bool IsUpOut()
    {
        return transform.position.y - GetBallHeight() / 2 > GetScreenUp();
    }

    bool IsDownOut()
    {
        return transform.position.y + GetBallHeight() / 2 < GetScreenDown();
    }


    bool IsRightOut()
    {
        return transform.position.x + GetBallWidth() / 2 > GetScreenRight();
    }

    bool IsLeftOut()
    {
        return transform.position.x - GetBallWidth() / 2 < GetScreenLeft();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
