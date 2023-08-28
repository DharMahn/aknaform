using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aknaform;
internal class Aknakereso
{
    sbyte[,] grid = new sbyte[0, 0];
    sbyte[,] mask = new sbyte[0, 0];

    int bombs = 10;
    bool gameover = false;
    readonly Random r = new();

    public int GetCellValue(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return -1;
        }
        return grid[x, y];
    }
    public bool SetCellValue(int x, int y, sbyte val)
    {
        if (val > 9 || val < 0)
        {
            return false;
        }
        grid[x, y] = val;
        return true;
    }
    public int GetMaskValue(int x, int y)
    {
        if (!InBounds(x, y))
        {
            return -10;
        }
        return mask[x, y];
    }
    public bool SetMaskValue(int x, int y, sbyte val)
    {
        if (val < -1 || val > 1)
        {
            return false;
        }
        grid[x, y] = val;
        return true;
    }
    private bool InBounds(int x, int y)
    {
        ///sima egyszerű index ellenőrzés
        if (x < 0 || x > grid.GetLength(0) - 1 || y < 0 || y > grid.GetLength(1) - 1)
        {
            return false;
        }
        return true;
    }
    public void LoadGame()
    {
        using (StreamReader sr = new("save.txt"))
        {
            int w = int.Parse(sr.ReadLine());
            int h = int.Parse(sr.ReadLine());
            int b = int.Parse(sr.ReadLine());
            grid = new sbyte[w, h];
            mask = new sbyte[w, h];
            bombs = b;
            while (!sr.EndOfStream)
            {
                string[] line;
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    line = sr.ReadLine().Split(",");
                    for (int x = 0; x < grid.GetLength(0); x++)
                    {
                        grid[x, y] = sbyte.Parse(line[x]);
                    }
                }
                for (int y = 0; y < mask.GetLength(1); y++)
                {
                    line = sr.ReadLine().Split(",");
                    for (int x = 0; x < mask.GetLength(0); x++)
                    {
                        mask[x, y] = sbyte.Parse(line[x]);
                    }
                }
            }
        }
        gameover = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vx">the cell x position</param>
    /// <param name="vy">the cell y position</param>
    /// <returns>false if the clicked cell is a bomb, true if not</returns>

    public bool ClickOnCell(int vx, int vy, bool doubleClick = false)
    {
        ///határcheck
        if (InBounds(vx, vy))
        {
            ///ha még nincs felfedve és nincs zászlózva
            if (mask[vx, vy] == 0)
            {
                ///legyen felfedve ha rákattintottunk
                mask[vx, vy] = -1;

                ///ha bomba van, cumi
                if (grid[vx, vy] == 9)
                {
                    gameover = true;
                    return false;
                }
                ///ha nincs szomszéd, akkor fedjünk fel mindent körülötte
                else if (grid[vx, vy] == 0)
                {
                    Queue<Point> cellIndices = new();
                    ///felső 3
                    cellIndices.Enqueue(new Point(vx - 1, vy - 1));
                    cellIndices.Enqueue(new Point(vx, vy - 1));
                    cellIndices.Enqueue(new Point(vx + 1, vy - 1));
                    ///középső 2(saját magunkra nem kattintunk)
                    cellIndices.Enqueue(new Point(vx - 1, vy));
                    cellIndices.Enqueue(new Point(vx + 1, vy));
                    ///alsó 3;
                    cellIndices.Enqueue(new Point(vx - 1, vy + 1));
                    cellIndices.Enqueue(new Point(vx, vy + 1));
                    cellIndices.Enqueue(new Point(vx + 1, vy + 1));

                    while (cellIndices.Count > 0)
                    {
                        Point item = cellIndices.Dequeue();
                        if (InBounds(item.X, item.Y))
                        {
                            if (mask[item.X, item.Y] == 0)
                            {
                                mask[item.X, item.Y] = -1;
                                if (grid[item.X, item.Y] == 0)
                                {
                                    ///felső 3
                                    cellIndices.Enqueue(new Point(item.X - 1, item.Y - 1));
                                    cellIndices.Enqueue(new Point(item.X, item.Y - 1));
                                    cellIndices.Enqueue(new Point(item.X + 1, item.Y - 1));
                                    ///középső 2(saját magunkra nem kattintunk)
                                    cellIndices.Enqueue(new Point(item.X - 1, item.Y));
                                    cellIndices.Enqueue(new Point(item.X + 1, item.Y));
                                    ///alsó 3;
                                    cellIndices.Enqueue(new Point(item.X - 1, item.Y + 1));
                                    cellIndices.Enqueue(new Point(item.X, item.Y + 1));
                                    cellIndices.Enqueue(new Point(item.X + 1, item.Y + 1));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (doubleClick)
                {
                    bool flagCountSame = GetFlagsAroundCell(vx, vy) >= grid[vx, vy];
                    if (flagCountSame)
                    {
                        ///felső 3
                        ClickOnCell(vx - 1, vy - 1);
                        ClickOnCell(vx, vy - 1);
                        ClickOnCell(vx + 1, vy - 1);
                        ///középső 2(saját magunkra nem kattintunk)
                        ClickOnCell(vx - 1, vy);
                        ClickOnCell(vx + 1, vy);
                        ///alsó 3;
                        ClickOnCell(vx - 1, vy + 1);
                        ClickOnCell(vx, vy + 1);
                        ClickOnCell(vx + 1, vy + 1);
                    }
                }
            }
        }
        return true;
    }
    private int GetFlagsAroundCell(int vx, int vy)
    {
        int flagCount = 0;
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                if (InBounds(x + vx, y + vy))
                {
                    if (mask[x + vx, y + vy] == 1)
                    {
                        flagCount++;
                    }
                }
            }
        }
        return flagCount;
    }
    public bool FlagCell(int x, int y)
    {
        if (InBounds(x, y))
        {
            ///nincs zászló (0)
            if (mask[x, y] == 0)
            {
                mask[x, y] = 1;
            }
            ///van zászló (1)
            else if (mask[x, y] == 1)
            {
                mask[x, y] = 0;
            }
            ///fel van fedve (-1)
            return true;
        }
        else
        {
            Debug.WriteLine("Rossz cella index!");
            return false;
        }
    }
    public void SaveGame()
    {
        using StreamWriter sw = new("save.txt");
        sw.WriteLine(grid.GetLength(0));
        sw.WriteLine(grid.GetLength(1));
        sw.WriteLine(bombs);
        for (int y = 0; y < grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                sw.Write(grid[x, y] + ",");
            }
            sw.WriteLine();
        }
        for (int y = 0; y < mask.GetLength(1); y++)
        {
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                sw.Write(mask[x, y] + ",");
            }
            sw.WriteLine();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x">the cell x position</param>
    /// <param name="y">the cell y position</param>
    /// <returns>false if the clicked cell is a bomb, true if not</returns>
    public bool CheckWin()
    {
        ///összeszámoljuk az összes cellát ami:
        ///1. nincs felfedve
        ///2. nincs alatta bomba
        int counter = 0;
        Parallel.For(0, mask.GetLength(1), y =>
        {
            for (int x = 0; x < mask.GetLength(0); x++)
            {
                if (mask[x, y] == 0 && grid[x, y] != 9)
                {
                    counter++;
                }
            }
        });
        ///ha nincs ilyen cella, akkor nyertél
        if (counter == 0)
        {
            return true;
        }
        return false;
    }
    public bool CheckLoss()
    {
        return gameover;
    }
    public void GenerateNewMap(int w, int h, int b, int firstClickX = -100000, int firstClickY = -100000)
    {
        gameover = false;
        bombs = b;
        ///csinálunk egy új pályát, w(idth) és h(eight) méretekkel
        grid = new sbyte[w, h];
        mask = new sbyte[w, h];
        ///generálunk random számot
        ///majd ha ott már van bomba(9-es szám)
        ///akkor újrapróbáljuk
        int itercnt = 0;
        for (int i = 0; i < bombs; i++)
        {
            int x = r.Next(w);
            int y = r.Next(h);
            while (grid[x, y] == 9 || (Math.Abs(x - firstClickX) < 2 && Math.Abs(y - firstClickY) < 2))
            {
                itercnt++;
                x = r.Next(w);
                y = r.Next(h);
            }
            grid[x, y] = 9;
        }

        ///megszámoljuk a szomszédos cellákat
        ///hogy ne csak 0 és 9-ből álljon minden
        Parallel.For(0, h, y =>
        {
            for (int x = 0; x < w; x++)
            {
                if (grid[x, y] != 9)
                {
                    grid[x, y] = CountBombs(x, y);
                }
            }
        });
    }
    private sbyte CountBombs(int cx, int cy)
    {
        ///összeszámoljuk az szomszédos bombákat
        ///ez kell a pálya felállításához
        ///megkapjuk a jelenleg nézett cellákat (cx,cy)
        ///és ehhez hozzáadjuk a dupla for ciklusban
        ///lévő offseteket, amivel egy 3x3-as területet
        ///le tudunk fedni
        sbyte bombs = 0;
        for (int y = -1; y < 2; y++)
        {
            for (int x = -1; x < 2; x++)
            {
                ///ha mindkettő nulla, akkor az ránk mutat
                ///az nem jó, folytassuk a kövivel
                if (x == 0 && y == 0)
                {
                    continue;
                }
                else
                {
                    ///nem akarunk index out of boundsot kapni
                    if (InBounds(cx + x, cy + y))
                    {
                        ///ha bombát találunk(9-es szám)
                        if (grid[cx + x, cy + y] == 9)
                        {
                            bombs++;
                        }
                    }
                }
            }
        }
        ///majd visszaadjuk a bombát
        return bombs;
    }
}
