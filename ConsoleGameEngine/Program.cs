﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using RetroEngine;

namespace RetroEngine
{
    public enum CoordinateSystemType { TopLeft, BottomLeft, Middle, TopRight, BottomRight, }

    public class GameObject
    {
        public ASCIISprite sprite { get; set; }
        public Transform transform { get; set; }
        public Events events { get; }
        public string name { get; set; }
        public int? identifier { get; private set; }
        public bool activeSelf { get; private set; }

        public GameObject()
        {
            sprite = new ASCIISprite();
            transform = new Transform();
            events = new Events();
            name = "gameobject";
            identifier = null;
            activeSelf = true;
        }
        public GameObject(ASCIISprite sprite)
        {
            this.sprite = sprite;
            transform = new Transform();
            events = new Events();
            name = "gameobject";
            identifier = null;
            activeSelf = true;
        }
        public GameObject(Transform transform)
        {
            this.transform = transform;
            sprite = new ASCIISprite();
            events = new Events();
            name = "gameobject";
            identifier = null;
            activeSelf = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        public GameObject(GameObject gameObject)
        {
            this.sprite = gameObject.sprite;
            this.transform = gameObject.transform;
            this.events = gameObject.events;
            this.name = gameObject.name;
            this.activeSelf = gameObject.activeSelf;
        }

        /// <summary>
        /// Gets the instantiated GameObject.
        /// </summary>
        /// <returns>Already instantiated GameObject</returns>
        public GameObject Get()
        {
            if (identifier == null || (int)identifier <= -1)
            {
                return null;
            }

            if (identifier > Game.Objects.Count - 1)
            {
                throw new Exceptions.GameObjectNotInstantiatedException();
            }

            //System.Diagnostics.Debug.WriteLine("ID: " + identifier);
            //System.Diagnostics.Debug.WriteLine("Objects: " + Game.Objects);

            return Game.Objects[(int)identifier];
        }
        
        /// <summary>
        /// Returns clone of current object.
        /// </summary>
        /// <returns>Cloned GameObject</returns>
        public GameObject Clone() => new GameObject(this);

        /// <summary>
        /// Replaces the instantiated GameObject with this.
        /// </summary>
        public void Update()
        {
            if (identifier == null)
            {
                return;
            }

            Game.Objects[(int)identifier] = this;
        }

        /// <summary>
        /// Destroys GameObject, optionally after delay.
        /// </summary>
        /// <param name="delay">Destroys GameObject after delay</param>
        public void Destroy(float delay = 0f)
        {
            if (delay <= 0)
            {
                Game.Objects[(int)this.identifier] = null;
            }
            else
            {
                Task.Run(async () =>
                {
                    await Task.Delay((int)(delay * 1000));
                    Game.Objects[(int)this.identifier] = null;
                });
            }
        }

        public void SetActive(bool value)
        {
            activeSelf = value;
        }

        /// <summary>
        /// Destroys GameObject specified.
        /// </summary>
        /// <param name="obj">The GameObject to destroy</param>
        /// <param name="delay">Time before GameObject is destroyed.</param>
        public static void Destroy(GameObject obj, float delay = 0f)
        {
            if (delay <= 0)
            {
                Game.Objects[(int)obj.identifier] = null;
            }
            else
            {
                Task.Run(async () =>
                {
                    await Task.Delay((int)(delay * 1000));
                    Game.Objects[(int)obj.identifier] = null;
                });
            }
        }

        /// <summary>
        /// Instantiates GameObject.
        /// </summary>
        /// <param name="obj">The GameObject to instantiate</param>
        public static GameObject Instantiate(GameObject obj)
        {
            GameObject t = obj.Clone();
            t.identifier = Game.Objects.Count;

            Game.Objects.Add(t);
            return t;
        }
        public static GameObject Instantiate(GameObject obj, string name)
        {
            GameObject t = obj.Clone();
            t.identifier = Game.Objects.Count;
            t.name = name;

            Game.Objects.Add(t);
            return t;
        }

        /// <summary>
        /// Find GameObject by name.
        /// </summary>
        /// <param name="name"></param>
        public static GameObject Find(string name) => Game.Objects.Where(i => i.name == name).FirstOrDefault();
        
        public class Events
        {
#pragma warning disable 0649
            public Func<int> OnCollisionEnter;
            public Func<int> OnCollisionStay;
            public Func<int> OnCollisionExit;
#pragma warning restore 0649

            public bool TryGetOnCollisionEnter(out Func<int> OnCollisionEnter)
            {
                OnCollisionEnter = this.OnCollisionEnter;
                return this.OnCollisionEnter != null;
            }

            public bool TryGetOnCollisionStay(out Func<int> OnCollisionStay)
            {
                OnCollisionStay = this.OnCollisionStay;
                return this.OnCollisionStay != null;
            }

            public bool TryGetOnCollisionExit(out Func<int> OnCollisionExit)
            {
                OnCollisionExit = this.OnCollisionExit;
                return this.OnCollisionExit != null;
            }
        }
    }

