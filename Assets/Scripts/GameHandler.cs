using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    public Player PlayerRef;
    // Prefab to use as hexagon
    public GameObject HexagonRef;
    // Sounds
    public AudioClip ExplosionSound;
    // Grid Size
    public int ColumnCount = 8;
    public int RowCount = 9;
    // Point per Gem
    public int PointsPerExplosion = 5;
    // Spawn Bomb Every
    public int BombSpawnRate = 250;
    public Color[] ColorChoices;
    // Grid responsible for all the hexagons active in game
    private GameObject[,] HexReferences;
    // Used for preventing inputs during turn or explosion
    public bool bIsTurning = false;
    // Grid for the Transform Positions
    private Vector3[,] HexGridPositions;
    // Array for Current Selected hexagons (3)
    private GameObject[] SelectedHexes;

    // Formulas holding the states, there is two because grid is not a perfectly lined so it has to adapt
    private Vector2Int[] HighlightPossibilities_High = new Vector2Int[]
    {
        new Vector2Int(0,-1),
        new Vector2Int(1,-1),
        new Vector2Int(1,0),
        new Vector2Int(0,1),
        new Vector2Int(-1,0),
        new Vector2Int(-1,-1),
        new Vector2Int(0,-1),
    };
    private Vector2Int[] HighlightPossibilities_Low = new Vector2Int[]
    {
        new Vector2Int(0,-1),
        new Vector2Int(1,0),
        new Vector2Int(1,1),
        new Vector2Int(0,1),
        new Vector2Int(-1,1),
        new Vector2Int(-1,0),
        new Vector2Int(0,-1)
    };

    // Position offsets while creating the grid for the first time 
    private float YDifference = .335f;
    private float XDifference = .577f;
    private float NewRowDifference = -.68f;
    private float FirstColumnX = -2.342128f;

    void Start()
    {
        HexGridPositions = new Vector3[ColumnCount, RowCount];
        HexReferences = new GameObject[ColumnCount, RowCount];
        SelectedHexes = new GameObject[3];
        SelectedHexes[0] = null;
        SelectedHexes[1] = null;
        SelectedHexes[2] = null;
        CreateGrid();
        PlayerRef.PlayerAudioSource.clip = ExplosionSound;
    }
    // Look for Debug On PC
    void Update()
    {
        // DEBUG Open to activate manual controls with Left and middle click for turning also for game over as well
        //if (Input.GetMouseButtonDown(1))
        //{
        //    TurnInput(true);
        //}
        //if (Input.GetMouseButtonDown(2))
        //{
        //    TurnInput(false);
        //}
        //// Game Over for Debugging
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    StartCoroutine(GameOver());
        //}
    }

    // Responsible for turning selected hexes Clockwise / CounterClockwise
    public void TurnInput(bool bIsRight)
    {
        if (bIsRight)
        {
            if (!bIsTurning && SelectedHexes[0] != null)
            {
                bIsTurning = true;
                // Turn Three Times to the RIGHT (back to initial if no match found)
                for (int i = 1; i <= 3; i++)
                {
                    if (i == 3)
                    {
                        StartCoroutine(TurnSelectedHexes(.2f * i, true, true));
                    }
                    else
                    {
                        StartCoroutine(TurnSelectedHexes(.2f * i, false, true));
                    }
                }
            }
        }
        else
        {
            if (!bIsTurning && SelectedHexes[0] != null)
            {
                bIsTurning = true;
                // Turn Three Times to the LEFT (back to initial if no match found)
                for (int i = 1; i <= 3; i++)
                {
                    if (i == 3)
                    {
                        StartCoroutine(TurnSelectedHexes(.2f * i, true, false));
                    }
                    else
                    {
                        StartCoroutine(TurnSelectedHexes(.2f * i, false, false));
                    }
                }
            }
        }
    }

    // Gets Called to create the initial grid
    void CreateGrid()
    {
        int c;
        int r;
        Vector3 CurrentCoordinate = new Vector3(-2.342128f, 2.659f, 0f);
        for (r = 0; r < RowCount; r++)
        {
            for (c = 0; c < ColumnCount; c++)
            {
                // If it should be placed higher...
                if (c % 2 == 0)
                {
                    if (c != 0) { CurrentCoordinate.y += YDifference; }
                    HexGridPositions[c, r] = CurrentCoordinate;
                    SpawnHexagon(c, r);
                }
                // or lower
                else
                {
                    if (c != 0) { CurrentCoordinate.y += -YDifference; }
                    HexGridPositions[c, r] = CurrentCoordinate;
                    SpawnHexagon(c, r);
                }
                CurrentCoordinate.x += XDifference;
            }
            // Increment CurrentCoordinate.y to make it ready for the next row
            if (c % 2 != 0)
            {
                CurrentCoordinate.y += NewRowDifference;
            }
            else
            {
                CurrentCoordinate.y += NewRowDifference + YDifference;
            }
            CurrentCoordinate.x = FirstColumnX;
        }
        SetCameraPositionAccordingToGridSize();
    }

    // Set Camera Position to adapt with the grid size
    private void SetCameraPositionAccordingToGridSize()
    {
        if (ColumnCount % 2 == 0)
        {
            PlayerRef.gameObject.transform.position = new Vector3(HexGridPositions[ColumnCount / 2 , RowCount / 2 ].x - XDifference / 2, HexGridPositions[ColumnCount / 2 , RowCount / 2].y, -((ColumnCount+RowCount) /2 +1) );
        }
        else
        {
            PlayerRef.gameObject.transform.position = new Vector3(HexGridPositions[ColumnCount / 2 , RowCount / 2 ].x, HexGridPositions[ColumnCount / 2 , RowCount / 2].y, -((ColumnCount+RowCount) /2 +1) );
        }
    }

    // Creates a Hex and sets its values according to its position and coordinate
    public void SpawnHexagon(int c, int r)
    {
        GameObject SpawnedHexagon_GO = Instantiate(HexagonRef, HexGridPositions[c, r], Quaternion.identity);
        HexagonScript HexagonScriptRef = SpawnedHexagon_GO.GetComponent<HexagonScript>();
        // Spawn a hex if its greater than the rate and reset it back to 0
        if (PlayerRef.BombSpawnTarget >= BombSpawnRate) { PlayerRef.BombSpawnTarget = 0; HexagonScriptRef.Type = 1; }
        HexagonScriptRef.Coordinate = new Vector2Int(c, r);
        HexagonScriptRef.CalculatePosition();
        HexagonScriptRef.ColorID = Random.Range(0, ColorChoices.Length);
        SpawnedHexagon_GO.GetComponent<SpriteRenderer>().material.color = ColorChoices[HexagonScriptRef.ColorID];
        HexReferences[c, r] = SpawnedHexagon_GO;
    }

    // Switch Positions of 3 Selected hexes programmatically
    public void SwitchPositions(Vector2Int Coord1, Vector2Int Coord2, Vector2Int Coord3, bool bIsRight)
    {
        // Get the Script references of hexagons according to their coordinate on the HexReferences Grid
        HexagonScript Hex0 = HexReferences[Coord1.x, Coord1.y].GetComponent<HexagonScript>();
        HexagonScript Hex1 = HexReferences[Coord2.x, Coord2.y].GetComponent<HexagonScript>();
        HexagonScript Hex2 = HexReferences[Coord3.x, Coord3.y].GetComponent<HexagonScript>();
        if (bIsRight)
        {
            // Switch out local Coordinates
            var TempCoord0 = Hex0.Coordinate;
            Hex0.Coordinate = Hex1.Coordinate;
            Hex1.Coordinate = Hex2.Coordinate;
            Hex2.Coordinate = TempCoord0;
            // Switch out Positions so it wont be bugged out if nearby walls
            var TempPos0 = Hex0.Position;
            Hex0.Position = Hex1.Position;
            Hex1.Position = Hex2.Position;
            Hex2.Position = TempPos0;
            // Switch out States so it wont be buggy if a match is made and its still highlighting // TODO :: Might Remove it after automated switch system
            var TempState0 = Hex0.State;
            Hex0.State = Hex1.State;
            Hex1.State = Hex2.State;
            Hex2.State = TempState0;
            // Switch out the references of the objects on the HexReferences grid to match with the newly placed coordinate
            var TempHex0 = HexReferences[Coord1.x, Coord1.y];
            var TempHex1 = HexReferences[Coord2.x, Coord2.y];
            HexReferences[Coord1.x, Coord1.y] = HexReferences[Coord3.x, Coord3.y];
            HexReferences[Coord2.x, Coord2.y] = TempHex0;
            HexReferences[Coord3.x, Coord3.y] = TempHex1;
            // Switch out the current Highlighted Hexes order so it remembers where it was last time // P.S [0] is the clicked hex and when it turns it stays same so we prevent it here
            SelectedHexes[0] = HexReferences[Coord1.x, Coord1.y];
            SelectedHexes[1] = TempHex0;
            SelectedHexes[2] = TempHex1;
        }
        else
        {
            // Switch out local Coordinates
            var TempCoord0 = Hex0.Coordinate;
            Hex0.Coordinate = Hex2.Coordinate;
            Hex2.Coordinate = Hex1.Coordinate;
            Hex1.Coordinate = TempCoord0;
            // Switch out Positions so it wont be bugged out if nearby walls
            var TempPos0 = Hex0.Position;
            Hex0.Position = Hex2.Position;
            Hex2.Position = Hex1.Position;
            Hex1.Position = TempPos0;
            // Switch out States so it wont be buggy if a match is made and its still highlighting // TODO :: Might Remove it after automated switch system
            var TempState0 = Hex0.State;
            Hex0.State = Hex2.State;
            Hex2.State = Hex1.State;
            Hex1.State = TempState0;
            // Switch out the references of the objects on the HexReferences grid to match with the newly placed coordinate
            var TempHex1 = HexReferences[Coord1.x, Coord1.y];
            var TempHex2 = HexReferences[Coord2.x, Coord2.y];
            var TempHex3 = HexReferences[Coord3.x, Coord3.y];
            HexReferences[Coord1.x, Coord1.y] = TempHex2;
            HexReferences[Coord3.x, Coord3.y] = TempHex1;
            HexReferences[Coord2.x, Coord2.y] = TempHex3;
            // Switch out the current Highlighted Hexes order so it remembers where it was last time // P.S [0] is the clicked hex and when it turns it stays same so we prevent it here
            SelectedHexes[0] = TempHex2;
            SelectedHexes[2] = TempHex1;
            SelectedHexes[1] = TempHex3;
        }

        for (int k = 0; k <= 2; k++)
        {
            SelectedHexes[k].GetComponent<HexagonScript>().ResetStates();
        }
    }

    // Gets called to highlight selected hexes
    public void HighlightHexagons(Vector2Int CoordinateRef, int StateRef)
    {
        StopHighlightingAll();
        Vector2Int[] HighlightPossibilities = CoordinateRef.x % 2 == 0 ? HighlightPossibilities_High : HighlightPossibilities_Low;
        HexReferences[CoordinateRef.x, CoordinateRef.y].GetComponent<HexagonScript>().Highlight(true);
        HexReferences[CoordinateRef.x + HighlightPossibilities[StateRef].x, CoordinateRef.y + HighlightPossibilities[StateRef].y].GetComponent<HexagonScript>().Highlight(true);
        HexReferences[CoordinateRef.x + HighlightPossibilities[StateRef + 1].x, CoordinateRef.y + HighlightPossibilities[StateRef + 1].y].GetComponent<HexagonScript>().Highlight(true);

        SelectedHexes[0] = HexReferences[CoordinateRef.x, CoordinateRef.y];
        SelectedHexes[1] = HexReferences[CoordinateRef.x + HighlightPossibilities[StateRef].x, CoordinateRef.y + HighlightPossibilities[StateRef].y];
        SelectedHexes[2] = HexReferences[CoordinateRef.x + HighlightPossibilities[StateRef + 1].x, CoordinateRef.y + HighlightPossibilities[StateRef + 1].y];
    }

    // Gets called everytime player selects a different hex to highlight for
    public void StopHighlightingAll()
    {
        HexagonScript[] AllHexagons;
        AllHexagons = FindObjectsOfType<HexagonScript>();
        SelectedHexes[0] = null;
        SelectedHexes[1] = null;
        SelectedHexes[2] = null;
        foreach (var Hexagon in AllHexagons)
        {
            Hexagon.Highlight(false);
        }
    }

    IEnumerator TurnSelectedHexes(float Delay, bool bIsLastTurn, bool bIsRight)
    {
        yield return new WaitForSeconds(Delay);
        Vector2Int[] CoordinatesToCheck = new Vector2Int[3];
        Vector3[] SelectedHexesPositions = new Vector3[3];
        // Getting Coordinates to check for every possible way, which is 3 times - // TODO Might Remove : Not necessary but easier to read
        for (int i = 0; i <= 2; i++)
        {
            try
            {
                SelectedHexesPositions[i] = SelectedHexes[i].transform.position;
                CoordinatesToCheck[i] = SelectedHexes[i].GetComponent<HexagonScript>().Coordinate;
            } catch (System.NullReferenceException) { }

        }
        // Switch Positions of the hexes visually
        if (bIsRight)
        {
            SelectedHexes[0].transform.position = SelectedHexesPositions[1];
            SelectedHexes[1].transform.position = SelectedHexesPositions[2];
            SelectedHexes[2].transform.position = SelectedHexesPositions[0];
        }
        else
        {
            SelectedHexes[0].transform.position = SelectedHexesPositions[2];
            SelectedHexes[1].transform.position = SelectedHexesPositions[0];
            SelectedHexes[2].transform.position = SelectedHexesPositions[1];
        }
        // Switch Positions of the hexes programmatically
        SwitchPositions(CoordinatesToCheck[0], CoordinatesToCheck[1], CoordinatesToCheck[2], bIsRight);
        // Checking every highlighted hex for a color match
        foreach (var HexToCheck in SelectedHexes)
        {
            HexagonScript SelectedScriptRef = HexToCheck.GetComponent<HexagonScript>();
            // If a match is found stop the search for the next turn
            if (LookForMatch(SelectedScriptRef, bIsRight)) break;
        }
        if (bIsLastTurn) { bIsTurning = false; }
    }

    public bool LookForMatch(HexagonScript SelectedScriptRef, bool bIsRight)
    {
        // Check all 6 blocks nearby of the incoming hex parameter for a match
        if (bIsRight)
        {
            return FilterResult(SelectedScriptRef, 0, 1);
        }
        else
        {
            return FilterResult(SelectedScriptRef, -1, 0);
        }
    }

    // Used to make it better to read ^^
    public bool FilterResult(HexagonScript SelectedScriptRef, int FirstHexDifference, int SecondHexDifference)
    {
        for (int StateRef = 0; StateRef <= 5; StateRef++)
        {
            try
            {
                // Set the Possiblity formula according to if a column is high or not
                bool bIsHighColumn = SelectedScriptRef.Coordinate.x % 2 == 0;
                Vector2Int[] Possibilities = bIsHighColumn ? HighlightPossibilities_High : HighlightPossibilities_Low;

                // Get the first and second hex reference nearby of the incoming hex parameter
                HexagonScript FirstHexNearby = HexReferences[SelectedScriptRef.Coordinate.x + Possibilities[StateRef + FirstHexDifference].x, SelectedScriptRef.Coordinate.y + Possibilities[StateRef + FirstHexDifference].y].GetComponent<HexagonScript>();
                HexagonScript SecondHexNearby = HexReferences[SelectedScriptRef.Coordinate.x + Possibilities[StateRef + SecondHexDifference].x, SelectedScriptRef.Coordinate.y + Possibilities[StateRef + SecondHexDifference].y].GetComponent<HexagonScript>();
                // If a match is made by 3 of the same colours
                if (SelectedScriptRef.ColorID == FirstHexNearby.ColorID && SelectedScriptRef.ColorID == SecondHexNearby.ColorID && FirstHexNearby.ColorID == SecondHexNearby.ColorID)
                {
                    // Prevent checking already checked hexes
                    if (FirstHexNearby == SelectedScriptRef || SecondHexNearby == SelectedScriptRef || FirstHexNearby == SecondHexNearby) { continue; }
                    bIsTurning = true;
                    PlayerRef.Turn++;
                    PlayerRef.UpdateTurnCount();
                    DecrementAllBombTimers();
                    // Destroy the matched hexes and return true so it stops checking for more
                    ExplodeMatch(SelectedScriptRef, FirstHexNearby, SecondHexNearby);
                    return true;
                }
            }
            // Catch if State is out of array
            catch (System.IndexOutOfRangeException)
            {
                continue;
            }
        }
        // If no match is made return false so it keeps on looping for the next hex
        return false;
    }

    // Gets called after every turn to decrement every bomb on the grid
    public void DecrementAllBombTimers()
    {
        HexagonScript[] AllHexagons;
        AllHexagons = FindObjectsOfType<HexagonScript>();
        foreach (var Hexagon in AllHexagons)
        {
            if (Hexagon.Type == 1)
            {
                Hexagon.DecrementBombTurnLeft();
            }
        }
    }

    // Checks all surroundings of a given hex if there is a match and explode it
    public bool LookForMatchSingleHex(HexagonScript HexToCheck)
    {
        for (int StateRef = 0; StateRef <= 5; StateRef++)
        {
            try
            {
                bool bIsHighColumn = HexToCheck.Coordinate.x % 2 == 0;
                Vector2Int[] Possibilities = bIsHighColumn ? HighlightPossibilities_High : HighlightPossibilities_Low;
                HexagonScript FirstHexNearby = HexReferences[HexToCheck.Coordinate.x + Possibilities[StateRef].x, HexToCheck.Coordinate.y + Possibilities[StateRef].y].GetComponent<HexagonScript>();
                HexagonScript SecondHexNearby = HexReferences[HexToCheck.Coordinate.x + Possibilities[StateRef + 1].x, HexToCheck.Coordinate.y + Possibilities[StateRef + 1].y].GetComponent<HexagonScript>();
                if (HexToCheck.ColorID == FirstHexNearby.ColorID && HexToCheck.ColorID == SecondHexNearby.ColorID && FirstHexNearby.ColorID == SecondHexNearby.ColorID)
                {
                    // Prevent checking already checked hexes
                    if (FirstHexNearby == HexToCheck || SecondHexNearby == HexToCheck || FirstHexNearby == SecondHexNearby) { continue; }
                    ExplodeMatch(HexToCheck, FirstHexNearby, SecondHexNearby);
                    return true;
                }
            }
            catch (System.IndexOutOfRangeException) { continue; }
        }
        return false;
    }

    // Explode the match and give points to player then UnHighlight everyhex
    public void ExplodeMatch(HexagonScript FirstHex, HexagonScript SecondHex, HexagonScript ThirdHex)
    {
        HexReferences[FirstHex.Coordinate.x, FirstHex.Coordinate.y] = null;
        HexReferences[SecondHex.Coordinate.x, SecondHex.Coordinate.y] = null;
        HexReferences[ThirdHex.Coordinate.x, ThirdHex.Coordinate.y] = null;
        Vector2Int[] CoordinateReferences = new Vector2Int[] { FirstHex.Coordinate, SecondHex.Coordinate, ThirdHex.Coordinate };

        Destroy(FirstHex.gameObject, .1f);
        Destroy(SecondHex.gameObject, .2f);
        Destroy(ThirdHex.gameObject,.3f);

        PlayerRef.Points += PointsPerExplosion*3;
        PlayerRef.BombSpawnTarget += PointsPerExplosion *3;
        PlayerRef.UpdatePoint();

        StopHighlightingAll();
        StartCoroutine(RecalculateCoordinates(.4f));
        StartCoroutine(SpawnNewHexagons(.7f));
    }

    // To access GameOver outside this class
    public void GameOverCaller()
    {
        StartCoroutine(GameOver());
    }

    // Lower the hanging Hexes
    IEnumerator RecalculateCoordinates(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        bIsTurning = true;
        for (int c = 0; c <= ColumnCount; c++)
        {
            for (int r = RowCount - 1; r >= 0; r--)
            {
                try
                {
                    if (HexReferences[c, r - 1] == null && HexReferences[c, r - 2] == null)
                    {
                        for (int i = r - 3; i >= 0; i--)
                        {
                            SortColumnToFillEmpty(c, i, 2);
                        }
                    }
                    else
                    if (HexReferences[c, r - 1] == null)
                    {
                        for (int i = r - 1; i >= 0; i--)
                        {
                            SortColumnToFillEmpty(c,i, 1);
                        }
                    }
                }
                catch (System.IndexOutOfRangeException) { continue; }
            }
        }
        /* Holdon to it incase the bug where the bottom row gets bugged out again ?
        for (int k = 0; k < ColumnCount; k++)
        {
            if (HexReferences[k, RowCount - 1] == null)
            {
                for (int b = RowCount - 2; b >= 0; b--)
                {
                    try
                    {
                        HexReferences[k, b].GetComponent<HexagonScript>().Coordinate = new Vector2Int(k, b + 1);
                        HexReferences[k, b].transform.position = HexGridPositions[k, b + 1];
                        HexReferences[k, b + 1] = HexReferences[k, b];
                        HexReferences[k, b + 1].GetComponent<HexagonScript>().CalculatePosition();
                        HexReferences[k, b + 1].GetComponent<HexagonScript>().CalculateState();
                        HexReferences[k, b] = null;
                    }
                    catch (System.IndexOutOfRangeException) { continue; }
                }
            }
        }
        */
        StartCoroutine(RecalculateMatches(Delay + .4f));
        yield return new WaitForSeconds(.2f);
        bIsTurning = false;
    }

    // Used to make it better to read ^^
    private void SortColumnToFillEmpty(int Column, int Row, int GapCount)
    {
        try
        {
            HexReferences[Column, Row].GetComponent<HexagonScript>().Coordinate = new Vector2Int(Column, Row + GapCount);
            HexReferences[Column, Row].transform.position = HexGridPositions[Column, Row + GapCount];
            HexReferences[Column, Row + GapCount] = HexReferences[Column, Row];
            HexReferences[Column, Row] = null;
            HexReferences[Column, Row + GapCount].GetComponent<HexagonScript>().CalculatePosition();
            HexReferences[Column, Row + GapCount].GetComponent<HexagonScript>().CalculateState();
        }
        catch (System.NullReferenceException)
        {
            //continue;
        }
    }

    // Checks if there is a match not made by player
    IEnumerator RecalculateMatches(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        for (int c = 0; c < ColumnCount; c++)
        {
            for (int r = RowCount - 1; r >= 0; r--)
            {
                if (LookForMatchSingleHex(HexReferences[c, r].GetComponent<HexagonScript>())) { bIsTurning = true; yield break; }
            }
        }
        bIsTurning = false;
    }

    // Spawns Hexagons on empty spots to fill the grid
    IEnumerator SpawnNewHexagons(float Delay)
    {
        yield return new WaitForSeconds(Delay);
        for (int c = 0; c < ColumnCount; c++)
        {
            for (int r = 0; r < RowCount; r++)
            {
                if (HexReferences[c, r] == null)
                {
                    SpawnHexagon(c,r);
                }
            }
        }
        //bIsTurning = false;
    }

    //Explode every hex and restart the level
    IEnumerator GameOver()
    {
        for (int r = 0; r <RowCount ; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                yield return new WaitForSeconds(.04f);
                Destroy(HexReferences[c,r]);
            }
        }
        Application.LoadLevel(Application.loadedLevel);
    }
}
