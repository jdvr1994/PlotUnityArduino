using System; 
using System.Collections.Generic; 
using System.Text;
using UnityEngine;
using UnityEngine.UI; 

namespace SignalPloter 
{ 
	class Plot 
	{ 
		/*=====================================================================
		//=============== Variables de visualizacion Graficas =================
		//====================================================================*/
		GameObject Panel; //Paneles (Fondo graficas)
		GameObject[] labelsY = new GameObject[11];
		GameObject[] labelsX = new GameObject[11];
		Vector3 pointZero; //Punto (0,0,0) para comenzar la grafica
		Vector3 size; //Tamaño del fondo de la grafica
		Vector3 PanelPosition; //Posicion del fondo de la grafica
		public Color color; //Color de linea
		Material mat; //Material usado para generar las lineas
		float numPuntos = 50.0f; 
		float numLineas = 10.0f; 
		int numLabelsY = 10;


		/*=====================================================================
		//=============== Variables de proceso para graficas ==================
		//====================================================================*/
		Int16 numMuestras = 0; //Numero total de muestras obtenidas
		int nMuestrasVentana = 0; //Numero de muestras impresas en el panel
		int contadorMuestras = 0; //Contador de muestras
		public float [] positions; //Almaceno N muestras recibidas desde los sensores (Ejemplo 1000 muestras, 3 Accel)
		int maximo; //Almaceno el maximo de cada sensor
		int minimo; //Almaceno el minimo de cada sensor
		float scale; //Escala de ejes para grafica
		public bool finished = false;

		/*=====================================================================================
		//==== Configurar los objetos de fondo para la grafica y el material de las lineas ====
		//=====================================================================================*/
		public Plot(GameObject Background, Material material) 
		{ 
			Panel = Background;
			mat = material;

			pointZero = new Vector3 (Panel.transform.position.x - Panel.transform.localScale.x * 0.5f, Panel.transform.position.y - Panel.transform.localScale.y * 0.5f, Panel.transform.position.z - 1.0f);
			size = new Vector3 (Panel.transform.localScale.x, Panel.transform.localScale.y, Panel.transform.localScale.z);
			Debug.Log (pointZero.x);
			Debug.Log (size.x);

			PanelPosition = Panel.transform.position;
		} 

		/*=====================================================================
		//===================== Configurar la cuadricula ======================
		//====================================================================*/

		public void GridConfig(float nPoints, float nLines) 
		{ 
			numPuntos = nPoints;
			numLineas = nLines;
		} 

		/*=====================================================================
		//=============== Almacenar e inicializar los Ylabels ================
		//====================================================================*/

		public void SetTextY1Axis ([SerializeField] Transform YPanel, GameObject[] labelTexts){
			labelsY = labelTexts;
			for (int label = 0; label <= numLabelsY; label++) {
				labelsY [label].GetComponent<Text> ().text = "" + (numLabelsY-label);
				labelsY [label].transform.SetParent (YPanel);
			}
		}

		/*=====================================================================
		//=============== Almacenar e inicializar los Xlabels ================
		//====================================================================*/

		public void SetTextX1Axis ([SerializeField] Transform XPanel, GameObject[] labelTexts){
			labelsX = labelTexts;
			for (int label = 0; label <= numLabelsY; label++) {
				labelsX [label].GetComponent<Text> ().text = "" + label;
				labelsX [label].transform.SetParent (XPanel);
			}
		}

		/*=====================================================================
		//========== Configurar el numero de muestras a graficar ==============
		//====================================================================*/
		public void SetNumberSamples(Int16 nSamples){
			numMuestras = nSamples;
		}

		/*=====================================================================
		//=========== Configurar el numero de muestras por ventana ============
		//====================================================================*/
		public void SetWindowSize(int windowSize){
			nMuestrasVentana = windowSize;
		}

		/*=====================================================================
		//================ Inicializar los maximos y minimos =================
		//====================================================================*/
		public void InitMaxAndMin(int max, int min){
			maximo =  max;
			minimo =  min;
		}

