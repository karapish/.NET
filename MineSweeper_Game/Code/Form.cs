using System;
using System.Windows.Forms;

namespace Minesweeper.UI
{

    public partial class MainForm : Form
    {
        static ushort s_size = 10;
        private ViewModel field = new ViewModel(s_size);

        public MainForm()
        {
            InitializeComponent();
            GenerateDataGrid();

        }

        /// <summary>
        /// Generates a data grid (fieldDimension x fieldDimension)
        /// </summary>
        private void GenerateDataGrid()
        {
            var columns = new DataGridViewColumn[s_size];
            var rows = new DataGridViewRow[s_size];

            for (var i = 0; i < s_size; ++i)
            {
                columns[i] = new DataGridViewTextBoxColumn();
                rows[i] = new DataGridViewRow();
            }

            this.gridField.Columns.AddRange(columns);
            this.gridField.Rows.AddRange(rows);
        }

        /// <summary>
        /// When user double clicks the mouse
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Data grid view cell event information. </param>
        private void gridField_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var affectedCellState = this.field.Do(CellAction.ClickOn, (ushort)e.RowIndex, (ushort)e.ColumnIndex);
            this.InvalidateDataGrid();
            
            if (affectedCellState == CellViewState.Exploded)
            {
                if (MessageBox.Show("Bomb exploded", "You lost!", MessageBoxButtons.OK) == DialogResult.OK)
                {
                    this.field.New();
                    this.InvalidateDataGrid();
                }
            }
        }

        /// <summary>
        /// When user clicks the mouse
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Data grid view cell mouse event information. </param>
        private void gridField_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.field.Do(CellAction.Tag, (ushort)e.RowIndex, (ushort)e.ColumnIndex);
                this.InvalidateDataGrid();
            }
        }

        /// <summary>
        /// Validate that user won
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btValidate_Click(object sender, EventArgs e)
        {
            string text, caption;
            if (this.field.IsCompleted())
            {
                text = "All bombs are found";
                caption = "You win!";
            }
            else
            {
                text = "Not all bombs are found";
                caption = "You lost!";
            }

            if (MessageBox.Show(text, caption, MessageBoxButtons.OK) == DialogResult.OK)
            {
                this.field.New();
                this.InvalidateDataGrid();
            }
        }

        /// <summary>
        /// Reset the current level
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btReset_Click(object sender, EventArgs e)
        {
            this.field.Reset();
            this.InvalidateDataGrid();
        }

        /// <summary>
        /// Create a new level
        /// </summary>
        ///
        /// <param name="sender"> Source of the event. </param>
        /// <param name="e">      Event information. </param>
        private void btNewMap_Click(object sender, EventArgs e)
        {
            this.field.New();
            this.InvalidateDataGrid();
        }

        /// <summary>
        /// Synchronize data grid with underlying game info
        /// </summary>
        private void InvalidateDataGrid()
        {
            for (ushort i = 0; i < s_size; ++i)
                for (ushort j = 0; j < s_size; ++j)
                {
                    string symbol = string.Empty;

                    switch (this.field.Cells[i, j])
                    {
                        case CellViewState.Empty: symbol = "E"; break;
                        case CellViewState.TaggedAsBomb: symbol = "B"; break;
                        case CellViewState.TaggedAsInquestion: symbol = "?"; break;
                        case CellViewState.Count_1: symbol = "1"; break;
                        case CellViewState.Count_2: symbol = "2"; break;
                        case CellViewState.Count_3: symbol = "3"; break;
                        case CellViewState.Count_4: symbol = "4"; break;
                        case CellViewState.Count_5: symbol = "5"; break;
                        case CellViewState.Exploded: symbol = "X"; break;
                    }

                    this.gridField[j, i].Value = symbol;
                }
        }
    }
}
