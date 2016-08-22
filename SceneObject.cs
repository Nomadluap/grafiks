using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;

namespace OpenGL
{
	class SceneObject
	{
		List<Vector3> vertices = new List<Vector3>();
		List<int[]> faces = new List<int[]>();

		public SceneObject(string filename)
		{
			string[] lines = File.ReadAllLines(filename);
			
			foreach(string line in lines)
			{
				string[] tokens = line.Split(null);
				if (tokens[0] == "v")
				{
					//vertex coordinate
					vertices.Add(new Vector3(
						float.Parse(tokens[1]),
						float.Parse(tokens[2]),
						float.Parse(tokens[3])));
				} else if (tokens[0] == "f")
				{
					//face info
					//split each token on '/' and take first element
					faces.Add(new int[]
					{
						int.Parse(tokens[1].Split((char)'/')[0]),
						int.Parse(tokens[2].Split((char)'/')[0]),
						int.Parse(tokens[3].Split((char)'/')[0])
					});
				} else
				{
					//we don't care.
				}
			}
		}

		public Vector3[] GetVertexList()
		{
			List<Vector3> points = new List<Vector3>();
			foreach (var face in this.faces)
			{
				for(int i = 0; i < 3; i++)
				{
					points.Add(vertices[face[i] - 1]);
				}
			}
			return points.ToArray();
		}



	}
}
