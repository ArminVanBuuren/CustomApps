using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Utils.ConditionEx.Utils
{
	public enum OSNames
	{
		Win95 = 0,
		Win98 = 1,
		Win98SecondEdition = 2,
		WinNt351 = 3,
		WinNt40 = 4,
		Win2000 = 5,
		WinXp = 6,
		Win7 = 7,
		Win8 = 8,
		WinMe = 9,
		None = 10
	}

	internal class XmlFunc : IDisposable
	{
		int _direction = 0;
		static List<Simbol> _xmlSimbl = new List<Simbol>
			{
				new Simbol {Param = "amp", Value = "&"},
				new Simbol {Param = "quot", Value = "\""},
				new Simbol {Param = "lt", Value = "<"},
				new Simbol {Param = "gt", Value = ">"},
				new Simbol {Param = "apos", Value = "'"},
				new Simbol {Param = "circ", Value = "ˆ"},
				new Simbol {Param = "tilde", Value = "˜"},
				new Simbol {Param = "ndash", Value = "–"},
				new Simbol {Param = "mdash", Value = "—"},
				new Simbol {Param = "lsquo", Value = "‘"},
				new Simbol {Param = "rsquo", Value = "’"},
				new Simbol {Param = "lsaquo", Value = "‹"},
				new Simbol {Param = "rsaquo", Value = "‹"}
			};

		public XmlFunc(int direction)
		{
			_direction = direction;
		}

		public void Dispose()
		{

		}

		public string Replace(Match m)
		{
			string find = m.Groups[1].ToString();
			if (_direction == 0)
			{
				foreach (Simbol sm in _xmlSimbl)
				{
					if (sm.Param == find)
						return sm.Value;
				}
			}
			else if (_direction == 2)
			{
				foreach (Simbol sm in _xmlSimbl)
				{
					if (sm.Value == find && sm.Value != "\'")
						return "&" + sm.Param + ";";
				}
			}
			else
			{
				foreach (Simbol sm in _xmlSimbl)
				{
					if (sm.Value == find)
						return "&" + sm.Param + ";";
				}
			}
			return find;
		}
		class Simbol
		{
			public string Param { get; set; }
			public string Value { get; set; }
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	internal class MEMORYSTATUSEX
	{
		public uint dwLength;
		public uint dwMemoryLoad;
		public ulong ullTotalPhys;
		public ulong ullAvailPhys;
		public ulong ullTotalPageFile;
		public ulong ullAvailPageFile;
		public ulong ullTotalVirtual;
		public ulong ullAvailVirtual;
		public ulong ullAvailExtendedVirtual;
		public MEMORYSTATUSEX()
		{
			dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
		}
	}

	public interface ISizes<out T>
	{
		T Avail { get; }
		T Used { get; }
		T Total { get; }
	}
	public class CollectionSizes : ISizes<double>
	{
		public CollectionSizes()
		{
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="isAvail">если true то value является свободной памятью, если false то является используемой</param>
		/// <param name="total">полный размер памяти</param>
		public CollectionSizes(ulong value, bool isAvail, ulong total)
		{
			Total = total;
			if (isAvail)
			{
				Avail = value;
				Used = GetDistBytes(Avail, Total);
			}
			else
			{
				Used = value;
				Avail = GetDistBytes(Used, Total);
			}
		}

		public double Avail { get; } = 0;

		public double Used { get; } = 0;

		public double Total { get; } = 0;

		internal double GetDistBytes(double value, double total)
		{
			if (value < total)
				return total - value;
			return value;
		}
	}

	public class MemorySizes : ISizes<PhysicalSizes>
	{
		bool _isAvail;
		/// <summary>
		/// 
		/// </summary>
		/// <param name="valueInBytes">значение должно быть в байтах</param>
		/// <param name="isAvail">если true то value является свободной памятью, если false то является используемой</param>
		/// <param name="total"></param>
		public MemorySizes(ulong valueInBytes, bool isAvail, ulong total)
		{
			_isAvail = isAvail;
			Total = new PhysicalSizes(total);
			if (isAvail)
			{
				Avail = new PhysicalSizes(valueInBytes);
				Used = new PhysicalSizes(GetDistBytes(Avail, Total));
			}
			else
			{
				Used = new PhysicalSizes(valueInBytes);
				Avail = new PhysicalSizes(GetDistBytes(Used, Total));
			}
		}

		public PhysicalSizes Avail { get; }

		public PhysicalSizes Used { get; }

		public PhysicalSizes Total { get; }

		internal ulong GetDistBytes(PhysicalSizes value, PhysicalSizes total)
		{
			if (value.b < total.b)
				return total.b - value.b;
			return value.b;
		}
		public override string ToString()
		{
			if (_isAvail)
				return Used.ToString();
			return Avail.ToString();
		}
	}
	public class PhysicalSizes
	{
		public PhysicalSizes(ulong bytes)
		{
			b = bytes;
			Kb = Math.Round(Determine(1024));
			Mb = Math.Round(Determine(1024 * 1024));
			Gb = Math.Round(Determine(1024 * 1024 * 1024));
		}
		public ulong b { get; } = 0;
		public double Kb { get; } = 0;
		public double Mb { get; } = 0;
		public double Gb { get; } = 0;
		double Determine(double num)
		{
			return (b > 0) ? double.Parse(b.ToString()) / num : 0;
		}
		public override string ToString()
		{
			return Mb.ToString();
		}
	}
}