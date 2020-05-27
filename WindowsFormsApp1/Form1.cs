using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Snake snake;
        private Food food;
        private float gameSpeed = 7;

        public Form1()
        {
            InitializeComponent();
            snake = new Snake();
            timer1.Interval = (int)TimeSpan.FromSeconds(1.0 / gameSpeed).TotalMilliseconds;

            food = new Food(new Rectangle(100,100,snake.RectangleWidth,snake.RectangleHeight));

            this.KeyDown += OnKeyDown;
            this.Paint += OnPaint;
            timer1.Start();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    snake.Direction = Direction.Left;
                    break;
                case Keys.Right:
                    snake.Direction = Direction.Right;
                    break;
                case Keys.Up:
                    snake.Direction = Direction.Up;
                    break;
                case Keys.Down:
                    snake.Direction = Direction.Down;
                    break;
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            using (var g = e.Graphics)
            {
                g.FillRectangle(food.FoodBrush,food.Rectangle);

                foreach (var rect in snake.Body)
                {
                    g.FillRectangle(snake.BodyBrush,rect);
                    g.DrawRectangle(Pens.Black,rect);
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CheckSnakeCollisions();
            snake.Update();
            this.Invalidate();
        }

        private void CheckSnakeCollisions()
        {
            CheckFoodCollisions();
            CheckSnakeCollisionsWithBorder();
            CheckSnakeCollisionsWithItself();
        }

        private void CheckSnakeCollisionsWithItself()
        {
            var snakeHead = snake.Body.Last();
            var bodyWithoutHead = snake.Body.Take(snake.Body.Length - 1);
            if(bodyWithoutHead.Any(rect => snakeHead.IntersectsWith(rect)))
                GameOver();
        }

        private void CheckSnakeCollisionsWithBorder()
        {
            var snakeHead = snake.Body[snake.Body.Length - 1];

            if (snakeHead.X + snakeHead.Width > ClientRectangle.Width) GameOver();
            else if(snakeHead.X < 0) GameOver();
            else if (snakeHead.Y + snakeHead.Height > ClientRectangle.Height) GameOver();
            else if (snakeHead.Y < 0) GameOver();
        }

        private void GameOver()
        {
            timer1.Stop();
            MessageBox.Show("Game over!");

            snake = new Snake();

            food = new Food(new Rectangle(100, 100, snake.RectangleWidth, snake.RectangleHeight));
            timer1.Start();
        }

        private void CheckFoodCollisions()
        {
            if (snake.Body.Any(rect => food.Rectangle.IntersectsWith(rect))) // Checking and handling food collisions
            {
                MakeNewFood();
                snake.Grow();
                IncreaseGameSpeed();
            }
        }

        private void IncreaseGameSpeed()
        {
            gameSpeed += 0.3f;
            timer1.Interval = (int)TimeSpan.FromSeconds(1.0 / gameSpeed).TotalMilliseconds;
        }

        private void MakeNewFood()
        {
            while(snake.Body.Any(rect => food.Rectangle.IntersectsWith(rect)))
            {
                var random = new Random();

                int x = random.Next(ClientRectangle.Width - food.Rectangle.Width);
                int y = random.Next(ClientRectangle.Height - food.Rectangle.Height);


                food = new Food(new Rectangle(x, y, food.Rectangle.Width, food.Rectangle.Height));
            }
        }
    }

    public class Food
    {
        public Rectangle Rectangle { get; }

        public Food(Rectangle rectangle) => Rectangle = rectangle;

        public Brush FoodBrush => Brushes.Blue;
    }

    public class Snake
    {
        public Rectangle[] Body { get; private set; }

        public int RectangleWidth => 20;
        public int RectangleHeight => 20;

        public Direction Direction { get; set; } = Direction.Right;

        public Snake()
        {
            Body = new Rectangle[7];
            (int x, int y) startingPoint = (10, 10);
            for(int i = 0;i < Body.Length;i++)
                Body[i] = new Rectangle(startingPoint.x + ((i + 1) * RectangleWidth),startingPoint.y,RectangleWidth,RectangleHeight);
        }

        public Brush BodyBrush => Brushes.Red;

        public void Update()
        {
            for (int i = 0; i < Body.Length - 1; i++)
            {
                Body[i].X = Body[i + 1].X;
                Body[i].Y = Body[i + 1].Y;
            }

            switch (Direction)
            {
                case Direction.Left:
                    Body[Body.Length - 1].X -= RectangleWidth;
                    break;
                case Direction.Right:
                    Body[Body.Length - 1].X += RectangleWidth;
                    break;
                case Direction.Up:
                    Body[Body.Length - 1].Y -= RectangleHeight;
                    break;
                case Direction.Down:
                    Body[Body.Length - 1].Y += RectangleHeight;
                    break;
            }
        }

        public void Grow()
        {
            var oldBody = Body;
            Body = new Rectangle[Body.Length+1];
            Array.Copy(oldBody,Body,oldBody.Length);

            var oldHead = oldBody[oldBody.Length - 1];

            Body[Body.Length - 1].Width = RectangleWidth;
            Body[Body.Length - 1].Height = RectangleHeight;

            if (Direction == Direction.Left)
            {
                Body[Body.Length - 1].X = oldHead.X - RectangleWidth;
                Body[Body.Length - 1].Y = oldHead.Y;
            }
            else if (Direction == Direction.Right)
            {
                Body[Body.Length - 1].X = oldHead.X + RectangleWidth;
                Body[Body.Length - 1].Y = oldHead.Y;
            }
            else if (Direction == Direction.Up)
            {
                Body[Body.Length - 1].X = oldHead.X;
                Body[Body.Length - 1].Y = oldHead.Y - RectangleHeight;
            }
            else if (Direction == Direction.Down)
            {
                Body[Body.Length - 1].X = oldHead.X;
                Body[Body.Length - 1].Y = oldHead.Y + RectangleHeight;
            }
        }
    }

    public enum Direction
    {
        Up,Down,Left,Right
    }
}
