using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Handles
{
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
        readonly bool _isAvail;
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
