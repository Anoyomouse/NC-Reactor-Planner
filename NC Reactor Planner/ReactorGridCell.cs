using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace NC_Reactor_Planner
{
    public class ReactorGridCell
    {
        public Block Block { get; set; }
        public int X { get; set; }
        public int Z { get; set; }
        public int LayerNumber { get; set; }
        //public Image Image { get; set; }

        public ReactorGridCell(int x, int layerNumber, int z)
        {
            X = x;
            LayerNumber = layerNumber;
            Z = z;
        }

        public void Clicked(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.Left:
                    Reactor.blocks[X, LayerNumber, Z] = Palette.BlockToPlace(Block);
                    break;
                case MouseButtons.None:
                    return;
                case MouseButtons.Right:
                    Reactor.blocks[X, LayerNumber, Z] = new Block("Air", BlockTypes.Air, Palette.textures["Air"], Block.Position);
                    break;
                case MouseButtons.Middle:
                    Reactor.blocks[X, LayerNumber, Z] = new FuelCell("FuelCell", Palette.textures["FuelCell"], Block.Position);
                    break;
                default:
                    return;
            }
            Block = Reactor.blocks[X, LayerNumber, Z];

            Reactor.UpdateStats();

            PlannerUI.Instance.RefreshStats();
            PlannerUI.gridToolTip.Active = true;
        }

        public void Mouse_Move(MouseButtons button)
        {
            if (button == MouseButtons.None)
                return;
            if (Palette.PlacingSameBlock(Block, button))
                return;
            Clicked(button);
        }

        public void Mouse_Down(MouseButtons button)
        {
            PlannerUI.gridToolTip.Active = false;
        }

        public void DrawSelf(Graphics g, int top_offset, int block_size)
        {
            g.DrawImage(Reactor.BlockAt(Block.Position).Texture, (this.X - 1) * block_size, top_offset + (this.Z - 1) * block_size, block_size, block_size);
            if (Block is Cooler cooler && !cooler.Active)
            {
                Pen errorPen = new Pen(Color.Red, 1);
                g.DrawRectangle(errorPen, (this.X - 1) * block_size, top_offset + (this.Z - 1) * block_size, block_size, block_size);
            }
        }
    }
}
