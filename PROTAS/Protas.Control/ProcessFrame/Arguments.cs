using System;

namespace Protas.Control.ProcessFrame
{
    internal interface IProcessor : IDisposable
    {
        bool IsCorrect { get; }
        bool Start();
        bool Stop();
    }


}