		/*=====================================================================
		//====== Configurar los maximos y minimos con Int16 Sensores ========
		//=====================================================================*/
		public void AutoMaxAndMin(Int16 Sensores){
			if (maximo < Sensores)maximo = Sensores;
			if (minimo > Sensores)minimo = Sensores;
		}

		/*=====================================================================
		//====== Configurar los maximos y minimos con la ultima medida ========
		//=====================================================================*/
		public void AutoMaxAndMin(){
			if (maximo< positions [contadorMuestras]) {
				maximo = (Int16)positions [contadorMuestras];
				SetDivValuesY();
			}
			if (minimo> positions [contadorMuestras]) {
				minimo = (Int16)positions [contadorMuestras];
				SetDivValuesY ();
			}
		}

		/*=====================================================================
		//============= Configurar los Labels Y de cada grafica ===============
		//=====================================================================*/
		public void SetDivValuesY(){
			//Acelerometros y Gyroscopos
			if (Math.Abs (maximo) > Math.Abs (minimo))
				scale = (2 * Math.Abs (maximo));
			else
				scale = (2 * Math.Abs (minimo));

			float divValue = scale / numLabelsY;
			for (int j = 0; j <= numLabelsY; j++) {
				float divLabel = (5 - j) * divValue;
				labelsY [j].GetComponent<Text> ().text = divLabel.ToString ("0.0");
			}
		}

		public void SetDivValuesX(){
			float divValue = nMuestrasVentana / numLabelsY;
			if(nMuestrasVentana==0) divValue = numMuestras / numLabelsY;
			float offsetX = 0;
			for (int j = 0; j <= numLabelsY; j++) {
				if (contadorMuestras > nMuestrasVentana) offsetX = contadorMuestras - nMuestrasVentana;
				if (nMuestrasVentana == 0) offsetX = 0;
				float divLabel = j* divValue + offsetX;
				labelsX [j].GetComponent<Text> ().text = divLabel.ToString ("0");
			}
		}

		/*=====================================================================
		//=================== Inicializar vector de datos =====================
		//=====================================================================*/
		public void InitPositions(){
			positions = new float[numMuestras];
		}

		/*=====================================================================
		//================== Introducir una nueva muestra =====================
		//=====================================================================*/
		public void SetNewPoints(Int16 Sensor){
			positions[contadorMuestras] = Sensor;
		}

		/*=====================================================================
		//=============== Actualizar el contador de muestras =================
		//=====================================================================*/
		public void SetCountSamples(int cont){
			contadorMuestras = cont;
			//Detectamos cuando termina de graficar
			if (contadorMuestras >= numMuestras - 1) {
				finished = true;
				countDrag = numMuestras;
				drag = 0;
			}
			if (contadorMuestras == 0) finished = false;
		}

		/*=====================================================================
		//================== Configurar colores de graficas ===================
		//=====================================================================*/
		public void SetColors(){
			color = Color.red;
		}

		/*=====================================================================
		//======================= Generar Grafica  ============================
		//====================================================================*/

		public void StartPlot(){
			// Si se define un numero de muestras se comienza a graficar
			if (contadorMuestras > 0) {
				GL.Begin (GL.LINES);
				mat.SetPass (0);

				//Se generan la cuadricula y la grafica
				drawGrid (numLineas, numPuntos);  //Cuadricula del Panel i
				graph (); //Grafica de Accel o Gyro (X,Y,Z)

				GL.End ();
			}
		}

