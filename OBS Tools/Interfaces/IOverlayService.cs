using System;
using System.Collections.Generic;
using System.Text;

namespace OBS_Tools.Interfaces
{
    public interface IOverlayService
    {
        void EditFiles(string file, string content);
        void SetImage();
        List<Team> GenerateTeams();
    }
}
