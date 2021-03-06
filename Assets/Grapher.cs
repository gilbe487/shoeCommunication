﻿using UnityEngine;
using System.Collections;

public class Grapher : MonoBehaviour {
	[Range(10,100)]
	public int resolution = 100;
	private int currentResolution;
	private ParticleSystem.Particle[] points;
	[Range(2000f,10000f)]
	public float data = 2000f;
	public ServerUDP SUDP;
	public GUIStyle testStyles;
	private float lastTime=0;
	public int shoeValve = -1;//must set in editor , 0-6 for left foot,  7-13 for right foot
	// Use this for initialization
	void Start () {
		CreatePoints();

	}
	private void CreatePoints() {
		currentResolution = resolution;
		points = new ParticleSystem.Particle[resolution];
		float increment = 0.5f / (resolution - 1);
		for(int i = 0; i < resolution; i++){
			float x = i * increment;
			points[i].position = new Vector3(x, 0f, 0f);
			points[i].color = new Color(x, 0f, 0f);
			points[i].size = 0.02f;
		}
	}

	private void MaintainPoints(float d){
		currentResolution = resolution;
		//points = new ParticleSystem.Particle[resolution];
		float increment = 0.5f / (resolution - 1);
//		Debug.Log (resolution);
		for(int i = 0; i < (resolution-1); i++){
			float x = i * increment;
			points[i].position = new Vector3(x, 0f, points[i+1].position.z);
			//points[i].position = new Vector3(x, 0f, data);
			points[i].color = new Color(x, 0f, 0f);
			points[i].size = 0.02f;
		}
		//points[resolution-1].position = new Vector3((resolution-1)*increment,0f,d);
		//float newData = points[resolution-1].position.z + Random.Range(-.01f,.01f);
		float newData = 0;
		if (SUDP.shoe == ServerUDP.shoeSide.left)
		{
			if (shoeValve > -1 && shoeValve< 7) // left foot
			{
				//70,000 - 150,000
				// 0 - .10
				// x - 70000  =   0 - 80,0000
				// 800,000/x = .10
				//x = 800,000
				newData = ((SUDP.leftShoePressureData[shoeValve] - 70000) / 800000);
			}
			else if (shoeValve > 6 && shoeValve < 14) //right foot
			{
				newData = ((SUDP.leftShoeProximityData[shoeValve-7] - 2000) / 80000);
				//2,000-10,000 >>
				//0    - .10
				//x-2000  = 0-8000
				//x/80000  8000/.10=x
				
			}
			else
			{
				Debug.LogError("Invalid shoeValve state, set  in editor");
			}

		}
		else//shoe is right foot
		{
			if (shoeValve > -1 && shoeValve< 7) // left foot
			{
				//70,000 - 150,000
				// 0 - .10
				// x - 70000  =   0 - 80,0000
				// 800,000/x = .10
				//x = 800,000
				newData = ((SUDP.rightShoePressureData[shoeValve] - 70000) / 800000);
			}
			else if (shoeValve > 6 && shoeValve < 14) //right foot
			{
				newData = ((SUDP.rightShoeProximityData[shoeValve-7] - 2000) / 80000);
				//2,000-10,000 >>
				//0    - .10
				//x-2000  = 0-8000
				//x/80000  8000/.10=x

			}
			else
			{
				Debug.LogError("Invalid shoeValve state, set  in editor");
				return;
			}
		}
		if (newData > 0.10f)
			newData = .10f;
		if (newData < 0)
			newData = 0f;
		points[resolution-1].position = new Vector3((resolution-1)*increment,0f,newData);
	
	}
	

	void OnGUI(){


	}

	// Update is called once per frame
	void Update () {



		if (currentResolution != resolution) {
			CreatePoints();
		}
		if (Time.time - lastTime > .1f)
		{
			MaintainPoints(data);
			lastTime = Time.time;	

			if (shoeValve == 0)//so it is only done once
			{
				//checks to see that the last sent valve state is correct
				SUDP.checkForResend();
			}
		}
		particleSystem.SetParticles(points, points.Length);
	
	}
}
