/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 26.10.2015
 * Time: 21:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using WacomMTDN;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MedVis_Projekt
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		private DataSet set;
		private int layerNum = 0;
		
		private bool glControlLoaded = false;
		
		private WacomMTDNManager multiTouchManager = WacomMTDNManager.GetInstance();
		
		private bool useMIP;
		private int layer1, layer2;

		int multiTouchManager_FingerEvent(WacomMTFingerList fingerPacket)
		{
			if(set == null)
			  return 0;

			if(fingerPacket.Fingers.Count.Equals(1))
			{
				layerNum = Convert.ToInt32((set.VoxelsZ - 1) * fingerPacket.Fingers[0].Y);
				useMIP = false;
				glControl1.Invalidate();
			}
			if(fingerPacket.Fingers.Count.Equals(2))
			{
				layer1 = Convert.ToInt32((set.VoxelsZ - 1) * fingerPacket.Fingers[0].Y);
				layer2 = Convert.ToInt32((set.VoxelsZ - 1) * fingerPacket.Fingers[1].Y);
				
				useMIP = true;
				glControl1.Invalidate();
			}
			return 0;
		}

		void MainFormLoad(object sender, EventArgs e)
		{
			WacomMTError err = multiTouchManager.WacomMTInitialize(NativeConstants.WACOM_MULTI_TOUCH_API_VERSION);
			if(err == WacomMTError.WMTErrorSuccess)
			{
				multiTouchManager.FingerEvent += multiTouchManager_FingerEvent;
			}
		}
        
		void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			DialogResult res = openFileDialog1.ShowDialog();
			if(res == DialogResult.OK)
			{
				set = new DataSet(openFileDialog1.FileName);
				glControl1.Invalidate();
			}
		}
		void GlControl1Load(object sender, EventArgs e)
		{
			glControlLoaded = true;
			GL.ClearColor(Color.Black);
			SetupViewport();
		}
		
		private void SetupViewport()
    	{
      		int w = glControl1.Width;
      		int h = glControl1.Height;
      		GL.MatrixMode(MatrixMode.Projection);
      		GL.LoadIdentity();
      		GL.Ortho(0, w, 0, h, -1, 1); // Bottom-left corner pixel has coordinate (0, 0)
      		GL.Viewport(0, 0, w, h); // Use all of the glControl painting area
    	}
		
		void GlControl1Paint(object sender, PaintEventArgs e)
		{
			try {
			if(!glControlLoaded)
				return;
			
		
			if(set == null)
			{
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				glControl1.SwapBuffers();
				return;
			}
			
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.MatrixMode(MatrixMode.Modelview);
		    GL.LoadIdentity();
		    GL.Enable(EnableCap.Texture2D);
		    if(!useMIP)
		    {
		    	drawLayer(layerNum);
		    }
		    else
		    {
		    	GL.Enable(EnableCap.Blend);
		    	GL.BlendEquation(BlendEquationMode.Max);
		    	int start, end;
		    	if(layer1 < layer2)
		    	{
		    		start = layer1; end = layer2;
		    	}
		    	else
		    	{
		    		start = layer2; end = layer1;
		    	}
		    	for(int i = start; i <= end; i++)
		    		drawLayer(i);
		    }
			glControl1.SwapBuffers();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString() + Environment.NewLine + "Number of layers: " + set.getOpenGLTextures().Length);
			}
		}
		
		private void drawLayer(int layerNum)
		{
			GL.BindTexture(TextureTarget.Texture2D, set.getOpenGLTextures()[layerNum]);
		    GL.Begin(PrimitiveType.Quads);
		    //GL.Translate((Width / 2) - (int)set.VoxelsX / 2, (Height / 2) - (int)set.VoxelsY / 2, 0);
		    GL.TexCoord2(0, 0); GL.Vertex2(0, 0);
		    GL.TexCoord2(1, 0); GL.Vertex2(set.VoxelsX, 0);
		    GL.TexCoord2(1, 1); GL.Vertex2(set.VoxelsX, set.VoxelsY);
		    GL.TexCoord2(0, 1); GL.Vertex2(0, set.VoxelsY);
		    GL.End();
			
		}
		
		void GlControl1Resize(object sender, EventArgs e)
		{
			if(!glControlLoaded)
				return;
			SetupViewport();
			glControl1.Invalidate();
		}
		void MainFormKeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Down)
				layerNum++;
			if(e.KeyCode == Keys.Up)
				layerNum--;
			glControl1.Invalidate();
		}
		void MainFormResizeEnd(object sender, EventArgs e)
		{
			SetupViewport();
			glControl1.Invalidate();
		}
	}
}
