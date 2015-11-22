/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 26.10.2015
 * Time: 22:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MedVis_Projekt
{
	/// <summary>
	/// Description of DataSet.
	/// </summary>
	public class DataSet
	{
		private DataSet()
		{
			// Who cares about default constructors, anyway?
		}
		
		private ulong voxelsX, voxelsY, voxelsZ;

		public ulong VoxelsX {
			get {
				return voxelsX;
			}
		}
		
		public ulong VoxelsY {
			get {
				return voxelsY;
			}
		}
		
		public ulong VoxelsZ {
			get {
				return voxelsZ;
			}
		}
		
		public struct DataLayer {
			public byte[] voxelData;
		}
		
		private ulong volumeNum;
		
		private double realSizeX, realSizeY, realSizeZ;
		
		private double sequenceDuration;

		public double SequenceDuration {
			get {
				return sequenceDuration;
			}
		}
		private ushort numBytesPerVoxel;

		public ushort NumBytesPerVoxel {
			get {
				return numBytesPerVoxel;
			}
		}
		
		private String dataFormat;

		public String DataFormat {
			get {
				return dataFormat;
			}
		}
		
		double[,] transformations = new double[4,4];
		
		//SDL.SDL_Surface[] layers;
		
		DataLayer[] datalayers;
		int[] textures;
		
		public int[] getOpenGLTextures()
		{
			return textures;
		}
		
		public DataSet(String path)
		{
			FileStream stream = File.OpenRead(path);
			byte[] buffer = new byte[4];
			
			// voxelsX:
			stream.Read(buffer, 0, 4);
			voxelsX = BitConverter.ToUInt32(buffer, 0);
			
			// voxelsX:
			stream.Read(buffer, 0, 4);
			voxelsY = BitConverter.ToUInt32(buffer, 0);
			
			// voxelsZ:
			stream.Read(buffer, 0, 4);
			voxelsZ = BitConverter.ToUInt32(buffer, 0);
			
			// volumeNum:
			stream.Read(buffer, 0, 4);
			volumeNum = BitConverter.ToUInt32(buffer, 0);
			
			
			// Buffer vergrößern:
			buffer = new byte[8];
			
			// realSizeX:
			stream.Read(buffer, 0, 8);
			realSizeX = BitConverter.ToDouble(buffer, 0);
			
			stream.Read(buffer, 0, 8);
			realSizeY = BitConverter.ToDouble(buffer, 0);
			
			stream.Read(buffer, 0, 8);
			realSizeZ = BitConverter.ToDouble(buffer, 0);
			
			stream.Read(buffer, 0, 8);
			sequenceDuration = BitConverter.ToDouble(buffer, 0);
			
			buffer = new byte[2];
			stream.Read(buffer, 0, 1);
			numBytesPerVoxel = BitConverter.ToUInt16(buffer, 0);
			
			buffer = new byte[128];
			stream.Read(buffer, 0, 128);
			dataFormat = Encoding.ASCII.GetString(buffer).Replace((char)0, '\t').Trim();
			
			buffer = new byte[8];
			for(int x = 0; x < 4; x++)
				for(int y = 0; y < 4; y++)
			{
				stream.Read(buffer, 0, 8);
				transformations[x, y] = BitConverter.ToDouble(buffer, 0);
			}
			stream.Seek(312, SeekOrigin.Begin);
			
			ulong _x = 0, _y = 0, _z = 0;
			datalayers = new DataLayer[(int)voxelsZ];
			textures = new int[(int)voxelsZ];
			GL.GenTextures((int)voxelsZ, textures);
			datalayers[0].voxelData = new byte[(int)this.voxelsX * (int)this.voxelsY];
			
			buffer = new byte[1];
			while(stream.Read(buffer, 0, 1) > 0)
			{
				if(dataFormat.EndsWith("8"))
				{
					try {
						datalayers[_z].voxelData[(int)_y * (int)this.voxelsY + (int)_x] = buffer[0];
					}
					catch(IndexOutOfRangeException)
					{
						return;
					}
				}
				else if(dataFormat.EndsWith("16"))
				{
					//TODO: Implement!
				}
				_x++;
				if(_x >= voxelsX)
				{
					_x = 0;
					_y++;
				}
				if(_y >= voxelsY)
				{
					GL.BindTexture(TextureTarget.Texture2D, textures[(int)_z]);
					GL.TexImage2D(TextureTarget.Texture2D, 0, 
					              PixelInternalFormat.Rgb,
					              (int)this.voxelsX, (int)this.voxelsY, 0, 
					              PixelFormat.Luminance, 
					              PixelType.UnsignedByte, datalayers[_z].voxelData);
					GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
					GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
					
					_y = 0;
					_z++;
					if(_z < voxelsZ)
					{
						datalayers[_z].voxelData = new byte[(int)this.voxelsX * (int)this.voxelsY];
					}
				}
			}
			stream.Close();
		}
		
		private ulong numberOfDataBytes()
		{
			return voxelsX * voxelsY * voxelsZ * volumeNum * numBytesPerVoxel;
		}
		
		public override String ToString()
		{
			return "=== DataSet follows: ===\r\n"
				+ "=== Voxels ===\r\n"
				+ "X: " + voxelsX + " | Y: " + voxelsY + " | Z: " + voxelsZ + "\r\n"
				+ "=== # of Volumes ===\r\n"
				+ volumeNum + "\r\n"
				+ "=== Real size ===\r\n"
				+ realSizeX + "x" + realSizeY + "x" + realSizeZ + "\r\n"
				+ "=== Seq. Duration ===\r\n"
				+ sequenceDuration + "\r\n"
				+ "=== No. Bytes / Voxel ===\r\n"
				+ numBytesPerVoxel + "\r\n"
				+ "=== Transformations ===\r\n"
				+ transformations + "\r\n"
				+ "=== Data format ===\r\n"
				+ dataFormat + "\r\n"
				//+ "=== total length ===\r\n"
				//+ stream.Length + "\r\n"
				//+ "=== No. DataBytes===\r\n"
				//+ numberOfDataBytes() + "\r\n"
				;
		}
	}
}