    public class Transform
    {
        public Vector2 position { get; set; }
        public int z_index { get; set; }

        public Transform()
        {
            position = new Vector2(0, 0);
            z_index = 10;
        }
        public Transform(Vector2 position)
        {
            this.position = position;
            z_index = 10;
        }

        /// <summary>
        /// 'Adds' vector2 to transform position.
        /// </summary>
        public void Translate(Vector2 translation)
        {
            position += translation;
        }
    }

    public class ASCIISprite
    {
        public char[,] draw { get; set; }
        public bool[,] collision { get; set; }
        public bool solid { get; set; }

        public ASCIISprite()
        {
            this.solid = true;
        }
        public ASCIISprite(char[,] draw)
        {
            this.solid = true;
            this.draw = draw;
        }
        public ASCIISprite(char[,] draw, bool[,] collision)
        {
            this.draw = draw;
            this.collision = collision;
        }

        /// <summary>
        /// Returns width of GameObject.
        /// </summary>
        public int width() => draw.GetLength(1);
        /// <summary>
        /// Returns height of GameObject.
        /// </summary>
        public int height() => draw.GetLength(0);

        /// <summary>
        /// Generates collision based on char array.
        /// </summary>
        /// <param name="charArray">Char array of GameObject</param>
        /// <param name="excluded">Character to exclude from collision generation, defaults to {float space}(' ')</param>
        /// <returns>Generated collision</returns>
        public static bool[,] GenerateCollision(char[,] charArray, char excluded = ' ')
        {
            bool[,] Collision = new bool[charArray.GetLength(0), charArray.GetLength(1)];

            for (int y = 0; y < charArray.GetLength(0); y++)
            {
                for (int x = 0; x < charArray.GetLength(1); x++)
                {
                    if (excluded == charArray[y, x])
                    {
                        continue;
                    }

                    Collision[y, x] = true;
                }
            }
            return Collision;
        }
        /// <summary>
        /// Generates collision based on char array with array of characters to exclude from generation.
        /// </summary>
        /// <param name="charArray">Char array of GameObject</param>
        /// <param name="excluded">Multiple characters to exclude from collision generation</param>
        /// <returns>Generated collision</returns>
        public static bool[,] GenerateCollision(char[,] charArray, char[] excluded)
        {
            bool[,] Collision = new bool[charArray.GetLength(0), charArray.GetLength(1)];

            for (int y = 0; y < charArray.GetLength(0); y++)
            {
                for (int x = 0; x < charArray.GetLength(1); x++)
                {
                    if (excluded.Contains(charArray[y, x]))
                    {
                        continue;
                    }

                    Collision[y, x] = true;
                }
            }
            return Collision;
        }
    }

    /// <summary>
    /// Vector2 holds 2-dimensionel coordinate set(x and y).
    /// </summary>
    public struct Vector2 : IEquatable<Vector2>
    {
        public float x { get; set; }
        public float y { get; set; }

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        #region Readonly directions
        /// <summary>
        /// Shorthand for writing Vector2(0, 0).
        /// </summary>
        public static Vector2 zero
        {
            get
            {
                return new Vector2(0, 0);
            }
        }

        public static Vector2 right
        {
            get
            {
                return new Vector2(1, 0);
            }
        }

        public static Vector2 left
        {
            get
            {
                return new Vector2(-1, 0);
            }
        }

        public static Vector2 up
        {
            get
            {
                return new Vector2(0, -1);
            }
        }

        public static Vector2 down
        {
            get
            {
                return new Vector2(0, 1);
            }
        }
        #endregion

