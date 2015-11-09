/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 26.10.2015
 * Time: 21:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 
using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

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
		
		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterTouchWindow(System.IntPtr hWnd, uint ulFlags);
		
		private const int WM_TOUCH = 0x0240;
		
		private IntPtr renderer, window;
		private DataSet set;
		private int layerNum = 0;
		
		
		void MainFormLoad(object sender, EventArgs e)
		{
			try {
			if(!RegisterTouchWindow(this.Handle, 0))
			{
				MessageBox.Show("Could not register touch input!");
			}
			}
			catch(Exception)
			{
				// Who cares about exceptions, anyway?
				MessageBox.Show("Error occurred while registering touch input!");
			}
			

		}
		
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            // Decode and handle WM_TOUCH message.
            bool handled = false;
            switch (m.Msg)
            {
                case WM_TOUCH:
                    //handled = DecodeTouch(ref m);
                    MessageBox.Show("Received touch event");
                    break;
                default:
                    handled = false;
                    //MessageBox.Show("Recv. other event");
                    break;
            }

            // Call parent WndProc for default message processing.
            base.WndProc(ref m);

            if (handled)
            {
                // Acknowledge event if handled.
                m.Result = new System.IntPtr(1);
            }
        }
        
		IntPtr layer;
		void OpenToolStripMenuItemClick(object sender, EventArgs e)
		{
			DialogResult res = openFileDialog1.ShowDialog();
			if(res == DialogResult.OK)
			{
				SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
				window = SDL.SDL_CreateWindow("Test", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED, 800, 600, 
				                              SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
				renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
				
				byte[] contents = File.ReadAllBytes(openFileDialog1.FileName);
				set = new DataSet(contents, renderer);
				MessageBox.Show(set.ToString());
							

				/*for(int x = 0; x < (int)set.VoxelsX; x++)
					for(int y = 0; y < (int)set.VoxelsY; y++)
						for(int z = 0; z < (int)set.VoxelsZ; z++)
				{
					Voxel v = set.getVoxels()[x, y, z];
					if(v == null)
						continue;
					byte val = v.toGreyscaleValue();
				}*/
				
				//for(int i = 0; i < set.voxels.Count; i++)
				//{
					//MessageBox.Show(((IVoxel)(set.voxels[i])).toHounsfieldT().ToString());
				//}
				
				
				renderZImage(15);
				

				while(true)
				{
					SDL.SDL_Event evt;
					while(SDL.SDL_PollEvent(out evt) == 1)
					{
						if(evt.type == SDL.SDL_EventType.SDL_KEYDOWN)
						{
							onKeyDown(evt);
						}
						if(evt.type == SDL.SDL_EventType.SDL_MULTIGESTURE)
						{
							MessageBox.Show("Multi-Gesture!");
						}
					}
				}
				
			}
		}
		
		void renderZImage(int z)
		{				
			//SDL.SDL_RenderClear(renderer);
			SDL.SDL_Surface layer_surf = set.getLayers()[z];
			unsafe {
				layer = Marshal.AllocHGlobal(sizeof(SDL.SDL_Surface));
			}

			Marshal.StructureToPtr(layer_surf, layer, true);
			SDL.SDL_BlitSurface(layer, IntPtr.Zero, SDL.SDL_GetWindowSurface(window), IntPtr.Zero);
			SDL.SDL_UpdateWindowSurface(window);
		
			/*
			for(ulong x = 0; x < set.VoxelsX; x++)
				for(ulong y = 0; y < set.VoxelsY; y++)
			{
				Voxel current;
				try {
					current = set.getVoxels()[(int)x, (int)y, (int)z];
				}
				catch(IndexOutOfRangeException)
				{
					continue;
				}
				if(current == null)
					continue;
				
				byte greyscale_val = current.toGreyscaleValue();
				
				SDL.SDL_SetRenderDrawColor(renderer, greyscale_val, greyscale_val, greyscale_val, 255);
				SDL.SDL_RenderDrawPoint(renderer, (int)x, (int)y);
			}*/
			//SDL.SDL_RenderPresent(renderer);
		}
		
		void onKeyDown(SDL.SDL_Event evt)
		{
			if(evt.key.keysym.sym == SDL.SDL_Keycode.SDLK_DOWN && layerNum < (int)set.VoxelsZ)
				layerNum++;
			if(evt.key.keysym.sym == SDL.SDL_Keycode.SDLK_UP && layerNum > 0)
				layerNum--;
			renderZImage(layerNum);
			SDL.SDL_SetWindowTitle(window, "Showing z-layer " + layerNum + " of " + (int)set.VoxelsZ);
		}
	}
}
