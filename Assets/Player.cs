using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Responsible for game logic
    public GameHandler HandlerRef;
    public Text PointsTXTRef;
    public Text TurnsTXTRef;
    public AudioSource PlayerAudioSource;

    public int Points = 0;
    public int BombSpawnTarget = 0;
    public int Turn = 0;
    // If dragged doesnt select a new hex group
    public bool bIsDragged = false;
    // Used to hold information on ButtonDown/ ButtonUp
    private Vector2 InitialMousePosition;
    private Vector2 EndMousePosition;

    void Start()
    {
        UpdatePoint();
        UpdateTurnCount();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LeftMouseHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            LeftMouseUp();
        }
    }

    public void UpdatePoint()
    {
        PointsTXTRef.text = "Points : " + Points;
    }

    public void UpdateTurnCount()
    {
        TurnsTXTRef.text = "Turn : " + Turn;
    }

    // Get and Set the InitialMousePosition for comparison when let go and set dragged to false
    private void LeftMouseHold()
    {
        InitialMousePosition = Input.mousePosition;
        bIsDragged = false;
    }

    // Compare Initial Position to End Position for determining if dragged Right or Left then Turn the hexes (Doesnt work if dragged so little)
    private void LeftMouseUp()
    {
        EndMousePosition = Input.mousePosition;

        var SlideRef = EndMousePosition - InitialMousePosition;
        // Preventing small draggings
        if (SlideRef.magnitude <= 50) { return; }
        bIsDragged = true;
        if (EndMousePosition.x > InitialMousePosition.x)
        {
            // Turn Right
            HandlerRef.TurnInput(true);
        }
        else
        {
            // Turn Left
            HandlerRef.TurnInput(false);
        }

    }
}