        /*
        /// <summary>
        /// Returns downwards direction with <c>Settings.CoordinateSystemCenter</c> in mind.
        /// </summary>
        public static Vector2 down()
        {
            switch (Settings.CoordinateSystemCenter)
            {
                case CoordinateSystemType.BottomRight:
                case CoordinateSystemType.TopRight:
                case CoordinateSystemType.TopLeft:
                    return new Vector2(0, 1);

                case CoordinateSystemType.BottomLeft:
                case CoordinateSystemType.Middle:
                    return new Vector2(0, -1);

                default: //This won't happen
                    return new Vector2();
            }
        }

        /// <summary>
        /// Returns upwards direction with <c>Settings.CoordinateSystemCenter</c> in mind.
        /// </summary>
        public static Vector2 up()
        {
            switch (Settings.CoordinateSystemCenter)
            {
                case CoordinateSystemType.BottomRight:
                case CoordinateSystemType.TopRight:
                case CoordinateSystemType.TopLeft:
                    return new Vector2(0, -1);

                case CoordinateSystemType.BottomLeft:
                case CoordinateSystemType.Middle:
                    return new Vector2(0, 1);

                default: //This won't happen
                    return new Vector2();
            }
        }


        /// <summary>
        /// Returns upwards direction with <c>Settings.CoordinateSystemCenter</c> in mind.
        /// </summary>
        public static Vector2 right()
        {
            switch (Settings.CoordinateSystemCenter)
            {
                case CoordinateSystemType.BottomRight:
                case CoordinateSystemType.TopRight:
                    return new Vector2(-1, 0);

                case CoordinateSystemType.BottomLeft:
                case CoordinateSystemType.TopLeft:
                    return new Vector2(1, 0);

                case CoordinateSystemType.Middle:
                    return new Vector2(1, 0);

                default: //This won't happen
                    return new Vector2();
            }
        }


        /// <summary>
        /// Returns upwards direction with <c>Settings.CoordinateSystemCenter</c> in mind.
        /// </summary>
        public static Vector2 left()
        {
            switch (Settings.CoordinateSystemCenter)
            {
                case CoordinateSystemType.BottomRight:
                case CoordinateSystemType.TopRight:
                    return new Vector2(1, 0);

                case CoordinateSystemType.BottomLeft:
                case CoordinateSystemType.TopLeft:
                    return new Vector2(-1, 0);

                case CoordinateSystemType.Middle:
                    return new Vector2(-1, 0);

                default: //This won't happen
                    return new Vector2();
            }
        }*/

        /// <summary>
        /// Compares Vector2, same as using the '==' operator.
        /// </summary>
        public bool Equals(Vector2 vector2)
        {
            return GetHashCode() == vector2.GetHashCode();
            /*
            if (ReferenceEquals(null, vector2))
            {
                return false;
            }
            if (ReferenceEquals(this, vector2))
            {
                return true;
            }

            return x == vector2.x && y == vector2.y;*/
        }
        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Compares Vector2, rounded to nearest whole integer.
        /// </summary>
        public bool EqualsInt(Vector2 vector2)
        {
            return (int)x == (int)vector2.x && (int)y == (int)vector2.y;
        }

        public Vector2 Integer()
        {
            return new Vector2((int)x, (int)y);
        }

