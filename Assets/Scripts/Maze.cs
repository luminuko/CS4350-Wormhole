﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour {

	public IntVector2 size;
	public MazeCell cellPrefab;
	public float generationStepDelay;
	public MazePassage passagePrefab;
	public MazeWall[] wallPrefabs;

	private MazeCell[,] cells;

    public MazeRoomSettings[] roomSettings;
    private List<MazeRoom> rooms = new List<MazeRoom>();

    public MazeDoor doorPrefab;
    [Range(0f, 1f)]
    public float doorProbability;

    private GameObject player;
	private GameObject gameManager;
	private CharacterMotor playerCharacterMotor;


	public int zombieAmount = 0;
	public int potionAmount = 5;
	public int crateAmount = 5;
	public int golemAmount = 0;
	public int clockAmount = 0;
	public int teleportAmount = 0;
	public GameObject clockInstance;
	public GameObject golemInstance;
	public GameObject zombieInstance;
	public GameObject potionInstance;
	public GameObject crateInstance;
	private BoxCollider mazeRangeCollider;
	public Feedback feedback;
	public ZombieSpawner spawner;
	private ArrayList teleporterList ;
	public int skeletonAmount = 0;
	public GameObject skeletonInstance;
	public int monsterAmount = 0;
	public GameObject monsterInstance;
	public GameObject teleportInstance;

	float DROP_DISTANCE = 5f;
	Vector2 baseI; //x: minI, y:maxI
	Vector2 baseJ; //x: minJ, y:minJ

    public List<MazeEdge> edgeList = new List<MazeEdge>();

    public class MazeEdge {
        public int edgeType; // 0: passage, 1: wall, 2: door
        public MazeCell cellFrom, cellTo;

        public MazeEdge(MazeCell one, MazeCell two, int type)
        {
            cellFrom = one;
            cellTo = two;
            edgeType = type;
        }
    }

    public List<MazeRoom> getAdjacentRooms(MazeCell myCell) {
        //List<MazeEdge> suspectEdges = new List<MazeEdge>();
        List<MazeRoom> neighborRooms = new List<MazeRoom>();
        MazeRoom myRoom = myCell.room;
        List<MazeCell> myCells = myRoom.cells;
        for (int i=0; i<myCells.Count; i++) {
            MazeCell currentCell = myCells[i];
            MazeCellEdge[] currentEdges = currentCell.edges;
            for (int j=0; j<currentEdges.Length; j++) {
                MazeCell neighborCell = currentEdges[j].otherCell;
				if (neighborCell != null) {
					MazeRoom neighborRoom = neighborCell.room;
					if (neighborRoom.settingsIndex != myRoom.settingsIndex){
						//MazeEdge suspectEdge = new MazeEdge(currentCell, neighborCell, 0);
						//suspectEdges.Add(suspectEdge);
						if (neighborRooms.Contains(neighborRoom)) {
							// no duplicates
						} else {
							neighborRooms.Add(neighborRoom);
						}
					}
				}
                
            }
        }
        return neighborRooms;
        /*
        for (int i=0; i<suspectEdges.Count; i++) {
            MazeEdge currentSuspect = suspectEdges[i];
            for (int j=0; j<edgeList.Count; j++) {
                MazeEdge currentEdge = edgeList[j];
                if ((currentSuspect.cellFrom == currentEdge.cellFrom && currentSuspect.cellTo == currentEdge.cellTo) ||
                    (currentSuspect.cellFrom == currentEdge.cellTo && currentSuspect.cellTo == currentEdge.cellFrom)) {
                    MazeEdge confirmedEdge = currentEdge;
                    if (confirmedEdge.edgeType)
                }
            }
        }
        */
    }


	void Awake(){
		gameManager = GameObject.FindGameObjectWithTag ("GameManager");
        feedback = GameObject.FindGameObjectWithTag("Feedback").GetComponent<Feedback>();
		spawner = GameObject.FindGameObjectWithTag ("GameManager").GetComponent<ZombieSpawner> ();
		teleporterList = new ArrayList();
	}

	void OnTriggerExit (Collider other)
	{
		if(other.gameObject.tag == "Player")
		{
			feedback.enterMaze();
			spawner.broadcastPlayerEntersMaze();
		}
	}

	void OnTriggerEnter (Collider other){
		if(other.gameObject.tag == "Player")
		{
			spawner.playerExitsMaze();
		}
	}
	
	/*
	void OnTriggerStay (Collider other)
	{
		if(other.gameObject.tag == "Player")
		{
			//??? do nothing in base
		}
	}
    */

	public IntVector2 RandomCoordinates {
		get {
			return new IntVector2(Random.Range(0, size.x), Random.Range(0, size.z));
		}
	}

	float getRandomXInMaze() {
		float randomX = Random.Range (-2 * size.x, 2 * size.x);
		return randomX;
	}

	float getRandomZInMaze() {
		float randomZ = Random.Range (-2 * size.z, 2 * size.z);
		return randomZ;
	}

	public bool ContainsCoordinates (IntVector2 coordinate) {
		return coordinate.x >= 0 && coordinate.x < size.x && coordinate.z >= 0 && coordinate.z < size.z;
	}

	public MazeCell GetCell (IntVector2 coordinates) {
		try {
			return cells[coordinates.x, coordinates.z];
		} catch (System.IndexOutOfRangeException exception) {
			return null;
		}
	}

	public IEnumerator Generate () {
		baseI = new Vector2(1,7);
		//baseJ = new Vector2(size.z-1, size.z-7);
		baseJ = new Vector2(1,7);

		/*
		GameObject leftWall = GameObject.Find ("Long Wall Left");
		GameObject rightWall = GameObject.Find ("Long Wall Right");
		float leftNewZ = 100f + 2 * size.z;
		float NewX = -2 * (size.x+1) + 0.2f;
		float rightNewZ = -100f - 2 * size.z;
		leftWall.transform.position = new Vector3 (NewX, leftWall.transform.position.y, leftNewZ);
		rightWall.transform.position = new Vector3 (NewX, rightWall.transform.position.y, rightNewZ);
		*/

		player = GameObject.FindGameObjectWithTag ("Player");
		//playerCharacterMotor = player.GetComponent <CharacterMotor> ();
		WaitForSeconds delay = new WaitForSeconds(generationStepDelay);
		cells = new MazeCell[size.x, size.z];
		List<MazeCell> activeCells = new List<MazeCell>();
		PlopBaseAtSide();
		DoFirstGenerationStep(activeCells);
		while (activeCells.Count > 0) {
			//yield return delay;
			DoNextGenerationStep(activeCells);
		}
		//playerCharacterMotor.movement.gravity = 9.81f;




		/*
		for (int i = 0; i < golemAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);
			Instantiate(golemInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}
		*/

		GameObject exitCell = GameObject.Find ("Maze Cell " + (size.x - 1) + ", " + (size.z - 2));
        foreach (Transform childT in exitCell.transform)
        {
            GameObject child = childT.gameObject;
            if (child.name == "Maze Wall(Clone)" && (childT.rotation.eulerAngles.y > 89))
            {
				{
					if (childT.rotation.eulerAngles.y != 270)//The wall opposite the exit
						Destroy(child);
				}
            }
        }
        GameObject entryCell = GameObject.Find("Maze Cell 0, 1");
        foreach (Transform childT in entryCell.transform)
        {
            GameObject child = childT.gameObject;
            if (child.name == "Maze Wall(Clone)" && (childT.rotation.eulerAngles.y > 269))
            {
                Destroy(child);
            }
        }


        //Destroy(GameObject.Find("Maze Cell 0, 1"));
        //Destroy(exitCell, 0f);
        //GameObject entryCell = GameObject.Find ("Maze Cell "+(int)baseI.y+", "+(int)(baseJ.x+3));
        //GameObject otherSideOfEntryCell = GameObject.Find ("Maze Cell "+(int)(baseI.y+1)+", "+(int)(baseJ.x+3));

        //Destroy(entryCell, 0f);
        //Destroy(otherSideOfEntryCell, 0f);




        for (int i = 0; i < potionAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);
			if(randomX < -2 * size.x+32 && randomZ < -2 * size.z+32){
				float XorZ = Random.Range(0, 1);
				if(XorZ > 0.5){
					randomX = Random.Range (-2 * size.x+32, 2 * size.x);
				} else {
					randomZ = Random.Range (-2 * size.z+32, 2 * size.z);
				}
			}
			randomX = Mathf.Floor(randomX);
			randomZ = Mathf.Floor(randomZ);
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}
			Instantiate(potionInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}

		/*
		for (int i = 0; i < clockAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);
			if(randomX < -2 * size.x+32 && randomZ < -2 * size.z+32){
				float XorZ = Random.Range(0, 1);
				if(XorZ > 0.5){
					randomX = Random.Range (-2 * size.x+32, 2 * size.x);
				} else {
					randomZ = Random.Range (-2 * size.z+32, 2 * size.z);
				}
			}
			randomX = Mathf.Floor(randomX);
			randomZ = Mathf.Floor(randomZ);
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}
			Instantiate(clockInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}
		*/

		for (int i = 0; i < teleportAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);
			if(randomX < -2 * size.x+32 && randomZ < -2 * size.z+32){
				float XorZ = Random.Range(0, 1);
				if(XorZ > 0.5){
					randomX = Random.Range (-2 * size.x+32, 2 * size.x);
				} else {
					randomZ = Random.Range (-2 * size.z+32, 2 * size.z);
				}
			}
			randomX = Mathf.Floor(randomX);
			randomZ = Mathf.Floor(randomZ);
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}
			GameObject t = (GameObject)Instantiate(teleportInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			CustomTeleporter s = t.GetComponentInChildren<CustomTeleporter>();
			s.destinationPad = new Transform[teleportAmount-1];
			teleporterList.Add (t);

		}

		for (int i = 0; i < teleportAmount; i++) {
			GameObject t = (GameObject)teleporterList[i];
			CustomTeleporter s = t.GetComponentInChildren<CustomTeleporter>();
			int index = 0;
			for (int j = 0; j < teleportAmount; j++) {
				if (j != i){
					GameObject target = (GameObject)teleporterList[j];
					s.destinationPad.SetValue(target.GetComponentInChildren<CustomTeleporter>().gameObject.transform, index);
					index++;
				}

			}
		}

		/*
		for (int i = 0; i < crateAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);

			if(randomX < -2 * size.x+32 && randomZ < -2 * size.z+32){
				float XorZ = Random.Range(0, 1);
				if(XorZ > 0.5){
					randomX = Random.Range (-2 * size.x+32, 2 * size.x);
				} else {
					randomZ = Random.Range (-2 * size.z+32, 2 * size.z);
				}
			}
			randomX = Mathf.Floor(randomX);
			randomZ = Mathf.Floor(randomZ);
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}

			Instantiate(crateInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}
		*/
		/*
		for (int i = 0; i < zombieAmount; i++) {
			float randomX = Random.Range (-2 * size.x, 2 * size.x);
			float randomZ = Random.Range (-2 * size.z, 2 * size.z);
			if(randomX < -2 * size.x+40 && randomZ < -2 * size.z+40){
				float XorZ = Random.Range(0, 1);
				if(XorZ > 0.5){
					randomX = Random.Range (-2 * size.x+40, 2 * size.x);
				} else {
					randomZ = Random.Range (-2 * size.z+40, 2 * size.z);
				}
			}
			randomX = Mathf.Floor(randomX);
			randomZ = Mathf.Floor(randomZ);
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}
			Instantiate(zombieInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}

		for (int i = 0; i < skeletonAmount; i++) {
			float randomX = Mathf.Floor(Random.Range (-2 * size.x+45, 2 * size.x));
			float randomZ = Mathf.Floor(Random.Range (-2 * size.z+45, 2 * size.z));
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}
			Instantiate(skeletonInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}

		for (int i = 0; i < monsterAmount; i++) {
			float randomX = Mathf.Floor(Random.Range (-2 * size.x+50, 2 * size.x));
			float randomZ = Mathf.Floor(Random.Range (-2 * size.z+50, 2 * size.z));
			if(randomX % 4 == 0){
				randomX = randomX + 2;
			}
			if(randomZ % 4 == 0){
				randomZ = randomZ + 2;
			}

			Instantiate(monsterInstance, new Vector3(randomX, DROP_DISTANCE, randomZ), Quaternion.identity);
			
		}
		*/
		mazeRangeCollider = gameObject.GetComponent<BoxCollider>();
		yield return new WaitForSeconds(0f);
	}

	private void PlopBaseAtSide(){
		//7x7base
		/*for (int i=(int)baseI.x; i<=(int)baseI.y; i++){
			for (int j=(int)baseJ.x; j<=(int)baseJ.y; j++){
				CreateCell(new IntVector2(i,j));
			}
		}*/
        GameObject baseObj = GameObject.Find("Base");
        baseObj.transform.Translate(-((size.x - 20) * 2) - 29,0, -((size.z - 20) * 2) - 13);
	}

	private void DoFirstGenerationStep (List<MazeCell> activeCells) {
		MazeCell newCell = CreateCell(RandomCoordinates);
		newCell.Initialize(CreateRoom(-1));
		activeCells.Add(newCell);
	}


	private void DoNextGenerationStep (List<MazeCell> activeCells) {
		int currentIndex = activeCells.Count - 1;
		MazeCell currentCell = activeCells[currentIndex];
		if (currentCell.IsFullyInitialized) {
			activeCells.RemoveAt(currentIndex);
			return;
		}
		MazeDirection direction = currentCell.RandomUninitializedDirection;
		IntVector2 coordinates = currentCell.coordinates + direction.ToIntVector2();
		if (ContainsCoordinates(coordinates)) {
			MazeCell neighbor = GetCell(coordinates);
			if (neighbor == null) {
				neighbor = CreateCell(coordinates);
				CreatePassage(currentCell, neighbor, direction);
				activeCells.Add(neighbor);
			}
            else if (currentCell.room == neighbor.room)
            {
                CreatePassageInSameRoom(currentCell, neighbor, direction);
                MazeEdge newEdge = new MazeEdge(currentCell, neighbor, 0);
                edgeList.Add(newEdge);
            }
            else {
				CreateWall(currentCell, neighbor, direction);
                MazeEdge newEdge = new MazeEdge(currentCell, neighbor, 1);
                edgeList.Add(newEdge);
            }
		}
		else {
			CreateWall(currentCell, null, direction);
        }
	}

	private MazeCell CreateCell (IntVector2 coordinates) {
		MazeCell newCell = Instantiate(cellPrefab) as MazeCell;
		cells[coordinates.x, coordinates.z] = newCell;
		newCell.coordinates = coordinates;
		newCell.name = "Maze Cell " + coordinates.x + ", " + coordinates.z;
		newCell.transform.parent = transform;
		newCell.transform.localPosition = new Vector3(coordinates.x - size.x * 0.5f + 0.5f, 0f, coordinates.z - size.z * 0.5f + 0.5f);
		return newCell;
	}

	private void CreatePassage (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazePassage prefab = Random.value < doorProbability ? doorPrefab : passagePrefab;
        MazePassage passage = Instantiate(prefab) as MazePassage;
		passage.Initialize(cell, otherCell, direction);
		passage = Instantiate(prefab) as MazePassage;
        if (passage is MazeDoor)
        {
            otherCell.Initialize(CreateRoom(cell.room.settingsIndex));
            MazeEdge newEdge = new MazeEdge(cell, otherCell, 2);
            edgeList.Add(newEdge);
        }
        else
        {
            otherCell.Initialize(cell.room);
            MazeEdge newEdge = new MazeEdge(cell, otherCell, 0);
            edgeList.Add(newEdge);
        }
        passage.Initialize(otherCell, cell, direction.GetOpposite());
	}

	private void CreateWall (MazeCell cell, MazeCell otherCell, MazeDirection direction) {
        MazeWall wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
        wall.Initialize(cell, otherCell, direction);
		if (otherCell != null) {
            wall = Instantiate(wallPrefabs[Random.Range(0, wallPrefabs.Length)]) as MazeWall;
            wall.Initialize(otherCell, cell, direction.GetOpposite());
		}
	}

    private MazeRoom CreateRoom(int indexToExclude)
    {
        MazeRoom newRoom = ScriptableObject.CreateInstance<MazeRoom>();
        newRoom.settingsIndex = Random.Range(0, roomSettings.Length);
        if (newRoom.settingsIndex == indexToExclude)
        {
            newRoom.settingsIndex = (newRoom.settingsIndex + 1) % roomSettings.Length;
        }
        newRoom.settings = roomSettings[newRoom.settingsIndex];
        rooms.Add(newRoom);
        return newRoom;
    }

    private void CreatePassageInSameRoom(MazeCell cell, MazeCell otherCell, MazeDirection direction)
    {
        MazePassage passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(cell, otherCell, direction);
        passage = Instantiate(passagePrefab) as MazePassage;
        passage.Initialize(otherCell, cell, direction.GetOpposite());
        /*if (cell.room != otherCell.room)
        {
            MazeRoom roomToAssimilate = otherCell.room;
            cell.room.Assimilate(roomToAssimilate);
            rooms.Remove(roomToAssimilate);
            Destroy(roomToAssimilate);
        
        }*/
    }

}