using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OpenGL
{
	class Program
	{
		static int vertexBuffer = 0;
		static GameWindow window;
		static int programID;
		static Matrix4 modelTransform;
		static Matrix4 viewTransform;
		static Matrix4 perspectiveTransform;
		static int numVertices = 0;
		static void Main(string[] args)
		{
			window = new GameWindow(1920, 1080, GraphicsMode.Default, "fubarbaz");
			//			window.MakeCurrent();
			//			GL.Viewport(0, 0, window.Width, window.Height);

			modelTransform = Matrix4.Identity;
			viewTransform =	Matrix4.LookAt(new Vector3(0.0f, 4.0f, 4.0f), 
				new Vector3(0.0f, 0.0f, 0.0f),
				new Vector3(0.0f, 1.0f, 0.0f));
			perspectiveTransform = Matrix4.CreatePerspectiveFieldOfView(3.14159f / 2.0f, 16.0f / 9.0f, 1.0f, 100.0f);

			var point = new Vector4(0f, 0f, 0f, 1.0f);


			window.RenderFrame += Window_RenderFrame;
			window.UpdateFrame += Window_UpdateFrame;
			window.Load += Window_Load;

			window.Run(30.0);
			
		}

		private static void Window_UpdateFrame(object sender, FrameEventArgs e)
		{

			if (window.Keyboard[OpenTK.Input.Key.Escape])
				window.Exit();

			//axis one of the joystick rotates about y-axis
			float yrot = (window.Keyboard[Key.Right]) ? 1.0f : (window.Keyboard[Key.Left]) ? -1.0f : 0.0f;
			float xrot = (window.Keyboard[Key.Up]) ? 1.0f : (window.Keyboard[Key.Down]) ? -1.0f : 0.0f;
			//we want to rotate the camera view by 1 radian per second
			//Matrix4.CreateFromAxisAngle(new Vector3(0f, 1f, 0f), yrot * 1.0f / (float)window.UpdatePeriod);
			Matrix4 yrotation = Matrix4.CreateFromAxisAngle(new Vector3(0f, 1f, 0f), 1.0f * yrot * (float)window.TargetUpdatePeriod);
			Matrix4 xrotation = Matrix4.CreateFromAxisAngle(Vector4.Transform(new Vector4(1f, 0f, 0f, 1f), modelTransform.Inverted()).Xyz, 1.0f * xrot * (float)window.TargetUpdatePeriod);
			modelTransform = xrotation * yrotation * modelTransform;
		}

		private static void Window_Load(object sender, EventArgs e)
		{

			SceneObject cubeHat = new SceneObject("cubehat.obj");

			var vertices = cubeHat.GetVertexList();
			numVertices = vertices.Length;

			//Vector3[] vertices = new Vector3[]
			//{
			//	new Vector3(-1.0f, -1.0f, 0.0f),
			//	new Vector3(1.0f, -1.0f, 0.0f),
			//	new Vector3(0.0f, 1.0f, 0.0f)
			//};

			GL.GenBuffers(1, out vertexBuffer);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * Vector3.SizeInBytes),
				vertices, BufferUsageHint.StaticDraw);


			int vertexShaderID = GL.CreateShader(ShaderType.VertexShader);
			int fragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);

			string vertexShaderCode = @"
#version 330 core
layout(location=0) in vec3 vertexPosition_modelspace;
out float invdepth;
uniform mat4 modelTransform;
uniform mat4 viewTransform;
uniform mat4 perspectiveTransform;
void main()
{
	gl_Position = perspectiveTransform * viewTransform * modelTransform * vec4(vertexPosition_modelspace, 1.0);
	invdepth = clamp(2.0 / pow(gl_Position.z, 2) + 0.3, 0.0, 1.0);
}
";
			string fragmentShaderCode = @"
#version 330 core
in float invdepth;
out vec3 color;
void main()
{
	color = vec3(invdepth, invdepth, invdepth);
}
";
			GL.ShaderSource(vertexShaderID, vertexShaderCode);
			GL.CompileShader(vertexShaderID);
			Console.WriteLine("vertex shader:" + GL.GetShaderInfoLog(vertexShaderID));

			GL.ShaderSource(fragmentShaderID, fragmentShaderCode);
			GL.CompileShader(fragmentShaderID);
			Console.WriteLine("fragment shader:" + GL.GetShaderInfoLog(fragmentShaderID));

			programID = GL.CreateProgram();
			GL.AttachShader(programID, vertexShaderID);
			GL.AttachShader(programID, fragmentShaderID);
			GL.LinkProgram(programID);
			Console.WriteLine("program:" + GL.GetProgramInfoLog(programID));

			GL.DetachShader(programID, vertexShaderID);
			GL.DetachShader(programID, fragmentShaderID);

			GL.DeleteShader(vertexShaderID);
			GL.DeleteShader(fragmentShaderID);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Less);
			
			GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
			GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
			GL.LineWidth(2.0f);

		}

		private static void Window_RenderFrame(object sender, FrameEventArgs e)
		{

			window.MakeCurrent();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.EnableVertexAttribArray(0);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
			GL.VertexAttribPointer(
				0,
				3,
				VertexAttribPointerType.Float,
				false,
				0,
				0);
			GL.UseProgram(programID);
			int uModel = GL.GetUniformLocation(programID, "modelTransform");
			int uView = GL.GetUniformLocation(programID, "viewTransform");
			int uPerspective = GL.GetUniformLocation(programID, "perspectiveTransform");

			//GL.BindBuffer(BufferTarget.UniformBuffer, mbuffer);
			GL.UniformMatrix4(uModel, false, ref modelTransform);

			//GL.BindBuffer(BufferTarget.UniformBuffer, mbuffer);
			GL.UniformMatrix4(uView, false, ref viewTransform);

			//GL.BindBuffer(BufferTarget.UniformBuffer, mbuffer);
			GL.UniformMatrix4(uPerspective, false, ref perspectiveTransform);

			//draw the triangle
			GL.DrawArrays(BeginMode.Triangles, 0, numVertices * 3);

			GL.DisableVertexAttribArray(0);

			window.SwapBuffers();
			
		}
	}
}
