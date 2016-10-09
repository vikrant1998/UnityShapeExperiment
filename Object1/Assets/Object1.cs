using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

// Vertices of the object.
public struct vertices
{
	public float x;
	public float y;
	public float z;
};

// Three vertices of a triangle.
public struct triangles
{
	public int pointOne;
	public int pointTwo;
	public int pointThree;
};

// Two vertices of an edge.
public struct edges
{
	public int pointOne;
	public int pointTwo;
};

public class Object1 : MonoBehaviour 
{
	vertices[]  coordinate;			// Array of vertices objects.
	triangles[] triangle;			// Array of triangles objects.
	edges[]     edge;				// Array of edges objects.
	
	int numVertices;				// Number of vertices of the object.
	int numFaces;					// Number of faces of the object.
	int numEdges;					// Number of edges of the object.
	
	int whichObject = 1;
	
	Vector3 centroid;				// Centroid of the object.
	
	/* Vertices of the object of type Vector3.
	   This is a copy of the coordinates of type vertices.
	   Vector3 is used as Unity does not allow to subtract 
	   vectors which have different struct types. */ 
	Vector3[] vertices;				
	
	Vector3[] startingPoint;		// Start point of a line.
	Vector3[] endPoint;				// End point of a line.
	
	public GameObject[] gameObj;	// Array of GameObjects to store each line drawn.
	
	Quaternion newRotation = new Quaternion();
	
	// .OFF File to parse.
	string text = System.IO.File.ReadAllText(@"C:\Users\Darwin\Downloads\Objects\Objects\1.off");
	char[] delimiterChars = {'\n', ' ', '\t'};
	
