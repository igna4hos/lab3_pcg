using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace main
{
    public partial class Form3 : Form
    {
        private readonly Circle[] circles;
        private Bitmap bitmap;

        private int pixelSize = 1; // Масштаб от 1 до 5
        private int offsetX = 0;   // Смещение по X
        private int offsetY = 0;   // Смещение по Y
        private bool showGrid = false; // Флаг для отображения координатной сетки
        private bool showCoordGrid = false;
        private bool backInfo;
        private int methodCircuitNum =  0;
        private int countAngle = 0;
        double[] angle_alpha = { 0, Math.PI / 4 };
        double[] angle_beta = { -Math.PI / 2, -Math.PI / 3 };

        public Form3()
        {
            this.ClientSize = new Size(800, 800); // Размер окна
            this.circles = new Circle[]
            {
                new Circle(0, 0, 5, 100, 180, 50),
                new Circle(0, 0, 3, 100, 180, 50),
            };
            this.Paint += new PaintEventHandler(DrawAllCirclesWithGrid);
            this.KeyDown += new KeyEventHandler(OnKeyDown); // Обработка клавиш
        }

        private void DrawAllCirclesWithGrid(object sender, PaintEventArgs e)
        {
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            double alpha = angle_alpha[countAngle]; // начальный угол в радианах
            double beta = angle_beta[countAngle]; // конечный угол в радианах
            Console.WriteLine($"Количество пикселей: {beta}");
            foreach (var circle in circles)
            {
                var (pixelCount, arcLength) = DrawCircleWithAngle(circle, alpha, beta, e);
                Console.WriteLine($"Количество пикселей: {pixelCount}");
                Console.WriteLine($"Длина дуги: {arcLength} пикселей");
            }
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

        private (long pixelCount, double arcLength) DrawCircleWithAngle(Circle circle, double alpha, double beta, PaintEventArgs e)
        {
            // Приводим углы к радианам
            alpha = alpha % (2 * Math.PI);
            beta = beta % (2 * Math.PI);
            if (alpha > beta)
            {
                double temp = alpha;
                alpha = beta;
                beta = temp;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int centerX = ClientSize.Width / 2 + offsetX * pixelSize;
            int centerY = ClientSize.Height / 2 + offsetY * pixelSize;
            double unitScale = 50; // Коэффициент для перевода условных единиц в пиксели
            int radiusInPixels = (int)(circle.Radius * unitScale);

            int cx = centerX + (int)(circle.X * unitScale * pixelSize);
            int cy = centerY - (int)(circle.Y * unitScale * pixelSize); // инвертируем Y для корректной ориентации

            Color color = Color.FromArgb(circle.R, circle.G, circle.B);

            long pixelCount = 0;

            // Функция для проверки, находится ли угол в диапазоне [alpha, beta]
            bool IsAngleInRange(double angle)
            {
                angle = angle % (2 * Math.PI);
                return (alpha <= angle && angle <= beta);
            }

            if (methodCircuitNum == 0)
            {
                // Метод полярных координат
                double step = 1.0 / radiusInPixels; // Шаг зависит от радиуса
                for (double theta = alpha; theta <= beta; theta += step)
                {
                    int x = (int)(radiusInPixels * Math.Cos(theta));
                    int y = (int)(radiusInPixels * Math.Sin(theta));

                    SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                    pixelCount++;
                    e.Graphics.DrawImage(bitmap, 0, 0);
                }
            }
            else if (methodCircuitNum == 1)
            {
                // Метод на основе уравнения окружности
                for (int x = -radiusInPixels; x <= radiusInPixels; x++)
                {
                    int y = (int)Math.Round(Math.Sqrt(radiusInPixels * radiusInPixels - x * x));

                    double angle1 = Math.Atan2(y, x);
                    double angle2 = Math.Atan2(-y, x);

                    if (IsAngleInRange(angle1))
                    {
                        SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(angle2))
                    {
                        SetPixelBlock(cx + x * pixelSize, cy - y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }
                    e.Graphics.DrawImage(bitmap, 0, 0);
                }
            }
            else if (methodCircuitNum == 2)
            {
                int x = 0;
                int y = radiusInPixels;
                int d = 3 - 2 * radiusInPixels;

                while (x <= y)
                {
                    // Проверка и отрисовка только в пределах [alpha, beta]
                    if (IsAngleInRange(Math.Atan2(y, x)))
                    {
                        SetPixelBlock(cx + x * pixelSize, cy + y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(y, -x)))
                    {
                        SetPixelBlock(cx - x * pixelSize, cy + y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(-y, x)))
                    {
                        SetPixelBlock(cx + x * pixelSize, cy - y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(-y, -x)))
                    {
                        SetPixelBlock(cx - x * pixelSize, cy - y * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(x, y)))
                    {
                        SetPixelBlock(cx + y * pixelSize, cy + x * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(x, -y)))
                    {
                        SetPixelBlock(cx - y * pixelSize, cy + x * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(-x, y)))
                    {
                        SetPixelBlock(cx + y * pixelSize, cy - x * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

                    if (IsAngleInRange(Math.Atan2(-x, -y)))
                    {
                        SetPixelBlock(cx - y * pixelSize, cy - x * pixelSize, color, pixelSize);
                        pixelCount++;
                    }

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
            double arcLength = (beta - alpha) * circle.Radius * unitScale;

            return (pixelCount, arcLength);
        }


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
                case Keys.N:
                    methodCircuitNum = (methodCircuitNum + 1) % 3;
                    break;
                case Keys.M:
                    countAngle = (countAngle + 1) % angle_alpha.Length;
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
