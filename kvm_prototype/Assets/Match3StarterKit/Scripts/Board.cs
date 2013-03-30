using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Timed out delegate.
/// </summary>
public delegate void TimedOutDelegate ();

/// <summary>
/// Level cleared delegate.
/// </summary>
public delegate void LevelClearedDelegate ();

/// <summary>
/// Game paused delegate.
/// </summary>
public delegate void GamePausedDelegate ();

/// <summary>
/// Check bonus delegate.
/// </summary>
public delegate void CheckBonusDelegate ();

public class Board : MonoBehaviour
{
#region Constants
	/// <summary>
	/// Constant holding the path to the boards.
	/// </summary>
	private const string STR_DataBoard = "Data/board";
	
	/// <summary>
	/// Constant holding the "failed to load the board file" message.
	/// </summary>
	private const string STR_FailedToLoadTheBoardFile = "Failed to load the board file!";

	/// <summary>
	/// Constant holding the ".".
	/// </summary>
	private const string STR_dot = ".";
#endregion

#region Implemented delegates
	/// <summary>
	/// The game paused method. This will be called when the <see cref="GamePaused"/> is True
	/// </summary>
	public GamePausedDelegate GamePausedMethod;
	
	/// <summary>
	/// The game paused method. This will be called when the <see cref="TimeOut"/> is True
	/// </summary>
	public LevelClearedDelegate LevelClearedMethod;
	
	/// <summary>
	/// The timed out method.
	/// </summary>
	public TimedOutDelegate TimedOutMethod;
	
	/// <summary>
	/// The check bonus method. This is called every time a set of pieces 
	/// has been destroyed. In this delegate you have to check the <see cref="totalDestroyedPieces"/>,
	/// <see cref="totalNormalPiecesDestroyed"/>, <see cref="totalStrongPiecesDestroyed"/>,
	/// <see cref="totalSuperStrongPiecesDestroyed"/> and <see cref="totalExtraStrongPiecesDestroyed"/>
	/// to see if it's the case to award a bonus or a multiplier to the player.
	/// </summary>
	public CheckBonusDelegate CheckBonusMethod;
#endregion
	
#region Public Static Members
	/// <summary>
	/// Gets the instance of this class.
	/// </summary>
	/// <value>
	/// The instance.
	/// </value>
	public static Board Instance {
		get {
			return instance;
		}
	}
	
	/// <summary>
	/// To set if the game paused.
	/// </summary>
	public static bool GamePaused = false;
	
	/// <summary>
	/// The total destroyed tiles during one move.
	/// It's static to facilitate its access from outside the bard manager.
	/// Check this in your <see cref="CheckBonusMethod"/> delegate method.
	/// </summary>
	public static int totalDestroyedPieces = 0;

	/// <summary>
	/// The total normal pieces destroyed.
	/// It's static to facilitate its access from outside the bard manager.
	/// Check this in your <see cref="CheckBonusMethod"/> delegate method.
	/// </summary>
	public static int totalNormalPiecesDestroyed = 0;
	
	/// <summary>
	/// The total strong pieces destroyed.
	/// It's static to facilitate its access from outside the bard manager.
	/// Check this in your <see cref="CheckBonusMethod"/> delegate method.
	/// </summary>
	public static int totalStrongPiecesDestroyed = 0;
	
	/// <summary>
	/// The total super strong pieces destroyed.
	/// It's static to facilitate its access from outside the bard manager.
	/// Check this in your <see cref="CheckBonusMethod"/> delegate method.
	/// </summary>
	public static int totalSuperStrongPiecesDestroyed = 0;
	
	/// <summary>
	/// The total extra strong pieces destroyed.
	/// It's static to facilitate its access from outside the bard manager.
	/// Check this in your <see cref="CheckBonusMethod"/> delegate method.
	/// </summary>
	public static int totalExtraStrongPiecesDestroyed = 0;

	/// <summary>
	/// The active piece.
	/// </summary>
	public static GridPoint ActivePiece = new GridPoint ();
	
	/// <summary>
	/// True if the player can move.
	/// </summary>
	public static bool PlayerCanMove = true;
	
	/// <summary>
	/// True if the was moving during last frame.
	/// </summary>
	public static bool wasMoving = false;
#endregion
	
#region Internal Static Members
	/// <summary>
	/// The step in between tiles.
	/// </summary>
	internal static float step = 0.0f;
	
	/// <summary>
	/// The half step in between tiles.
	/// </summary>
	internal static float halfStep = 0.0f;
	
	/// <summary>
	/// The playing pieces.
	/// </summary>
	internal static PlayingPiece[,] PlayingPieces;
	
	/// <summary>
	/// The grid descriptor.
	/// This is a container of int used to search the pieces very fast.
	/// </summary>
	internal static int[,] gridDescriptor;
	
	/// <summary>
	/// The clean slate flag tells us if the board has been cleared (winning condition).
	/// </summary>
	internal static bool CleanSlate = false;
	
	/// <summary>
	/// The time out flag tells us if the user ran out of time.
	/// </summary>
	internal static bool TimeOut = false;
	
	/// <summary>
	/// The tap on mobile.
	/// </summary>
	internal static bool tap = false;
	
	/// <summary>
	/// The tap position on mobile.
	/// </summary>
	internal static Vector2 tapPosition = Vector2.zero;
#endregion
	
#region Public members
	/// <summary>
	/// NGUI label keeping the score.
	/// </summary>
	public UILabel scoreLabel;
	
	/// <summary>
	/// NGUI label keeping the amont of tiles destroyed during last move.
	/// </summary>
	public UILabel matchesLabel;
	
	/// <summary>
	/// NGUI slider showing the remaining time.
	/// </summary>
	public UISlider timeBarSlider;
	
	/// <summary>
	/// Time (in minutes) to finish the level.
	/// </summary>
	public float levelTime = 10f;
	
	/// <summary>
	/// Time (in seconds) since last move before to show a hint.
	/// </summary>
	public float hintTime = 40f;
	
	/// <summary>
	/// The gameobject to show as hint (usually a particle system).
	/// </summary>
	public GameObject hintEffect;
	
	/// <summary>
	/// The sound effect played when a hint is shown.
	/// </summary>
	public AudioClip hintSound;
	
	/// <summary>
	/// Point Value of normal tiles.
	/// </summary>
	public int PointsNormal = 5;
	
	/// <summary>
	/// Point Value of strong tiles.
	/// </summary>
	public int PointsStrong = 10;
	
	/// <summary>
	/// Point Value of extra strong tiles.
	/// </summary>
	public int PointsExtraStrong = 15;
	
	/// <summary>
	/// Point Value of super strong tiles.
	/// </summary>
	public int PointsSuperStrong = 20;

	/// <summary>
	/// Empty GameObject marking the left edge of the active part of the board
	/// </summary>
	public GameObject leftMark;
	
	/// <summary>
	/// Empty GameObject marking the right edge of the active part of the board
	/// </summary>
	public GameObject rightMark;
	
	/// <summary>
	/// Set this True If this is a Match 4 instead of a Match 3 
	/// </summary>
	public bool isMatch4 = false;
	
