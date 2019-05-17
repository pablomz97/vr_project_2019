using UnityEngine;
using System;

public class PaintChalk : MonoBehaviour
{

	public LineRenderer lineRenderer;

	public int resolution;

	public Transform refPoint0, refPoint1, refPoint2, refPoint3;

	public Transform[] refTransforms;

	private Vector3[] refPoints;

	// Start is called before the first frame update
	void Start()
    {
		refPoints = new Vector3[refTransforms.Length];
		for(int i=0; i < refTransforms.Length; i++)
		{
			refPoints[i] = refTransforms[i].position;
		}
		//AddBezierCurve(refPoint0.position, refPoint1.position, refPoint2.position, refPoint3.position);
	}

    // Update is called once per frame
    void Update()
    {
		lineRenderer.positionCount = 0;
		DrawRefPoints();
	}


	void DrawRefPoints()
	{
		if (refPoints.Length <= 1)
			return;

		for (int i = 0; i < refPoints.Length - 1; i++)
		{
			Vector3 a = refPoints[i];
			Vector3 b = refPoints[i + 1];
			if (i == 0)
			{
				if(refPoints.Length == 2)
				{
					AddLinearBezierCurve(a, b);
					continue;
				}
				AddQuadraticBezierCurve(a, b, getFirstSupportVec(refPoints, i + 1));
				continue;
			}
			if(i == refPoints.Length - 2)
			{
				AddQuadraticBezierCurve(a, b, getSecondSupportVec(refPoints, i));
				continue;
			}

			Tuple<Vector3, Vector3> aSupportVecs = getSupportVecs(refPoints, i);
			Tuple<Vector3, Vector3> bSupportVecs = getSupportVecs(refPoints, i + 1);

			AddCubicBezierCurve(a, aSupportVecs.Item2, b, bSupportVecs.Item1);//
		}
	}

	Tuple<Vector3, Vector3> getSupportVecs(Vector3[] points, int index)
	{
		if (index <= 0 || index >= points.Length - 1)
		{
			throw new ArgumentException("illegal index");
		}

		Vector3 prevToPos = points[index] - points[index - 1];
		float prevToPosLength = prevToPos.magnitude;
		prevToPos.Normalize();
		Vector3 posToNext = points[index + 1] - points[index];
		float posToNextLength = posToNext.magnitude;
		posToNext.Normalize();
		Vector3 tangent = prevToPos + posToNext;
		tangent.Normalize();

		return new Tuple<Vector3, Vector3>(points[index] - tangent / 3 * prevToPosLength, points[index] + tangent / 3 * posToNextLength);
	}

	Vector3 getFirstSupportVec(Vector3[] points, int index)
	{
		if (index <= 0 || index > points.Length - 1)
		{
			throw new ArgumentException("illegal index");
		}

		Vector3 prevToPos = points[index] - points[index - 1];
		float prevToPosLength = prevToPos.magnitude;
		Vector3 tangent = getTangent(points, index);

		return points[index] - tangent / 3 * prevToPosLength;
	}

	Vector3 getSecondSupportVec(Vector3[] points, int index)
	{
		if (index < 0 || index >= points.Length - 1)
		{
			throw new ArgumentException("illegal index");
		}

		Vector3 posToNext = points[index + 1] - points[index];
		float posToNextLength = posToNext.magnitude;
		Vector3 tangent = getTangent(points, index);

		return points[index] + tangent / 3 * posToNextLength;
	}

	Vector3 getTangent(Vector3[] points, int index)
	{
		if(index != 0)
		{
			if (index != points.Length - 1)
			{
				Vector3 prevToPos = points[index] - points[index - 1];
				prevToPos.Normalize();
				Vector3 posToNext = points[index + 1] - points[index];
				posToNext.Normalize();
				Vector3 tangent = prevToPos + posToNext;
				tangent.Normalize();
				return tangent;
			}
			else
			{
				Vector3 prevToPos = points[index] - points[index - 1];
				prevToPos.Normalize();
				return prevToPos;
			}
		}
		else
		{
			Vector3 posToNext = points[index + 1] - points[index];
			posToNext.Normalize();
			return posToNext;
		}
	}


	void AddLinearBezierCurve(Vector3 a, Vector3 b)
	{
		int oldPostionCount = lineRenderer.positionCount;
		lineRenderer.positionCount += 2;
		lineRenderer.SetPosition(oldPostionCount, a);
		lineRenderer.SetPosition(oldPostionCount + 1, b);
	}

	void AddQuadraticBezierCurve(Vector3 a, Vector3 b, Vector3 supp)
	{
		int oldPostionCount = lineRenderer.positionCount;
		lineRenderer.positionCount += resolution + 1;

		float inc = 1.0f / this.resolution;
		int i = oldPostionCount;
		for (float t = 0; Math.Round(t, 6) <= 1; t += inc)
		{
			lineRenderer.SetPosition(i++, GetQuadraticBezierPoint(a, b, supp, t));
		}
	}

	Vector3 GetQuadraticBezierPoint(Vector3 a, Vector3 b, Vector3 supp, float t)
	{
		return Mathf.Pow(1 - t, 2) * a + 2 * (1 - t) * t * supp + Mathf.Pow(t, 2) * b;
	}


	void AddCubicBezierCurve(Vector3 a, Vector3 aSupp, Vector3 b, Vector3 bSupp)
	{
		int oldPostionCount = lineRenderer.positionCount;
		lineRenderer.positionCount += resolution + 1;

		float inc = 1.0f / this.resolution;
		int i = oldPostionCount;
		for (float t = 0; Math.Round(t, 6) <=1; t += inc)
		{
			lineRenderer.SetPosition(i++, GetCubicBezierPoint(a, aSupp, b, bSupp, t));
		}
	}

	Vector3 GetCubicBezierPoint(Vector3 a, Vector3 aSupp, Vector3 b, Vector3 bSupp, float t)
	{
		return Mathf.Pow(1-t, 3) * a + 3 * Mathf.Pow(1-t,2) * t * aSupp + 3 * (1 - t) * Mathf.Pow(t, 2) * bSupp + Mathf.Pow(t, 3) * b;
	}
}
