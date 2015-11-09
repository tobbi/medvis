/*
 * Created by SharpDevelop.
 * User: tobbi
 * Date: 28.10.2015
 * Time: 19:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.Windows.Forms;

namespace MedVis_Projekt
{
	/// <summary>
	/// Description of ByteStreamParser.
	/// </summary>
	public static class ByteStreamParser
	{
				
		public static byte[] getSubArray(byte[] array, int startIndex, int endIndex)
		{
			if(startIndex > endIndex)
				throw new InvalidOperationException();
			
			Byte[] _array = new byte[endIndex + 1 - startIndex];
			for(int i = startIndex; i <= endIndex; i++)
				_array[i % _array.Length] = array[i];
			
			return _array;
		}
		
		public static ulong getLongValue(Byte[] array, int startIndex, int endIndex)
		{
			return BitConverter.ToUInt64(getSubArray(array, startIndex, endIndex), 0);
		}
		
		public static double getDoubleValue(Byte[] array, int startIndex, int endIndex)
		{
			return BitConverter.ToDouble(getSubArray(array, startIndex, endIndex), 0);
		}
		
		public static ushort getUShortValue(Byte[] array, int startIndex, int endIndex)
		{
			return BitConverter.ToUInt16(getSubArray(array, startIndex, endIndex), 0);
		}
		
		public static string getStringValue(Byte[] array, int startIndex, int endIndex)
		{	
			return Encoding.ASCII.GetString(getSubArray(array, startIndex, endIndex)).Trim();
		}
		
		public static UInt32 getUint32Value(Byte[] array, int startIndex, int endIndex)
		{
			return BitConverter.ToUInt32(getSubArray(array, startIndex, endIndex), 0);
		}
		
		public static Char getCharValue(Byte[] array, int startIndex, int endIndex)
		{
			return BitConverter.ToChar(getSubArray(array, startIndex, endIndex), 0);
		}
	}
}