	/// <summary>
	/// True if new pieces shall drop from the top of the screen.
	/// </summary>
	public bool newPiecesFromTop = false;
	
	/// <summary>
	/// The game style.
	/// </summary>
	public GameStyle gameStyle = GameStyle.Marinas;
	
	/// <summary>
	/// The tile prefab.
	/// </summary>
	public GameObject tile;
	
	/// <summary>
	/// The tile position on Z axis.
	/// </summary>
	public float zTilePosition = 0.0f;
	
	/// <summary>
	/// The piece position on Z axis.
	/// </summary>
	public float zPiecePosition = 0.0f;
	
	/// <summary>
	/// The tile todo normal material.
	/// </summary>
	public Material tileTodoNormalMaterial;
	
	/// <summary>
	/// The tile done material.
	/// </summary>
	public Material tileDoneMaterial;
	
	/// <summary>
	/// The tile blocked material.
	/// </summary>
	public Material tileBlockedMaterial;
	
	/// <summary>
	/// The active effect played on the active piece.
	/// </summary>
	public ParticleSystem activeEffect;
	
	/// <summary>
	/// Z axis position for the Active piece effect
	/// </summary>
	public float zActiveEffectPosition = 0.0f;
	
	/// <summary>
	/// The piece destroyed effect.
	/// </summary>
	public GameObject pieceDestroyedEffect;
	
	/// <summary>
	/// The special piece prefab.
	/// </summary>
	public GameObject specialPiece;
	
	/// <summary>
	/// The pieces normal.
	/// </summary>
	public List<GameObject> PiecesNormal;
	
	/// <summary>
	/// The pieces strong.
	/// </summary>
	public List<GameObject> PiecesStrong;
	
	/// <summary>
	/// The pieces extra strong.
	/// </summary>
	public List<GameObject> PiecesExtraStrong;
	
	/// <summary>
	/// The pieces super strong.
	/// </summary>
	public List<GameObject> PiecesSuperStrong;
	
	/// <summary>
	/// The max pieces that can be on the board at the same time.
	/// </summary>
	public int maxPieces = 10;
	
	/// <summary>
	/// The clip to play when a new piece is spwan.
	/// </summary>
	public AudioClip newPiece;
	
	/// <summary>
	/// he clip to play when a piece is destroyed.
	/// </summary>
	public AudioClip destroyPiece;
	
	/// <summary>
	/// The clip to play when a piece slides.
	/// </summary>
	public AudioClip SlidePiece;
	
	/// <summary>
	/// The board number (refers to the file to load).
	/// This is used to generate the board file name.
	/// </summary>
	public int boardNumber = 1;
	
	/// <summary>
	/// The total rows in the board.
	/// This is used to generate the board file name.
	/// </summary>
	public int rows = 10;
	
	/// <summary>
	/// The total columns in the board.
	/// This is used to generate the board file name.
	/// </summary>
	public int columns = 10;
	
	/// <summary>
	/// If true fill the board on the screen width.
	/// </summary>
	public bool FillOnX = false;
	
	/// <summary>
	/// If true centers the board on the screen width.
	/// </summary>
	public bool CentreOnX = false;
#endregion
	
#region Private Static Members
	/// <summary>
	/// The grid points list.
	/// </summary>
	private static List<GridPoint> gridPointsList = new List<GridPoint> ();
	
	/// <summary>
	/// The game timer used to count down.
	/// </summary>
	private static float gameTimer = 0f;
	
	/// <summary>
	/// The start position.
	/// </summary>
	private static Vector3 startPosition = new Vector3 (0f, 0f, 0f);
	
	/// <summary>
	/// The grid array keeping the tiles.
	/// </summary>
	private static BoardTile[,] grid;
	
	/// <summary>
	/// The array of int grid descriptor for fast operations.
	/// </summary>
	private static int[,] intGridDescriptor;
	
	/// <summary>
	/// The active marker effect.
	/// </summary>
	private static ParticleSystem activeMarker = null;
	
	/// <summary>
	/// The normal pieces in use.
	/// </summary>
	private static List<GameObject> piecesToUseNormal = new List<GameObject> ();
	
	/// <summary>
	/// The strong pieces in use.
	/// </summary>
	private static List<GameObject> piecesToUseStrong = new List<GameObject> ();
	
	/// <summary>
	/// The extra strong pieces in use.
	/// </summary>
	private static List<GameObject> piecesToUseExtraStrong = new List<GameObject> ();
	
	/// <summary>
	/// The super strong pieces in use.
	/// </summary>
	private static List<GameObject> piecesToUseSuperStrong = new List<GameObject> ();
	
	/// <summary>
	/// The destroyed flag used in the Update.
	/// </summary>
	private static bool destroyed = false;
	
	/// <summary>
	/// The check timer counter to drive the positions update.
	/// </summary>
	private static float checkTimer = 0.0f;
	
	/// <summary>
	/// The general scale.
	/// </summary>
	private static Vector3 generalScale = new Vector3 (1f, 1f, 1f);
	
	/// <summary>
	/// The instance of this script.
	/// </summary>
	private static Board instance = null;
	
	/// <summary>
	/// The touch object for mobile.
	/// </summary>
	private static Touch touch;
#endregion

#region Private Members
	/// <summary>
	/// The hint timer.
	/// </summary>
	private float hintTimer = 0f;
	
	/// <summary>
	/// The total blocked tiles.
	/// </summary>
	private int totalBlockedTiles = 0;
	
	/// <summary>
	/// The vector position.
	/// </summary>
	private Vector2 vectorPosition;
	
	/// <summary>
	/// The type of the tile strenght.
	/// </summary>
	private TileType tileStrenghtType;
	
	/// <summary>
	/// The current position.
	/// </summary>
	private Vector2 currentPosition;
	
	/// <summary>
	/// The active piece vector position.
	/// </summary>
	private Vector2 activePieceVectorPosition;
	
	/// <summary>
	/// The started flag telling if the sctipt has started.
	/// </summary>
	private bool started = false;
#endregion
	
