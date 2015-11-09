/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 04.11.2015
 * Time: 10:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;

namespace MedVis_Projekt
{
	/// <summary>
	/// Description of Voxel.
	/// </summary>
	public class Voxel
	{
		private byte p_cValue;
		
		private ushort p_uValue;
		
		private enum VoxelType {
			VOXEL_8, VOXEL_16
		}
		
		VoxelType p_voxelType;
		
		public Voxel(byte value)
		{
			p_cValue = value;
			p_voxelType = VoxelType.VOXEL_8;
		}
		
		public Voxel(ushort value)
		{
			p_uValue = value;
			p_voxelType = VoxelType.VOXEL_16;
		}
		
		private double getVal()
		{
			double val = 0.0;
			if(p_voxelType == VoxelType.VOXEL_8)
			{
				val = p_cValue / 255;
			}
			else
			{
				val = p_uValue / ushort.MaxValue;
			}
			return val;
		}
		
		public int getHounsfieldValue()
		{
			return Convert.ToInt32(getVal() * 4096) - 1024;
		}
		
		public byte toGreyscaleValue()
		{
			if(p_voxelType == VoxelType.VOXEL_8)
				return p_cValue;
			return Convert.ToByte(getVal() * 255); 
		}
	}
}
