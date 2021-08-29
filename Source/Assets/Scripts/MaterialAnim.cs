using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MaterialAnim : MonoBehaviour
{
	[Serializable]
	public class Index
	{
		public string name;
		public Texture texture;
		public int index;
	}

	protected int faceIndex;
	protected int faceFrame;
	[SerializeField]
	protected Renderer faceMat;
	[SerializeField]
	protected int materialIndex = 0;

	[SerializeField]
	protected Index[] faces = new Index[3];
	[SerializeField]
	protected Texture2D[] facesTextures = new Texture2D[6];

	[SerializeField]
	protected float blink;
	[SerializeField]
	protected float blinkSpeed = 24;
	[SerializeField]
	protected float blinkWait = 48;
	[SerializeField]
	protected int blinkTwice = 2;
	[SerializeField]
	protected float twiceDelay = 10f;

	protected int c;
	protected int blinkCount = 0;

	private void Start()
	{
		Blink();
	}

	void Update()
	{
		if (faceIndex != 0)
			return;

		SetFace();
	}

	protected virtual void SetFace()
	{
		faceMat.materials[materialIndex].mainTexture = faces[GetFaceId()].texture;
	}


	public void SetDefFace(int index)
	{
		faces[0].texture = facesTextures[index];
	}

	protected int GetFaceId()
	{
		int id = 0;

		blink += Time.deltaTime * blinkSpeed;
		int blinkIndex = Mathf.CeilToInt(blink);
		if (blink > blinkWait)
		{
			blink = 0;
			blinkCount++;
		}
		if (blinkCount >= blinkTwice)
		{
			if (blink > twiceDelay)
			{
				blink = 0;
				blinkCount = 0;
			}
		}
		if (blinkIndex <= 2)
		{
			id = blinkIndex;
		}
		else
		{
			if (blinkIndex == 3)
				id = 1;
			if (blinkIndex >= 4)
				id = 0;
		}
		return id;
	}

	public void Blink()
	{
		faceIndex = 0;
		enabled = true;
	}

	public virtual void FaceChange(string faceName)
	{
		Index s = null;
		s = Array.Find(faces, face => face.name == faceName);

		if (s == null)
			return;
		faceIndex = 1;
		faceFrame = s.index;

		faceMat.materials[materialIndex].mainTexture = faces[faceFrame].texture;
		enabled = false;
	}

}