        /// <summary>
        /// Sets the x and y value with one call.
        /// </summary>
        public void Set(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public void Add(float x, float y)
        {
            this.x += x;
            this.y += y;
        }


        public override string ToString()
        {
            return $"({x.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {y.ToString(System.Globalization.CultureInfo.InvariantCulture)})";
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator *(float d, Vector2 a)
        {
            return new Vector2(a.x * d, a.y * d);
        }
        public static Vector2 operator *(Vector2 a, float d)
        {
            return new Vector2(a.x * d, a.y * d);
        }
        public static Vector2 operator *(decimal d, Vector2 a)
        {
            return new Vector2(a.x * (float)d, a.y * (float)d);
        }
        public static Vector2 operator *(Vector2 a, decimal d)
        {
            return new Vector2(a.x * (float)d, a.y * (float)d);
        }
    }

    public static class UI
    {

        public class Image
        {

        }
    }

    public static class Settings
    {
        /// <summary>
        /// Defines the coordinate (0, 0) point.
        /// </summary>
        public static CoordinateSystemType CoordinateSystemCenter = CoordinateSystemType.TopLeft;
        public static bool FPSCounter = true;

        public static int GameSizeWidth { get; set; } = 100;
        public static int GameSizeHeight { get; set; } = 50;

        /// <summary>
        /// Makes the grid in the console equal size.
        /// The width - height ratio changes from 1:1 to 2:1. 
        /// (A char placed in one cell is placed in 2 cells.)
        /// </summary>
        public static bool SquareMode { get; set; } = false;
    }

    public static class Input
    {
        /// <summary>
        /// Checks for key presses during each frame.
        /// </summary>
        public static bool ListenForKeys { get; set; } = true;

        public static float HorizontalAxis { get; private set; }
        public static float VerticalAxis { get; private set; }

        private static Dictionary<int, ConsoleKey> frameKeys = new Dictionary<int, ConsoleKey>();
        private static Task keyListener = null;
        //private static int lastFrame = 0;

        public static bool GetKey(ConsoleKey key)
        {
            if (frameKeys.TryGetValue(Game.TotalFrames + 1, out ConsoleKey pressedKey))
            {
                return pressedKey == key;
            }
            else { return false; }
        }

        /// <summary>
        /// Initiates key listening thread.
        /// </summary>
        public static void ListenKeys()
        {
            if (keyListener != null)
            {
                return;
            }

            keyListener = Task.Run(() =>
            {
                ListenForKeys = true;
                while (ListenForKeys)
                {
                    ConsoleKey key = Console.ReadKey(true).Key;
                    frameKeys[Game.TotalFrames + 1] = key;
                    //frameKeys.Add(Game.TotalFrames + 1, Console.ReadKey(true).Key);
                    //lastFrame = Game.TotalFrames + 1;
                }
            });
        }

        public enum Axis { Horizontal, Vertical, }

        /// <summary>
        /// Try using <code>HorizontalAxis</code>/<code>VerticalAxis</code> instead.
        /// </summary>
        public static float GetAxis(Axis axis)
        {
            if (axis.Equals(Axis.Horizontal))
            {
                return HorizontalAxis;
            }
            else
            {
                return VerticalAxis;
            }
        }
    }

    public static class Game
    {
        /// <summary>
        /// List of all GameObjects.
        /// </summary>
        /// <remarks>Objects must NOT be removed from this list, they should be nullified.</remarks>
        public static List<GameObject> Objects { get; } = new List<GameObject>();
        public static Action UpdateMethod { get; set; }
        public static Action StartMethod { get; set; }
        public static int TotalFrames { get; private set; }
        public static long GameStartedTimestamp { get; private set; }

        //private static Timer updateTimer;
        private static bool gamePlaying;
        //private static long previousFrameTimestamp = 0;
        private static char[,] gamefield { get; set; } = new char[Settings.GameSizeHeight, Settings.GameSizeWidth];
        private static char[,] gamefieldRendered { get; set; } = new char[Settings.GameSizeHeight, Settings.GameSizeWidth];
        private static int?[,] collisionMap { get; set; } = new int?[Settings.GameSizeHeight, Settings.GameSizeWidth];
        private static int?[,] collisionMapRendered { get; set; } = new int?[Settings.GameSizeHeight, Settings.GameSizeWidth];
        public static Tuple<bool, List<int>>[,] collidedMap { get; set; } = new Tuple<bool, List<int>>[Settings.GameSizeHeight, Settings.GameSizeWidth];
        private static List<GameObject> previousFrameObjects = new List<GameObject>();

        public static void Exit() => gamePlaying = false;

        public static void Play()
        {
            // Internal start
            long currentTimestamp = Utility.TimeStamp();
            Console.CursorVisible = false;
            Input.ListenKeys();

            // External start method
            if (StartMethod != null)
            {
                StartMethod.Invoke();
            }

            // Draw gameobjects
            foreach (GameObject obj in Utility.SortGameObjects(Objects))
            {
                HandleGameObject(obj);
            }

            //previousFrameObjects = Objects;
            GameStartedTimestamp = Utility.TimeStamp();

            long previousFrameTimestamp = currentTimestamp;
            currentTimestamp = Utility.TimeStamp();

            gamePlaying = true;

            // Prevents window from closing
            while (gamePlaying)
            {

                // Internal Update loop
                previousFrameObjects = Objects;
                gamefield = gamefieldRendered;
                previousFrameTimestamp = currentTimestamp;
                currentTimestamp = Utility.TimeStamp();

                float delta = currentTimestamp - previousFrameTimestamp;
                Time.deltaTime = delta != 0 ? delta / (float)1000 : 0;

                if (Settings.FPSCounter)
                {
                    float FPS;
                    float timepassed;
                    try
                    {
                        timepassed = (float)(Utility.TimeStamp() - GameStartedTimestamp) / (float)1000;
                        FPS = TotalFrames / timepassed;
                    }
                    catch (DivideByZeroException) { FPS = 0; timepassed = 0; }

                    Console.Title = $"FPS: {FPS}, deltaTime: {Time.deltaTime}";
                }

                if (Debug.DrawCoordinateSystemEveryFrame)
                    Debug.DrawCoordinateSystem();

                // Draw gameobjects
                //collisionMap = new int?[GameSizeHeight, GameSizeWidth];

                //gamefield = new char[GameSizeHeight, GameSizeWidth];
                //gamefieldRendered = new char[GameSizeHeight, GameSizeWidth];
                foreach (GameObject obj in Utility.SortGameObjects(Objects))
                {
                    HandleGameObject(obj);
                }

                CleanBuffered();
                UpdateBuffer();

                // External update loop
                if (UpdateMethod != null)
                {
                    UpdateMethod.Invoke();
                }

                TotalFrames++;
            }
        }

        private static void HandleGameObject(GameObject obj)
        {
            if (!obj.activeSelf || obj.sprite.draw == null)
            {
                return;
            }

            //HandleCollisions(obj.sprite.collision, obj.transform.position, (int)obj.identifier);
            PlaceCharArray(obj.sprite.draw, (int)obj.transform.position.x, (int)obj.transform.position.y);
        }

        private static void PlaceCharArray(char[,] sprite, int x, int y)
        {
            if (sprite == null)
            {
                return;
            }
            //Vector2 position = obj.transform.position;
            //char[][] sprite = obj.sprite.draw;

            for (int loop_y = 0; loop_y < sprite.GetLength(0); loop_y++)
            {
                for (int loop_x = 0; loop_x < sprite.GetLength(1); loop_x++)
                {
                    SetCell(sprite[loop_y, loop_x], x + loop_x, y + loop_y);
                    /*
                    if (sprite[y][x] != ' ')
                    {
                        //Utility.SetPixel(sprite[y][x], (int)pos.x + x, (int)pos.y + y);
                        SetCell(sprite[y][x], (int)pos.x + x, (int)pos.y + y);
                    }*/
                }
            }
        }

        private static void HandleCollisions(bool[,] collision, Vector2 position, int identifier)
        {
            if (collision == null)
            {
                return;
            }

            bool collided = false;

            for (int y = 0; y < collision.GetLength(0); y++)
            {
                for (int x = 0; x < collision.GetLength(1); x++)
                {
                    //TODO: Implement collision system
                    if (collisionMap[(int)position.y + y, (int)position.x + x] == null)
                    {
                        collisionMap[(int)position.y + y, (int)position.x + x] = identifier;
                    }
                    else
                    {
                        collided = true;
                    }
                }
            }

            if (collided)
            {
                System.Diagnostics.Debug.WriteLine("Collided");
                //Objects[identifier].events.OnCollisionEnter
            }
        }

        private static void CleanBuffered()
        {
            //List<GameObject> objectsList = Objects;
            for (int i = 0; i < Math.Min(previousFrameObjects.Count, Objects.Count); i++)
            {
                //Debug.Log($"previus: {previousFrameObjects[i].transform.position}, now: {objectsList[i].transform.position}");
                Debug.Log(previousFrameObjects[i].transform.position.x == Objects[i].transform.position.x && previousFrameObjects[i].transform.position.y == Objects[i].transform.position.y);
                if (true)
                {
                    //Debug.Log("Object is not equal to new transform");
                    Vector2 position = Objects[i].transform.position;
                    for (int y = 0; y < Objects[i].sprite.draw.GetLength(0); y++)
                    {
                        for (int x = 0; x < Objects[i].sprite.draw.GetLength(1); x++)
                        {
                            gamefield[(int)position.y + y, (int)position.x + x] = ' ';
                            //SetCell(' ', (int)position.x + x, (int)position.y + y);
                        }
                    }
                }
            }
        }

        private static void UpdateBuffer()
        {

            for (int y = 0; y < Settings.GameSizeHeight; y++)
            {
                for (int x = 0; x < Settings.GameSizeWidth; x++)
                {
                    char value = gamefield[y, x];
                    if (value != '\0')
                    {
                        Utility.SetPixel(value, x, y);
                    }
                }
            }
        }

        /// <summary>
        /// Set a specific cell's value.
        /// </summary>
        public static void SetCell(char value, int x, int y)
        {
            if (0 < x && x < Settings.GameSizeWidth &&
                0 < y && y < Settings.GameSizeHeight)
            {
                gamefield[y, x] = gamefieldRendered[y, x] == value ? '\0' : value;
            }
        }
        /// <summary>
        /// Sets cell values by an character array, at a location.
        /// </summary>
        /// <param name="HorizontalText">Char array put horizontally or vertically.</param>
        public static void SetCell(char[] values, int x, int y, bool HorizontalText = true)
        {
            if (HorizontalText)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    SetCell(values[i], x + i, y);
                }
            }
            else
            {
                for (int i = 0; i < values.Length; i++)
                {
                    SetCell(values[i], x, y + i);
                }
            }
        }
    }

