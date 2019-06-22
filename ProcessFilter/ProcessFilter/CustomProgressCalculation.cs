using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeleSharp.TL;
using Utils.WinForm.CustomProgressBar;

namespace SPAFilter
{
    public class CustomProgressCalculation : IDisposable
    {
        Action _offlineCalcWhenStartOpen = null;
        Action _offlineCalcWhenStopOpen = null;
        Action _offlineCalcWhenStartRead = null;
        Action _offlineCalcWhenStopRead = null;
        Action _calcReadLine = null;

        private ProgressCalculationAsync _progressCalc;
        
        private int _openFileIterator = 0;
        private int _loadFileIterator = 0;
        private int _readLinesIterator = 0;

        public int SCIteratorsIntended { get; } = 0;
        public int SCPercentIntended { get; } = 0;

        public int XslxServicesIteratorsIntended { get; } = 0;
        public int XslxServicesPercentIntended { get; } = 0;
        

        public int CurrentProgressIterator => _progressCalc?.CurrentProgressIterator ?? 0;
        public int TotalProgressIterator => _progressCalc?.TotalProgressIterator ?? 0;

        public CustomProgressCalculation(IProgressBar progressBar, int generateSCIterators)
        {
            SCIteratorsIntended = generateSCIterators;

            _progressCalc = new ProgressCalculationAsync(progressBar, SCIteratorsIntended);
        }

        public CustomProgressCalculation(IProgressBar progressBar, int generateSCIterators, FileInfo xslxRdService)
        {
            SCIteratorsIntended = generateSCIterators;

            int fileKb = (int)(xslxRdService.Length / 1024);
            if (fileKb > 200)
            {            
                // ------------------------------------------
                // По примерным подсчетам считывание 4Kb файла xslx равен обработки одной операции для SC.
                // Например:
                // 1280 файлов операций = 40% всей работы; 3200 файлов = 100%
                // 7800 Kb файла xslx   = 60% всей работы; 13000 Kb    = 100%
                // Отношение файла к операциям ~ 4  (точно = 4.06)
                // ------------------------------------------
                // 1280 + (7800 / 4) = 3230 (100%)
                // 1280 * 100 / 3230 = 39.6%
                // (7800 / 4) * 100 / 3230 = 60.3%
                // ------------------------------------------

                int xslxRdServiceIterators = fileKb / 4;
                int hundredPercentIterators = SCIteratorsIntended + xslxRdServiceIterators;

                SCPercentIntended = (SCIteratorsIntended * 100) / hundredPercentIterators;
                XslxServicesPercentIntended = (xslxRdServiceIterators * 100) / hundredPercentIterators;

                if (SCPercentIntended > 0)
                    XslxServicesIteratorsIntended = (XslxServicesPercentIntended * SCIteratorsIntended) / SCPercentIntended;
                else
                    XslxServicesIteratorsIntended = (85 * SCIteratorsIntended) / 15;

                GetXslxPartsIterators();

                InitXslxRdServiceProgressCalculation(xslxRdService);
            }
            else
            {
                XslxServicesIteratorsIntended = (15 * SCIteratorsIntended) / 85;

                GetXslxPartsIterators();

                _offlineCalcWhenStopOpen = () => { _progressCalc.Append(_openFileIterator); };
                _offlineCalcWhenStopRead = () => { _progressCalc.Append(_loadFileIterator); };
            }

            int totalProgressIterator = SCIteratorsIntended + XslxServicesIteratorsIntended;

            _progressCalc = new ProgressCalculationAsync(progressBar, totalProgressIterator);
        }

        void GetXslxPartsIterators()
        {
            _openFileIterator = XslxServicesIteratorsIntended / 7;                                    // обработку файлов делим на 7 частей. 1/7 часть занимает открытие файла
            _loadFileIterator = XslxServicesIteratorsIntended - (_openFileIterator * 2);               // 5/7 занимает считывание данных
            _readLinesIterator = XslxServicesIteratorsIntended - _loadFileIterator - _openFileIterator; // 1/7 часть считывание всех строк
        }

        void InitXslxRdServiceProgressCalculation(FileInfo xslxRdService)
        {
            if (XslxServicesIteratorsIntended < 7)
                return;

            bool cancel = false;
            int mSecEachPart = ((int) (xslxRdService.Length / 1900)) / 7; // примерное количество миллисекунд на каждый часть обработки

            var processChecking = new Func<int, int, int>((int iterations, int sleepMSec) =>
            {
                if (sleepMSec == 0)
                    return 0;

                cancel = false;
                int i = 0;
                while (i <= iterations)
                {
                    _progressCalc++;
                    i++;
                    if (cancel)
                        return i;
                    Thread.Sleep(sleepMSec);
                }

                return i;
            });
            IAsyncResult result = null;

            _offlineCalcWhenStartOpen = () => { result = processChecking.BeginInvoke(_openFileIterator, mSecEachPart / _openFileIterator, null, null); };

            _offlineCalcWhenStopOpen = () =>
            {
                cancel = true;
                int openRes = processChecking.EndInvoke(result);
                if (openRes < _openFileIterator)
                    _progressCalc.Append(_openFileIterator - openRes);
            };

            _offlineCalcWhenStartRead = () => { result = processChecking.BeginInvoke(_loadFileIterator, (mSecEachPart * 5) / _loadFileIterator, null, null); };

            _offlineCalcWhenStopRead = () =>
            {
                cancel = true;
                int loadRes = processChecking.EndInvoke(result);
                if (loadRes < _loadFileIterator)
                    _progressCalc.Append(_loadFileIterator - loadRes);
            };

        }

        public void BeginOpenXslxFile()
        {
            _offlineCalcWhenStartOpen?.Invoke();
        }

        public void BeginReadXslxFile()
        {
            _offlineCalcWhenStopOpen?.Invoke();
            _offlineCalcWhenStartRead?.Invoke();
        }

        public void EndReadXslxFile(int totalRows)
        {
            _offlineCalcWhenStopRead?.Invoke();

            if (_readLinesIterator <= 0)
                return;

            int lineIterNumber = 0;
            int lineIterator = totalRows / _readLinesIterator;
            if (lineIterator > 1)
            {
                _calcReadLine = () =>
                {
                    lineIterNumber++;
                    if (lineIterNumber < lineIterator)
                        return;

                    _progressCalc++;
                    lineIterNumber = 0;
                };
            }
        }

        public void ReadXslxFileLine()
        {
            _calcReadLine?.Invoke();
        }

        public void EndOpenXslxFile()
        {
            if (_progressCalc.CurrentProgressIterator < XslxServicesIteratorsIntended)
            {
                _progressCalc.Append(XslxServicesIteratorsIntended - _progressCalc.CurrentProgressIterator);
            }
        }

        public void Append()
        {
            _progressCalc++;
        }

        public void Append(int value)
        {
            _progressCalc.Append(value);
        }

        public override string ToString() => _progressCalc?.ToString() ?? "";

        public void Dispose()
        {
            _progressCalc?.Stop();
            _progressCalc?.Dispose();
        }
    }
}
