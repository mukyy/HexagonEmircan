using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HexagonScript : MonoBehaviour
{
    public GameHandler HandlerRef;
    public Vector2Int Coordinate;
    public GameObject BombTextRef;
    // Used To Determine its type (0 = Regular , 1 = Bomb)
    public int Type = 0;
    // Used to count remaining turns of the Bomb
    public int TurnLeft = -1;
    public bool bIsHighlighted = false;
    // Position type to adapt to selection at walls and corners
    public int Position = -1;
    // Used to determine start point to adap to selection at walls and corners 
    public int State = -1;
    // Used to find matches according to this ID instead of the color itself for optimization
    public int ColorID = -1;

    private void Awake()
    {
        HandlerRef = FindObjectOfType<GameHandler>();
        Coordinate = new Vector2Int(-1,-1);
    }

    void Start()
    {
        // If its a bomb
        if (Type == 1)
        {
            SpawnBombAndActivate(Random.Range(4, 7));
        }
    }

    void Update()
    {
    }

    private void OnMouseOver()
    {
        // Uncomment to check the current coordinate of the hex the cursor is on
        //Debug.Log("Color : " + ColorID + " Coordinate : " + Coordinate);
        if (Input.GetMouseButtonUp(0))
        {
            StartCoroutine(SelectDelayer());
        }
        
    }
    // Used Small Delay to prevent player from selecting a new hex when dragged
    IEnumerator SelectDelayer()
    {
        yield return new WaitForSeconds(.05f);
        if (HandlerRef.bIsTurning || HandlerRef.PlayerRef.bIsDragged) { yield break; }
        if (!bIsHighlighted) { State = 0; }
        else { State = CalculateState(); }
        HandlerRef.HighlightHexagons(Coordinate, CalculateState());
    }

    // Play Explosion Sound, and stop game over caller if it gets destroyed during 0 turn left
    private void OnDestroy()
    {
        try
        {
            HandlerRef.PlayerRef.PlayerAudioSource.Play();
        }
        catch (MissingReferenceException) { }
        StopCoroutine(GameOverCaller());
    }

    // Calculate Starting State according to position it is in
    public int CalculateState()
    {
        if (!bIsHighlighted)
        {
            switch (Position)
            {
                case 1: { State = 2; return State; }
                case 2: { State = 3; return State; }
                case 3: { State = 5; return State; }
                case 4: { State = 2; return State; }
                case 5: { State = 3; return State; }
                case 6: { if (Coordinate.x % 2 == 0) { State = 4; } else { State = 5; } return State; }
                default: { State = 0; return State; }
            }
        }
        return State;
    }

    /*
    * 0 : Left Wall    4 : Top-Left Corner
    * 1 : Top Wall     5 : Top-Right Corner
    * 2 : Right Wall   6 : Bottom-Right Corner
    * 3 : Bottom Wall  7 : Bottom-Left Corner
    */// Calculates position according to the surroundings of the Hex
    public void CalculatePosition()
    {
        int ColumnLimit = HandlerRef.ColumnCount - 1;
        int RowLimit = HandlerRef.RowCount - 1;
        if (Coordinate.x == 0 && Coordinate.y == 0) { Position = 4; return; }
        if (Coordinate.x == ColumnLimit && Coordinate.y == 0) { Position = 5; return; }
        if (Coordinate.x == ColumnLimit && Coordinate.y == RowLimit) { Position = 6; return; }
        if (Coordinate.x == 0 && Coordinate.y == RowLimit) { Position = 7; return; }
        if (Coordinate.x == 0) { Position = 0; return; }
        if (Coordinate.y == 0) { Position = 1; return; }
        if (Coordinate.x == ColumnLimit) { Position = 2; return; }
        if (Coordinate.y == RowLimit) { Position = 3; return; }
        if (Coordinate.x > 0 && Coordinate.x < ColumnLimit && Coordinate.y > 0 && Coordinate.y < RowLimit) { Position = -1; return; }
    }

    // Resets starting state according to the new position Hex is at
    public void ResetStates()
    {
        State = CalculateState();
    }

    // Changes the color of the selected hexes and marks them as selected
    public void Highlight(bool IsHighlighting)
    {
        Color DefaultColor = HandlerRef.ColorChoices[ColorID];
        if (IsHighlighting)
        {
            GetComponent<SpriteRenderer>().material.color = new Color(DefaultColor.r*1.4f,DefaultColor.g*1.4f,DefaultColor.b*1.4f);
            //GetComponent<SpriteRenderer>().material.EnableKeyword("_EMISSION");
            bIsHighlighted = true;
            if (State + 1 != 6 || (Position == 2 && State +1 !=7) || Position == 6 )
            {
                State = ClampStateAccordingToPosition(Position, State);
            }
            else
            {
                State = 0;
            }
        }
        else
        {
            GetComponent<SpriteRenderer>().material.color = DefaultColor;
            ResetStates();
            bIsHighlighted = false;
        }
    }

    // Resets the State to needed state so it skips the unnecessary select process while selecting walls / corners
    public int ClampStateAccordingToPosition(int Position, int CurrentState)
    {
        switch (Position)
        {
            case 0:
                {
                    if (CurrentState != 2)
                    { return CurrentState + 1; }
                    else
                    { return 0; }
                }
            case 1:
                {
                    if (Coordinate.x % 2 == 0)
                    {
                        if (CurrentState != 3)
                        { return CurrentState + 1; }
                        else
                        { return 2; }
                    }
                    else
                    {
                        if (CurrentState != 4)
                        { return CurrentState + 1; }
                        else
                        { return 1; }
                    }
                }
            case 2:
                {
                    if (CurrentState != 5)
                    { return CurrentState + 1; }
                    else
                    { return 3; }
                }
            case 3:
                {
                    if (Coordinate.x % 2 == 0)
                    {
                        if (CurrentState != 1)
                        { return CurrentState + 1; }
                        else
                        { return 4; }
                    }
                    else
                    {
                        if (CurrentState != 0)
                        { return CurrentState + 1; }
                        else
                        { return 5; }
                    }
                }
            case 4:
                {
                    return CalculateState();
                }
            case 5:
                {
                    if (Coordinate.x % 2 == 0)
                    { return 3; }
                    else
                    {
                        if (CurrentState != 4)
                        { return CurrentState + 1; }
                        else
                        { return 3; }
                    }
                }
            case 6:
                {
                    if (Coordinate.x % 2 == 0)
                    {
                        if (CurrentState != 5)
                        { return CurrentState + 1; }
                        else
                        { return 4; }
                    }
                    else
                    { return 5; }
                }
            case 7:
                {
                    if (CurrentState != 1)
                    { return CurrentState + 1; }
                    else
                    { return 0; }
                }
            default: return CurrentState + 1;
        }
    }
    // Activate text on the hex to visualize it for the player and set TurnLeft to start countdown
    public void SpawnBombAndActivate(int BombCountDown)
    {
        BombTextRef = gameObject.transform.GetChild(0).transform.GetChild(0).gameObject;
        BombTextRef.SetActive(true);
        BombTextRef.GetComponent<TextMeshProUGUI>().text = BombCountDown.ToString();
        TurnLeft = BombCountDown;
    }
    
    public void DecrementBombTurnLeft()
    {
        TurnLeft -= 1;
        BombTextRef.GetComponent<TextMeshProUGUI>().text = TurnLeft.ToString();
        if (TurnLeft <= 0)
        {
            StartCoroutine(GameOverCaller());
        }
    }

    // Used small delay, so it has a little time to explode if collides with newly spawned or dropped gems
    IEnumerator GameOverCaller()
    {
        yield return new WaitForSeconds(2f);
        HandlerRef.GameOverCaller();
    }

}
