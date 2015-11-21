/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 26.10.2015
 * Time: 22:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
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
		
		Byte[] stream;
		
		double[,] transformations = new double[4,4];
		
		private Voxel[,,] voxels;
		
		public Voxel[,,] getVoxels()
		{
			return voxels;
		}
		
		//SDL.SDL_Surface[] layers;
		
		DataLayer[] datalayers;
		int[] textures;
		
		public int[] getOpenGLTextures()
		{
			return textures;
		}
		
		public DataSet(Byte[] stream)
		{	
			this.stream = stream;

			voxelsX = ByteStreamParser.getUint32Value(stream, 0, 3);
			voxelsY = ByteStreamParser.getUint32Value(stream, 4, 7);
			voxelsZ = ByteStreamParser.getUint32Value(stream, 8, 11);
			//voxels = new Voxel[voxelsX, voxelsY, voxelsZ];
			//layers = new SDL.SDL_Surface[voxelsZ];
			volumeNum = ByteStreamParser.getUint32Value(stream, 12, 15);
			
			realSizeX = ByteStreamParser.getDoubleValue(stream, 16, 23);
			realSizeY = ByteStreamParser.getDoubleValue(stream, 24, 31);
			realSizeZ = ByteStreamParser.getDoubleValue(stream, 32, 39);
			
			sequenceDuration = ByteStreamParser.getDoubleValue(stream, 40, 47);
			
			numBytesPerVoxel = ByteStreamParser.getUShortValue(stream, 48, 49);
			
			dataFormat = ByteStreamParser.getStringValue(stream, 50, 177);
			
			
			int index = 178;
			Console.WriteLine("Transformations:");
			for(int x = 0; x < 4; x++)
			{
				for(int y = 0; y < 4; y++)
				{
					transformations[x,y] = ByteStreamParser.getDoubleValue(stream, index, index + 8);
					index += 8;
					//Console.Write(transformations[x, y]);
				}
				//Console.WriteLine();
			}
			
			index = 312;
			
			ulong _x = 0, _y = 0, _z = 0;
			datalayers = new DataLayer[(int)voxelsZ];
			textures = new int[(int)voxelsZ];
			GL.GenTextures((int)voxelsZ, textures);
			datalayers[0].voxelData = new byte[(int)this.voxelsX * (int)this.voxelsY];
			while(index < stream.Length - 1)
			{
				Voxel voxel = null;
				if(dataFormat.EndsWith("8"))
				{
					byte val = ByteStreamParser.getSubArray(stream, index, index + 1)[0];
					try {
						datalayers[_z].voxelData[(int)_y * (int)this.voxelsY + (int)_x] = val;
					}
					catch(IndexOutOfRangeException)
					{
						return;
					}
					//voxel = new Voxel(val);
				}
				else if(dataFormat.EndsWith("16"))
				{
					//voxel = new Voxel(ByteStreamParser.getUShortValue(stream, index, index + 1));
				}
				//voxels[_x, _y, _z] = voxel;
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
				index++;
			}
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
				+ "=== total length ===\r\n"
				+ stream.Length + "\r\n"
				+ "=== No. DataBytes===\r\n"
				+ numberOfDataBytes() + "\r\n";
		}
	}
}