    public static class Debug
    {
        /// <summary>
        /// Currently affects performance massively.  For drawing coordinate system only once refer to DrawCoordinateSystem method.
        /// </summary>
        public static bool DrawCoordinateSystemEveryFrame { get; set; } = false;

        /// <summary>
        /// The maximum width of coordinate system, setting the value to null results in max width being equal to Game.GameSizeWidth.
        /// </summary>
        public static int? CoordinateWidth { get; set; } = null; //100
        /// <summary>
        /// The maximum height of coordinate system, setting the value to null results in max height being equal to Game.GameSizeHeight.
        /// </summary>
        public static int? CoordinateHeight { get; set; } = null; //30
        /// <summary>
        /// The interval of numbers displayed on each axis.
        /// </summary>
        public static int CoordinateInterval { get; set; } = 5;

        /// <summary>
        /// Log mode, <code>Spool</code> to log the output to a file, <code>DebugIDE</code> to write to special IDE debugging channel.
        /// </summary>
        public static logMode LogMode = logMode.DebugIDE;

        private static StreamWriter fs = null;

        /// <summary>
        /// Draws coordinate system
        /// </summary>
        public static void DrawCoordinateSystem()
        {
            int Width = CoordinateWidth == null ? Settings.GameSizeWidth : (int)CoordinateWidth;
            int Height = CoordinateHeight == null ? Settings.GameSizeHeight : (int)CoordinateHeight;


            Game.SetCell('x', 0, 0);
            for (int x = CoordinateInterval; x + CoordinateInterval < Math.Min(Width, Console.BufferWidth); x += CoordinateInterval)
            {
                //Utility.SetPixel('|', x, 0);
                Game.SetCell('|', x, 1);
                //Utility.SetPixel(x.ToString(), x, 2);
                Game.SetCell(x.ToString().ToCharArray(), x, 3);
            }

            for (int y = CoordinateInterval; y + CoordinateInterval < Math.Min(Height, Console.BufferHeight); y += CoordinateInterval)
            {
                //Utility.SetPixel("--", 0, y);
                Game.SetCell("--".ToCharArray(), 1, y);

                //Utility.SetPixel(y.ToString(), 4, y);
                Game.SetCell(y.ToString().ToCharArray(), 4, y);
            }
        }

