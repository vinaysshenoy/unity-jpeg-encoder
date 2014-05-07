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
		Debug.Log("Press 'p' on the keyboard to take a screendump");	
	}
	

	private IEnumerator ScreenshotEncode()
	{
		yield return new WaitForEndOfFrame();
		
		// create a texture to pass to encoding
		texture = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
		
		// put buffer into texture
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		texture.Apply(false, false);
		
		
//		string fullPath = Application.dataPath + "/../testscreen-" + count + ".jpg";
//		JPGEncoder encoder = new JPGEncoder(texture, 75, fullPath );
		
		string fullPath = Application.dataPath + "/../testscreen-" + count + ".png";
		PNGEncoder encoder = new PNGEncoder( texture, fullPath );
		
		//How to encode without save to disk
//		JPGEncoder encoder = new JPGEncoder(texture, 75);
				
		//encoder is threaded; wait for it to finish
		while(!encoder.isDone)
			yield return null;
		
		
//		How to use the JPG encoder as a blocking method. This way it isn't needed to wait for it to finish
//		But the unity main thread will freeze while the encoding takes place (just like Texture.EncodeToPNG() does)
//		JPGEncoder encoder1 = new JPGEncoder(texture, 75, true);

		
		Debug.Log("Screendump saved at : " + fullPath);
		Debug.Log("Done encoding and bytes ready for use. e.g. send over network, write to disk");
		Debug.Log("Size: " + encoder.GetBytes().Length + " bytes");
		
		count++;
		
	}
	
}