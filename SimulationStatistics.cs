using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class SimulationStatistics
    {
        public int AlivePlants { get; set; }
        public int BornPlants { get; set; }
        public int DeadPlants { get; set; }

        public int AliveAnimals { get; set; }
        public int BornAnimals { get; set; }
        public int DeadAnimals { get; set; }

        public int Ticks { get; set; }

        public void ResetStatistics()
        {
            AlivePlants = 0;
            BornPlants = 0;
            DeadPlants = 0;

            AliveAnimals = 0;
            BornAnimals = 0;
            DeadAnimals = 0;

            Ticks = 0;
        }
    }
}
