using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class TestEncode : MonoBehaviour 
{

	private int count = 0;
	
	public Texture2D texture; 
	
	// Update is called once per frame
	private void Update() 
	{
		if(Input.GetKeyDown("p"))
			StartCoroutine("ScreenshotEncode");
	}
	
	
	private void Start()
	{
		Debug.Log("Press 'p' on the keyborad to take a screen dump");	
	}
	

	private IEnumerator ScreenshotEncode()
	{
		yield return new WaitForEndOfFrame();
		
		// create a texture to pass to encoding
		texture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		// put buffer into texture
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		texture.Apply(false, false);
		
		string fullPath = Application.dataPath + "/../testscreen-" + count + ".jpg";
		JPGEncoder encoder = new JPGEncoder(texture, 75, fullPath );
		
		//Exsample of how to encode without save to disk
//		JPGEncoder encoder = new JPGEncoder(texture, 75);
		
		
		//encoder is threaded; wait for it to finish
		while(!encoder.isDone)
			yield return null;
		
		Debug.Log("Screendump saved at : " + fullPath);
		Debug.Log("Done encoding and bytes ready for use. e.g. send over network, write to disk");
		Debug.Log("Size: " + encoder.GetBytes().Length + " bytes");
		
		count++;
		
	}
	
}



///**
//* Take the screen buffer and spit out a JPG
//*/
//function ScreenshotEncode()
//{
//	// wait for graphics to render
//	yield WaitForEndOfFrame();
//	
//	// create a texture to pass to encoding
//	var texture:Texture2D = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
//	
//	// put buffer into texture
//	texture.ReadPixels(Rect(0.0, 0.0, Screen.width, Screen.height), 0.0, 0.0);
//	texture.Apply();
//
//	// split the process up--ReadPixels() and the GetPixels() call inside of the encoder are both pretty heavy
//	yield;
//	
//	// create our encoder for this texture
//	var encoder:JPGEncoder = new JPGEncoder(texture, 75.0);
//	
//	// encoder is threaded; wait for it to finish
//	while(!encoder.isDone)
//		yield;
//	
//	// save our test image (could also upload to WWW)
//	File.WriteAllBytes(Application.dataPath + "/../testscreen-" + count + ".jpg", encoder.GetBytes());
//	count++;
//}