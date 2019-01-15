using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace NC_Reactor_Planner
{
    public class ReactorGridLayer : Panel
    {
        //private ReactorGridCell[,] cells;
        private List<ReactorGridCell> cells;
        private MenuStrip menu;

        public int LayerNumber { get; private set; }
        public int LayerWidth { get; private set; }
        public int LayerHeight { get; private set; }

        public ReactorGridLayer(int y) : base()
        {
            InitializeComponent();

            LayerWidth = (int)Reactor.interiorDims.X;
            LayerNumber = y;
            LayerHeight = (int)Reactor.interiorDims.Z;

            Width = LayerWidth * PlannerUI.blockSize;
            Visible = true;
            BorderStyle = BorderStyle.FixedSingle;

            ConstructMenu();
            Height = LayerHeight * PlannerUI.blockSize + menu.Height;
            ReloadCells();
        }

        private void ConstructMenu()
        {
            menu = new MenuStrip();
            menu.Dock = DockStyle.None;
            ToolStripMenuItem editMenu = new ToolStripMenuItem { Name = "Edit", Text = "Edit" };
            editMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Clear", Text = "Clear layer" });
            editMenu.DropDownItems["Clear"].Click += new EventHandler(MenuClear);
            editMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Copy", Text = "Copy layer" });
            editMenu.DropDownItems["Copy"].Click += new EventHandler(MenuCopy);
            editMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Paste", Text = "Paste layer" });
            editMenu.DropDownItems["Paste"].Click += new EventHandler(MenuPaste);
            menu.Items.Add(editMenu);

            ToolStripMenuItem manageMenu = new ToolStripMenuItem { Name = "Manage", Text = "Manage" };
            manageMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Delete", Text = "Delete layer" });
            manageMenu.DropDownItems["Delete"].Click += new EventHandler(MenuDelete);
            manageMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Insert after", Text = "Insert a new layer after this one" });
            manageMenu.DropDownItems["Insert after"].Click += new EventHandler(MenuInsertAfter);
            manageMenu.DropDownItems.Add(new ToolStripMenuItem { Name = "Insert before", Text = "Insert a new layer before this one" });
            manageMenu.DropDownItems["Insert before"].Click += new EventHandler(MenuInsertBefore);
            menu.Items.Add(manageMenu);

            ToolStripMenuItem layerLabel = new ToolStripMenuItem { Name = "LayerLabel", Text = "Layer " + LayerNumber.ToString() };
            menu.Items.Add(layerLabel);

            RescaleMenu();

            menu.Location = new Point(0, 0);
            menu.Visible = true;
            Controls.Add(menu);
            Refresh();
        }

        public void ReloadCells()
        {
            if (cells != null)
            {
                cells.Clear();
            }
            else
            {
                cells = new List<ReactorGridCell>();
            }
            Point location;

            for (int x = 0; x < LayerWidth; x++)
                for (int z = 0; z < LayerHeight; z++)
                {
                    location = new Point(PlannerUI.blockSize * x, menu.Height + PlannerUI.blockSize * z);
                    ReactorGridCell cell = (new ReactorGridCell(x + 1, LayerNumber, z + 1)
                    {
                        Block = Reactor.blocks[x + 1, LayerNumber, z + 1]
                    });
                    cells.Add(cell);
                }
        }

        private ReactorGridCell GetCell(int x, int z)
        {
            return cells[x * LayerHeight + z];
        }

        public void Rescale()
        {
            int bs = PlannerUI.blockSize;
            Size = new Size(bs * LayerWidth, bs * LayerHeight + menu.Height);

            RescaleMenu();

            this.Refresh();
        }

        private void RescaleMenu()
        {
            if (this.Width < 130)
            {
                menu.Items["Edit"].Text = "E";
                menu.Items["Manage"].Text = "M";
                menu.Items["LayerLabel"].Text = "L " + LayerNumber.ToString();
            }
            else
            {
                menu.Items["Edit"].Text = "Edit";
                menu.Items["Manage"].Text = "Manage";
                menu.Items["LayerLabel"].Text = "Layer " + LayerNumber.ToString();
            }
        }

        public Bitmap DrawToImage(int scale = 2)
        {
            //Redraw();
            int bs = PlannerUI.blockSize;
            this.Refresh();
            Bitmap layerImage = new Bitmap(LayerWidth * bs, LayerHeight * bs);
            this.DrawToBitmap(layerImage, new Rectangle(0, 0, LayerWidth * bs, LayerHeight * bs));
            return layerImage;
        }

        private void MenuClear(object sender, EventArgs e)
        {
            Reactor.ClearLayer(this);
            ((PlannerUI)(Parent.Parent)).RefreshStats();
        }

        private void MenuCopy(object sender, EventArgs e)
        {
            Reactor.CopyLayer(this);
        }

        private void MenuPaste(object sender, EventArgs e)
        {
            Reactor.PasteLayer(this);
            PlannerUI.Instance.RefreshStats();
        }

        private void MenuDelete(object sender, EventArgs e)
        {
            if (Reactor.layers.Count <= 1)
                return;
            Reactor.DeleteLayer(LayerNumber);
            PlannerUI.Instance.NewResetLayout(true);
        }

        private void MenuInsertBefore(object sender, EventArgs e)
        {
            if (Reactor.layers.Count >= Configuration.Fission.MaxSize)
            {
                MessageBox.Show("Reactor at max size!");
                return;
            }
            Reactor.InsertLayer(LayerNumber);
            PlannerUI.Instance.NewResetLayout(true);
        }

        private void MenuInsertAfter(object sender, EventArgs e)
        {
            if (Reactor.layers.Count >= Configuration.Fission.MaxSize)
            {
                MessageBox.Show("Reactor at max size!");
                return;
            }
            Reactor.InsertLayer(LayerNumber + 1);
            PlannerUI.Instance.NewResetLayout(true);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ReactorGridLayer
            // 
            this.Click += new System.EventHandler(this.ReactorGridLayer_Click);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ReactorGridLayer_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ReactorGridLayer_MouseDown);
            this.MouseEnter += new System.EventHandler(this.ReactorGridLayer_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.ReactorGridLayer_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ReactorGridLayer_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ReactorGridLayer_MouseUp);
            this.ResumeLayout(false);

        }

        private void ReactorGridLayer_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);

            var block_size = PlannerUI.blockSize;
            foreach (var cell in cells)
            {
                // X, Z
                cell.DrawSelf(e.Graphics, menu.Height, block_size);
            }

            if (MouseInBlock)
            {
                var pnt = this.PointToClient(Control.MousePosition);
                e.Graphics.DrawRectangle(Pens.Blue, pnt.X - 2, pnt.Y - 2, 5, 5);

                e.Graphics.DrawRectangle(Pens.Green, CurrentBlockX * block_size, (CurrentBlockZ * block_size) + menu.Height, block_size, block_size);
            }
        }

        private void ReactorGridLayer_Click(object sender, EventArgs args)
        {
            if (!(args is MouseEventArgs))
                return;

            var e = args as MouseEventArgs;
            var point = e.Location;
            if (point.Y <= menu.Height)
                return;
            var block_size = PlannerUI.blockSize;
            var block_x = (int)(point.X / block_size);
            var block_z = (int)((point.Y - menu.Height) / block_size);

            cells[block_x * this.LayerHeight + block_z].Clicked(e.Button);

            this.Refresh();
        }

        private void ReactorGridLayer_MouseDown(object sender, MouseEventArgs e)
        {
            var point = e.Location;
            if (point.Y <= menu.Height)
                return;
            var block_size = PlannerUI.blockSize;
            var block_x = (int)(point.X / block_size);
            var block_z = (int)((point.Y - menu.Height) / block_size);

            cells[block_x * this.LayerHeight + block_z].Mouse_Down(e.Button);
        }

        private int CurrentBlockX { get; set; }
        private int CurrentBlockZ { get; set; }
        private bool MouseInBlock { get; set; } = false;

        private void ReactorGridLayer_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.Location;
            if (point.Y <= menu.Height)
                return;
            var block_size = PlannerUI.blockSize;
            var block_x = (int)(point.X / block_size);
            var block_z = (int)((point.Y - menu.Height) / block_size);

            CurrentBlockX = block_x;
            CurrentBlockZ = block_z;

            cells[block_x * this.LayerHeight + block_z].Mouse_Move(e.Button);
        }

        private void ReactorGridLayer_MouseUp(object sender, MouseEventArgs e)
        {
            var point = e.Location;
            if (point.Y <= menu.Height)
                return;
            var block_size = PlannerUI.blockSize;
            var block_x = (int)(point.X / block_size);
            var block_z = (int)((point.Y - menu.Height) / block_size);
        }

        private void ReactorGridLayer_MouseEnter(object sender, EventArgs e)
        {
            MouseInBlock = true;
        }

        private void ReactorGridLayer_MouseLeave(object sender, EventArgs e)
        {
            MouseInBlock = false;
        }
    }
}