		/*=====================================================================
		//===================== Graficar cuadricula  ==========================
		//====================================================================*/
		void drawGrid(float numLineas, float numPuntos){
			//--------------------------Dibujo GRID ----------------------------------
			//------------------------------------------------------------------------
			GL.Color(Color.gray);
			for (int l = 1; l < numLineas; l++) {
				for (int p = 0; p < numPuntos; p++) {
					GL.Vertex3 (pointZero.x + p * size.x / (numPuntos), pointZero.y + l*size.y/numLineas, pointZero.z);
					GL.Vertex3 (pointZero.x + (p * 3.0f + 1.0f) * size.x / (numPuntos * 3.0f), pointZero.y + l*size.y/numLineas, pointZero.z);
					GL.Vertex3 (pointZero.x + l*size.x/numLineas, pointZero.y + p * size.y / (numPuntos), pointZero.z);
					GL.Vertex3 (pointZero.x + l*size.x/numLineas, pointZero.y + (p * 3.0f + 1.0f) * size.y / (numPuntos * 3.0f), pointZero.z);
				}
			}

			//------------------------ Linea Eje X ----------------------------------------
			//-----------------------------------------------------------------------------
			GL.Color(Color.white);
			for (int p = 0; p < numPuntos; p++) {
				GL.Vertex3 (pointZero.x + p*size.x/(numPuntos), PanelPosition.y, pointZero.z);
				GL.Vertex3 (pointZero.x + (p*3.0f+1.0f)*size.x/(numPuntos*3.0f), PanelPosition.y, pointZero.z);
			}
		}

		/*=====================================================================
		//============== Graficar Datos Acelerometros y Gyros  ================
		//====================================================================*/
		void graph(){
			//--------------------------- Se grafica los datos de los sensores ---------------------
			//--------------------------------------------------------------------------------------
			GL.Color(color);
			if (Math.Abs (maximo) > Math.Abs (minimo))
				scale = size.y / (2 * Math.Abs(maximo));
			else
				scale = size.y / (2 * Math.Abs(minimo));

			//---------------------Graficar N ultimas muestras (Ventana)---------------------------------
			int nMuestras = nMuestrasVentana;
			if(nMuestrasVentana == 0) nMuestras = numMuestras;

			if (contadorMuestras > nMuestras || finished) {
				if(countDrag+drag>nMuestras & countDrag+drag<numMuestras & finished)contadorMuestras = countDrag + drag;
				SetDivValuesX ();
				GL.Vertex3 (pointZero.x, PanelPosition.y + positions [contadorMuestras - nMuestras - 1] * scale, pointZero.z);
				for (int j = contadorMuestras - nMuestras; j < contadorMuestras; j++) {
					GL.Vertex3 (pointZero.x + (float)(j - contadorMuestras + nMuestras + 1) * size.x / nMuestras, PanelPosition.y + positions [j] * scale, pointZero.z);
					GL.Vertex3 (pointZero.x + (float)(j - contadorMuestras + nMuestras + 1) * size.x / nMuestras, PanelPosition.y + positions [j] * scale, pointZero.z);
				}
				GL.Vertex3 (pointZero.x + (float)(nMuestras) * size.x / nMuestras, PanelPosition.y + positions [contadorMuestras] * scale, pointZero.z);

			} else {
				SetDivValuesX ();
				GL.Vertex3 (pointZero.x,PanelPosition.y,pointZero.z);
				for (int j = 0; j <= contadorMuestras; j++) {
					GL.Vertex3 (pointZero.x + (float)(j+1)*size.x/nMuestras, PanelPosition.y + positions[j]*scale, pointZero.z);
					GL.Vertex3 (pointZero.x + (float)(j+1)*size.x/nMuestras, PanelPosition.y + positions[j]*scale, pointZero.z);
				}
				GL.Vertex3 (pointZero.x + (float)(contadorMuestras+1)*size.x/nMuestras, PanelPosition.y + positions[contadorMuestras]*scale, pointZero.z);
			}

		}


		/*=====================================================================
		//================= Movimiento en el eje de Tiempo  ===================
		//====================================================================*/
		int dragPosZero = 0;
		int drag = 0;
		int countDrag=0;
		public void setClickPosition(){
			if (finished) {
				dragPosZero = (int)Input.mousePosition.x;
				countDrag = contadorMuestras;
			}
		}

		public void dragPlot(){
			if (finished) {
				drag = dragPosZero - (int)Input.mousePosition.x;
			}
		}

	} 
} 
