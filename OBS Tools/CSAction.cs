using System;
using System.Collections.Generic;
using System.Text;

namespace OBS_Tools
{
    public class CSAction
    {
        public int actorCellId { get; set; }
        public int championId { get; set; }
        public bool completed { get; set; }
        public int id { get; set; }
        public bool isInProgress { get; set; }
        public int pickTurn { get; set; }
        public string type { get; set; }
    }
}
