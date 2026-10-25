
using System;

namespace GameOfLife
{
    public class SimulationEngine
    {
        private readonly Random random = new Random();
        public SimulationStatistics Statistics { get; } = new SimulationStatistics();

        public Cell[,] Step(Cell[,] current, SimulationParameters parameters)
        {
            int height = current.GetLength(0);
            int width = current.GetLength(1);

            Cell[,] next = CopyBoard(current);

            bool[,] occupied = new bool[height, width];

            // removing animals from next[,] and adding them to occupied[,]
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    occupied[y, x] = current[y, x].Animal != null;
                    next[y, x].Animal = null;
                }
            }

            // animals behaviour
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Animal animal = current[y, x].Animal;

                    if (animal == null)
                        continue;

                    int neighbors = CountAnimals(current, x, y);

                    if (parameters.L <= 0 || animal.Age >= parameters.L || !parameters.Z.Contains(neighbors))
                    {
                        occupied[y, x] = false;
                        Statistics.DeadAnimals++;
                        continue;
                    }

                    int newX = x;
                    int newY = y;

                    if (current[y, x].Plant == null)
                    {
                        MoveAnimal(current, occupied, x, y, parameters.M, out newX, out newY);
                    }

                    if (newX != x || newY != y)
                    {
                        occupied[y, x] = false;
                        occupied[newY, newX] = true;
                    }

                    next[newY, newX].Animal = new Animal
                    {
                        Age = animal.Age + 1
                    };

                    next[newY, newX].Plant = null;
                }
            }

            // animals breeding
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (current[y, x].Animal != null ||
                        next[y, x].Animal != null)
                    {
                        continue;
                    }

                    int animals = CountAnimals(current, x, y);
                    int plants = CountPlants(current, x, y);

                    if (parameters.B.Contains(animals) &&
                        parameters.P.Contains(plants))
                    {
                        next[y, x].Animal = new Animal();
                        next[y, x].Plant = null;

                        Statistics.BornAnimals++;
                    }
                }
            }

            // plants
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (next[y, x].Animal != null)
                    {
                        next[y, x].Plant = null;
                        continue;
                    }

                    if (current[y, x].Animal != null)
                    {
                        next[y, x].Plant = null;
                        continue;
                    }

                    int plants = CountPlants(current, x, y);

                    if (current[y, x].Plant != null)
                    {
                        if (parameters.W.Contains(plants))
                        {
                            next[y, x].Plant = new Plant();
                        }
                        else
                        {
                            next[y, x].Plant = null;
                        }
                    }
                    else if (parameters.R.Contains(plants))
                    {
                        next[y, x].Plant = new Plant();
                    }
                    else
                    {
                        next[y, x].Plant = null;
                    }
                }
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool hadPlant = current[y, x].Plant != null;
                    bool hasPlant = next[y, x].Plant != null;

                    if (!hadPlant && hasPlant)
                        Statistics.BornPlants++;

                    if (hadPlant && !hasPlant)
                        Statistics.DeadPlants++;
                }
            }

            Statistics.AlivePlants = CountPopulation(next, cell => cell.Plant != null);

            Statistics.AliveAnimals = CountPopulation(next, cell => cell.Animal != null);

            Statistics.Ticks++;

            return next;
        }

        private Cell[,] CopyBoard(Cell[,] board)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            Cell[,] copy = new Cell[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    copy[y, x] = new Cell();

                    if (board[y, x].Plant != null)
                        copy[y, x].Plant = new Plant();

                    if (board[y, x].Animal != null)
                    {
                        copy[y, x].Animal = new Animal
                        {
                            Age = board[y, x].Animal.Age
                        };
                    }
                }
            }

            return copy;
        }

        private int CountPlants(Cell[,] board, int x, int y)
        {
            return CountNeighbors( board, x, y, cell => cell.Plant != null);
        }

        private int CountAnimals(Cell[,] board, int x, int y)
        {
            return CountNeighbors(board, x, y, cell => cell.Animal != null);
        }

        private int CountNeighbors(Cell[,] board, int x, int y, Func<Cell, bool> predicate)
        {
            int count = 0;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (!IsInside(board, nx, ny))
                        continue;

                    if (predicate(board[ny, nx]))
                        count++;
                }
            }

            return count;
        }

        private void MoveAnimal(Cell[,] board, bool[,] occupied, int x, int y, int maxSteps, out int newX, out int newY)
        {
            newX = x;
            newY = y;

            if (maxSteps <= 0)
                return;

            var target = FindNearestPlant(board, x, y);

            if (target.HasValue)
            {
                int targetX = target.Value.X;
                int targetY = target.Value.Y;

                for (int i = 0; i < maxSteps; i++)
                {
                    if (newX == targetX && newY == targetY)
                        break;

                    int stepX = Math.Sign(targetX - newX);
                    int stepY = Math.Sign(targetY - newY);

                    int candidateX = newX + stepX;
                    int candidateY = newY + stepY;

                    if (!IsInside(board, candidateX, candidateY) ||
                        occupied[candidateY, candidateX])
                    {
                        break;
                    }

                    newX = candidateX;
                    newY = candidateY;
                }
            }
            else
            {
                int[,] directions =
                {
                    { -1, -1 }, { 0, -1 }, { 1, -1 },
                    { -1,  0 },             { 1,  0 },
                    { -1,  1 }, { 0,  1 }, { 1,  1 }
                };

                int start = random.Next(8);

                for (int i = 0; i < 8; i++)
                {
                    int direction = (start + i) % 8;

                    int candidateX = x + directions[direction, 0];
                    int candidateY = y + directions[direction, 1];

                    if (IsInside(board, candidateX, candidateY) &&
                        !occupied[candidateY, candidateX])
                    {
                        newX = candidateX;
                        newY = candidateY;
                        break;
                    }
                }
            }
        }



        private (int X, int Y)? FindNearestPlant(
            Cell[,] board,
            int x,
            int y)
        {
            int height = board.GetLength(0);
            int width = board.GetLength(1);

            int bestDistance = int.MaxValue;

            var nearestPlants = new System.Collections.Generic.List<(int X, int Y)>();

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    if (board[py, px].Plant == null)
                        continue;

                    int distance = Math.Max(
                        Math.Abs(px - x),
                        Math.Abs(py - y));

                    if (distance == 0)
                        continue;

                    if (distance < bestDistance)
                    {
                        bestDistance = distance;

                        nearestPlants.Clear();

                        nearestPlants.Add((px, py));
                    }
                    else if (distance == bestDistance)
                    {
                        nearestPlants.Add((px, py));
                    }
                }
            }

            if (nearestPlants.Count == 0)
                return null;

            int index = random.Next(nearestPlants.Count);

            return nearestPlants[index];
        }


        private bool IsInside(Cell[,] board, int x, int y)
        {
            return x >= 0 &&
                   y >= 0 &&
                   x < board.GetLength(1) &&
                   y < board.GetLength(0);
        }

        private int CountPopulation(Cell[,] board, Func<Cell, bool> predicate)
        {
            int count = 0;

            for (int y = 0; y < board.GetLength(0); y++)
            {
                for (int x = 0; x < board.GetLength(1); x++)
                {
                    if (predicate(board[y, x]))
                        count++;
                }
            }

            return count;
        }

        public void ResetStatistics()
        {
            Statistics.ResetStatistics();
        }
    }
}
