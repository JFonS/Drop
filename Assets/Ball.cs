using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Ball : MonoBehaviour
{
    private static readonly string WinText = "YOU WIN", LoseText = "YOU LOSE";
    private static readonly float DelimiterOffset = 0.3f; //This is the ratio of the screen at which the colors lerp when it's somebody's turn

    public float delimiter = 0.5f; //This is the imaginary line which when is crossed by the ball, the ball is automatically dropped
    public float delimiterSpeed = 1.0f;

    public static Vector3 initialBallPosition, initialBallScale;
    public bool gameStarted = false, gameFinished;

    public static readonly float Gravity = 0.25f;
    public bool upwardsGravity = false;
    public bool turnReady = true, topTurn = true;
    public bool holding = false;

    public float decreaseRate = 0.05f; //The rate at which the scale of the ball decreases
    public float speedX = 0.0f, speedY = 0.0f;
    public float accX = 1.0f;
    public float disaccelerationY = 100.0f; //The amount of disacceleration (speed of the Lerp) when the ball is thrown very fast on the y axis

    private int pickingFingerId = -1;

    private int topScore, botScore;

    public Image bgUp, bgDown;
    public Text topWinText, botWinText;
    private Color pointBallInitialColorTop, pointBallInitialColorBot;
    public Image pointBallTop0, pointBallTop1, pointBallTop2;
    public Image pointBallBot0, pointBallBot1, pointBallBot2;

    public Color idleBallColor;

    void Start()
    {
        topTurn = IsScreenUp();
        initialBallPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        initialBallScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);

        pointBallInitialColorTop = pointBallTop0.color;
        pointBallInitialColorBot = pointBallBot0.color;
        topScore = botScore = 0;

        topTurn = (Random.Range(1, 3) == 2);
        Reset();
    }

    void FixedUpdate()
    {
        if (!holding) speedY += Gravity * (upwardsGravity ? 1.0f : -1.0f);
    }

    void Update()
    {
        Debug.Log("PICKINGID: " + pickingFingerId + " HOLDING: " + holding);

        //Check back button
        if (Input.GetKey(KeyCode.Escape)) { Application.LoadLevel("Menu"); return; }

        if (!gameFinished)
        {
            if ((topTurn && !IsScreenUp() && turnReady) || (!topTurn && IsScreenUp() && turnReady)) OnDrop();
            if (turnReady && (IsUpOut() || IsDownOut())) OnScore();

            foreach (Touch t in Input.touches)
            {
                Debug.Log("touch " + t.fingerId);
                if (holding && pickingFingerId != t.fingerId && (turnReady || pickingFingerId != -1)) continue;

                Vector3 touchWorldPoint = GetWorldPoint(t.position);

                if (t.phase == TouchPhase.Canceled || t.phase == TouchPhase.Ended)
                {
                    OnDrop();
                }
                else if (t.phase == TouchPhase.Began)
                {

                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(touchWorldPoint.x, touchWorldPoint.y), Vector2.zero, Mathf.Infinity);
                    if (hit && hit.collider && hit.collider.gameObject == gameObject)
                    {
                        OnHold();
                        pickingFingerId = t.fingerId;
                    }
                }
                else if (holding && t.phase == TouchPhase.Moved)
                {

                    //Touch version
                    Vector2 delta = GetWorldVector(t.deltaPosition);

                    if (Mathf.Abs(delta.x) >= 0.001)
                        if (t.deltaTime > 0) speedX = delta.x / t.deltaTime * 0.5f;
                        else
                            speedX = 0.0f;

                    if (Mathf.Abs(delta.y) >= 0.001 && t.deltaTime > 0)
                        if (t.deltaTime > 0) speedY = delta.y / t.deltaTime * 0.5f;
                        else
                            speedY = 0.0f;
                }

                if (holding)
                {
                    //Move the ball to the touch position
                    Vector3 newPos = Vector3.Lerp(transform.position, touchWorldPoint, Time.deltaTime * accX);
                    transform.position = new Vector3(newPos.x, newPos.y, 0.0f);

                    if (IsRightOut() || IsLeftOut()) speedX = 0.0f;
                    CorrectXPosition();
                    if (topTurn && IsScreenUp() || !topTurn && !IsScreenUp()) SetTurnReady(true);
                }
            }

            if (!holding && gameStarted)
            {

                transform.position += Vector3.right * speedX * Time.deltaTime;
                transform.position += Vector3.up * speedY * Time.deltaTime; //move with the gravity

                if (IsRightOut() || IsLeftOut()) speedX *= -0.8f; //Bounce on side walls
                CorrectXPosition();
            }

            if (holding && (topTurn && IsScreenUp() || !topTurn && !IsScreenUp())) SetTurnReady(true);
        }
        else
        {
            //Game has finished, we have a winner
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began) EndGame();
            }
            if (Input.GetMouseButtonDown(0)) EndGame();
        }

        //Update the graphical(canvas bgs) delimiter
        UpdateDelimiter();
    }

    void SetTurnReady(bool state)
    {
        if (state)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().color = idleBallColor;
        }
        turnReady = state;
    }

    void OnHold()
    {
        if (!turnReady) return;

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
        SetTurnReady(false);
    }

    void OnDrop()
    {
        pickingFingerId = -1;
        if (turnReady)
            holding = false;
    }

    void OnScore()
    {
        if (!IsScreenUp()) topScore++; else botScore++;
        FillBallPoints();
        if (topScore >= 3 || botScore >= 3) OnWin();
        else Reset();
    }

    void OnWin()
    {
        if (topScore >= 3)
        {
            topWinText.text = WinText;
            botWinText.text = LoseText;
        }
        else
        {
            topWinText.text = LoseText;
            botWinText.text = WinText;
        }
        gameFinished = true;
    }

    void EndGame()
    {
        Reset();
        topScore = botScore = 0; FillBallPoints();
    }
    void Reset()
    {
        gameStarted = gameFinished = false;
        holding = false;
        transform.localScale = initialBallScale;
        topTurn = !topTurn;
        if (topTurn) transform.position = initialBallPosition;
        else
        {
            float initY = GetScreenDown() + (GetScreenUp() - initialBallPosition.y);
            transform.position = new Vector3(initialBallPosition.x, initY, initialBallPosition.z);
        }
        this.GetComponent<SpriteRenderer>().color = idleBallColor;
        upwardsGravity = !topTurn;

        topWinText.text = "";
        botWinText.text = "";
    }

    void FillBallPoints()
    {
        pointBallTop0.color = pointBallInitialColorTop;
        pointBallTop1.color = pointBallInitialColorTop;
        pointBallTop2.color = pointBallInitialColorTop;
        pointBallBot0.color = pointBallInitialColorBot;
        pointBallBot1.color = pointBallInitialColorBot;
        pointBallBot2.color = pointBallInitialColorBot;

        if (topScore >= 1) pointBallTop0.color = Color.white;
        if (topScore >= 2) pointBallTop1.color = Color.white;
        if (topScore >= 3) pointBallTop2.color = Color.white;

        if (botScore >= 1) pointBallBot0.color = Color.white;
        if (botScore >= 2) pointBallBot1.color = Color.white;
        if (botScore >= 3) pointBallBot2.color = Color.white;
    }

    //If out of the X boundaries(left or right), puts the ball inside the screen
    void CorrectXPosition()
    {
        if (IsRightOut()) transform.position = new Vector3(GetScreenRight() - GetBallWidth() / 2 * 1.05f, transform.position.y, transform.position.z);
        else if (IsLeftOut()) transform.position = new Vector3(GetScreenLeft() + GetBallWidth() / 2 * 1.05f, transform.position.y, transform.position.z);
    }

    //Gets and Isses //////////////////////////////////////////////////////////////////////////////////////////////////
    Vector3 GetWorldPoint(Vector3 screenPos) { return Camera.main.ScreenToWorldPoint(screenPos); }

    Vector2 GetWorldVector(Vector2 v)
    { /// THIS ONLY WORKS IF THE CAMERA IS AT (0,0)
        v.x += Screen.width / 2;
        v.y += Screen.height / 2;
        return Camera.main.ScreenToWorldPoint(v);
    }

    void UpdateDelimiter()
    {
        delimiter = Mathf.Lerp(delimiter, (topTurn ? 0.25f : 0.75f), Time.deltaTime * delimiterSpeed);
        bgUp.GetComponent<LayoutElement>().flexibleHeight = delimiter;
        bgDown.GetComponent<LayoutElement>().flexibleHeight = 1.0f - delimiter;
    }

    bool IsScreenUp()
    {
        return transform.position.y >= Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height * (topTurn ? 0.75f : 0.25f), 0.0f)).y;
    }
    float GetBallWidth() { return GetComponent<SpriteRenderer>().bounds.size.x; }
    float GetBallHeight() { return GetComponent<SpriteRenderer>().bounds.size.y; }
    float GetScreenUp() { return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, Screen.height, 0.0f)).y; }
    float GetScreenDown() { return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)).y; }
    float GetScreenRight() { return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0.0f, 0.0f)).x; }
    float GetScreenLeft() { return Camera.main.ScreenToWorldPoint(new Vector3(0.0f, 0.0f, 0.0f)).x; }
    bool IsUpOut() { return transform.position.y - GetBallHeight() / 2 > GetScreenUp(); }
    bool IsDownOut() { return transform.position.y + GetBallHeight() / 2 < GetScreenDown(); }
    bool IsRightOut() { return transform.position.x + GetBallWidth() / 2 > GetScreenRight(); }
    bool IsLeftOut() { return transform.position.x - GetBallWidth() / 2 < GetScreenLeft(); }
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