        /// <summary>
        /// Logs a message to 'latest_log.txt'.
        /// </summary>
        public static void Log(object message)
        {
            switch (LogMode)
            {
                case logMode.Spool:
                    write(message, logType.INFO);
                    return;
                case logMode.DebugIDE:
                    System.Diagnostics.Debug.WriteLine(message);
                    return;
            }
        }
        public static void LogError(object message)
        {
            switch (LogMode)
            {
                case logMode.Spool:
                    write(message, logType.ERROR);
                    return;
                case logMode.DebugIDE:
                    System.Diagnostics.Debug.WriteLine(message);
                    return;
            }
        }
        public static void LogWarning(object message)
        {
            switch (LogMode)
            {
                case logMode.Spool:
                    write(message, logType.WARNING);
                    return;
                case logMode.DebugIDE:
                    System.Diagnostics.Debug.WriteLine(message);
                    return;
                default:
                    break;
            }
        }

        private enum logType { INFO, WARNING, ERROR, }
        public enum logMode { Spool, DebugIDE }

        private static void write(object message, logType logType)
        {
            if (fs == null)
            {
                fs = initLog();
            }

            fs.WriteLineAsync($"[{DateTime.Now.ToShortTimeString()}][{logType.ToString()}] " + message);
        }

        private static StreamWriter initLog()
        {
            if (File.Exists("latest_log.txt"))
            {
                File.Move("latest_log.txt", $"log_{File.ReadLines("latest_log.txt").First()}.txt");
            }

            StreamWriter sw = new StreamWriter("latest_log.txt");
            sw.WriteLine(Utility.TimeStamp());
            sw.WriteLine("\nLog initialized\n");
            return sw;
        }
    }