	/// <summary>
	/// Awake method, the closest thing to a counstructor in Unity.
	/// </summary>
	void Awake ()
	{
		QualitySettings.vSyncCount = 0; // Don't use vSync as we set the target framerate
		Application.targetFrameRate = 30; // no need to push beyond 30FPS
		if (instance != null && instance != this) {
			Destroy (this);
			return;
		}
		instance = this;
	}

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		StartBoard ();
	}

	/// <summary>
	/// Starts the board. This is what you call in case you want to restart the level.
	/// You can alse set by code <see cref="rows"/>, <see cref="columns"/>
	/// and <see cref="boardNumber"/> to load a different board layout.
	/// </summary>
	internal void StartBoard ()
	{
		// Check Error conditions and leave in such case
		if (PiecesNormal.Count != PiecesStrong.Count)
		{
			Debug.LogError("Strong Pieces must be as many as the normal ones!");
			return;
		}
		if (PiecesNormal.Count != PiecesExtraStrong.Count)
		{
			Debug.LogError("Extra Strong Pieces must be as many as the normal ones!");
			return;
		}

		if (PiecesNormal.Count != PiecesSuperStrong.Count)
		{
			Debug.LogError("Super Strong Pieces must be as many as the normal ones!");
			return;
		}
		TextAsset TXTFile = (TextAsset)Resources.Load (STR_DataBoard + boardNumber.ToString () + STR_dot + rows.ToString () + STR_dot + columns.ToString ());
		if (TXTFile == null) {
			Debug.LogError (STR_FailedToLoadTheBoardFile);
			return;
		}

		// In case we are restarting a new level we need to clean up
		// all the old tiles and pieces beforecreating the new level.

		// Clen up the old tiles if any
		if (grid!=null)
		{
			foreach (object o in grid)
			{
				BoardTile b = o as BoardTile;
				if (b != null && b.Tile != null)
				{
					DestroyImmediate(b.Tile);
				}
			}
		}

		// Clen up the old pieces if any
		if (PlayingPieces!=null)
		{
			foreach (object o in PlayingPieces)
			{
				PlayingPiece p = o as PlayingPiece;
				if (p != null && p.Piece != null)
				{
					DestroyImmediate(p.Piece);
				}
			}
		}
		
		// Setup the size of the current board
		PlayingPieces = new PlayingPiece[columns, rows];
		grid = new BoardTile[columns, rows];
		gridDescriptor = new int[columns, rows];
		intGridDescriptor = new int[columns, rows];
		// No active piece
		ActivePiece.x = -1;
		ActivePiece.y = -1;

		// Get the pieces that will actually be used during this game
		GetPiecesToUse ();
		
		// Compute the world positions and scales
		float val = Mathf.Max ((float)(columns), (float)(rows));
		if (!FillOnX) {
			step = (Mathf.Abs (leftMark.transform.position.x) + Mathf.Abs (rightMark.transform.position.x)) / val;
			halfStep = step / 2.0f;
			startPosition = new Vector3 (leftMark.transform.position.x + halfStep, leftMark.transform.position.y - halfStep, zTilePosition);
		} else {
			Vector3 left = Camera.main.ScreenToWorldPoint (new Vector3 (0f, 0f, Camera.main.transform.position.z));
			Vector3 right = Camera.main.ScreenToWorldPoint (new Vector3 (Mathf.Min ((float)Screen.width, (float)Screen.height), 0f, Camera.main.transform.position.z));
			step = (Mathf.Abs (left.x) + Mathf.Abs (right.x)) / val;
			halfStep = step / 2.0f;
			startPosition = new Vector3 (-(Mathf.Abs (left.x)) + halfStep, left.y - halfStep, zTilePosition);
		}
		float tilesScale = ((step * val) / (tile.renderer.bounds.size.x * val));
		if (CentreOnX) {
			Vector3 realRight = Camera.main.ScreenToWorldPoint (new Vector3 ((float)Screen.width, 0f, Camera.main.transform.position.z));
			startPosition.x += Mathf.Abs (Mathf.Abs (realRight.x * 2f) - (step * columns)) / 2f;
		}
		generalScale = new Vector3 (tilesScale, tilesScale, 1f);
		specialPiece.transform.localScale = generalScale;
		if (activeMarker == null) {
			activeMarker = Instantiate (activeEffect, new Vector3 (0, 0, zActiveEffectPosition), Quaternion.identity) as ParticleSystem;
			activeMarker.transform.localScale = generalScale;
		}
		activeMarker.Stop(true);
		// Store the relations between world and grid positions
		GridPositions.Init ();

		// Setup the tiles desctriptors as defined in the file
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				gridDescriptor [x, y] = new int ();
				if (!int.TryParse ((TXTFile.text [x + y * columns].ToString ()), out gridDescriptor [x, y]))
					gridDescriptor [x, y] = (int)TileType.NoTile;
			}
		}
		
		// Now get the tiles and pieces on the board
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				float xp, yp = 0f;
				if (gridDescriptor [x, y] != (int)TileType.NoTile) {
					xp = startPosition.x + (x * step);
					yp = startPosition.y - (y * step);
					GridPositions.SetPosition (x, y, xp, yp);
					grid [x, y] = new BoardTile (Instantiate (tile, new Vector3 (xp, yp, zTilePosition), Quaternion.identity) as GameObject, (TileType)gridDescriptor [x, y]);
					grid [x, y].Tile.transform.localScale = generalScale;
					
					// Check and count the blocked tiles and set the proper material
					if (gridDescriptor [x, y] == (int)TileType.BlockedTile) {
						grid [x, y].Tile.renderer.material = tileBlockedMaterial;
						totalBlockedTiles++;
					} else {
						grid [x, y].Tile.renderer.material = tileTodoNormalMaterial;
					}
					
					// Get the piece for this tile
					if (gridDescriptor [x, y] != (int)TileType.BlockedTile) {
						bool again = false;
						// Make sure that we don't create matches in the start position
						do {
							int t = Random.Range (0, maxPieces);
							switch (gridDescriptor [x, y]) {
							case (int)TileType.TodoNormal:
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseNormal [t], new Vector3 (xp, yp, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, (PieceColour)t);
								break;
							case (int)TileType.TodoStrong:
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseStrong [t], new Vector3 (xp, yp, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, (PieceColour)t);
								break;
							case (int)TileType.TodoExtraStrong:
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseExtraStrong [t], new Vector3 (xp, yp, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, (PieceColour)t);
								break;
							case (int)TileType.TodoSuperStrong:
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseSuperStrong [t], new Vector3 (xp, yp, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, (PieceColour)t);
								break;
							}
							if (CheckTileMatchX (x, y, true) || CheckTileMatchY (x, y, true)) {
								DestroyImmediate (PlayingPieces [x, y].Piece);
								PlayingPieces [x, y] = null;
								again = true;
							} else
								again = false;
						} while (again);
						// Set the values and tell the piece to move in position
						PlayingPieces [x, y].pieceScript.currentStrenght = (TileType)gridDescriptor [x, y];
						PlayingPieces [x, y].pieceScript.MoveTo (x, y, zPiecePosition);
						PlayingPieces [x, y].Piece.transform.localScale = generalScale;
					}
				}
			}
		}
		started = true;
	}
	
	/// <summary>
	/// Gets the pieces to use.
	/// </summary>
	private void GetPiecesToUse ()
	{
		// Clear all the containers
		piecesToUseNormal.Clear ();
		piecesToUseStrong.Clear ();
		piecesToUseExtraStrong.Clear ();
		piecesToUseSuperStrong.Clear ();
		
		// Pick random pieces up to the board's maximum allowed pieces
		for (int i = 0; i < maxPieces; i++) {
			bool redo = true;
			int tempIdx = 0;
			do {
				// Pick one
				tempIdx = Random.Range (0, PiecesNormal.Count);				
				GameObject tempObj = PiecesNormal [tempIdx];
				// If we didn't pick this yet we keep it
				if (!piecesToUseNormal.Contains (tempObj)) {
					piecesToUseNormal.Add (tempObj);
					redo = false;
				}
			} while (redo);
			// Now assign the same pieces in their stronger versions
			piecesToUseStrong.Add (PiecesStrong [tempIdx]);
			piecesToUseExtraStrong.Add (PiecesExtraStrong [tempIdx]);
			piecesToUseSuperStrong.Add (PiecesSuperStrong [tempIdx]);
		}
	}

	/// <summary>
	/// A version of the grid made by just integers
	/// for fast access and evaluation
	/// </summary>
	void FillIntGrid ()
	{
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				intGridDescriptor [x, y] = new int ();
				if (PlayingPieces [x, y] != null)
					intGridDescriptor [x, y] = (int)PlayingPieces [x, y].Type;
				else
					intGridDescriptor [x, y] = -1;
			}
		}
	}
	
	/// <summary>
	/// Activates the hint.
	/// </summary>
	/// <param name='x1'>
	/// X1.
	/// </param>
	/// <param name='y1'>
	/// Y1.
	/// </param>
	/// <param name='x2'>
	/// X2.
	/// </param>
	/// <param name='y2'>
	/// Y2.
	/// </param>
	private void ActivateHint (int x1, int y1, int x2, int y2)
	{
		hintTimer = 0;
		if (hintEffect != null) {
			Instantiate (hintEffect, PlayingPieces [x1, y1].Piece.transform.position, Quaternion.identity);
			Instantiate (hintEffect, PlayingPieces [x2, y2].Piece.transform.position, Quaternion.identity);
			if (hintSound != null)
				audio.PlayOneShot (hintSound);
		}
	}
	
	/// <summary>
	/// Checks the tile match on x.
	/// </summary>
	/// <returns>
	/// The tile match x.
	/// </returns>
	/// <param name='x'>
	/// If set to <c>true</c> x.
	/// </param>
	/// <param name='y'>
	/// If set to <c>true</c> y.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchX (int x, int y, bool justCheck)
	{
		if (!isMatch4)
			return CheckTileMatchX (x, y, x, y, justCheck);
		else
			return CheckTileMatchX4 (x, y, x, y, justCheck);
	}
	
	/// <summary>
	/// Checks the tile match on x given source and destination.
	/// </summary>
	/// <returns>
	/// The tile match x.
	/// </returns>
	/// <param name='x1'>
	/// If set to <c>true</c> x1.
	/// </param>
	/// <param name='y1'>
	/// If set to <c>true</c> y1.
	/// </param>
	/// <param name='x2'>
	/// If set to <c>true</c> x2.
	/// </param>
	/// <param name='y2'>
	/// If set to <c>true</c> y2.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchX (int x1, int y1, int x2, int y2, bool justCheck)
	{
		bool match = false;
		if (x1 < 0 || x1 > columns - 1 || y1 < 0 || y1 > rows - 1 || x2 < 0 || x2 > columns - 1 || y2 < 0 || y2 > rows - 1)
			return false;
		FillIntGrid ();
		if (intGridDescriptor [x1, y1] == -1 || intGridDescriptor [x2, y2] == -1)
			return false;
		int z = intGridDescriptor [x1, y1];
		intGridDescriptor [x1, y1] = intGridDescriptor [x2, y2];
		intGridDescriptor [x2, y2] = z;

		if ((gridDescriptor [x2, y2] != (int)TileType.NoTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x1, y1] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoSuperStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoSuperStrong) &&
            PlayingPieces [x2, y2] != null &&
            PlayingPieces [x2, y2].Piece != null) {
			if (((x2 + 2) < columns) &&
                (intGridDescriptor [x2 + 1, y2] != -1) &&
                (intGridDescriptor [x2 + 2, y2] != -1) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 2, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 + 1, y2].Moving &&
                    !PlayingPieces [x2 + 2, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 + 1, y2].Selected = true;
					PlayingPieces [x2 + 2, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((x2 + 1) < columns) && ((x2 - 1) >= 0) &&
                (intGridDescriptor [x2 + 1, y2] != -1) &&
                (intGridDescriptor [x2 - 1, y2] != -1) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 1, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 + 1, y2].Moving &&
                    !PlayingPieces [x2 - 1, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 + 1, y2].Selected = true;
					PlayingPieces [x2 - 1, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((x2 - 2) >= 0) &&
                (intGridDescriptor [x2 - 1, y2] != -1) &&
                (intGridDescriptor [x2 - 2, y2] != -1) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 2, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 - 1, y2].Moving &&
                    !PlayingPieces [x2 - 2, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 - 1, y2].Selected = true;
					PlayingPieces [x2 - 2, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}
		}

		return match;
	}
	
	/// <summary>
	/// Checks the tile match 4 on x given source and destination.
	/// </summary>
	/// <returns>
	/// The tile match x4.
	/// </returns>
	/// <param name='x1'>
	/// If set to <c>true</c> x1.
	/// </param>
	/// <param name='y1'>
	/// If set to <c>true</c> y1.
	/// </param>
	/// <param name='x2'>
	/// If set to <c>true</c> x2.
	/// </param>
	/// <param name='y2'>
	/// If set to <c>true</c> y2.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchX4 (int x1, int y1, int x2, int y2, bool justCheck)
	{
		bool match = false;
		if (x1 < 0 || x1 > columns - 1 || y1 < 0 || y1 > rows - 1 || x2 < 0 || x2 > columns - 1 || y2 < 0 || y2 > rows - 1)
			return false;
		FillIntGrid ();
		if (intGridDescriptor [x1, y1] == -1 || intGridDescriptor [x2, y2] == -1)
			return false;
		int z = intGridDescriptor [x1, y1];
		intGridDescriptor [x1, y1] = intGridDescriptor [x2, y2];
		intGridDescriptor [x2, y2] = z;

		if ((gridDescriptor [x2, y2] != (int)TileType.NoTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x1, y1] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoSuperStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoSuperStrong) &&
            PlayingPieces [x2, y2] != null &&
            PlayingPieces [x2, y2].Piece != null) {
			if (((x2 + 3) < columns) &&
                (intGridDescriptor [x2 + 1, y2] != -1) &&
                (intGridDescriptor [x2 + 2, y2] != -1) &&
                (intGridDescriptor [x2 + 3, y2] != -1) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 3, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 + 3, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 2, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 3, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 + 1, y2].Moving &&
                    !PlayingPieces [x2 + 2, y2].Moving &&
                    !PlayingPieces [x2 + 3, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 + 1, y2].Selected = true;
					PlayingPieces [x2 + 2, y2].Selected = true;
					PlayingPieces [x2 + 3, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((x2 + 2) < columns) && ((x2 - 1) >= 0) &&
                (intGridDescriptor [x2 + 1, y2] != -1) &&
                (intGridDescriptor [x2 + 2, y2] != -1) &&
                (intGridDescriptor [x2 - 1, y2] != -1) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 + 2, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 2, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 1, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 + 1, y2].Moving &&
                    !PlayingPieces [x2 + 2, y2].Moving &&
                    !PlayingPieces [x2 - 1, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 + 1, y2].Selected = true;
					PlayingPieces [x2 + 2, y2].Selected = true;
					PlayingPieces [x2 - 1, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((x2 + 1) < columns) && ((x2 - 2) >= 0) &&
                (intGridDescriptor [x2 + 1, y2] != -1) &&
                (intGridDescriptor [x2 - 2, y2] != -1) &&
                (intGridDescriptor [x2 - 1, y2] != -1) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 + 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 + 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 2, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 1, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 + 1, y2].Moving &&
                    !PlayingPieces [x2 - 2, y2].Moving &&
                    !PlayingPieces [x2 - 1, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 + 1, y2].Selected = true;
					PlayingPieces [x2 - 2, y2].Selected = true;
					PlayingPieces [x2 - 1, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((x2 - 3) >= 0) &&
                (intGridDescriptor [x2 - 1, y2] != -1) &&
                (intGridDescriptor [x2 - 2, y2] != -1) &&
                (intGridDescriptor [x2 - 3, y2] != -1) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 3, y2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2 - 1, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 2, y2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2 - 3, y2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 1, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 2, y2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2 - 3, y2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2 - 1, y2].Moving &&
                    !PlayingPieces [x2 - 2, y2].Moving &&
                    !PlayingPieces [x2 - 3, y2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2 - 1, y2].Selected = true;
					PlayingPieces [x2 - 2, y2].Selected = true;
					PlayingPieces [x2 - 3, y2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}
		}

		return match;
	}
	
	/// <summary>
	/// Checks the tile match on y.
	/// </summary>
	/// <returns>
	/// The tile match y.
	/// </returns>
	/// <param name='x'>
	/// If set to <c>true</c> x.
	/// </param>
	/// <param name='y'>
	/// If set to <c>true</c> y.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchY (int x, int y, bool justCheck)
	{
		if (!isMatch4)
			return CheckTileMatchY (x, y, x, y, justCheck);
		else
			return CheckTileMatchY4 (x, y, x, y, justCheck);
	}
	
	/// <summary>
	/// Checks the tile match on y given source and destination.
	/// </summary>
	/// <returns>
	/// The tile match y.
	/// </returns>
	/// <param name='x1'>
	/// If set to <c>true</c> x1.
	/// </param>
	/// <param name='y1'>
	/// If set to <c>true</c> y1.
	/// </param>
	/// <param name='x2'>
	/// If set to <c>true</c> x2.
	/// </param>
	/// <param name='y2'>
	/// If set to <c>true</c> y2.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchY (int x1, int y1, int x2, int y2, bool justCheck)
	{
		bool match = false;
		if (x1 < 0 || x1 > columns - 1 || y1 < 0 || y1 > rows - 1 || x2 < 0 || x2 > columns - 1 || y2 < 0 || y2 > rows - 1)
			return false;
		FillIntGrid ();
		if (intGridDescriptor [x1, y1] == -1 || intGridDescriptor [x2, y2] == -1)
			return false;
		int z = intGridDescriptor [x1, y1];
		intGridDescriptor [x1, y1] = intGridDescriptor [x2, y2];
		intGridDescriptor [x2, y2] = z;

		if ((gridDescriptor [x2, y2] != (int)TileType.NoTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x1, y1] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoSuperStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoSuperStrong) &&
            PlayingPieces [x2, y2] != null &&
            PlayingPieces [x2, y2].Piece != null) {
			if (((y2 + 2) < rows) &&
                (intGridDescriptor [x2, y2 + 1] != -1) &&
                (intGridDescriptor [x2, y2 + 2] != -1) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 + 1].Moving &&
                    !PlayingPieces [x2, y2 + 2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 + 1].Selected = true;
					PlayingPieces [x2, y2 + 2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((y2 + 1) < rows) && ((y2 - 1) >= 0) &&
                (intGridDescriptor [x2, y2 + 1] != -1) &&
                (intGridDescriptor [x2, y2 - 1] != -1) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 1])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 + 1].Moving &&
                    !PlayingPieces [x2, y2 - 1].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 + 1].Selected = true;
					PlayingPieces [x2, y2 - 1].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((y2 - 2) >= 0) &&
                (intGridDescriptor [x2, y2 - 1] != -1) &&
                (intGridDescriptor [x2, y2 - 2] != -1) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 2])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 - 1].Moving &&
                    !PlayingPieces [x2, y2 - 2].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 - 1].Selected = true;
					PlayingPieces [x2, y2 - 2].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}
		}

		return match;
	}
	
	/// <summary>
	/// Checks the tile match 4 on y given source and destination.
	/// </summary>
	/// <returns>
	/// The tile match y4.
	/// </returns>
	/// <param name='x1'>
	/// If set to <c>true</c> x1.
	/// </param>
	/// <param name='y1'>
	/// If set to <c>true</c> y1.
	/// </param>
	/// <param name='x2'>
	/// If set to <c>true</c> x2.
	/// </param>
	/// <param name='y2'>
	/// If set to <c>true</c> y2.
	/// </param>
	/// <param name='justCheck'>
	/// If set to <c>true</c> just check.
	/// </param>
	internal bool CheckTileMatchY4 (int x1, int y1, int x2, int y2, bool justCheck)
	{
		bool match = false;
		if (x1 < 0 || x1 > columns - 1 || y1 < 0 || y1 > rows - 1 || x2 < 0 || x2 > columns - 1 || y2 < 0 || y2 > rows - 1)
			return false;
		FillIntGrid ();
		if (intGridDescriptor [x1, y1] == -1 || intGridDescriptor [x2, y2] == -1)
			return false;
		int z = intGridDescriptor [x1, y1];
		intGridDescriptor [x1, y1] = intGridDescriptor [x2, y2];
		intGridDescriptor [x2, y2] = z;

		if ((gridDescriptor [x2, y2] != (int)TileType.NoTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x1, y1] != (int)TileType.BlockedTile) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoExtraStrong) &&
            (gridDescriptor [x2, y2] != (int)TileType.TodoSuperStrong) &&
            (gridDescriptor [x1, y1] != (int)TileType.TodoSuperStrong) &&
            PlayingPieces [x2, y2] != null &&
            PlayingPieces [x2, y2].Piece != null) {
			if (((y2 + 3) < rows) &&
                (intGridDescriptor [x2, y2 + 1] != -1) &&
                (intGridDescriptor [x2, y2 + 2] != -1) &&
                (intGridDescriptor [x2, y2 + 3] != -1) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 3] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 + 3] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 3])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 + 1].Moving &&
                    !PlayingPieces [x2, y2 + 2].Moving &&
                    !PlayingPieces [x2, y2 + 3].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 + 1].Selected = true;
					PlayingPieces [x2, y2 + 2].Selected = true;
					PlayingPieces [x2, y2 + 3].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((y2 + 2) < rows) && ((y2 - 1) >= 0) &&
                (intGridDescriptor [x2, y2 + 1] != -1) &&
                (intGridDescriptor [x2, y2 + 2] != -1) &&
                (intGridDescriptor [x2, y2 - 1] != -1) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 + 2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 1])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 + 1].Moving &&
                    !PlayingPieces [x2, y2 + 2].Moving &&
                    !PlayingPieces [x2, y2 - 1].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 + 1].Selected = true;
					PlayingPieces [x2, y2 + 2].Selected = true;
					PlayingPieces [x2, y2 - 1].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((y2 + 1) < rows) && ((y2 - 2) >= 0) &&
                (intGridDescriptor [x2, y2 + 1] != -1) &&
                (intGridDescriptor [x2, y2 - 2] != -1) &&
                (intGridDescriptor [x2, y2 - 1] != -1) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 + 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 + 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 1])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 + 1].Moving &&
                    !PlayingPieces [x2, y2 - 2].Moving &&
                    !PlayingPieces [x2, y2 - 1].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 + 1].Selected = true;
					PlayingPieces [x2, y2 - 2].Selected = true;
					PlayingPieces [x2, y2 - 1].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}

			if (((y2 - 3) >= 0) &&
                (intGridDescriptor [x2, y2 - 1] != -1) &&
                (intGridDescriptor [x2, y2 - 2] != -1) &&
                (intGridDescriptor [x2, y2 - 3] != -1) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 3] != (int)TileType.NoTile) &&
                (gridDescriptor [x2, y2 - 1] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 2] != (int)TileType.BlockedTile) &&
                (gridDescriptor [x2, y2 - 3] != (int)TileType.BlockedTile) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 1]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 2]) &&
                (intGridDescriptor [x2, y2] == intGridDescriptor [x2, y2 - 3])) {
				if (!justCheck &&
                    !PlayingPieces [x1, y1].Moving &&
                    !PlayingPieces [x2, y2 - 1].Moving &&
                    !PlayingPieces [x2, y2 - 2].Moving &&
                    !PlayingPieces [x2, y2 - 3].Moving) {
					PlayingPieces [x1, y1].Selected = true;
					PlayingPieces [x2, y2 - 1].Selected = true;
					PlayingPieces [x2, y2 - 2].Selected = true;
					PlayingPieces [x2, y2 - 3].Selected = true;
					match = true;
				} else {
					if (hintTimer >= hintTime) {
						ActivateHint (x1, y1, x2, y2);
					}
					return true;
				}
			}
		}

		return match;
	}

	// Update is called once per frame
	void Update ()
	{
		// If the game is paused call the delegate
		if (GamePaused && GamePausedMethod != null)
			GamePausedMethod ();
		// If the time is finished call the delegate
		if (TimeOut && TimedOutMethod != null)
			TimedOutMethod ();
		// if the level has been cleared call the delegate
		if (CleanSlate && LevelClearedMethod != null)
			LevelClearedMethod ();
		// Don't continue if the game is ended or paused
		if (TimeOut || CleanSlate || GamePaused)
			return;
		// Display the points
		scoreLabel.text = ScoresManager.CurrentPoints.ToString ();
		
		// Here follows the mobile touch implementation.
		// feel free to change it with your preferred gesture manager 
		// if you want to, just mind the logic ;)
#if UNITY_IPHONE || UNITY_ANDROID
        tap = false;
        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);
            tap = touch.tapCount > 0 ? true : false;
            if (tap)
            {
                tapPosition = touch.position;
                Vector3 wCoord = Camera.main.ScreenToWorldPoint(new Vector3(tapPosition.x, tapPosition.y, Camera.main.transform.position.z)) * -1f;
                GridPoint mPos = GridPositions.GetGridPosition(new Vector2(wCoord.x, wCoord.y));
                if (mPos.x != -1 && PlayingPieces[mPos.x, mPos.y] != null)
                {
                    PlayingPieces[mPos.x, mPos.y].pieceScript.Clicked();
                }
            }
        }
		
		// **********************************************************************
		// * Piece of code to QUIT the app: uncomment it for testing if needed. *
		// * DO NOT LEAVE IT HERE UNCOMMENTED IN YOUR RELEASED GAME!!!          *
		// **********************************************************************
        /*
		if (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Home) || Input.GetKey(KeyCode.Menu))
            Application.Quit();
        */
