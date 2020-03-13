using System;
using System.Collections.Generic;
using System.Linq;
using Game.Entity;
using EditorTools.Roadster;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.SatNav {
    public class SatNav {
        private Roads roads;
        
        public SatNav(Roads roads) {
            this.roads = roads;
        }
    }
}