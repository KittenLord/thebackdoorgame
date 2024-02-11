using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectTransformExtensions
{
    public static Rect GetWorldRect (this RectTransform rt, Vector2 scale) 
    {
		Vector3[] corners = new Vector3[4];
		rt.GetWorldCorners(corners);
		Vector3 topLeft = corners[0];
		Vector2 scaledSize = new Vector2(scale.x * rt.rect.size.x, scale.y * rt.rect.size.y);
		return new Rect(topLeft, scaledSize);
	}
}