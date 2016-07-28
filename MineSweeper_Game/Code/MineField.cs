using System;

namespace Minesweeper.Model
{
    /// <summary>
    /// A map of mines for a current level
    /// </summary>
    public class MineField
    {
        /// <summary> 
        /// Number of cells (in height and width)
        /// </summary>
        private readonly ushort fieldSize;

        /// <summary> 
        /// Indicates if a cell contains a mine or not 
        /// </summary>
        protected bool[,] mines;
        public bool this [ushort row, ushort column] 
        {
            get
            {
                return this.mines[row, column];
            }
        }

        /// <summary> 
        /// Array of random numbers 
        /// </summary>
        private ushort[] rngArray;

        /// <summary> Constructor </summary>
        /// <param name="size"> Number of cells over height and width. </param>
        public MineField(ushort size)
        {
            this.fieldSize = size;
            this.mines = new bool[this.fieldSize, this.fieldSize];
            this.BeforeGenerateMap();

            // Two passes of placing mines
            this.PlaceMines();
            this.PlaceMines();
        }

        private ushort numberMines;
        /// <summary> Returns number of mines places </summary>
        public ushort NumberMines
        {
            get
            {
                return this.numberMines;
            }
        }

        /// <summary>
        /// Initialize an array of N integers with the values [0..N-1] and set a variable, max, to the current max index of the array.
        /// At the beginning, max = N-1.
        /// </summary>
        private void BeforeGenerateMap()
        {
            this.rngArray = new ushort[this.fieldSize];

            // Initialize 
            for (ushort i = 0; i < this.fieldSize; ++i)
                rngArray[i] = i;
        }

        /// <summary> 
        /// Generate a game map 
        /// </summary>
        public void PlaceMines()
        {            
            this.numberMines = 0;

            // Knuth-Fisher-Yates algorithm to generate random numbers in O(1) time.
            // Code and description is taken from http://stackoverflow.com/questions/196017/unique-non-repeating-random-numbers-in-o1.
            //
            // 0. Initialize an array of N integers with the values [0..N-1]. (Done in BeforeGenerateMap method)
            // 1. Set a variable, max, to the current max index of the array. max = N-1. 
            // 2. Get a random number from some RNG (variable r) between 0 and max
            // 3. Swap the number at the position (index in array) r with the number at position max and return the number now at position max.
            // 4. Decrement max by 1 and continue. 
            // 5. When max is 0, go to step 0). Start again WITHOUT the need to reinitialize the array.
            //
            //      +----------------------------+
            //      v                            v
            //+--+--+--+--+--+--+--+--+--+--+--+--+--+--+
            //| 0 | 8 | 2 | 10 | 4 | 5 | 6 | 9 | 1: 7| 3|
            //+--+--+--+--+--+--+--+--+--+--+--+--+--+--+

            var max = this.fieldSize - 1;
            var rng = new Random();

            // "Toss a coin" several times
            for(var i = 0; i < this.fieldSize; ++i)
            {
                ushort r = (ushort)rng.Next(0, max + 1); // max+1 is needed because upper boundary is NOT inclusive

                // Swap values in array[r] and array[max]
                var temp = this.rngArray[r];
                this.rngArray[r] = this.rngArray[max];
                this.rngArray[max] = temp;

                max--;

                if((i+1)%2 == 0)
                {
                    // A pair of random numbers was generated, use it to place a mine
                    this.mines[
                        rngArray[(this.fieldSize-1) - i + 1],
                        rngArray[(this.fieldSize-1) - i]] = true;

                    this.numberMines++;
                }
            }
        }
    }
}
