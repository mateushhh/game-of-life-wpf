using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    public class SimulationParameters
    {
        public HashSet<int> R = new HashSet<int> {1, 2, 3, 4 }; // ile sąsiadujących roślin żeby nowa roślina
        public HashSet<int> W = new HashSet<int> {1, 2, 3, 4, 5, 6 }; // ile sąsiadujących roślin żeby roślina przeżyła
        public HashSet<int> B = new HashSet<int> {2, 3 }; // ile sąsiadujących zwierząt żeby nowe zwierze
        public HashSet<int> P = new HashSet<int> {0, 1 }; // ile sąsiadujących roślin żeby nowe zwierze
        public HashSet<int> Z = new HashSet<int> {0, 1, 2, 3 }; // ile sąsiadujących zwierząt żeby zwierze przeżyło

        public int M = 0; // Dystans ruchu zwierzęcia
        public int L = 0; // Maksymalny wiek zwierzęcia
    }
}