#endif
		// Advance the timers
		gameTimer += Time.deltaTime;
		hintTimer += Time.deltaTime;
		checkTimer += Time.deltaTime;
		
		// Scale the time for use in the user interface
		float remainingTimeScaled = ((levelTime * 60f) - gameTimer) / (levelTime * 60f);
		// Display the remaining time
		timeBarSlider.sliderValue = remainingTimeScaled;
		
		// Check against timeout
		if (remainingTimeScaled <= 0f) {
			TimeOut = true;
			return;
		}
		
		// Check if the active marker is needed and if it's properly positioned
		if (ActivePiece.x != -1) {
			activePieceVectorPosition = GridPositions.GetVector (ActivePiece.x, ActivePiece.y);
			activeMarker.transform.position = new Vector3 (activePieceVectorPosition.x, activePieceVectorPosition.y, zActiveEffectPosition);
			activeMarker.Play(true);
		} else {
			activeMarker.Stop(true);
		}
		
		// Main checks, only do this every 100ms
		if (started && checkTimer >= 0.1f && !CleanSlate) {
			checkTimer = 0.0f;
			
			// if pieces have been destroyed
			// slide down and create new ones
			if (destroyed) {
				destroyed = false;
				for (int x = 0; x < columns; x++) {
					SlideDown (x, rows - 1);
				}
				NewPieces ();
				wasMoving = true;
				// No need to continue
				return;
			}
			
			// Check if there are still moving pieces on the board
			// and if so just leave to allow them to reach their position
			for (int y = 0; y < rows; y++) {
				for (int x = 0; x < columns; x++) {
					if (PlayingPieces [x, y] != null && PlayingPieces [x, y].Piece != null && PlayingPieces [x, y].Moving) {
						// Mark the activity
						wasMoving = true;
						// just leave
						return;
					}
				}
			}
			
			// As nothing is moving check if the level is clear
			if (IsLevelClear ()) {
				CleanSlate = true;
				return;
			}
			// Check if there are possible moves
			CheckMoves ();
			
			if (!wasMoving)
				return;
			wasMoving = false;
			
			// don't allow the player to move while we are checking matches
			PlayerCanMove = false;
			destroyed = false;
			
			// Mark all the matching pieces so that we can
			// loop throught and destroy them
			for (int y = 0; y < rows; y++) {
				for (int x = 0; x < columns; x++) {
					CheckTileMatchX (x, y, false);
					CheckTileMatchY (x, y, false);
				}
			}
			// Clear the countrers
			int specialCount = 0;
			totalDestroyedPieces = 0;
			totalNormalPiecesDestroyed = 0;
			totalStrongPiecesDestroyed = 0;
			totalSuperStrongPiecesDestroyed = 0;
			totalExtraStrongPiecesDestroyed = 0;
			
			// Destruction of the pieces marked as matching
			for (int y = 0; y < rows; y++) {
				for (int x = 0; x < columns; x++) {
					if (PlayingPieces [x, y] != null && PlayingPieces [x, y].Selected && PlayingPieces [x, y].Piece != null) {
						// Keep track of the special pieces
						if (PlayingPieces [x, y].SpecialPiece)
							specialCount++;
						// Special pieces action unlocking the locked cell
						if (specialCount == 3) {
							specialCount = 0;
							for (int a = 0; a < columns; a++) {
								for (int b = 0; b < rows; b++) {
									if (gridDescriptor [a, b] == (int)TileType.BlockedTile) {
										grid [a, b].Tile.renderer.material = tileTodoNormalMaterial;
										grid [a, b].Type = TileType.TodoNormal;
										totalBlockedTiles--;
										gridDescriptor [a, b] = (int)TileType.TodoNormal;
										vectorPosition = GridPositions.GetVector (a, b);
										GameObject tmpEffect = Instantiate (pieceDestroyedEffect, new Vector3 (vectorPosition.x, vectorPosition.y, zTilePosition), Quaternion.identity) as GameObject;
										tmpEffect.transform.localScale = generalScale;
										a = rows;
										b = columns;
									}
								}
							}
						}
						// instantiate the destruction effect
						GameObject tmpEffect0 = Instantiate (pieceDestroyedEffect, PlayingPieces [x, y].pieceScript.MyTransform.position, Quaternion.identity) as GameObject;
						tmpEffect0.transform.localScale = generalScale;
						currentPosition = GridPositions.GetVector (x, y);
						// Now evaluate the type of piece
						switch (PlayingPieces [x, y].pieceScript.currentStrenght) {
						case TileType.TodoNormal: // a normal piece, just destroy it
							Destroy (PlayingPieces [x, y].Piece);
							PlayingPieces [x, y].Piece = null;
							PlayingPieces [x, y].Selected = false;
							PlayingPieces [x, y] = null;
							gridDescriptor [x, y] = (int)TileType.Done;
							grid [x, y].Tile.renderer.material = tileDoneMaterial;
							ScoresManager.AddPoints (PointsNormal);
							totalNormalPiecesDestroyed++;
							break;
						case TileType.TodoStrong: // Downgrade the piece
							Destroy (PlayingPieces [x, y].Piece);
							PlayingPieces [x, y].Piece = Instantiate (piecesToUseNormal [(int)PlayingPieces [x, y].Type], new Vector3 (currentPosition.x, currentPosition.y, zPiecePosition), Quaternion.identity) as GameObject;
							PlayingPieces [x, y].pieceScript.currentStrenght = TileType.TodoNormal;
							PlayingPieces [x, y].Selected = false;
							PlayingPieces [x, y].Piece.transform.localScale = generalScale;
							gridDescriptor [x, y] = (int)TileType.TodoNormal;
							grid [x, y].Type = TileType.TodoNormal;
							ScoresManager.AddPoints (PointsStrong);
							totalStrongPiecesDestroyed++;
							break;
						case TileType.TodoExtraStrong: // Downgrade the piece
							Destroy (PlayingPieces [x, y].Piece);
							PlayingPieces [x, y].Piece = Instantiate (piecesToUseStrong [(int)PlayingPieces [x, y].Type], new Vector3 (currentPosition.x, currentPosition.y, zPiecePosition), Quaternion.identity) as GameObject;
							PlayingPieces [x, y].pieceScript.currentStrenght = TileType.TodoStrong;
							PlayingPieces [x, y].Selected = false;
							PlayingPieces [x, y].Piece.transform.localScale = generalScale;
							gridDescriptor [x, y] = (int)TileType.TodoStrong;
							grid [x, y].Type = TileType.TodoStrong;
							ScoresManager.AddPoints (PointsExtraStrong);
							totalExtraStrongPiecesDestroyed++;
							break;
						case TileType.TodoSuperStrong: // Downgrade the piece
							Destroy (PlayingPieces [x, y].Piece);
							PlayingPieces [x, y].Piece = Instantiate (piecesToUseExtraStrong [(int)PlayingPieces [x, y].Type], new Vector3 (currentPosition.x, currentPosition.y, zPiecePosition), Quaternion.identity) as GameObject;
							PlayingPieces [x, y].pieceScript.currentStrenght = TileType.TodoExtraStrong;
							PlayingPieces [x, y].Selected = false;
							PlayingPieces [x, y].Piece.transform.localScale = generalScale;
							gridDescriptor [x, y] = (int)TileType.TodoExtraStrong;
							grid [x, y].Type = TileType.TodoExtraStrong;
							ScoresManager.AddPoints (PointsSuperStrong);
							totalSuperStrongPiecesDestroyed++;
							break;
						}
						audio.PlayOneShot (destroyPiece);
						destroyed = true;
						totalDestroyedPieces++;
					}
				}
			}
			
			// If there aren't any blocked tiles then destroy 
			// the remaining special pieces on the board
			if (totalBlockedTiles == 0) {
				for (int a = 0; a < columns; a++) {
					for (int b = 0; b < rows; b++) {
						if (PlayingPieces [a, b] != null && PlayingPieces [a, b].SpecialPiece) {
							Destroy (PlayingPieces [a, b].Piece);
							PlayingPieces [a, b].Piece = null;
							PlayingPieces [a, b].Selected = false;
							PlayingPieces [a, b] = null;
							destroyed = true;
						}
					}
				}
			}
			
			// Check if we did destroy any piece
			if (totalDestroyedPieces > 0) {
				// Update the amount of matched tiles on  screen
				if (matchesLabel)
					matchesLabel.text = totalDestroyedPieces.ToString ();
				// Call the delegate to 
				if (CheckBonusMethod != null)
					CheckBonusMethod ();
			}
			// Now the player can move again
			PlayerCanMove = true;
			// Reset the hint counter
			hintTimer = 0f;
		}
	}
	
	/// <summary>
	/// Determines whether this level is clear.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is level clear; otherwise, <c>false</c>.
	/// </returns>
	private bool IsLevelClear ()
	{
		for (int y = 0; y < rows; y++) {
			for (int x = 0; x < columns; x++) {
				if ((gridDescriptor [x, y] != (int)TileType.NoTile) &&
                    (gridDescriptor [x, y] != (int)TileType.Done)) {
					return false;
				}
			}
		}
		return true;
	}
	
	/// <summary>
	/// Generates new pieces from the pool of the pieces in play
	/// in the current game.
	/// </summary>
	private void NewPieces ()
	{
		for (int x = 0; x < columns; x++) {
			for (int y = 0; y < rows; y++) {
				if (gridDescriptor [x, y] != (int)TileType.BlockedTile && gridDescriptor [x, y] != (int)TileType.NoTile && PlayingPieces [x, y] == null) {
					int chance = Random.Range (0, 7);
					Vector2 v0 = GridPositions.GetVector (x, y);
					if (totalBlockedTiles > 0 && chance == 3) {
						PlayingPieces [x, y] = new PlayingPiece (Instantiate (specialPiece, new Vector3 (v0.x, v0.y, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, PieceColour.Special);
						PlayingPieces [x, y].SpecialPiece = true;
					} else {
						bool again = false;
						do {
							int t = Random.Range (0, maxPieces);
							if (!newPiecesFromTop)
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseNormal [t], new Vector3 (v0.x, v0.y, zPiecePosition - Random.Range (20f, 30f)), Quaternion.identity) as GameObject, (PieceColour)t);
							else
								PlayingPieces [x, y] = new PlayingPiece (Instantiate (piecesToUseNormal [t], new Vector3 (v0.x, v0.y + Random.Range (20f, 30f), zPiecePosition), Quaternion.identity) as GameObject, (PieceColour)t);
							if (CheckTileMatchX (x, y, true) || CheckTileMatchY (x, y, true)) {
								DestroyImmediate (PlayingPieces [x, y].Piece);
								PlayingPieces [x, y] = null;
								again = true;
							} else
								again = false;
						} while (again);
					}
					audio.PlayOneShot (newPiece);
					PlayingPieces [x, y].pieceScript.currentStrenght = TileType.TodoNormal;
					PlayingPieces [x, y].pieceScript.MoveTo (x, y, zPiecePosition);
					PlayingPieces [x, y].Selected = false;
					PlayingPieces [x, y].Piece.transform.localScale = generalScale;
				} else if (gridDescriptor [x, y] != (int)TileType.NoTile && PlayingPieces [x, y] != null && PlayingPieces [x, y].pieceScript.currentStrenght != TileType.TodoNormal) {
					break;
				}
			}
		}
		if (ActivePiece.x != -1 && PlayingPieces [ActivePiece.x, ActivePiece.y] == null)
			ActivePiece.x = -1;
	}
	
	/// <summary>
	/// Checks if there are possible moves or else shuffle.
	/// </summary>
	void CheckMoves ()
	{
		if (gameStyle == GameStyle.Marinas && ActivePiece.x == -1)
			return;
		GridPoint gp = new GridPoint ();
		if (gameStyle == GameStyle.Marinas) {
			for (gp.x = 0; gp.x < columns; gp.x++) {
				for (gp.y = 0; gp.y < rows; gp.y++) {
					if (gp != ActivePiece) {
						if (!isMatch4) {
							if (CheckTileMatchX (ActivePiece.x, ActivePiece.y, gp.x, gp.y, true) ||
                                CheckTileMatchY (ActivePiece.x, ActivePiece.y, gp.x, gp.y, true) ||
                                CheckTileMatchX (gp.x, gp.y, ActivePiece.x, ActivePiece.y, true) ||
                                CheckTileMatchY (gp.x, gp.y, ActivePiece.x, ActivePiece.y, true))
								return;
						} else {
							if (CheckTileMatchX4 (ActivePiece.x, ActivePiece.y, gp.x, gp.y, true) ||
                                CheckTileMatchY4 (ActivePiece.x, ActivePiece.y, gp.x, gp.y, true) ||
                                CheckTileMatchX4 (gp.x, gp.y, ActivePiece.x, ActivePiece.y, true) ||
                                CheckTileMatchY4 (gp.x, gp.y, ActivePiece.x, ActivePiece.y, true))
								return;
						}
					}
				}
			}
		} else {
			for (gp.x = 0; gp.x < columns; gp.x++) {
				for (gp.y = 0; gp.y < rows; gp.y++) {
					if (!isMatch4) {
						if (CheckTileMatchX (gp.x, gp.y, gp.x + 1, gp.y, true) ||
                            CheckTileMatchY (gp.x, gp.y, gp.x - 1, gp.y, true) ||
                            CheckTileMatchX (gp.x, gp.y, gp.x, gp.y + 1, true) ||
                            CheckTileMatchY (gp.x, gp.y, gp.x, gp.y - 1, true))
							return;
					} else {
						if (CheckTileMatchX4 (gp.x, gp.y, gp.x + 1, gp.y, true) ||
                            CheckTileMatchY4 (gp.x, gp.y, gp.x - 1, gp.y, true) ||
                            CheckTileMatchX4 (gp.x, gp.y, gp.x, gp.y + 1, true) ||
                            CheckTileMatchY4 (gp.x, gp.y, gp.x, gp.y - 1, true))
							return;
					}
				}
			}
		}
		// there aren't any possible moves, so shuffle the pieces
		gridPointsList.Clear ();
		GridPoint gp1 = new GridPoint ();
		for (int count = 0; count < ((rows * columns) / 4); count++) {
			gp.x = Random.Range (0, columns);
			gp.y = Random.Range (0, rows / 2);
			while ((gridDescriptor[gp.x, gp.y] == (int)TileType.NoTile) ||
                   (PlayingPieces[gp.x, gp.y] == null) ||
                   (PlayingPieces[gp.x, gp.y].Piece == null) ||
                   gridPointsList.Contains(gp) ||
                   PlayingPieces[gp.x, gp.y].pieceScript.currentStrenght != TileType.TodoNormal) {
				gp.x = Random.Range (0, columns);
				gp.y = Random.Range (0, rows / 2);
			}

			gridPointsList.Add (gp);

			gp1.x = Random.Range (0, columns);
			gp1.y = Random.Range (5, rows / 2);
			while ((gridDescriptor[gp1.x, gp1.y] == (int)TileType.NoTile) ||
                   (PlayingPieces[gp1.x, gp1.y] == null) ||
                   (PlayingPieces[gp1.x, gp1.y].Piece == null) ||
                   gridPointsList.Contains(gp1) ||
                   PlayingPieces[gp1.x, gp1.y].pieceScript.currentStrenght != TileType.TodoNormal) {
				gp1.x = Random.Range (0, columns);
				gp1.y = Random.Range (rows / 2, rows);
			}
			gridPointsList.Add (gp1);
			PlayingPieces [gp.x, gp.y].pieceScript.MoveTo (gp1.x, gp1.y);
			PlayingPieces [gp1.x, gp1.y].pieceScript.MoveTo (gp.x, gp.y);
			PlayingPiece tmp = PlayingPieces [gp.x, gp.y];
			PlayingPieces [gp.x, gp.y] = PlayingPieces [gp1.x, gp1.y];
			PlayingPieces [gp1.x, gp1.y] = tmp;
		}
		gridPointsList.Clear ();
	}
	
	/// <summary>
	/// Slides the tiles down.
	/// </summary>
	/// <param name='x'>
	/// X.
	/// </param>
	/// <param name='y'>
	/// Y.
	/// </param>
	private void SlideDown (int x, int y)
	{
		for (int y1 = y; y1 >= 1; y1--) {
			if (gridDescriptor [x, y1] != (int)TileType.NoTile && gridDescriptor [x, y1] != (int)TileType.BlockedTile && PlayingPieces [x, y1] == null) {
				int fy = -1;
				for (int z = y1 - 1; z >= 0; z--) {
					if (gridDescriptor [x, z] != (int)TileType.NoTile && gridDescriptor [x, y1] != (int)TileType.BlockedTile && PlayingPieces [x, z] != null) {
						fy = z;
						break;
					}
				}
				if (fy != -1) {
					if (PlayingPieces [x, fy].pieceScript.currentStrenght == TileType.TodoNormal) {
						PlayingPieces [x, fy].Selected = false;
						PlayingPieces [x, fy].pieceScript.MoveTo (x, y1);
						PlayingPieces [x, y1] = PlayingPieces [x, fy];
						PlayingPieces [x, fy] = null;
						audio.PlayOneShot (SlidePiece);
						SlideDown (x, y1);
						//PlayerCanMove = false;
						break;
					}
				}
			}
		}
	}
}
