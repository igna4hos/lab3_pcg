using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace main
{
    public partial class Form4 : Form
    {
        private readonly Circle[] circles;
        private Bitmap bitmap;
        private int pixelSize = 1; // Масштаб от 1 до 5
        private int offsetX = 0;   // Смещение по X
        private int offsetY = 0;   // Смещение по Y
        private bool showGrid = false; // Флаг для отображения координатной сетки
        private bool showCoordGrid = true;
        private bool backInfo = true;
        private readonly int methodCircuitNum = 1;
        private int methodPaintingNum = 0;
        private int currentCircleIndex = 0; // Индекс текущей окружности
        private bool testPlay = false;

        public Form4()
        {
            this.ClientSize = new Size(800, 800); // Размер окна
            this.circles = new Circle[]
            {
                new Circle(0, 0, 10, 255, 0, 0),
                new Circle(0, 0, 30, 255, 127, 0),
                new Circle(0, 0, 100, 0, 255, 0),
                new Circle(0, 0, 150, 0, 0, 255),
                new Circle(0, 0, 200, 75, 0, 130),
            };
            this.Paint += new PaintEventHandler(DrawAllCirclesWithGrid);
            this.KeyDown += new KeyEventHandler(OnKeyDown); // Обработка клавиш
        }

        private void DrawAllCirclesWithGrid(object sender, PaintEventArgs e)
        {
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            Circle circle = circles[currentCircleIndex]; // Рисуем только текущую окружность
            if (methodPaintingNum == 0 && circle.Radius > 30)
                methodPaintingNum++;
            DrawCircle(circle);
            (long tickFillCircle, int pixelCount) = FillCircle(circle, e); // Вызов закраски
            Console.WriteLine($"Закрашено пикселей в окружности {currentCircleIndex}: {pixelCount}"); // Выводим количество пикселей
            Console.WriteLine($"Тики выполнения по методу {methodPaintingNum}: {tickFillCircle}"); // Выводим количество пикселей

            String drawString = $"Закрашено пикселей в окружности {currentCircleIndex}: {pixelCount}";

            // Create font and brush.
            Font drawFont = new Font("Arial", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Create point for upper-left corner of drawing.
            float a = 0.0F;
            float b = 0.0F;
            // Draw string to screen.
            e.Graphics.DrawString(drawString, drawFont, drawBrush, a, b);

            String drawString2 = $"Тики выполнения по методу {methodPaintingNum}: {tickFillCircle}";

            // Create point for upper-left corner of drawing.
            float a2 = 0.0F;
            float b2 = 12.0F;
            // Draw string to screen.
            e.Graphics.DrawString(drawString2, drawFont, drawBrush, a2, b2);


            if (showGrid && pixelSize == 5) DrawCoordinateGrid(); // Координатная сетка при масштабе 5x
            if (showCoordGrid) DrawGrid();
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        private void DrawGrid()
        {
            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;

            // Оси с учетом смещения и масштаба
            for (int x = 0; x < ClientSize.Width; x += pixelSize)
            {
                SetPixelBlock(centerX + x, centerY, Color.LightGray, pixelSize);
                SetPixelBlock(centerX - x, centerY, Color.LightGray, pixelSize);
            }

            for (int y = 0; y < ClientSize.Height; y += pixelSize)
            {
                SetPixelBlock(centerX, centerY + y, Color.LightGray, pixelSize);
                SetPixelBlock(centerX, centerY - y, Color.LightGray, pixelSize);
            }
        }

        private void DrawCoordinateGrid()
        {
            if (pixelSize != 5) return; // Сетка отображается только при максимальном увеличении

            int gridSpacing = pixelSize; // Интервал между линиями сетки, масштабированный
            int startX = offsetX % gridSpacing; // Начальная позиция с учётом смещения
            int startY = offsetY % gridSpacing;

            // Рисуем вертикальные линии сетки
            for (int x = startX; x < ClientSize.Width; x += gridSpacing)
            {
                for (int y = 0; y < ClientSize.Height; y++)
                {
                    SetPixelBlock(x, y, Color.Gray, 1); // 1 пиксель ширины линии
                }
            }

            // Рисуем горизонтальные линии сетки
            for (int y = startY; y < ClientSize.Height; y += gridSpacing)
            {
                for (int x = 0; x < ClientSize.Width; x++)
                {
                    SetPixelBlock(x, y, Color.Gray, 1); // 1 пиксель высоты линии
                }
            }
        }


        private long DrawCircle(Circle circle)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;
            double unitScale = 1; // Коэффициент для перевода условных единиц в пиксели
            int radiusInPixels = (int)(circle.Radius * unitScale);

            int cx = centerX + (int)(circle.X * unitScale * pixelSize);
            int cy = centerY - (int)(circle.Y * unitScale * pixelSize); // инвертируем Y для корректной ориентации

            Color color = Color.FromArgb(circle.R, circle.G, circle.B);

            if (methodCircuitNum == 0)
            {
                double step = 1.0 / radiusInPixels; // Шаг зависит от радиуса
                // Алгоритм закраски в полярных координатах
                for (double theta = 0; theta < 2 * Math.PI; theta += step)
                {
                    int x = (int)(radiusInPixels * Math.Cos(theta));
                    int y = (int)(radiusInPixels * Math.Sin(theta));
                    SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                }
            }
            else if (methodCircuitNum == 1)
            {
                // Алгоритм закраски на основе уравнения окружности
                for (int x = -radiusInPixels; x <= radiusInPixels; x++)
                {
                    int y = (int)Math.Round(Math.Sqrt(radiusInPixels * radiusInPixels - x * x));
                    SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                    SetPixelBlock(cx + x * pixelSize, cy - y * pixelSize, color, pixelSize);
                }
            }
            else if (methodCircuitNum == 2)
            {
                // Алгоритм Брезенхема для окружности
                int x = 0;
                int y = radiusInPixels;
                int d = 3 - 2 * radiusInPixels;

                while (x <= y)
                {
                    SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                    SetPixelBlock(cx - x * pixelSize, cy + y * pixelSize, color, pixelSize);
                    SetPixelBlock(cx + x * pixelSize, cy - y * pixelSize, color, pixelSize);
                    SetPixelBlock(cx - x * pixelSize, cy - y * pixelSize, color, pixelSize);
                    SetPixelBlock(cx + y * pixelSize, cy + x * pixelSize, color, pixelSize);
                    SetPixelBlock(cx - y * pixelSize, cy + x * pixelSize, color, pixelSize);
                    SetPixelBlock(cx + y * pixelSize, cy - x * pixelSize, color, pixelSize);
                    SetPixelBlock(cx - y * pixelSize, cy - x * pixelSize, color, pixelSize);

                    x++;
                    if (d <= 0)
                    {
                        d = d + 4 * x + 6;
                    }
                    else
                    {
                        y--;
                        d = d + 4 * (x - y) + 10;
                    }
                }
            }

            stopwatch.Stop();
            long tickDrawCircle = stopwatch.ElapsedTicks;
            return tickDrawCircle;
        }

        private (long tickFillCircle, int pixelCount) FillCircle(Circle circle, PaintEventArgs e)
        {
            pixelCount = 0; // Сброс счётчика пикселей перед началом закраски
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            if (methodPaintingNum == 0)
            {
                FillCircleSeed4Connected(circle, e);
            }
            else if (methodPaintingNum == 1)
            {
                FillCircleRowByRow(circle, e); // Построчное заполнение
            }
            else if (methodPaintingNum == 2)
            {
                FillCircleStackModified8Connected(circle, e); // Модифицированный стэковый алгоритм для 8-связной области
            }

            stopwatch2.Stop();
            long tickFillCircle = stopwatch2.ElapsedTicks;
            return (tickFillCircle, pixelCount);
        }

        private void FillCircleSeed4Connected(Circle circle, PaintEventArgs e)
        {
            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;
            double unitScale = 1; // Коэффициент для перевода условных единиц в пиксели
            int radiusInPixels = (int)(circle.Radius * unitScale);

            int cx = centerX + (int)(circle.X * unitScale * pixelSize);
            int cy = centerY - (int)(circle.Y * unitScale * pixelSize); // Инвертируем Y для корректной ориентации

            // Проверка, чтобы начальная точка закраски находилась в пределах изображения
            if (cx < 0 || cx >= bitmap.Width || cy < 0 || cy >= bitmap.Height)
            {
                return; // Прекращаем выполнение, если начальная точка вне изображения
            }

            // Определение старого цвета в затравочной точке
            Color oldColor = bitmap.GetPixel(cx, cy);
            Color targetColor = Color.FromArgb(circle.R, circle.G, circle.B); // Новый цвет заливки

            // Если старый цвет совпадает с целевым, то закраска уже выполнена, и выходим
            if (oldColor.ToArgb() == targetColor.ToArgb()) return;

            // Запуск рекурсивной функции для заполнения области
            Fill(cx, cy, radiusInPixels, oldColor, targetColor, cx, cy, e);
        }

        private void Fill(int x, int y, int radiusInPixels, Color oldColor, Color targetColor, int cx, int cy, PaintEventArgs e)
        {
            // Проверка, чтобы не выйти за границы изображения
            if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height) return;

            // Проверка, чтобы пиксель находился в пределах окружности
            if (Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2) > radiusInPixels * radiusInPixels) return;

            // Проверка, что цвет текущего пикселя совпадает со старым цветом
            if (bitmap.GetPixel(x, y).ToArgb() != oldColor.ToArgb()) return;

            // Закрашиваем текущий пиксель
            SetPixelBlock(x, y, targetColor, pixelSize);

            if (testPlay)
            {
                e.Graphics.DrawImage(bitmap, 0, 0);
                Thread.Sleep(15);
            }

            // Рекурсивный вызов для 4-связных соседей
            Fill(x + 1, y, radiusInPixels, oldColor, targetColor, cx, cy, e); // Правый сосед
            Fill(x - 1, y, radiusInPixels, oldColor, targetColor, cx, cy, e); // Левый сосед
            Fill(x, y + 1, radiusInPixels, oldColor, targetColor, cx, cy, e); // Нижний сосед
            Fill(x, y - 1, radiusInPixels, oldColor, targetColor, cx, cy, e); // Верхний сосед
        }




        private void FillCircleStackModified8Connected(Circle circle, PaintEventArgs e)
        {
            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;
            double unitScale = 1;
            int radiusInPixels = (int)(circle.Radius * unitScale);

            int cx = centerX + (int)(circle.X * unitScale * pixelSize);
            int cy = centerY - (int)(circle.Y * unitScale * pixelSize);

            // Проверяем, находится ли точка (cx, cy) в пределах изображения
            if (cx < 0 || cx >= bitmap.Width || cy < 0 || cy >= bitmap.Height)
            {
                return; // Если начальная точка за пределами изображения, выходим из метода
            }

            Color oldColor = bitmap.GetPixel(cx, cy);
            Color targetColor = Color.FromArgb(circle.R, circle.G, circle.B);

            if (oldColor.ToArgb() == targetColor.ToArgb()) return;

            Stack<Point> stack = new Stack<Point>();
            stack.Push(new Point(cx, cy));

            while (stack.Count > 0)
            {
                Point point = stack.Pop();
                int x = point.X;
                int y = point.Y;

                // Проверка, чтобы не выйти за границы изображения
                if (x < 0 || x >= bitmap.Width || y < 0 || y >= bitmap.Height) continue;

                // Проверяем цвет текущего пикселя; если это старый цвет, то закрашиваем
                if (bitmap.GetPixel(x, y).ToArgb() == oldColor.ToArgb())
                {
                    // Закрашиваем текущий пиксель
                    SetPixelBlock(x, y, targetColor, pixelSize);

                    if (testPlay)
                    {
                        e.Graphics.DrawImage(bitmap, 0, 0);
                        Thread.Sleep(15);
                    }

                    // Проверка на границу радиуса круга, чтобы не выходить за его пределы
                    if (Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2) <= radiusInPixels * radiusInPixels)
                    {
                        // Добавляем соседние пиксели в стек для проверки
                        stack.Push(new Point(x + 1, y));    // Вправо
                        stack.Push(new Point(x - 1, y));    // Влево
                        stack.Push(new Point(x, y + 1));    // Вних
                        stack.Push(new Point(x, y - 1));    // Вверх
                        stack.Push(new Point(x + 1, y + 1)); // Вверх вправо
                        stack.Push(new Point(x - 1, y + 1)); // Вверх влево
                        stack.Push(new Point(x + 1, y - 1)); // Вних вправо
                        stack.Push(new Point(x - 1, y - 1)); // Вниз влево
                    }
                }
            }
        }


        private void FillCircleRowByRow(Circle circle, PaintEventArgs e)
        {
            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;
            double unitScale = 1; // Коэффициент для перевода условных единиц в пиксели
            int radiusInPixels = (int)(circle.Radius * unitScale);

            int cx = centerX + (int)(circle.X * unitScale * pixelSize);
            int cy = centerY - (int)(circle.Y * unitScale * pixelSize); // инвертируем Y для корректной ориентации
            Color color = Color.FromArgb(circle.R, circle.G, circle.B);

            for (int y = -radiusInPixels; y <= radiusInPixels; y++)
            {
                int dx = (int)Math.Round(Math.Sqrt(radiusInPixels * radiusInPixels - y * y));
                for (int x = -dx; x <= dx; x++)
                {
                    SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);

                    if (testPlay)
                    {
                        e.Graphics.DrawImage(bitmap, 0, 0);
                        Thread.Sleep(15);
                    }
                }
            }
        }
        private int pixelCount = 0; // Счетчик закрашенных пикселей
        private void SetPixelBlock(int x, int y, Color color, int size)
        {
            for (int dx = 0; dx < size; dx++)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < bitmap.Width && py >= 0 && py < bitmap.Height)
                    {
                        bitmap.SetPixel(px, py, color);
                        pixelCount++; // Увеличиваем счетчик при закраске пикселя
                    }
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    pixelSize = Math.Min(pixelSize + 1, 5);
                    break;
                case Keys.Down:
                    pixelSize = Math.Max(pixelSize - 1, 1);
                    break;
                case Keys.W:
                    offsetY += 10; // Двигаемся вверх
                    break;
                case Keys.S:
                    offsetY -= 10; // Двигаемся вниз
                    break;
                case Keys.A:
                    offsetX += 10; // Двигаемся влево
                    break;
                case Keys.D:
                    offsetX -= 10; // Двигаемся вправо
                    break;
                case Keys.E:
                    if (pixelSize == 5) showGrid = !showGrid; // Включение/выключение сетки
                    break;
                case Keys.R:
                    if (!showCoordGrid)
                        showCoordGrid = true;
                    else
                        showCoordGrid = false;
                    break;
                case Keys.O:
                    if (!backInfo)
                        backInfo = true;
                    else
                        backInfo = false;
                    break;
                case Keys.B:
                    methodPaintingNum = (methodPaintingNum + 1) % 3; 
                    break;
                case Keys.V:
                    currentCircleIndex = (currentCircleIndex + 1) % circles.Length; // Переход к следующей окружности
                    break;
                case Keys.T:
                    if (!testPlay)
                        testPlay = true;
                    else
                        testPlay = false;
                    break;
            }
            Invalidate();
        }

        private class Circle
        {
            public double X { get; }
            public double Y { get; }
            public double Radius { get; }
            public int R { get; }
            public int G { get; }
            public int B { get; }

            public Circle(double x, double y, double radius, int r, int g, int b)
            {
                X = x;
                Y = y;
                Radius = radius;
                R = r;
                G = g;
                B = b;
            }
        }
    }
}