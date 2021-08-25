using Spacemaker.JavascriptWrappers;
using System;
using System.Collections.Generic;

namespace Spacemaker.Data
{
    public class WorkSurfaceState
    {
        public List<Solution> Solutions { get; set; }
        public Solution SelectedSolution => Solutions.Count > SelectedIndex && SelectedIndex >= 0? Solutions[SelectedIndex] : null;
        public int SelectedIndex { get; set; } = -1;
        public List<JSPolygon> SelectedPolygons => SelectedSolution != null? SelectedSolution.SelectedPolygons : new List<JSPolygon>();

        public delegate void PreSelectedSolutionChangedDelegate();

        public PreSelectedSolutionChangedDelegate PreSolutionStateChangedDelegate;


        public delegate void PostSelectedSolutionChangedDelegate();

        public PostSelectedSolutionChangedDelegate PostSolutionStateChangedDelegate;


        public delegate void SelectedPolygonsChangedDelegate();

        public SelectedPolygonsChangedDelegate PolygonsStateChangedDelegate;
    }
}
