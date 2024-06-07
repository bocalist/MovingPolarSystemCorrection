using Godot;
using System;
using System.Collections.Generic;

public partial class Main : Node
{
    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  VARIABLES

    // Utilities
    private Vector2 windowsSize;
    private Vector2 middlePoint = Vector2.Zero;
    private double timeOnInit;

    // BALL PROPERTIES
    // Creation Type
    internal enum BallCreationType
    {
        CERCLE,
        LINE
    }

    // Movement Type
    internal enum BallMovementType
    {
        STATIC,
        SINUS
    }

    // Utilities
    [Export] private PackedScene ballFactory;

    // Properties
    [Export(PropertyHint.Range, "1,10000,1")] private uint ballCount = 5;
    [Export] private float ballDistance = 200;
    [Export(PropertyHint.Range, "0,1,0.001")] private float ballRatio = 1;
    [Export] private float ballOffsetMax = 1;
    [Export] private float ballScale = 1f;

    // Ball Creation
    [Export] private BallCreationType ballCreationType = BallCreationType.CERCLE;
    [Export] private bool isBallsVisible = true;
    private List<Action> allBallCreationType;
    private Action CreateBallAction;

    // Line Properties
    [Export] private Color lineColor = new Color("dc61ff");
    [Export] private bool isLineVisible = false;
    [Export] private bool isLineLooping = true;
    private Line2D line = new Line2D();

    // Movements
    [Export] private BallMovementType ballMovementType = BallMovementType.STATIC;
    private List<Action> allBallMovementType;
    private Action MoveBallAction;

    private List<BallMotion> allBallsMotion = new List<BallMotion>();

    internal class BallMotion
    {
        internal Node2D ball;
        internal Vector2 refPosition;
        internal Vector2 direction;
        internal float rotation;
        internal float offset;
        internal float distanceMax;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Init

    public override void _Ready()

    {
        SetUtilities();
        SetActions();
        CreateBallAction();
        CreateLine();
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Update

    public override void _Process(float delta)
    {
        MoveBallAction();
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Ball Creation

    private void CreateBallInCercle()
    {
        // Init
        Vector2 lCurrentBallPosition = Vector2.Zero;
        Vector2 lCurrentBallDirection;
        float lCurrentBallRotation;
        float lCurrentBallRatio;
        float lAngle = Mathf.Deg2Rad(360f) * ballRatio;
        float lAngleCorrection = Mathf.Deg2Rad(-90f);
        float lOffset = Mathf.Pi * ballOffsetMax;

        // Create All Balls
        for (int lCurrentBallIndex = 0; lCurrentBallIndex < ballCount; lCurrentBallIndex++)
        {
            // Init
            lCurrentBallRatio = (float)lCurrentBallIndex / (float)ballCount;
            lCurrentBallRotation = lAngle * lCurrentBallRatio;
            lCurrentBallPosition.x = Mathf.Cos(lCurrentBallRotation);
            lCurrentBallPosition.y = Mathf.Sin(lCurrentBallRotation);
            lCurrentBallPosition = lCurrentBallPosition * ballDistance;
            lCurrentBallDirection = Vector2.Right.Rotated(lCurrentBallRotation);

            CreateBall(lCurrentBallIndex, lCurrentBallRatio, lCurrentBallPosition, middlePoint, lCurrentBallDirection);
        }

        if (ballCount > 1 && isLineVisible && isLineLooping)
        {
            // Duplicate The First One To Loop When In Line
            allBallsMotion.Add(allBallsMotion[0]);
            ballCount++;
        }
    }

    private void CreateBallInLine()
    {
        // Init
        Vector2 lCurrentBallPosition;
        Vector2 lCurrentBallRefPosition;
        float lCurrentBallRatio;

        // Create All Balls
        for (int lCurrentBallIndex = 0; lCurrentBallIndex < ballCount; lCurrentBallIndex++)
        {
            // Init
            lCurrentBallRatio = (float)lCurrentBallIndex / (float)(ballCount - 1) - .5f;
            lCurrentBallPosition = Vector2.Right * windowsSize * lCurrentBallRatio * ballRatio;
            lCurrentBallRefPosition = lCurrentBallPosition + middlePoint;
            GD.Print(lCurrentBallPosition);

            // Add To The Scene
            CreateBall(lCurrentBallIndex, lCurrentBallRatio, lCurrentBallPosition, lCurrentBallRefPosition, Vector2.Up);

        }
    }

    private void CreateBall(int pIndex, float pRatio, Vector2 pPosition, Vector2 pRefPosition, Vector2 pDirection)
    {
        // Init
        BallMotion lCurrentBallMotion = new BallMotion();
        lCurrentBallMotion.ball = (Node2D)ballFactory.Instance();

        // Add To The Scene
        allBallsMotion.Add(lCurrentBallMotion);
        AddChild(lCurrentBallMotion.ball);

        // Set Properties
        // Ball
        lCurrentBallMotion.ball.Name = "Position_" + (pIndex + 1);
        lCurrentBallMotion.ball.Position = pPosition + middlePoint;
        lCurrentBallMotion.ball.Scale = Vector2.One * ballScale;
        lCurrentBallMotion.ball.Visible = isBallsVisible;
        // Motion
        lCurrentBallMotion.refPosition = pRefPosition;
        lCurrentBallMotion.direction = pDirection;
        lCurrentBallMotion.offset = pRatio * Mathf.Pi * ballOffsetMax;
        lCurrentBallMotion.distanceMax = ballDistance;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# // Line

    private void CreateLine()
    {
        AddChild(line);
        line.DefaultColor = lineColor;
    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Ball Movements

    private void BallMovementStatic() { }

    private void BallMovementSinus()
    {
        // Init
        float lCurrentTime = GetCurrentTime();

        if (isLineVisible) line.ClearPoints();

        foreach (BallMotion lCurrentBallMotion in allBallsMotion)
        {
            lCurrentBallMotion.ball.Position = lCurrentBallMotion.refPosition
                                             + lCurrentBallMotion.direction
                                             * lCurrentBallMotion.distanceMax * Mathf.Sin(lCurrentTime + lCurrentBallMotion.offset);
            if (isLineVisible) line.AddPoint(lCurrentBallMotion.ball.Position);
        }

    }

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Time

    private float GetCurrentTime() => (float)(Time.GetUnixTimeFromSystem() - timeOnInit);

    // ----------------~~~~~~~~~~~~~~~~~~~==========================# //  Set Properties

    private void SetUtilities()
    {
        windowsSize = GetViewport().GetVisibleRect().Size;
        middlePoint = windowsSize * .5f;
        timeOnInit = Time.GetUnixTimeFromSystem();
    }

    private void SetActions()
    {
        // Init Creation Type
        allBallCreationType = new List<Action>()
        {
            CreateBallInCercle,
            CreateBallInLine
        };

        // Init Movement Type
        allBallMovementType = new List<Action>()
        {
            BallMovementStatic,
            BallMovementSinus
        };

        // Set Ball Creation And Movement Type
        CreateBallAction = allBallCreationType[(int)ballCreationType];
        MoveBallAction = allBallMovementType[(int)ballMovementType];

    }

}
