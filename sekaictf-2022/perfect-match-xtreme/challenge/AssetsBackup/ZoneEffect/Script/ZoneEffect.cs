using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneEffect : MonoBehaviour {
	public float TransTime = 0.5f;
	public string Layer = "Player";

	public bool UseLight = true;
	private Light _sceneLight;
	private float _sceneLightIntensity;
	private Color _sceneLightColor;
	private Quaternion _sceneLightRotate;
	private float _sceneLightShadow;
	public Light AlternateLight;

	public bool UseFog = true;      //whether or not to use alternate fog settings
	private float _fogDensity;
	private Color _fogColor;
	public float AltFogDensity;
	public Color AltFogColor;

	public bool UseSky = true;
	public Material AlternateSky;
	private Material _defaultSky;
	private Material _tempSky;


	// Use this for initialization
	void Start () {
		_sceneLight = RenderSettings.sun; //storing the active settings for later use
		_sceneLightIntensity = _sceneLight.intensity;
		_sceneLightColor = _sceneLight.color;
		_sceneLightRotate = _sceneLight.transform.rotation;
		_sceneLightShadow = _sceneLight.shadowStrength;
		if (AlternateLight != null) {
			AlternateLight.gameObject.SetActive (false);
		}
		_sceneLight.gameObject.SetActive (true);

		_fogDensity = RenderSettings.fogDensity;
		_fogColor = new Color (RenderSettings.fogColor.r, RenderSettings.fogColor.g, RenderSettings.fogColor.b, RenderSettings.fogColor.a);

		//make a clone of the skybox specified in render settings and store as the return skybox value;
		_defaultSky = RenderSettings.skybox;
		_tempSky = new Material (Shader.Find ("Skybox/Procedural"));
		RenderSettings.skybox = _tempSky;
		RenderSettings.skybox.Lerp (RenderSettings.skybox, _defaultSky, 1);
	}

	// Update is called once per frame
	void Update () {

	}

	void OnTriggerEnter(Collider other) {
		//print ("enter");
		if (other.gameObject.layer == LayerMask.NameToLayer (Layer)) {
			StopAllTransitions ();
			if (UseLight && AlternateLight != null) {
				StartCoroutine (AltLightTrans ());
			}

			if (UseFog) {
				StartCoroutine (AltFogTrans ());
			}

			if (UseSky && AlternateSky != null) {
				StartCoroutine (AltSkyTrans ());
			}
		}
	}

	void OnTriggerExit(Collider other) {
		//print ("exit");
		if (other.gameObject.layer == LayerMask.NameToLayer (Layer)) {
			StopAllTransitions ();

			StartCoroutine (DefaultLightTrans ());

			StartCoroutine (DefaultFogTrans ());

			StartCoroutine (DefaultSkyTrans ());
		}
	}

	void OnApplicationQuit () {
		RenderSettings.skybox = _defaultSky;
	}

	void StopAllTransitions () {
		var zoneEffects = GameObject.FindObjectsOfType<ZoneEffect> ();
		foreach (var z in zoneEffects) { 
			z.StopAllCoroutines ();
		}
	}

	IEnumerator AltLightTrans () {
		float t = 0f;

		while (t < 1.0f) {
			//change light specified in lighting render settings to the alternate light specified in inspector
			_sceneLight.intensity = Mathf.Lerp (_sceneLight.intensity, AlternateLight.intensity, t);
			_sceneLight.color = Color.Lerp (_sceneLight.color, AlternateLight.color, t);
			_sceneLight.transform.rotation = Quaternion.Lerp (_sceneLight.transform.rotation, AlternateLight.transform.rotation, t);
			_sceneLight.shadowStrength = Mathf.Lerp (_sceneLight.shadowStrength, AlternateLight.shadowStrength, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}

	IEnumerator DefaultLightTrans() {
		float t = 0f;

		while (t < 1.0f) {
			//return settings to those specified in lighting render settings
			_sceneLight.intensity = Mathf.Lerp (_sceneLight.intensity, _sceneLightIntensity, t);
			_sceneLight.color = Color.Lerp (_sceneLight.color, _sceneLightColor, t);
			_sceneLight.transform.rotation = Quaternion.Lerp (_sceneLight.transform.rotation, _sceneLightRotate, t);
			_sceneLight.shadowStrength = Mathf.Lerp (_sceneLight.shadowStrength, _sceneLightShadow, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}

	IEnumerator AltFogTrans() {
		float t = 0f;

		while (t < 1.0f) {
			RenderSettings.fogDensity = Mathf.Lerp (RenderSettings.fogDensity, AltFogDensity, t);
			RenderSettings.fogColor = Color.Lerp (RenderSettings.fogColor, AltFogColor, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}

	IEnumerator DefaultFogTrans() {
		float t = 0f;

		while (t < 1.0f) {
			RenderSettings.fogDensity = Mathf.Lerp (RenderSettings.fogDensity, _fogDensity, t);
			RenderSettings.fogColor = Color.Lerp (RenderSettings.fogColor, _fogColor, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}

	IEnumerator AltSkyTrans() {
		float t = 0f;

		while (t < 1.0f) {
			RenderSettings.skybox.Lerp (RenderSettings.skybox, AlternateSky, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}

	IEnumerator DefaultSkyTrans() {
		float t = 0f;

		while (t < 1.0f) {
			//          Debug.Log ("DefaultSkyTrans" + t, this);
			//          print (RenderSettings.skybox + "rendersettings");
			//          print (_defaultSky + "defaultsky");
			RenderSettings.skybox.Lerp (RenderSettings.skybox, _defaultSky, t);

			t += (TransTime * Time.deltaTime) / 50;

			yield return null;
		}
	}
}