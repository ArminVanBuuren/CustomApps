using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Utils.Handles
{
    /// <inheritdoc />
    /// <summary>
    /// Summary description for MathProgression.
    /// </summary>
    [Serializable]
    public class MathProgression : ICloneable
    {
        private enum MathType
        {
            Algebraic,
            Geometric
        }

        private int _maxCounter = 0;
        private double _timeoutValue = 0;
        private double _progressionCoef = 0;
        private MathType _progressionType;

        #region Constructors

        private MathProgression()
        {
        }

        public MathProgression(XmlNode config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var doc = ReadConfigurationDocument(config.OuterXml);
            var str = doc.DocumentElement?.Attributes["MaxAttemptCount"].Value;
            this._maxCounter = Convert.ToInt32(str);
            str = doc.DocumentElement?.Attributes["Timeout"].Value;
            this._timeoutValue = Convert.ToDouble(str);
            str = doc.DocumentElement?.Attributes["ProgressionCoefficient"].Value;
            this._progressionCoef = Convert.ToDouble(str);
            str = doc.DocumentElement?.Attributes["ProgressionType"].Value;
            this._progressionType = (MathType)Enum.Parse(typeof(MathType), str);
        }

        /// <summary>
        /// Create algebraic progression handler
        /// </summary>
        /// <param name="elementsCount">Number of elements to be calculated</param>
        /// <param name="initialValue">Initial value of progression (e.g. timeout in milliseconds)</param>
        /// <param name="commonDifference">Diffence between two values</param>
        /// <returns></returns>
        public static MathProgression CreateAlgebraic(int elementsCount, double initialValue, double commonDifference)
        {
            if (elementsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementsCount), elementsCount, "Number of elements must not be negative or zero");

            var m = new MathProgression
            {
                _maxCounter = elementsCount,
                _timeoutValue = initialValue,
                _progressionCoef = commonDifference,
                _progressionType = MathType.Algebraic
            };

            return m;
        }

        /// <summary>
        /// Create geometric progression handler
        /// </summary>
        /// <param name="elementsCount">Number of elements to be calculated</param>
        /// <param name="scaleFactor">Scale factor is an initial value of progression (e.g. timeout in milliseconds)</param>
        /// <param name="commonRatio">Common ratio is a multiplier for every next item</param>
        /// <returns>New instance of MathProgression object</returns>
        public static MathProgression CreateGeometric(int elementsCount, double scaleFactor, double commonRatio)
        {
            if (elementsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementsCount), elementsCount, "Number of elements must not be negative or zero");
            if (commonRatio == 0.0 || commonRatio == 1.0)
                throw new ArgumentOutOfRangeException(nameof(commonRatio), commonRatio, "Common ration must not be 0 or 1");

            MathProgression m = new MathProgression();
            m._maxCounter = elementsCount;
            m._timeoutValue = scaleFactor;
            m._progressionCoef = commonRatio;
            m._progressionType = MathType.Geometric;

            return m;
        }

        #endregion

        #region Utilities

        private static XmlDocument ReadConfigurationDocument(string xmlConfig)
        {
            var thisAsm = Assembly.GetExecutingAssembly();
            var schemaStream = thisAsm.GetManifestResourceStream("Utils.Handles.MathProgression.xsd");
            var cfgReader = new XmlTextReader(new StringReader(xmlConfig));

            var doc = XML.ReadAndValidateXML(schemaStream, cfgReader);
            cfgReader.Close();
            schemaStream?.Close();

            return doc;
        }

        private static double NextGeometricValue(int i, double scaleFactor, double commonRatio)
        {
            if (i == 0)
                return scaleFactor;

            return scaleFactor * Math.Pow(commonRatio, i - 1);
        }

        private static double NextAlgebraicValue(int i, double initialValue, double commonDifference)
        {
            if (i == 0)
                return initialValue;

            i = i + 1; // accordingly to formula the first index is 1 but not 0
            return initialValue + commonDifference * (i - 1);
        }

        #endregion // Utilities

        #region Public methods

        public bool IsCounterExceeded(Int32 iteration)
        {
            CurrentCounter = iteration;
            return IsCounterExceeded();
        }

        public bool IsCounterExceeded()
        {
            return this.CurrentCounter >= this._maxCounter;
        }

        public int CalculateTimeout()
        {
            return Convert.ToInt32(this.NextValue());
        }

        public int CalculateTimeout(int iteration)
        {
            CurrentCounter = iteration;
            return CalculateTimeout();
        }

        public double NextValue()
        {
            double timeout = 0;

            if (this._timeoutValue > 0 && this.CurrentCounter < this._maxCounter)
            {
                if (this.CurrentCounter == 0)
                {
                    timeout = this._timeoutValue;
                }
                else if (this._progressionType == MathType.Algebraic)
                {
                    timeout = this._timeoutValue + this._progressionCoef * (this.CurrentCounter - 1);
                }
                else if (this._progressionType == MathType.Geometric)
                {
                    timeout = this._timeoutValue * Math.Pow(this._progressionCoef, this.CurrentCounter - 1);
                }
            }

            return timeout;
        }

        public void Sleep(int timeout)
        {
            if (timeout > 0)
                Thread.Sleep(timeout);

            this.CurrentCounter++;
        }

        public void Sleep()
        {
            var timeout = this.CalculateTimeout();

            if (timeout > 0)
                Thread.Sleep(timeout);

            this.CurrentCounter++;
        }

        public void Reset()
        {
            this.CurrentCounter = 0;
        }

        public int CurrentCounter { get; private set; } = 0;

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("MaxAttemptCount = {0}\r\n", this._maxCounter);
            sb.AppendFormat("Timeout = {0}\r\n", this._timeoutValue);
            sb.AppendFormat("ProgressionType = {0}\r\n", this._progressionType.ToString());
            sb.AppendFormat("ProgressionCoefficient = {0}\r\n", this._progressionCoef);
            sb.AppendFormat("{0}\r\n", new string('*', 40));

            if (this._timeoutValue > 0)
            {
                var sum = this.CalculateSum();
                sb.AppendFormat("Sum of progression = {0} ms\r\n", sum);
            }
            sb.AppendFormat("Current index = {0}", this.CurrentCounter);

            return sb.ToString();
        }

        #endregion

        #region Sum calculation methods

        public double CalculateSum()
        {
            return this.CalculateSum(this._maxCounter);
        }

        public double CalculateSum(int elementsCount)
        {
            if (elementsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementsCount), elementsCount, "Number of items must not be negative or zero");

            switch (this._progressionType)
            {
                case MathType.Algebraic:
                    return CalculateAlgebraicSum(elementsCount, this._timeoutValue, this._progressionCoef);
                case MathType.Geometric:
                    return CalculateGeometricSum(elementsCount, this._timeoutValue, this._progressionCoef);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("Progression type \"{0}\" ", this._progressionType.ToString());
            sb.AppendFormat("is invalid");
            throw new Exception(sb.ToString());
        }

        private static double CalculateGeometricSum(int i, double scaleFactor, double commonRatio)
        {
            var sum = scaleFactor;

            if (i > 0)
            {
                sum *= 1 - Math.Pow(commonRatio, i);
                sum /= 1 - commonRatio;
            }

            return sum;
        }

        private static double CalculateAlgebraicSum(int i, double initialValue, double commonDifference)
        {
            var sum = initialValue;

            if (i > 0)
            {
                i += 1; // accordingly to formula the first index is 1 but not 0
                //sum = commonDifference * (i - 1);
                //sum += 2 * initialValue;
                //sum *= i / 2;

                var lastSum = 0.0;
                for (int j = 0; j < i - 1; ++j)
                    lastSum += initialValue + commonDifference * (j);

                sum += lastSum + commonDifference * (i - 1);
            }

            return sum;
        }

        #endregion

        #region Series calculation methods

        public DataTable CalculateGeometricSeries(Int32 elementsCount, Double scaleFactor, Double commonRatio)
        {
            if (elementsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementsCount), elementsCount, "Number of elements must not be negative or zero");
            if (commonRatio == 0.0 || commonRatio == 1.0)
                throw new ArgumentOutOfRangeException(nameof(commonRatio), commonRatio, "Common ration must not be 0 or 1");

            var dt = new DataTable();
            dt.Columns.Add("i");
            dt.Columns.Add("a");
            dt.Columns.Add("sum");

            for (int i = 0; i < elementsCount; i++)
            {
                var dr = dt.NewRow();
                dr["i"] = i;
                dr["a"] = NextGeometricValue(i, scaleFactor, commonRatio);
                dr["sum"] = CalculateGeometricSum(i, scaleFactor, commonRatio);
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public DataTable CalculateAlgebraicSeries(Int32 elementsCount, Double initialValue, Double commonDifference)
        {
            if (elementsCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(elementsCount), elementsCount, "Number of elements must not be negative or zero");

            var dt = new DataTable();
            dt.Columns.Add("i");
            dt.Columns.Add("a");
            dt.Columns.Add("sum");

            for (int i = 0; i < elementsCount; i++)
            {
                var dr = dt.NewRow();
                dr["i"] = i;
                dr["a"] = NextAlgebraicValue(i, initialValue, commonDifference);
                dr["sum"] = CalculateAlgebraicSum(i, initialValue, commonDifference); ;
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public DataTable CalculateSeries()
        {
            switch (this._progressionType)
            {
                case MathType.Geometric:
                    return this.CalculateGeometricSeries(this._maxCounter, this._timeoutValue, this._progressionCoef);
                case MathType.Algebraic:
                    return this.CalculateAlgebraicSeries(this._maxCounter, this._timeoutValue, this._progressionCoef);
            }

            var sb = new StringBuilder();
            sb.AppendFormat("Progression type \"{0}\" ", this._progressionType.ToString());
            sb.AppendFormat("is invalid");
            throw new Exception(sb.ToString());
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