    public static class Time
    {
        /// <summary>
        /// Still experimenting with this...
        /// </summary>
        public static float deltaTime
        {
            get; set;
        }

    }

    public static class Utility
    {
        public static void SetPixel(string value, int x, int y)
        {
            if (0 < x && x < Console.BufferWidth &&
                0 < y && y < Console.BufferHeight)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(value);
            }
        }
        public static void SetPixel(string value, Vector2 coordinates)
        {
            if (0 < coordinates.x && coordinates.x < Console.BufferWidth &&
                0 < coordinates.y && coordinates.y < Console.BufferHeight)
            {
                Console.SetCursorPosition((int)coordinates.x, (int)coordinates.y);
                Console.Write(value);
            }
        }
        public static void SetPixel(char value, int x, int y)
        {
            if (0 < x && x < Console.BufferWidth &&
                0 < y && y < Console.BufferHeight)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(value);
            }
        }
        public static void SetPixel(char value, Vector2 coordinates)
        {
            if (0 < coordinates.x && coordinates.x < Console.BufferWidth &&
                0 < coordinates.y && coordinates.y < Console.BufferHeight)
            {
                Console.SetCursorPosition((int)coordinates.x, (int)coordinates.y);
                Console.Write(value);
            }
        }

        /// <summary>
        /// Returns unix timestamp.
        /// </summary>
        public static long TimeStamp() => DateTimeOffset.Now.ToUnixTimeMilliseconds();

        /*
        /// <summary>
        /// Gets all gameobjects that are static
        /// </summary>
        /// <returns></returns>
        public static List<GameObject> GetGameObjects()
        {
            List<GameObject> list = new List<GameObject>();
            list.AddRange(Game.Objects);
            return list;
        }*/

        /// <summary>
        /// Sorts and removes disabled objects.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<GameObject> SortGameObjects(List<GameObject> list)
        {
            return list;
            //list.RemoveAll(a => !a.activeSelf);
            //list.RemoveAll(a => a == null);
            //return list.OrderBy(o => o.transform.z_index).ToList();
        }
    }

    public static class Exceptions
    {
        public class GameObjectNotInstantiatedException : Exception
        {
            public GameObjectNotInstantiatedException()
            {
            }

            public GameObjectNotInstantiatedException(string message) : base(message)
            {
            }
        }

        public class HeightOrWidthLessThanOrEqualToZeroException : Exception
        {
            public HeightOrWidthLessThanOrEqualToZeroException()
            {
            }

            public HeightOrWidthLessThanOrEqualToZeroException(string message) : base(message)
            {
            }
        }
    }

}


#region Timer
/*
updateTimer = new Timer((e) =>
{
    // Internal Update loop
    long currentTimestamp = TimeStamp();
    float delta = (float)currentTimestamp - (float)previousFrameTimestamp;

    try { Time.deltaTime = delta / 1000; }
    catch (DivideByZeroException) { Time.deltaTime = 0; }

    previousFrameTimestamp = TimeStamp();


    if (Settings.FPSCounter)
    {
        float FPS;
        float timepassed;
        try
        {
            timepassed = (float)(TimeStamp() - gameStartedTimestamp) / (float)1000;
            FPS = TotalFrames / timepassed;
        }
        catch (DivideByZeroException)
        {
            FPS = 0;
            timepassed = 0;
        }

        Console.Title = $"FPS: {FPS}, time: {timepassed}";
    }


    Console.Clear();


    if (Debug.ShowCoordinateSystem)
    {
        Debug.DrawCoordinateSystem();
    }

    // Draw gameobjects
    foreach (GameObject obj in Utility.SortGameObjects(Utility.GetGameObjects()))
    {

        DrawGameObject(obj);
    }

    // External update loop
    if (UpdateMethod != null)
    {
        UpdateMethod.Invoke();
    }

    TotalFrames++;
}, null, 0, (int)(1000 / TargetFramesPerSecond));
*/
#endregion