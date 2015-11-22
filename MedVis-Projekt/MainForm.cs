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

		int multiTouchManager_FingerEvent(WacomMTFingerList fingerPacket)
		{
			if(set == null)
			  return 0;

			if(fingerPacket.Fingers.Count.Equals(1))
			{
				layerNum = Convert.ToInt32((set.VoxelsZ - 1) * fingerPacket.Fingers[0].Y);
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
			GL.ClearColor(Color.SkyBlue);
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
				return;
			
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.MatrixMode(MatrixMode.Modelview);
		    GL.LoadIdentity();
		    GL.Enable(EnableCap.Texture2D);
		    GL.BindTexture(TextureTarget.Texture2D, set.getOpenGLTextures()[layerNum]);
		    GL.Begin(BeginMode.Quads);
		    GL.TexCoord2(0, 0); GL.Vertex2(10, 10);
		    GL.TexCoord2(1, 0); GL.Vertex2(set.VoxelsX, 10);
		    GL.TexCoord2(1, 1); GL.Vertex2(set.VoxelsX, set.VoxelsY);
		    GL.TexCoord2(0, 1); GL.Vertex2(10, set.VoxelsY);
		    GL.End();
			
			glControl1.SwapBuffers();
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.ToString() + Environment.NewLine + "Number of layers: " + set.getOpenGLTextures().Length);
			}
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
			MessageBox.Show(layerNum.ToString());
			glControl1.Invalidate();
		}
	}
}
