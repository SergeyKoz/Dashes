using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace WpfApplication1
{
    public enum CellMode
    {
        Empty = 0,
        Border,
        Player1,
        Player2,
        Computer
    };

    public enum GameMode
    {
        Player1Player2,
        Player1Computer
    };

    public enum PlayerMode
    {
        Player1,
        Player2,
        Computer,
    };

    public enum WinMode
    {
        Player1,
        Player2,
        Computer,
        None
    };

    public enum LineMode
    {
        Horizontal,
        Vertical,
        None
    };

    public class GameField
    {
        public GameMode GameMode;
        public PlayerMode PlayerMode;

        public int x = 10;
        public int y = 10;

        private int cellSize = 20;
        private int dasheThickness = 4;

        private string GameFileName = "/dashes.dat";

        public GameCell[,] fileldArray;

        public GameCursor GameCursor;
        
        public GameField()
        {
            this.CellSize = cellSize;
            this.DasheThickness = dasheThickness;
        }

        public void InitGameField(){
            GameCursor = new GameCursor();

            string Directory = System.IO.Directory.GetCurrentDirectory();
            string GameFile = Directory + this.GameFileName;

            GameCursor.CursorLineMode = LineMode.Horizontal;
            GameMode = GameMode.Player1Player2;
            
            if (!System.IO.File.Exists(GameFile))
            {
                NewGame();
            }
            else
            {
                LoadGame();
            }
        }

        private void InitFieldArray(int fx, int fy)
        {
            if (fileldArray != null)
            {
                //clean field
                int lx = fileldArray.GetLength(0);
                int ly = fileldArray.GetLength(1);
                for (int iy = 0; iy <= ly - 1; iy++)
                {
                    for (int ix = 0; ix <= lx - 1; ix++)
                    {   
                        fileldArray[ix, iy].Clean();
                    }
                }
            }

            fileldArray = new GameCell[fx, fy];
            
            for (int iy = 0; iy <= fy - 1; iy++)
            {
                for (int ix = 0; ix <= fx - 1; ix++)
                {
                    if (fileldArray[ix, iy] is GameCell)
                    {
                        fileldArray[ix, iy].Clean(); 
                    }
                    else
                    {
                        fileldArray[ix, iy] = new GameCell(ix, iy, CellMode.Empty, CellMode.Empty, WinMode.None);
                    }
                }
            }
        }

        public int CellSize
        {
            get;
            private set; 
        }

        public int DasheThickness
        {
            get;
            private set;
        }

        public void NewGame()
        {
            PlayerMode = PlayerMode.Player1;

            //set window
            ((MainWindow)(App.Current.MainWindow)).Width = CellSize * x-5;
            ((MainWindow)(App.Current.MainWindow)).Height = CellSize * y+55;

            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;
            FieldCanvas.Width = CellSize * x - 20;
            FieldCanvas.Height = CellSize * y - 20;

            InitFieldArray(x, y);

            //set border
            for (int ix = 0; ix <= x - 2; ix++)
            {
                fileldArray[ix, 0].SetCell(CellMode.Border, CellMode.Empty);
                fileldArray[ix, y - 1].SetCell(CellMode.Border, CellMode.Empty);
            }

            for (int iy = 0; iy <= y - 2; iy++)
            {
                fileldArray[0, iy].SetCell((iy == 0 ? CellMode.Border : CellMode.Empty), CellMode.Border);
                fileldArray[x - 1, iy].SetCell(CellMode.Empty, CellMode.Border);
            }

            for (int ix = 0; ix < x; ix++)
            {
                for (int iy = 0; iy < y; iy++)
                {
                    if (iy>0 && iy<y-1 && ix>0 && ix<x-1){
                        fileldArray[ix, iy].SetCell(CellMode.Empty, CellMode.Empty);
                    }
                   
                    fileldArray[ix, iy].SetCell(WinMode.None);                    
                }
            }
            UpdateWinStatus();
            SaveGame();            
        }

        private void SaveGame() {
            
            GameSave GameSave = new GameSave();
            GameSave.x = this.x;
            GameSave.y = this.y;
            GameSave.GameMode = this.GameMode;
            GameSave.PlayerMode = this.PlayerMode;
            GameSave.fileldArray = new CellSave[x, y];

            for (int iy = 0; iy <= y - 1; iy++)
            {
                for (int ix = 0; ix <= x - 1; ix++)
                {
                    GameSave.fileldArray[ix, iy] = new CellSave(this.fileldArray[ix, iy].Horizontal, this.fileldArray[ix, iy].Vertical, this.fileldArray[ix, iy].WinMode);
                }
            }

            string Directory = System.IO.Directory.GetCurrentDirectory();
            string GameFile = Directory + this.GameFileName;

            IFormatter Formatter = new BinaryFormatter();
            Stream stream = new FileStream(GameFile, FileMode.Create, FileAccess.Write, FileShare.None);
            Formatter.Serialize(stream, GameSave);
            stream.Close();
        }

        private void LoadGame()
        {
            string Directory = System.IO.Directory.GetCurrentDirectory();
            string GameFile = Directory + this.GameFileName;

            IFormatter Formatter = new BinaryFormatter();

            GameSave GameLoad = new GameSave();

            Stream stream = new FileStream(GameFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            IFormatter formatter = new BinaryFormatter();
            GameLoad = (GameSave)formatter.Deserialize(stream);
            stream.Close();

            //set game state
            x = GameLoad.x;
            y = GameLoad.y;

            GameMode = GameLoad.GameMode;
            PlayerMode = GameLoad.PlayerMode;

            InitFieldArray(x, y);

            CellSave CellSave;

            for (int iy = 0; iy <= y - 1; iy++)
            {
                for (int ix = 0; ix <= x - 1; ix++)
                {
                    CellSave = GameLoad.fileldArray[ix, iy];
                    this.fileldArray[ix, iy].SetCell(CellSave.Horizontal, CellSave.Vertical);
                    this.fileldArray[ix, iy].SetCell(CellSave.WinMode);
                }
            }

            //set window
            ((MainWindow)(App.Current.MainWindow)).Width = CellSize * x - 5;
            ((MainWindow)(App.Current.MainWindow)).Height = CellSize * y + 55;

            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;
            FieldCanvas.Width = CellSize * x - 20;
            FieldCanvas.Height = CellSize * y - 20;
            UpdateWinStatus(true);
        }

        public void SetStep(object sender, MouseEventArgs e)
        {
            CellMode StepModeHorizontal = CellMode.Empty;
            CellMode StepModeVertical = CellMode.Empty;
            if (GameCursor.InGameField)
            {
                GameCursor.CursorPosition.SetPosition(e);

                GameCell Cell = fileldArray[GameCursor.CursorPosition.X, GameCursor.CursorPosition.Y];

                bool IsEnable = false;

                if (GameCursor.CursorLineMode == LineMode.Horizontal && Cell.Horizontal == CellMode.Empty)
                {
                    switch (PlayerMode)
                    {
                        case PlayerMode.Player1:
                            StepModeHorizontal = CellMode.Player1;
                            break;
                        case PlayerMode.Player2:
                            StepModeHorizontal = CellMode.Player2;
                            break;
                        case PlayerMode.Computer:
                            StepModeHorizontal = CellMode.Computer;
                            break;
                    }
                    StepModeVertical = Cell.Vertical;
                    IsEnable = true;
                }

                if (GameCursor.CursorLineMode == LineMode.Vertical && Cell.Vertical == CellMode.Empty)
                {
                    StepModeHorizontal = Cell.Horizontal;
                    switch (PlayerMode)
                    {
                        case PlayerMode.Player1:
                            StepModeVertical = CellMode.Player1;
                            break;
                        case PlayerMode.Player2:
                            StepModeVertical = CellMode.Player2;
                            break;
                        case PlayerMode.Computer:
                            StepModeVertical = CellMode.Computer;
                            break;
                    }
                    IsEnable = true;
                }

                if (IsEnable)
                {
                    Cell.SetCell(StepModeHorizontal, StepModeVertical);
                    
                    if (!Cell.CheckWinCells())
                    {
                        if (GameMode == GameMode.Player1Player2)
                        {
                            switch (PlayerMode)
                            {
                                case PlayerMode.Player1:
                                    PlayerMode = PlayerMode.Player2;
                                    break;
                                case PlayerMode.Player2:
                                    PlayerMode = PlayerMode.Player1;
                                    break;
                            }
                        }

                        if (GameMode == GameMode.Player1Computer)
                        {
                            PlayerMode = PlayerMode.Computer;
                            SetComputerStep();
                            PlayerMode = PlayerMode.Player1;

                            /*switch (PlayerMode)
                            {
                                case PlayerMode.Player1:
                                    PlayerMode = PlayerMode.Computer;
                                    break;
                                case PlayerMode.Computer:
                                    PlayerMode = PlayerMode.Player1;
                                    break;
                            }*/
                        }
                    }

                    SaveGame();
                }
            }
        }

        public void UpdateWinStatus(bool Recalculate=false)
        {
            if (Recalculate)
            {
                ItemCollection StatusBar = ((MainWindow)(App.Current.MainWindow)).statusBar1.Items;

                TextBlock StatusBarPalyer1 = (TextBlock)((StatusBarItem)(StatusBar[0])).Content;
                TextBlock StatusBarPalyer2 = (TextBlock)((StatusBarItem)(StatusBar[2])).Content;

                int P1 = 0;
                int P2 = 0;
                string Palyer2="";
                
                for (int ix = 0; ix <= x - 2; ix++)
                {
                    for (int iy = 0; iy <= y - 2; iy++)
                    {
                        if (GameMode == GameMode.Player1Player2)
                        {
                            if (fileldArray[ix, iy].WinMode == WinMode.Player1)
                            {
                                P1++;
                            }

                            if (fileldArray[ix, iy].WinMode == WinMode.Player2)
                            {
                                P2++;
                            }
                        }

                        if (GameMode == GameMode.Player1Computer)
                        {
                            if (fileldArray[ix, iy].WinMode == WinMode.Player1)
                            {
                                P1++;
                            }

                            if (fileldArray[ix, iy].WinMode == WinMode.Computer)
                            {
                                P2++;
                            }
                        } 
                    }
                }

                if (GameMode == GameMode.Player1Player2)
                {
                    Palyer2="Palyer2";
                }

                if (GameMode == GameMode.Player1Computer)
                {
                    Palyer2="Computer";
                }

                StatusBarPalyer1.Text = "Palyer1: " + P1.ToString();
                StatusBarPalyer2.Text = Palyer2+": " + P2.ToString();
            }
        }

        private void SetComputerStep() {
            
            // create steps map
            List<ComputerWinSet> ComputerWinMap = new List<ComputerWinSet>();
            List<ComputerLoseSet> ComputerLoseMap = new List<ComputerLoseSet>();

            GameCell Cell;
            for (int iy = 0; iy <= y-2; iy++)
            {
                for (int ix = 0; ix <= x-2; ix++)
                {
                    Cell = fileldArray[ix, iy];
                    ComputerWinMap.Add(new ComputerWinSet(Cell, GetWinsCell(ix, iy)));
                }
            }

            foreach (var ComputerWinSetItem in ComputerWinMap)
            {
                if (ComputerWinSetItem.Win > 0)
                {
                    SetComputerStep(ComputerWinSetItem.Cell);
                }
            }

            for (int iy = 0; iy <= y - 2; iy++)
            {
                for (int ix = 0; ix <= x - 2; ix++)
                {
                    Cell = fileldArray[ix, iy];                    
                    GetLoseCell(ComputerLoseMap, Cell);
                }
            }

            if (ComputerLoseMap.Count>0)
            {
                //find optimal step
                int MinLose = (from item in ComputerLoseMap select item.Lose).Min();
                ComputerLoseSet ComputerLoseSet = (from item in ComputerLoseMap where item.Lose == MinLose orderby Guid.NewGuid() select item).FirstOrDefault();

                if (ComputerLoseSet.HV)
                {
                    ComputerLoseSet.Cell.SetCell(CellMode.Computer, ComputerLoseSet.Cell.Vertical);
                }
                else
                {
                    ComputerLoseSet.Cell.SetCell(ComputerLoseSet.Cell.Horizontal, CellMode.Computer);
                }
            }
        }

        private void SetComputerStep(GameCell Cell)
        {
            int[,] WinCells;
            WinCells = new int[x, y];
            if (Cell.WinMode == WinMode.None)
            {
                WaveCells(Cell.x, Cell.y, WinCells);
                GameCell Cell1, CellD, CellL;

                for (int iy = 0; iy <= y - 1; iy++)
                {
                    for (int ix = 0; ix <= x - 1; ix++)
                    {
                        if (WinCells[ix, iy] == 1)
                        {
                            Cell1=fileldArray[ix, iy];
                            CellD=fileldArray[ix, iy + 1];
                            CellL=fileldArray[ix + 1, iy];
                            Cell1.SetCell(WinMode.Computer);
                            Cell1.SetCell((Cell1.Horizontal == CellMode.Empty ? CellMode.Computer : Cell1.Horizontal), (Cell1.Vertical == CellMode.Empty ? CellMode.Computer : Cell1.Vertical));
                            CellD.SetCell((CellD.Horizontal == CellMode.Empty ? CellMode.Computer : CellD.Horizontal), CellD.Vertical);
                            CellL.SetCell(CellL.Horizontal, (CellL.Vertical == CellMode.Empty ? CellMode.Computer : CellL.Vertical));
                        }
                    }
                }
            }
        }

        public int GetWinsCell(int ix, int iy)
        {
            int WinItems=-1;

            //find first win position
            GameCell Cell, CellH, CellV;
            
            int i;
            
            int[,] WinCells;

            Cell = fileldArray[ix, iy];
            
            if (Cell.WinMode == WinMode.None)
            {
                i = 0;
                WinItems = 0;
                
                CellH = fileldArray[ix + 1, iy];
                CellV = fileldArray[ix, iy + 1];

                if (Cell.Horizontal != CellMode.Empty)
                {
                    i++;
                }

                if (Cell.Vertical != CellMode.Empty)
                {
                    i++;
                }

                if (CellH.Vertical != CellMode.Empty)
                {
                    i++;
                }

                if (CellV.Horizontal != CellMode.Empty)
                {
                    i++;
                }

                if (i == 3)
                {
                    // it win cell 
                    WinCells = new int[x, y];
                    WaveCells(ix, iy, WinCells);

                    for (int cy = 0; cy <= y - 1; cy++)
                    {
                        for (int cx = 0; cx <= x - 1; cx++)
                        {
                            if (WinCells[cx, cy] == 1)
                            {
                                WinItems++;
                            }
                        }
                    }
                }
            }
            
            return WinItems;
        }

        public void GetLoseCell(List<ComputerLoseSet> ComputerLoseMap, GameCell Cell)
        {
            int lose1, lose2;

            int ix = Cell.x;
            int iy = Cell.y;

            if (Cell.WinMode == WinMode.None)
            {
                if (Cell.Horizontal == CellMode.Empty)
                {
                    Cell.Horizontal=CellMode.Computer;
                    lose1 = GetWinsCell(ix, iy);
                    lose2 = 0;
                    if (iy > 0)
                    {
                        if (fileldArray[ix, iy - 1].WinMode == WinMode.None)
                        {
                            lose2 = GetWinsCell(ix, iy - 1);
                        }
                    }
                    Cell.Horizontal = CellMode.Empty;

                    ComputerLoseMap.Add(new ComputerLoseSet(Cell, lose1 > lose2 ? lose1 : lose2, true));
                }

                if (Cell.Vertical == CellMode.Empty)
                {
                    Cell.Vertical = CellMode.Computer;
                    lose1 = GetWinsCell(ix, iy);
                    lose2 = 0;
                    if (ix > 0)
                    {
                        if (fileldArray[ix - 1, iy].WinMode == WinMode.None)
                        {
                            lose2 = GetWinsCell(ix - 1, iy);
                        }
                    }
                    Cell.Vertical = CellMode.Empty;
                    ComputerLoseMap.Add(new ComputerLoseSet(Cell, lose1 > lose2 ? lose1 : lose2, false));
                }
            }
        }

        private void WaveCells(int ix, int iy, int[,] WinCells)
        {
            GameCell Cell, Cell1, Cell2;
            WinCells[ix, iy] = 1;
            int i;

            // to left
            if (ix > 0)
            {
                if (WinCells[ix - 1, iy]!=1){
                    Cell = fileldArray[ix, iy];
                    if (Cell.Vertical == CellMode.Empty) {
                        Cell1= fileldArray[ix - 1, iy];
                        Cell2 = fileldArray[ix - 1, iy + 1];
                        i = 1;

                        if (Cell1.Horizontal != CellMode.Empty) {
                            i++;
                        }

                        if (Cell1.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell2.Horizontal != CellMode.Empty)
                        {
                            i++;
                        }

                        if (i == 3)
                        {
                            WaveCells(ix - 1, iy, WinCells);
                        }

                    }
                }
            }

            // to right
            if (ix < x - 1)
            {
                if (WinCells[ix + 1, iy] != 1)
                {
                    Cell = fileldArray[ix + 1, iy];
                    if (Cell.Vertical == CellMode.Empty)
                    {
                        Cell1 = fileldArray[ix + 2, iy];
                        Cell2 = fileldArray[ix + 1, iy + 1];
                        i = 1;

                        if (Cell.Horizontal != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell1.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell2.Horizontal != CellMode.Empty)
                        {
                            i++;
                        }

                        if (i == 3)
                        {
                            WaveCells(ix + 1, iy, WinCells);
                        }

                    }
                }
            }

            // to up
            if (iy > 0)
            {
                if (WinCells[ix, iy-1] != 1)
                {
                    Cell = fileldArray[ix, iy];
                    if (Cell.Horizontal == CellMode.Empty)
                    {
                        Cell1 = fileldArray[ix, iy - 1];
                        Cell2 = fileldArray[ix + 1, iy - 1];
                        i = 1;

                        if (Cell1.Horizontal != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell1.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell2.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (i == 3)
                        {
                            WaveCells(ix, iy - 1, WinCells);
                        }

                    }
                }
            }

            // to down
            if (iy < y - 1)
            {
                if (WinCells[ix, iy + 1] != 1)
                {
                    Cell = fileldArray[ix, iy + 1];
                    if (Cell.Horizontal == CellMode.Empty)
                    {
                        Cell1 = fileldArray[ix, iy + 2];
                        Cell2 = fileldArray[ix + 1, iy + 1];
                        i = 1;

                        if (Cell.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell1.Horizontal != CellMode.Empty)
                        {
                            i++;
                        }

                        if (Cell2.Vertical != CellMode.Empty)
                        {
                            i++;
                        }

                        if (i == 3)
                        {
                            WaveCells(ix, iy + 1, WinCells);
                        }

                    }
                }
            }
        }
    }

    [Serializable]
    public class GameSave
    {
        public GameMode GameMode;
        public PlayerMode PlayerMode;

        public int x;
        public int y;

        public CellSave[,] fileldArray;
        //public int[,] fileldArray;
    }

    [Serializable]
    public class CellSave
    {   
        public CellMode Horizontal;
        public CellMode Vertical;
        public WinMode WinMode;

        public CellSave(CellMode Horizontal, CellMode Vertical, WinMode WinMode)
        {

            this.Horizontal = Horizontal;
            this.Vertical = Vertical;
            this.WinMode = WinMode;
        }
    }

    public class GameCursor
    {
        private Line CursorLine;

        public CursorPosition CursorPosition;

        public bool InGameField = false;

        public GameCursor()
        {   
            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;

            GameField Game = ((App)(Application.Current)).Game;
            

            CursorPosition = new CursorPosition();
            CursorLine = new Line();
            CursorLine.StrokeThickness = Game.DasheThickness;
            CursorLine.Stroke = Brushes.Transparent;
            CursorLine.Opacity = 0.5;
            FieldCanvas.Children.Add(CursorLine);
        }

        public LineMode CursorLineMode
        {
            get;
            set;
        }

        public void CursorMove(object sender, MouseEventArgs e){
            CursorPosition.SetPosition(e);
            CursorWrite(CursorPosition.X, CursorPosition.Y);
        }

        public void ChangeLineModeField(object sender, MouseEventArgs e)
        {
            if (CursorLineMode == LineMode.Horizontal)
            {
                CursorLineMode = LineMode.Vertical;
            }
            else
            {
                CursorLineMode = LineMode.Horizontal;
            }

            CursorPosition.SetPosition(e);
            CursorWrite(CursorPosition.X, CursorPosition.Y);
        }

        public void CursorWrite(int x, int y)
        {
            GameField Game = ((App)(Application.Current)).Game;
            int CellSize = Game.CellSize;
            PlayerMode PlayerMode = Game.PlayerMode;

            CursorLine.X1 = x * CellSize;
            CursorLine.Y1 = y * CellSize;

            if (Game.x-1 > x && Game.y-1 > y)
            {
                InGameField = true;
                switch (PlayerMode)
                {
                    case PlayerMode.Player1:
                        CursorLine.Stroke = Brushes.Blue;
                        break;
                    case PlayerMode.Player2:
                        CursorLine.Stroke = Brushes.Green;
                        break;
                    case PlayerMode.Computer:
                        CursorLine.Stroke = Brushes.Red;
                        break;
                }

                if (CursorLineMode == LineMode.Horizontal)
                {
                    CursorLine.X2 = x * CellSize + CellSize + 1;
                    CursorLine.Y2 = y * CellSize;
                }

                if (CursorLineMode == LineMode.Vertical)
                {
                    CursorLine.X2 = x * CellSize;
                    CursorLine.Y2 = y * CellSize + CellSize + 1;
                }
            }
            else
            {
                CursorLine.Stroke = Brushes.Transparent;
                InGameField = false;
            }
        }

        public void CursorLeave(object sender, MouseEventArgs e)
        {
            CursorLine.Stroke = Brushes.Transparent;
            InGameField = false;
        }

        
    }

    public class CursorPosition
    {   
        public void SetPosition(MouseEventArgs e)
        {
            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;
            Point position = e.GetPosition(FieldCanvas);

            int CellSize = ((App)(Application.Current)).Game.CellSize;

            double xi = Math.Round(position.X / CellSize);
            double yi = Math.Round(position.Y / CellSize);
            X=Convert.ToInt16(xi);
            Y=Convert.ToInt16(yi);            
        }

        public int X
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }
    }
    
    public class GameCell
    {   
        public int x;
        public int y;

        private Line FieldItem;
        private Line HorizontalFieldItem;
        private Line VerticalFieldItem;
        private Rectangle FillFieldItem;

        public GameCell( int x, int y, CellMode Horizontal, CellMode Vertical, WinMode WinMode)
        {
            
            this.x = x;
            this.y = y;

            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;

            FieldItem = new Line();
            FieldCanvas.Children.Add(FieldItem);

            HorizontalFieldItem = new Line();
            FieldCanvas.Children.Add(HorizontalFieldItem);
            
            VerticalFieldItem = new Line();
            FieldCanvas.Children.Add(VerticalFieldItem);

            FillFieldItem = new Rectangle();
            FieldCanvas.Children.Add(FillFieldItem);

            SetCell(Horizontal, Vertical);
        }

        public CellMode Horizontal
        {
            get;
            set;
        }

        public CellMode Vertical
        {
            get;
            set;
        }

        public WinMode WinMode
        {
            get;
            set;
        }

        public void SetCell(CellMode Horizontal, CellMode Vertical)
        {   
            int CellSize = ((App)(Application.Current)).Game.CellSize;
            int DasheThickness = ((App)(Application.Current)).Game.DasheThickness;

            
            FieldItem.X1 = x * CellSize;
            FieldItem.Y1 = y * CellSize;
            FieldItem.X2 = x * CellSize + 1;
            FieldItem.Y2 = y * CellSize;
            FieldItem.StrokeThickness = 1;
            FieldItem.Stroke = Brushes.Black;
            
            HorizontalFieldItem.X1 = x * CellSize;
            HorizontalFieldItem.Y1 = y * CellSize;
            HorizontalFieldItem.X2 = x * CellSize + CellSize + 1;
            HorizontalFieldItem.Y2 = y * CellSize;
            HorizontalFieldItem.StrokeThickness = DasheThickness;
               
            switch (Horizontal)
            {
                case CellMode.Border:
                    HorizontalFieldItem.Stroke = Brushes.Black;
                    break;
                case CellMode.Player1:
                    HorizontalFieldItem.Stroke = Brushes.Blue;
                    break;
                case CellMode.Player2:
                    HorizontalFieldItem.Stroke = Brushes.Green;
                    break;
                case CellMode.Computer:
                    HorizontalFieldItem.Stroke = Brushes.Red;
                    break;
                case CellMode.Empty:
                    HorizontalFieldItem.Stroke = Brushes.Transparent;
                    break;
            }
            this.Horizontal = Horizontal;
               
            VerticalFieldItem.X1 = x * CellSize;
            VerticalFieldItem.Y1 = y * CellSize;
            VerticalFieldItem.X2 = x * CellSize;
            VerticalFieldItem.Y2 = y * CellSize + CellSize + 1;
            VerticalFieldItem.StrokeThickness = DasheThickness;

            switch (Vertical)
            {
                case CellMode.Border:
                    VerticalFieldItem.Stroke = Brushes.Black;
                    break;
                case CellMode.Player1:
                    VerticalFieldItem.Stroke = Brushes.Blue;
                    break;
                case CellMode.Player2:
                    VerticalFieldItem.Stroke = Brushes.Green;
                    break;
                case CellMode.Computer:
                    VerticalFieldItem.Stroke = Brushes.Red;
                    break;

                case CellMode.Empty:
                    VerticalFieldItem.Stroke = Brushes.Transparent;
                    break;
            }
            this.Vertical = Vertical;
        }

        public void SetCell(WinMode WinMode)
        {
            GameField Game = ((App)(Application.Current)).Game;
            int CellSize = Game.CellSize;

            FillFieldItem.Width = CellSize;
            FillFieldItem.Height = CellSize;
            FillFieldItem.Opacity = 0.5;

            switch (WinMode)
            {

                case WinMode.Player1:
                    FillFieldItem.Fill = Brushes.Blue;
                    break;
                case WinMode.Player2:
                    FillFieldItem.Fill = Brushes.Green;
                    break;
                case WinMode.Computer:
                    FillFieldItem.Fill = Brushes.Red;
                    break;
                case WinMode.None:
                    FillFieldItem.Fill = Brushes.Transparent;
                    break;
            }

            Canvas.SetLeft(FillFieldItem, x * CellSize);
            Canvas.SetTop(FillFieldItem, y * CellSize);
            this.WinMode = WinMode;

            //Game.UpdateWinStatus(WinMode);
            Game.UpdateWinStatus(true);
        }
        
        public bool CheckWinCells()
        {
            GameField Game = ((App)(Application.Current)).Game;
            
            bool f = false;
            GameCell Cell1, Cell2;

            WinMode Win = WinMode.None;
            
            Cell1 = Game.fileldArray[x + 1, y];
            Cell2 = Game.fileldArray[x, y + 1];

            switch (Game.PlayerMode)
            {
                case PlayerMode.Player1:
                    Win = WinMode.Player1;
                    break;
                case PlayerMode.Player2:
                    Win = WinMode.Player2;
                    break;
                case PlayerMode.Computer:
                    Win = WinMode.Computer;
                    break;
            }

            if (WinMode == WinMode.None && Horizontal != CellMode.Empty && Vertical != CellMode.Empty && Cell1.Vertical != CellMode.Empty && Cell2.Horizontal != CellMode.Empty)
            {
                SetCell(Win);
                f = true;
            }

            if (y > 0)
            {
                Cell1 = Game.fileldArray[x, y - 1];
                Cell2 = Game.fileldArray[x + 1, y - 1];

                if (Cell1.WinMode == WinMode.None && Horizontal != CellMode.Empty && Cell1.Horizontal != CellMode.Empty && Cell1.Vertical != CellMode.Empty && Cell2.Vertical != CellMode.Empty)
                {
                    Cell1.SetCell(Win);
                    f = true;
                }
            }

            if (x > 0)
            {
                Cell1 = Game.fileldArray[x - 1, y];
                Cell2 = Game.fileldArray[x - 1, y + 1];

                if (Cell1.WinMode == WinMode.None && Vertical != CellMode.Empty && Cell1.Horizontal != CellMode.Empty && Cell1.Vertical != CellMode.Empty && Cell2.Horizontal != CellMode.Empty)
                {
                    Cell1.SetCell(Win);
                    f = true;
                }
            }

            return f;
        }

        public void Clean() {
            Canvas FieldCanvas = ((MainWindow)(App.Current.MainWindow)).GameFieldCanvas;
            FieldCanvas.Children.Remove(FieldItem);
            FieldCanvas.Children.Remove(HorizontalFieldItem);
            FieldCanvas.Children.Remove(VerticalFieldItem);
            FieldCanvas.Children.Remove(FillFieldItem);
        }
    }

    public class ComputerWinSet
    {
        public ComputerWinSet(GameCell cell, int win)
        {
            this.Cell = cell;
            this.Win = win;            
        }

        public GameCell Cell
        {
            get;
            set;
        }

        public int Y
        {
            get;
            set;
        }

        public int Win
        {
            get;
            set;
        }
    }

    public class ComputerLoseSet
    {
        public ComputerLoseSet(GameCell cell, int lose, bool hv)
        {
            this.Cell = cell;
            this.HV = hv;
            this.Lose = lose;
        }

        public GameCell Cell
        {
            get;
            set;
        }

        public bool HV
        {
            get;
            set;
        }

        public int Lose
        {
            get;
            set;
        }
    }
}
