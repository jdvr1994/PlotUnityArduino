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

public class ReadSensorUDP : MonoBehaviour {
	/*=====================================================================
	//====================== Comunicacion Serial ============================
	//====================================================================*/
	public String ip = "192.168.0.15";
	UdpClient udpClient = new UdpClient();
	Byte[] sendBytes;

	/*=====================================================================
	//========================= Elementos UI ==============================
	//====================================================================*/
	Plot graficas;
	public GameObject unityPlot;

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
	ClickDetect[] GraphsMouseEvents = new ClickDetect[3];
		
	/*=====================================================================
	//========================== Start Function ===========================
	//====================================================================*/
	void Start () {
		//Debug.Log(unityPlot.transform.GetChild(1).GetChild(1).transform);

		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 180;

		bindGraphs();

		//------------------ Enlazamos los eventos de los elementos de la UI ---------------------
		//----------------------------------------------------------------------------------------

		for(int i=0;i<3;i++){
			GraphsMouseEvents[i] = Panel.GetComponent<ClickDetect>();

			GraphsMouseEvents[i].onMouseClickAction = delegate() {
				//Almaceno la posicionX0
				graficas.setClickPosition();
			};

			GraphsMouseEvents [i].onMouseClickRightAction = delegate {
				if(graficas.finished)Debug.Log("CLICK DERECHO: "+Input.mousePosition);
			};

			GraphsMouseEvents[i].onMouseDragAction = delegate() {
				//Calculo la diferencia entre la actual PosicionX y PosicionX0
				graficas.dragPlot();
			};
		}
	}
	
	/*=====================================================================
	//========================= Update Function ==========================
	//====================================================================*/
	void Update () {
		KeyDownRead ();
	}

	/*=====================================================================
	//=============== Teclas para lectura acelerometros ===================
	//====================================================================*/
	void KeyDownRead(){
		// Comenzar muestreo
		if (Input.GetKeyDown (KeyCode.Space)) {
			BtnCalibrarGyroEvent ();
		}

		if (Input.GetKeyDown (KeyCode.C)) {
			BtnConectarEvent();
		}
	}

	/*=====================================================================
	//=========================== FUNCION ================================
	//============== Recibir datos Gyros/Accel/Mag (UDP) =================
	//====================================================================*/
	void LeerDatos(){
		try{
			String send = ";";
			// Envio ; que indica al microcontrolador que deseo obtener una medida la señal
			sendBytes=Encoding.ASCII.GetBytes(send);
			udpClient.Send(sendBytes, sendBytes.Length);

			/* Recibo la informacion de los sensores desde Particle Photon 2 Bytes
			 *  1 Byte = SX_high
			 *  2 Byte = SX_low
			 */
			IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
			Byte[] data = udpClient.Receive(ref RemoteIpEndPoint);

			/* Almaceno en Sensores el valor del sensor (16 bits)
			 *  Sensores = SX_high SX_low
			 */
			Sensores = (Int16)((data [0] << 8) | data [1]);
		}catch (Exception){}
	}

	/*=====================================================================
	//=========== Cargamos el numero de muestras deseadas ================
	//====================================================================*/
	void getNumeroMuestras(){
		try{
			graficas.SetNumberSamples(numMuestras);
		}catch(FormatException f){
			Debug.Log("Entrada invalida::: "+ f);
		}
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
		
	//---------------------- Coorutina para toma de datos y calibracion de los Giroscopios -------------------
	//--------------------------------------------------------------------------------------------------------
	IEnumerator gyroCalibration(){
		float t1 = Time.realtimeSinceStartup;
		graficas.InitPositions();
		graficas.InitMaxAndMin (-33000,33000);

		for (int i = 0; i < numMuestras; i++) {
			ReadAndPlot (i);
			yield return null;
		}
			
		float dt = Time.realtimeSinceStartup-t1;
		Debug.Log("Tiempo: "+dt + " s\tFs: "+(numMuestras/dt) + " Hz");
		clearUdp ();
	}

	/*=====================================================================
	//============== Funcion Recibir Datos y graficarlos  ================
	//====================================================================*/
	void ReadAndPlot(int i){
		//LeerDatos ();
		graficas.SetCountSamples (i);
		Sensores = (Int16) (Math.Sin(2*Math.PI*5*(i*0.001))*1000);
		graficas.SetNewPoints (Sensores);
		graficas.AutoMaxAndMin ();
	}


	//------------------ Funcion para limpiar buffer udp------------------
	//----------------------------------------------------------------
	void clearUdp(){
		IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
		Byte[] data = new byte[]{0};

		while(data!=null){
			try{
				data = udpClient.Receive(ref RemoteIpEndPoint);
			}catch(SocketException){
				Debug.Log ("Muestreo Completado");
				data = null;
			}
		}
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
	public void BtnConectarEvent(){
		udpClient.Connect(ip, 8888);
		udpClient.Client.ReceiveTimeout = 30;
	}

	public void BtnCalibrarGyroEvent(){
		getNumeroMuestras();
		Debug.Log ("Inicia el muestreo Giroscopos("+ numMuestras +" muestras)"+"...");
		StartCoroutine("gyroCalibration");
	}
}