	// Assigns values for variables to use in the update function. (any key is pressed.)
	void Start () 
	{
		// Parsing the .OFF file with the given delimiters.
		string[] words = text.Split(delimiterChars);
		
		string fileFormat = words[0];				// File format. 
		numVertices   = Int32.Parse(words[1]);		// Number of vertices.
		numFaces      = Int32.Parse(words[2]);		// Number of faces.
		numEdges      = Int32.Parse(words[3]);		// Number of edges.
		
		coordinate = new vertices[numVertices];		// coordinate now has space to store the vertices.
		triangle   = new triangles[numFaces];		// triangle now has space to store information about how triangles are drawn.
		edge       = new edges[numEdges];			// edge now has space to store information about how edges are drawn.
		centroid = new Vector3 (0, 0, 0);			// Centroid definition, has space to store the centroid.
		
		startingPoint = new Vector3[numEdges];		// startingPoint now has space to store the starting points of all the edges to be drawn.
		endPoint = new Vector3[numEdges];			// endPoint now has space to store the end point of the edges to be drawn.
		
		int k = 4;								 
		for (int i = 0; i < numVertices; i++)
		{
			// Storing the vertices of the object in coordinate.
			coordinate[i].x = float.Parse(words[k++]);			
			coordinate[i].y = float.Parse(words[k++]);
			coordinate[i].z = float.Parse(words[k++]);
			
			// Adding all the coordinate vectors and storing the result in centroid.
			centroid += new Vector3(coordinate[i].x, coordinate[i].y, coordinate[i].z); 
		}
		// Storing the average of all vector coordinates in centroid.
		centroid = centroid / numVertices;
		
		// Storing the three points with which each triangle is drawn in each triangle element.
		for (int i = 0; i < numFaces; i++)
		{
			triangle[i].pointOne   = Int32.Parse(words[k++]);
			triangle[i].pointTwo   = Int32.Parse(words[k++]);
			triangle[i].pointThree = Int32.Parse(words[k++]);
		}
		
		// Storing the two points with which each edge is drawn in each edge element.
		for (int i = 0; i < numEdges; i++)
		{
			edge[i].pointOne   = Int32.Parse(words[k++]);
			edge[i].pointTwo   = Int32.Parse(words[k++]);
		}
		
		// Creating space to store vertices which is a Vector3.
		vertices = new Vector3[numVertices];
		
		/* Storing each vertex of the object in a vertices element of type Vector3.
		   This is useful if we do vector arithmetic. */
		for (int i = 0; i < numVertices; i++)
		{
			vertices[i] = new Vector3(coordinate[i].x, coordinate[i].y, coordinate[i].z);
		}
		
		// Allocating space to store each line to be drawn.
		gameObj = new GameObject[numEdges];
		
		// Creating each edge to be displayed on the object.
		for (int i = 0; i < numEdges; i++)
		{
			// One GameObject called LineObject per line.
			gameObj[i] = new GameObject("LineObject");	
			
			// Adding a component called the LineRenderer which makes the lines visible in Oculus Mode.
			var line = gameObj[i].AddComponent<LineRenderer>();	
			
			// Getting x,y,z component of the starting position of the line to be drawn. 
			float pointOne_x = coordinate[edge[i].pointOne].x;
			float pointOne_y = coordinate[edge[i].pointOne].y;
			float pointOne_z = coordinate[edge[i].pointOne].z;
			
			// Getting x,y,z component of the end position of the line to be drawn.
			float pointTwo_x = coordinate[edge[i].pointTwo].x;
			float pointTwo_y = coordinate[edge[i].pointTwo].y;
			float pointTwo_z = coordinate[edge[i].pointTwo].z;
			
			// pos1 is the vector from origin to starting point of the line.
			Vector3 pos1 = new Vector3 (pointOne_x, pointOne_y, pointOne_z);
			
			// pos2 is the vector from origin to the end point of the line.
			Vector3 pos2 = new Vector3 (pointTwo_x, pointTwo_y, pointTwo_z);
			
			// Finding the unit vector from centroid to starting point and end point.
			Vector3 pos1_Relative = (pos1 - centroid);
			Vector3 pos2_Relative = (pos2 - centroid);
			float pos1_Relative_magnitude = pos1_Relative.sqrMagnitude;
			float pos2_Relative_magnitude = pos2_Relative.sqrMagnitude;
			
			// pos1_Unit is the unit vector from centroid to starting point of the line.
			Vector3 pos1_Unit = pos1_Relative / pos1_Relative_magnitude;
			
			// pos2_Unit is the unit vector from centroid to the end point of the line.
			Vector3 pos2_Unit = pos2_Relative / pos2_Relative_magnitude;
			
			// The starting point and end point are shifted to make the lines more visible.
			startingPoint[i] =  200 * pos1_Unit + new Vector3 (pointOne_x , pointOne_y , pointOne_z);
			endPoint[i] = 200 * pos2_Unit + new Vector3 (pointTwo_x, pointTwo_y , pointTwo_z);
		} 
		
		// Setting the euler angles for rotation, the object rotates clockwise initially.
		newRotation.eulerAngles = new Vector3(0, Time.deltaTime * 10, 0);
	}
	
