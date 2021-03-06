﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell {
	Point[,,] points;
	List<Edge> edges;

	bool dirty = true;
	Vector3 point;

	public Cell(int x, int y, int z, float cellSize, Point[,,] points, List<Edge> edges, IIsoSurface surface)
	{
		this.edges = new List<Edge>();
		this.points = new Point[2,2,2];
		for(int xx = 0; xx < 2; xx++)
		{
			for(int yy = 0; yy < 2; yy++)
			{
				for(int zz = 0; zz < 2; zz++)
				{
					this.points[xx,yy,zz] = getPoint(x + xx, y + yy, z + zz, cellSize, points, surface);
				}
			}	
		}

		point = new Vector3(x + 0.5f,y + 0.5f,z + 0.5f);

		addEdge(this.points[0,0,0], this.points[1,0,0], surface, edges);
		addEdge(this.points[0,0,0], this.points[0,1,0], surface, edges);
		addEdge(this.points[0,0,0], this.points[0,0,1], surface, edges);

		addEdge(this.points[0,0,1], this.points[0,1,1], surface, edges);
		addEdge(this.points[0,0,1], this.points[1,0,1], surface, edges);

		addEdge(this.points[0,1,0], this.points[0,1,1], surface, edges);
		addEdge(this.points[0,1,0], this.points[1,1,0], surface, edges);

		addEdge(this.points[0,1,1], this.points[1,1,1], surface, edges);

		addEdge(this.points[1,0,0], this.points[1,0,1], surface, edges);
		addEdge(this.points[1,0,0], this.points[1,1,0], surface, edges);

		addEdge(this.points[1,0,1], this.points[1,1,1], surface, edges);

		addEdge(this.points[1,1,0], this.points[1,1,1], surface, edges);
	}

	Point getPoint(int x, int y, int z, float cellSize, Point[,,] points, IIsoSurface surface){
		Point point = points[x,y,z];
		if(point == null){
			Vector3 p = new Vector3(x * (float)cellSize,y * (float)cellSize,z * (float)cellSize);
			point = new Point(surface.sample(p.x,p.y,p.z), p, x, y, z);
			points[x,y,z] = point;
		}

		return point;
	}

	void addEdge(Point A, Point B, IIsoSurface surface, List<Edge> edges)
	{
		//Debug.DrawLine(A.p, B.p, new Color(0.2f,0.2f,0.2f,0.2f), 30);
		if((A.iso < 0 && B.iso > 0) || (A.iso > 0 && B.iso < 0))
		{

			Edge edge = A.getEdge(B, surface, edges);

			if(A!=edge.A && B != edge.B)
				if(A!=edge.B&&A!=edge.A)
				{
					//Debug.DrawLine(A.p, B.p, new Color(0,1,0,0.4f), 30);
					//Debug.DrawLine(edge.A.p, edge.B.p, new Color(1,0,0,0.4f), 30);
				}

			edges.Add(edge);
			this.edges.Add(edge);
			edge.addCell(this);
		}
	}

	public void DebugDraw(IIsoSurface surface)
	{
		if(this.edges.Count < 1) return;

		Vector3 p = getPoint();
		Debug.DrawRay(p,surface.sampleDerivative(p.x,p.y,p.z), new Color(0,0.4f,0,0.4f));
		//Debug.DrawLine(Vector3.zero, p, Color.blue);
	}

	public Vector3 getPoint()
	{
		if(dirty)
		{
			dirty = false;

			List<Vector3> p = new List<Vector3>();
			List<Vector3> n = new List<Vector3>();

			foreach(Edge edge in edges)
			{
				p.Add(edge.p);
				n.Add(edge.n);
			}

			point = QEFSolver.minimize(p.ToArray(),n.ToArray(),point,50,0.02f);
		}

		return point;
	}
}
