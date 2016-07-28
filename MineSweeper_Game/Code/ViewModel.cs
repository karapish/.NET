using Minesweeper.Model;
using System;

namespace Minesweeper.UI
{
    /// <summary>
    /// Values that represent cell view states.
    /// </summary>
    public enum CellViewState
    {
        Default,
        Empty,
        TaggedAsBomb,
        TaggedAsInquestion,
        Exploded,
        Count_1,
        Count_2,
        Count_3,
        Count_4,
        Count_5,
        Count_6,
        Count_7,
        Count_8 // Surrounded by bombs
    }

    /// <summary>
    /// Values that represent cell actions.
    /// </summary>
    public enum CellAction
    {
        ClickOn,
        Tag
    }

    /// <summary>
    /// A view model for interacting with the model.
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Underlying data about mines (model)
        /// </summary>
        private MineField model;

        /// <summary>
        /// Cells accessors
        /// </summary>
        private CellViewState[,] cells; 
        public CellViewState[,] Cells
        {
            get
            {
                return this.cells;
            }
        }


        /// <summary>
        /// Number of cells per width (height)
        /// </summary>
        private readonly ushort size;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        ///
        /// <param name="size"> Number of cells per width (height) </param>
        /// 
        public ViewModel(ushort size = 10)
        {
            this.size = size;
            this.New();
        }

        /// <summary>
        /// Resets the current level
        /// </summary>
        public void Reset()
        {
            this.cells = new CellViewState[this.size, this.size];
        }

        /// <summary>
        /// Creates a new level
        /// </summary>
        public void New()
        {
            this.model = new MineField(this.size);
            this.Reset();
        }

        /// <summary>
        /// Executes an action in the current cell
        /// </summary>
        /// <param name="what"> Action to perform on cell </param>
        /// <param name="row">        The row. </param>
        /// <param name="column">     The column. </param>
        /// <returns> State of a cell where action was performed on </returns>
        public CellViewState Do(CellAction what, ushort row, ushort column)
        {
            if (row > this.size || column > this.size)
                throw new ArgumentException("row or column parameter is out of bound");

            if(what == CellAction.ClickOn)
            {
                // Check if user failed and clicked on a bomb cell
                if (this.model[row, column])
                {
                    this.cells[row, column] = CellViewState.Exploded;
                    return CellViewState.Exploded;
                }
                else
                {
                    // If user is lucky to select a cell without any bombs in neighbor cells auto-open similar cells nearby
                    this.Autodiscover(row, column);
                }
            }
            else if(what == CellAction.Tag)
            {
                // We can only tag hidden cells, tagged as bomb or tagged as inquestion
                if (this.cells[row, column] != CellViewState.Empty)
                {
                    if (this.cells[row, column] == CellViewState.TaggedAsBomb)
                        this.cells[row, column] = CellViewState.TaggedAsInquestion;
                    else if (this.cells[row, column] == CellViewState.TaggedAsInquestion)
                        this.cells[row, column] = CellViewState.Default;
                    else if (this.cells[row, column] == CellViewState.Default)
                        this.cells[row, column] = CellViewState.TaggedAsBomb;

                    return this.cells[row, column];
                }
            }

            return this.cells[row, column];
        }

        /// <summary>
        /// Check if the level is completed by tagged bombs
        /// </summary>
        ///
        /// <returns>
        /// true if completed, false if not.
        /// </returns>
        public bool IsCompleted()
        {
            for (ushort i = 0; i < this.size; ++i)
                for (ushort j = 0; j < this.size; ++j)
                {
                    if(this.model[i, j] && 
                       this.cells[i,j] != CellViewState.TaggedAsBomb)
                    {
                        return false;
                    }
                }

            return true;
        }        

        /// <summary>
        /// Auto discovery for trivial neighbor cells 
        /// </summary>
        ///
        /// <param name="row">    The row. </param>
        /// <param name="column"> The column. </param>
        public void Autodiscover(int row, int column)
        {                                                                                                       
            int numberOfAdjacentBombs = 0;

            if (row < 0 || row >= size || column < 0 || column >= size ||
                this.cells[(ushort)row, (ushort)column] == CellViewState.Empty)
                return;

            // Check adjacent cells
            //    1   2   3
            //    4   X   5
            //    6   7   8
           
            Func<int, int, sbyte> containsBomb = delegate (int r, int c)
            {
                bool hasBomb = false;
                ushort u_r = (ushort)r, u_c = (ushort)c;

                try
                {
                    if (this.cells[u_r, u_c] != CellViewState.Empty)
                    {
                        // Check if we have already visited this cell
                        hasBomb = this.model[u_r, u_c];
                    }
                }
                catch { };

                return Convert.ToSByte(hasBomb);
            };


            // 1
            numberOfAdjacentBombs += containsBomb(row - 1, column - 1);

            // 2
            numberOfAdjacentBombs += containsBomb(row - 1, column);

            // 3
            numberOfAdjacentBombs += containsBomb(row - 1, column + 1);

            // 4
            numberOfAdjacentBombs += containsBomb(row, column - 1);

            // 5
            numberOfAdjacentBombs += containsBomb(row, column + 1);

            // 6
            numberOfAdjacentBombs += containsBomb(row + 1, column - 1);

            // 7
            numberOfAdjacentBombs += containsBomb(row + 1, column);

            // 8
            numberOfAdjacentBombs += containsBomb(row + 1, column + 1);


            switch (numberOfAdjacentBombs)
            {
                case 0:
                {
                    this.cells[row, column] = CellViewState.Empty;
                    
                    // Recursively call top, bottom, left and right cells    
                    // 2
                    Autodiscover(row - 1, column);

                    // 4
                    Autodiscover(row, column - 1);

                    // 5
                    Autodiscover(row, column + 1);

                    // 7
                    Autodiscover(row + 1, column);

                    break;
                }
                    
                case 1: this.cells[row, column] = CellViewState.Count_1; break;
                case 2: this.cells[row, column] = CellViewState.Count_2; break;
                case 3: this.cells[row, column] = CellViewState.Count_3; break;
                case 4: this.cells[row, column] = CellViewState.Count_4; break;
                case 5: this.cells[row, column] = CellViewState.Count_5; break;
                case 6: this.cells[row, column] = CellViewState.Count_6; break;
                case 7: this.cells[row, column] = CellViewState.Count_7; break;
                case 8: this.cells[row, column] = CellViewState.Count_8; break;
            }
        }
    }
}
