using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class Cell
    {
        public Animal? Animal { get; set; } = null;
        public Plant? Plant { get; set; } = null;
    }
}
