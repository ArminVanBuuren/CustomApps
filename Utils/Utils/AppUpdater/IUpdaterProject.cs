using System;

namespace Utils.AppUpdater
{
    public interface IUpdaterProject
    {
        Uri Uri { get; }
        string BuildsInfoName { get; }
        Uri BuildsInfoUri { get; }
    }
}