	void Update()
	{
		if (Input.GetKeyDown("right"))
		{
			// Write current object specs to OFF file.
			string inputPath = "C:\\Users\\Darwin\\Downloads\\Objects\\Objects_Output\\" + whichObject + "_Out.OFF";
			WriteOFF (inputPath);
			
			// Destroy current Object.
			whichObject++;
			for (int i = 0; i < numEdges; i++)
			{
				if (gameObj[i] != null)
				{
					Destroy (gameObj[i]);
				}
			}
			
			// Load next object
			string outputPath = "C:\\Users\\Darwin\\Downloads\\Objects\\Objects\\" + whichObject + ".OFF";
			print (outputPath);
			text = System.IO.File.ReadAllText(@outputPath);
			Start ();

		}
		else if (Input.GetKeyDown("left"))
		{
			// Write current object specs to OFF file.
			string inputPath = "C:\\Users\\Darwin\\Downloads\\Objects\\Objects_Output\\" + whichObject + "_Out.OFF";
			WriteOFF (inputPath);
			
			// Destroy current Object.
			whichObject--;
			for (int i = 0; i < numEdges; i++)
			{
				if (gameObj[i] != null)
				{
					Destroy (gameObj[i]);
				}
			}
			
			// Load next object
			string outputPath = "C:\\Users\\Darwin\\Downloads\\Objects\\Objects\\" + whichObject + ".OFF";
			print (outputPath);
			text = System.IO.File.ReadAllText(@outputPath);
			Start ();
		}

		// Get the Mesh
		MeshFilter mf = GetComponent <MeshFilter>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		
		// Creating center of type Vector3 and making it equal to centroid.
		Vector3 center = centroid;
		
		// Increase speed of rotation in clockwise direction.
		if (Input.GetKey("up"))
		{
			newRotation.eulerAngles += new Vector3(0, Time.deltaTime, 0);
		}
		
		// Increase speed of rotation in anti-clockwise direction. 
		else if (Input.GetKey("down"))
		{
			newRotation.eulerAngles -= new Vector3(0, Time.deltaTime, 0);
		}
		
		// Stop object rotation.
		else if (Input.GetKey("space"))
		{
			newRotation.eulerAngles = new Vector3(0, 0, 0);
		}
		
		// Updating the vertices of the object - (Rotating based on the euler angles.)
		for(int i = 0; i < numVertices; i++) 
		{	
			vertices[i] = newRotation * (vertices[i] - center) + center;
		}
		
		/*  
		
			VERTICES ARE MODIFIED THIS WAY 
			    ----              ----
		        |                    |
			    |          x         |
 			    |          y         |
			    | m1*x + m2*y + m3*z |
			    |					 |
			    ----              ----
			
		*/
		
		// If Q is pressed. m1 parameter is increased, m2 and m3 are zero.
		if (Input.GetKey(KeyCode.Q))
		{
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] += new Vector3(0, 0, vertices[i].x / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		// If A is pressed. m1 parameter is decreased, m2 and m3 are zero.
		else if (Input.GetKey(KeyCode.A))
		{
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] -= new Vector3(0, 0, vertices[i].x / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		// If W is pressed. m2 parameter is increased, m1 and m3 are zero.
		else if (Input.GetKey(KeyCode.W))
		{
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] += new Vector3(0, 0, vertices[i].y / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		// If S is pressed. m2 parameter is decreased, m1 and m3 are zero.
		else if (Input.GetKey(KeyCode.S))
		{
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] -= new Vector3(0, 0, vertices[i].y / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		// If E is pressed. m3 parameter is increased, m1 and m2 are zero.
		else if (Input.GetKey(KeyCode.E))
		{	
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] += new Vector3(0, 0, vertices[i].z / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		// If D is pressed. m3 parameter is decreased, m1 and m2 are zero.
		else if (Input.GetKey(KeyCode.D))
		{
			// Updating centroid.
			centroid = new Vector3(0, 0, 0);
			for (int i = 0; i < numVertices; i++)
			{
				vertices[i] -= new Vector3(0, 0, vertices[i].z / 100);
				centroid += vertices[i];
			}
			centroid /= numVertices;
		}
		
		int[] tri = AssignTriangles ();						// Assigning the triangles to be drawn.
		Vector3[] normals = AssignNormals ();				// Assigning the normals.
		Vector2[] uv = AssignUV ();							// UVs (textures are displayed)
		AssignMesh (vertices, tri, normals, uv, mesh);      // Assign Arrays to the mesh.
		DisplayLine();										// Display the lines.
	}
	
	// Assigning the triangles to be drawn.
	int[] AssignTriangles ()
	{
		// Triangles
		// tri is an array which holds how each triangle is going to be drawn.
		int[] tri = new int[numFaces * 3];
		
		// Storing how each triangle will be drawn.
		for (int i = 0, j = 0; i < numFaces * 3; i += 3, j++)
		{
			tri[i] =  triangle[j].pointOne;
			tri[i + 1] =  triangle[j].pointTwo;
			tri[i + 2] =  triangle[j].pointThree;
		}
		return tri;
	}
	
	// Writes content to OFF file.
	void WriteOFF (string inputPath)
	{
		string content = "OFF\n";
		content = content + numVertices + "\t" + numFaces + "\t" + numEdges;
		
		for (int i = 0; i < numVertices; i++)
		{	
			content = content + "\n" + vertices[i].x + "\t" + vertices[i].y + "\t" + vertices[i].z;
		}
			
		for (int i = 0; i < numFaces; i++)
		{	
			content = content + "\n" + triangle[i].pointOne + "\t" + triangle[i].pointTwo + "\t" + triangle[i].pointThree;
		}
			
		for (int i = 0; i < numEdges; i++)
		{	
			content = content + "\n" + edge[i].pointOne + "\t" + edge[i].pointTwo;
		}

		System.IO.File.WriteAllText(inputPath, content);
	}
	
	// Assigning the normals
	Vector3[] AssignNormals ()
	{
		// Normals - (To display the object)
		Vector3[] normals = new Vector3[numVertices];
		for (int i = 0; i < numVertices; i++)
		{
			normals[i] = -Vector3.forward;
		}
		return normals;
	}
	
	// Assigning the UVs
	Vector2[] AssignUV ()
	{
		Vector2[] uv = new Vector2[numVertices];
		for (int i = 0; i < numVertices; i++)
		{
			uv[1] = new Vector2(1, 1);
		}
		return uv;
	}	
	
	// Assign Arrays to the mesh.
	void AssignMesh (Vector3[] vertices, int[] tri, Vector3[] normals, Vector2[] uv, Mesh mesh)
	{
		mesh.vertices = vertices;
		mesh.triangles = tri;
		mesh.normals = normals;
		mesh.uv = uv;
		gameObject.GetComponent<Renderer> ().material.color = Color.green;
	}
	
	// Display the lines.
	void DisplayLine ()
	{
		for (int i = 0; i < numEdges; i++)
		{
			// Getting the line renderer component which we initialized in start function.
			
			var line = gameObj[i].GetComponent<LineRenderer>();
			
			// Getting x,y,z component of the starting position of the line to be drawn.
			float pointOne_x = vertices[edge[i].pointOne].x;
			float pointOne_y = vertices[edge[i].pointOne].y;
			float pointOne_z = vertices[edge[i].pointOne].z;
			
			// Getting x,y,z component of the end position of the line to be drawn.
			float pointTwo_x = vertices[edge[i].pointTwo].x;
			float pointTwo_y = vertices[edge[i].pointTwo].y;
			float pointTwo_z = vertices[edge[i].pointTwo].z;
			
			// pos1 is the vector from origin to starting point of the line.
			Vector3 pos1 = new Vector3 (pointOne_x, pointOne_y, pointOne_z);
			
			// pos2 is the vector from origin to the end point of the line.
			Vector3 pos2 = new Vector3 (pointTwo_x, pointTwo_y, pointTwo_z);
			
			// Finding the unit vector from centroid to starting point and end point.
			Vector3 pos1_Relative = (pos1 - centroid);
			Vector3 pos2_Relative = (pos2 - centroid);
			float pos1_Relative_magnitude = pos1_Relative.sqrMagnitude;
			float pos2_Relative_magnitude = pos2_Relative.sqrMagnitude;
			
			// pos1_Unit is the unit vector from centroid to starting point of the line.
			Vector3 pos1_Unit = pos1_Relative / pos1_Relative_magnitude;
			
			// pos2_Unit is the unit vector from centroid to the end point of the line.
			Vector3 pos2_Unit = pos2_Relative / pos2_Relative_magnitude;
			
			// The starting point and end point are shifted to make the lines more visible.
			startingPoint[i] =  200 * pos1_Unit + new Vector3 (pointOne_x , pointOne_y , pointOne_z);
			endPoint[i] = 200 * pos2_Unit + new Vector3 (pointTwo_x, pointTwo_y , pointTwo_z);
			
			// Number of vertices of the line, we need two vertices for drawing the edge.
			line.SetVertexCount(2);		
			
			// Setting up the line and displaying it.
			line.SetPosition(0, startingPoint[i]);
			line.SetPosition(1, endPoint[i]);
			line.SetWidth(4f, 4f);
			line.useWorldSpace = true;
		} 
	}
	

}
