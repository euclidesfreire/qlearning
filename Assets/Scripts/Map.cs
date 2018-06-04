using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {

	//tiles
	public GameObject tileRock;
	public GameObject tileHole;
	public GameObject tileMouse;
	public GameObject tileCheese;
	public GameObject tileWon;

	//tile estatico -> usado no metodo estatico
	//private static GameObject tileRockStatic; 

	//tamanho do tile
	public static float tileSize = 0.32f;
	//escala do tile
	public static float tileScale = 3.0f;

	//tamanho do mapa: largura e altura
	public static int mapWidth = 12;
	public static int mapHeight = 12;

	//Matrizes R e Q
	public static int[,] R, Q;

	//Estado atual
	int currentStateI;
	int currentStateJ;

	//acumulador de tempo
	float accTime = 0;

	//Pausar
	bool paused = false;

	//Estado Alvo
	int alvoI = 5, alvoJ = 1;

	//Quantidade de Interações
	int interations = 350;

	//Gama
	float gama = 0.8f;

	//States
	int stateI, stateJ;
	int PStateI = 0, PStateJ = 0;
	int[] state;
	int[,] acao;

	//Recompensa anterior para Update
	int recomp = 0;

	//método de inicializacao
	void Start () {

		// definicao do mapa
		Map.R = new int[12, 12] {
			{ -2, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0 },
			{ 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			{ 5, -1, 0, 0, -1, 0, -1, 0, 0, 0, -1, 0 },
			{ 10, -1, -1, 0, 0, 0, -1, 0, -1, 0, -1, -1 },
			{ 15, 40, 0, 0, 0, -1, -1, -1, -1, 0, 0, -1 },
			{ 30, 50, 0, 0, 0, 0, 0, 0, -1, 0, 0, -1 },
			{ -1, 0, 0, -1, -1, 0, 0, 0, 0, 0, 0, 0 },
			{ -1, 0, 0, 0, 0, 0, -1, 0, -1, -1, -1, 0},
			{ -1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			{ -1, -1, -1, 0, 0, -1, -1, -1, 0, -1, 0, 0 },
			{ 0, 0, 0, 0, 0, 0, 0, 0, 0, -1, 0, 0 },
			{ 0, 0, -1, -1, -1, -1, -1, 0, 0, -1, 0, 0 }
		};


		drawMap ();

		Map.R [0, 0] = 0;

		//Instanciar Matrizes R e Q
		Q = new int[12, 12];

		//Instanciar a  matriz State e Ação para Update
		state = new int[4];
		acao = new int[4,2];

		//Inicializar matriz Q
		initialize ();

		//Interations do Q-Learning 
		for (int i = 0; i <= interations; i++) {
			
			if (i == 50) {
				gama = 0.5f;
			}

			if (i == 150) {
				gama = 0.1f;
			}

			do {
				stateI = Random.Range (0, 11);
				stateJ = Random.Range (0, 11);

			} while(Map.R [stateI, stateJ] == -1);

			episode (stateI, stateJ);
		}
	}

	// Update is called once per frame
	void Update () {
		
		if (paused) {
			drawMap ();
			Time.timeScale = 0;
		}

		accTime+=Time.deltaTime;
		//o resto do método só executa quando o tempo acumulado for maior que 0.5s
		if (accTime > 0.5f)
			accTime = 0;
		else
			return;	

		R [PStateI, PStateJ] = recomp;

		if (PStateI > 0) {
			if (R [PStateI - 1, PStateJ] != -1) {
				state [0] = Q [PStateI - 1, PStateJ];
				acao [0, 0] = PStateI - 1;
				acao [0, 1] = PStateJ;
			} else {
				state [0] = -1;
				acao [0, 0] = -1;
				acao [0, 1] = -1;
			}
		} else {
			state [0] = -1;
			acao [0, 0] = -1;
			acao [0, 1] = -1;
		}

		if (PStateI < mapWidth - 1) {
			if (R [PStateI + 1, PStateJ] != -1) {
				state [1] = Q [PStateI + 1, PStateJ];
				acao [1, 0] = PStateI + 1;
				acao [1, 1] = PStateJ;
			}  else {
				state [1] = -1;
				acao [1, 0] = -1;
				acao [1, 1] = -1;
			}
		} else {
			state [1] = -1;
			acao [1, 0] = -1;
			acao [1, 1] = -1;
		}

		if (PStateJ > 0) {
			if (R [PStateI, PStateJ - 1] != -1) {
				state [2] = Q [PStateI, PStateJ - 1];
				acao [2, 0] = PStateI;
				acao [2, 1] = PStateJ - 1;
			} else {
				state [2] = -1;
				acao [2, 0] = -1;
				acao [2, 1] = -1;
			}
		} else {
			state [2] = -1;
			acao [2, 0] = -1;
			acao [2, 1] = -1;
		}

		if (PStateJ < mapHeight - 1) {
			if (R [PStateI, PStateJ + 1] != -1) {
				state [3] = Q [PStateI, PStateJ + 1];
				acao [3, 0] = PStateI;
				acao [3, 1] = PStateJ + 1;
			} else {
				state [3] = -1;
				acao [3, 0] = -1;
				acao [3, 1] = -1;
			}
		} else {
			state [3] = -1;
			acao [3, 0] = -1;
			acao [3, 1] = -1;
		}

		int aux = state [0];
		PStateI = acao[0, 0];
		PStateJ = acao[0, 1];

		for (int i = 1; i < 4; i++) {
			if (state [i] > aux) {
				aux = state [i];
				PStateI = acao[i, 0];
				PStateJ = acao[i, 1];
			} 
		}
			
		recomp = Map.R [PStateI, PStateJ];
		Map.R [PStateI, PStateJ] = -2;
		drawMap ();

		if (PStateI == alvoI && PStateJ == alvoJ) {
			paused = true;
			print ("Ok");
			R [PStateI, PStateJ] = -3;
		}
	}

	void drawMap(){

		//lendo o mapa para instanciar os tiles correspondentes:
		//-1 - tile de pedra
		//>0 - tile de passagem
		//-2 - tile de Rato
		//250 - tile de Queijo
		for (int i = 0; i < mapWidth; i++) {
			for (int j = 0; j < mapHeight; j++) {

				if (R [i, j] == -1) {
					Instantiate (tileRock, new Vector2 (j * tileSize * tileScale, -1 * i * tileSize * tileScale), Quaternion.identity).name = i + "-" + j;
				
				} else if (R [i, j] == -2) {
					Instantiate (tileMouse, new Vector2 (j * tileSize * tileScale, -1 * i * tileSize * tileScale), Quaternion.identity).name = i + "-" + j;
				} else if (i == alvoI && j == alvoJ) {
					Instantiate (tileCheese, new Vector2 (j * tileSize * tileScale, -1 * i * tileSize * tileScale), Quaternion.identity).name = i + "-" + j;
					if (R [i, j] == -3)
						Instantiate (tileWon, new Vector2 (j * tileSize * tileScale, -1 * i * tileSize * tileScale), Quaternion.identity).name = i + "-" + j;
				} else if( R [i, j] >= 0) {
					Instantiate (tileHole, new Vector2 (j * tileSize * tileScale, -1 * i * tileSize * tileScale), Quaternion.identity).name = i + "-" + j;
				}
			}
		}
	}

	void initialize(){
		
		for (int i = 0; i < mapWidth; i++) {
			for (int j = 0; j < mapHeight; j++) {
				Q [i, j] = 0;
			}
		}

	}

	void episode(int initialStateI, int initialStateJ){
		//int randStatus = Random.Range (0, 11);
		currentStateI = initialStateI;
		currentStateJ = initialStateJ;

		do {
			chooseAnAction ();	
		} while(currentStateI != alvoI && currentStateJ != alvoJ);
	}

	void chooseAnAction(){
		int r;
		int possibleActionI = 0, possibleActionJ = 0;
		int[] boxReward = new int[4];
		int[,] acaoReward = new int[4, 2];

		if (currentStateI > 0) {
			possibleActionI = currentStateI - 1;
			possibleActionJ = currentStateJ;
			if (Map.R [possibleActionI, possibleActionJ] != -1) {
				boxReward [0] = 1;
				acaoReward [0, 0] = possibleActionI;
				acaoReward [0, 1] = possibleActionJ;
			} else {
				boxReward [0] = -1;
			}
		} else {
			boxReward [0] = -1;
		}

		if (currentStateI < mapWidth - 1) {
			possibleActionI = currentStateI + 1;
			possibleActionJ = currentStateJ;
			if (Map.R [possibleActionI, possibleActionJ] != -1) {
				boxReward [1] = 1;
				acaoReward [1, 0] = possibleActionI;
				acaoReward [1, 1] = possibleActionJ;
			} else {
				boxReward [0] = -1;
			}
		} else {
			boxReward [0] = -1;
		}

		if (currentStateJ > 0) {
			possibleActionI = currentStateI;
			possibleActionJ = currentStateJ - 1;
			if (Map.R [possibleActionI, possibleActionJ] != -1) {
				boxReward [2] = 1;
				acaoReward [2, 0] = possibleActionI;
				acaoReward [2, 1] = possibleActionJ;
			} else {
				boxReward [0] = -1;
			}
		} else {
			boxReward [0] = -1;
		}

		if (currentStateJ < mapHeight - 1) {
			possibleActionI = currentStateI;
			possibleActionJ = currentStateJ + 1;
			if (Map.R [possibleActionI, possibleActionJ] != -1) {
				boxReward [3] = 1;
				acaoReward [3, 0] = possibleActionI;
				acaoReward [3, 1] = possibleActionJ;
			} else {
				boxReward [0] = -1;
			}
		} else {
			boxReward [0] = -1;
		}

		do{
			r = Random.Range(0,3);
		}while(boxReward[r] == -1);

		possibleActionI = acaoReward [r, 0];
		possibleActionJ = acaoReward [r, 1];

		Q [possibleActionI, possibleActionJ] = reward (possibleActionI, possibleActionJ);;
		currentStateI = possibleActionI;
		currentStateJ = possibleActionJ;
	}

	int maxi(int actionI, int actionJ){

		int max = Map.Q [actionI, actionJ];

		if (actionI > 0) {
			if(max < Map.Q [actionI - 1, actionJ])
				max = Map.Q [actionI - 1, actionJ];
		}

		if (actionI < Map.mapWidth - 1) {
			if(max < Map.Q [actionI + 1, actionJ])
				max = Map.Q [actionI + 1, actionJ];
		}

		if (actionJ > 0) {
			if(max < Map.Q [actionI, actionJ - 1])
				max = Map.Q [actionI, actionJ - 1];
		}

		if (actionJ < Map.mapHeight - 1) {
			if(max < Map.Q [actionI, actionJ + 1])
				max = Map.Q [actionI, actionJ + 1];
		}

		return max;
	}

	int reward (int actionI, int actionJ){
		return (int)(Map.R[actionI,actionJ] + (gama * maxi(actionI, actionJ)));
	}
		
}
