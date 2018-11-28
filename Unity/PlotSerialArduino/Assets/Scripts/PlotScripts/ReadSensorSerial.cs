using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SignalPloter;
using System.Net.NetworkInformation;
using System.IO.Ports;

public class ReadSensorSerial : MonoBehaviour {
	/*=====================================================================
	//========================= Elementos UI ==============================
	//====================================================================*/
	Plot graficas;
	public GameObject unityPlot;

	/*=====================================================================
	//====================== Comunicacion Serial ==========================
	//====================================================================*/
	private SerialPort sp;
	public int Com = 15;
	public int baudRate = 9600;

	/*=====================================================================
	//=============== Variables de proceso para graficas ==================
	//====================================================================*/
	public Int16 numMuestras = 0; //Numero total de muestras obtenidas
	public int nMuestrasVentana = 0; //Numero de muestras impresas en el panel
	Int16 Sensores = 0; //Almaceno la informacion enviada desde el sensor

	/*=====================================================================
	//=============== Variables de visualizacion Graficas =================
	//====================================================================*/
	GameObject Panel; //Paneles (Fondo graficas)
	public Material mat; //Material usado para generar las lineas
	[SerializeField] GameObject TextPrefab;
	Transform menuPanelY;
	[SerializeField] GameObject TextPrefabX;
	Transform menuPanelX;
	ClickDetect GraphsMouseEvents;

	/*=====================================================================
	//========================== Start Function ===========================
	//====================================================================*/
	void Start () {
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 180;

		SerialBegin (Com,baudRate);

		bindGraphs();
		MouseEvents();
	}

	/*=====================================================================
	//========================= Update Function ==========================
	//====================================================================*/
	void Update () {
		KeyDownRead ();
	}

	void SerialBegin(int com, int baudRate){
		sp = new SerialPort ( "\\\\.\\COM"+com.ToString(), baudRate, Parity.None, 8, StopBits.One) ;
		sp.Open();

		if (sp.IsOpen) {
			Debug.Log ("Conexion Exitosa");
		}
		sp.Encoding = Encoding.GetEncoding(28591);
		sp.ReadTimeout = 10000;
	}

	/*=====================================================================
	//========================= Quit Function ==========================
	//====================================================================*/
	private void OnApplicationQuit()
	{
		sp.Close();
	}

	/*=====================================================================
	//======================= Lectura de Teclado ==========================
	//====================================================================*/
	void KeyDownRead(){
		// Comenzar muestreo
		if (Input.GetKeyDown (KeyCode.Space)) {
			PlotBegin ();
		}
	}

	/*=====================================================================
	//======================= Lectura de Mouse ==========================
	//====================================================================*/
	void MouseEvents(){
		GraphsMouseEvents = Panel.GetComponent<ClickDetect>();
		GraphsMouseEvents.onMouseClickAction = delegate() {
			graficas.setClickPosition();
		};
		GraphsMouseEvents.onMouseDragAction = delegate() {
			graficas.dragPlot();
		};
	}

	/*=====================================================================
	//=========================== FUNCION ================================
	//====================== Recibir datos Serial ========================
	//====================================================================*/
	Int16 LeerDatos(){
		byte[] bytesWrite = new byte[1];
		bytesWrite[0] = 49;
		sp.Write (bytesWrite, 0, 1);

		Byte[] bytesRead = Encoding.GetEncoding(28591).GetBytes(sp.ReadLine());
		Int16 value = (Int16)(bytesRead [0] << 8 | bytesRead [1]);
		return value;
	}

	/*=====================================================================
	//=================== Generacion de graficas  =========================
	//====================================================================*/
	void OnPostRender() {
		// Comprobar si hay material asignado en el Script
		if (!mat) {
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}

		// Si se define un numero de muestras se comienza a graficar
		graficas.StartPlot();
	}

	//---------------------------- Coorutina para tomar datos y graficar -------------------------
	//--------------------------------------------------------------------------------------------
	IEnumerator gyroCalibration(){
		float t1 = Time.realtimeSinceStartup;
		graficas.InitPositions();
		graficas.InitMaxAndMin (-5000,5000);
		LeerDatos ();
		for (int i = 0; i < numMuestras; i++) {
			ReadAndPlot (i);
			yield return null;
		}

		float dt = Time.realtimeSinceStartup-t1;
		Debug.Log("Tiempo: "+dt + " s\tFs: "+(numMuestras/dt) + " Hz");
	}

	/*=====================================================================
	//============== Funcion Recibir Datos y graficarlos  ================
	//====================================================================*/
	void ReadAndPlot(int i){
		graficas.SetCountSamples (i);
		//Sensores = (Int16) (Math.Sin(2*Math.PI*5*(i*0.001))*1000);
		Sensores = LeerDatos ();
		graficas.SetNewPoints (Sensores);
		graficas.AutoMaxAndMin ();
	}

	//----------------------------CONFIGURACION UI----------------------------
	//------------------------------------------------------------------------
	//------------------------------------------------------------------------

	//------------------------- Enlazamos los panelees para las graficas con el script y rescatamos sus dimensiones ------------------------------
	//--------------------------------------------------------------------------------------------------------------------------------------------
	void bindGraphs(){
		Panel = unityPlot.transform.GetChild(0).gameObject;
		menuPanelY = unityPlot.transform.GetChild(1).GetChild(1).transform;
		menuPanelX = unityPlot.transform.GetChild(1).GetChild(2).transform;

		Debug.Log (Panel.transform.localScale);

		graficas = new Plot (Panel,mat);
		graficas.SetWindowSize (nMuestrasVentana);
		graficas.SetColors ();

		GameObject[] textY = new GameObject[11];

		for(int i= 0; i<=10;i++){
			textY[i] = (GameObject)Instantiate (TextPrefab);
		}
		graficas.SetTextY1Axis(menuPanelY,textY);

		GameObject[] textX = new GameObject[11];

		for(int i= 0; i<=10;i++){
			textX[i] = (GameObject)Instantiate (TextPrefabX);
			//textX [i].GetComponent<Text> ().fontSize = 18;
		}

		graficas.SetTextX1Axis(menuPanelX,textX);
	}

	//-------------------------------EVENTOS UI-------------------------------
	//------------------------------------------------------------------------
	//------------------------------------------------------------------------
	public void PlotBegin(){
		graficas.SetNumberSamples(numMuestras);
		Debug.Log ("Inicia el muestreo Giroscopos("+ numMuestras +" muestras)"+"...");
		StartCoroutine("gyroCalibration");
	}
}
