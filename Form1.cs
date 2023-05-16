using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace Self_Driving
{
    public partial class Form1 : Form
    {

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private int slidePosition;
        private int carY;
        private int carSpeed;
        private const int MaxSpeed = 10;
        private const int Acceleration = 5;
        private const int Deceleration = 6;
        private bool accelerating = false;
        private bool braking = false;
        // AUTOPILOT CAR
        private int startDistance;
        private int followingDistance;
        private int selfdriveY;
        private int selfdriveSpeed;
        private int selfdriveX;

        // addtl cars
        private int car1Y;
        private int car2Y;
        private int car3Y;
        private int car4Y;
        private int car5Y;

        private int[,] cars = new int[6, 2];


        public Form1()
        {
            InitializeComponent();
            timer.Interval = 100;
            timer.Tick += timer1_Tick;
            carY = pictureBox1.Height / 2 - 200;
            carSpeed = 0;
            // AUTOPILOT CAR
            followingDistance = 60;
            startDistance = -90;
            selfdriveY = pictureBox1.Height / 2 + startDistance;
            selfdriveX = pictureBox1.Width / 2 - 20;
            // addtl cars
            car1Y = pictureBox1.Height / 2 + 0;
            car2Y = pictureBox1.Height / 2 + 100;
            car3Y = pictureBox1.Height / 2 + 0;
            car4Y = pictureBox1.Height / 2 + -150;
            car5Y = pictureBox1.Height / 2 + -250;


            int midLane = pictureBox1.Width / 2 - 20;
            cars[0, 0] = midLane;
            cars[0, 1] = carY;
            cars[1, 0] = midLane - 60;
            cars[1, 1] = car1Y;
            cars[2, 0] = midLane - 60;
            cars[2, 1] = car2Y;
            cars[3, 0] = midLane + 60;
            cars[3, 1] = car3Y;
            cars[4, 0]  = midLane + 60;
            cars[4, 1] = car4Y;
            cars[5, 0] = midLane - 60;
            cars[5, 1] = car5Y;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // autopilot laser
            DrawLaser(e.Graphics);

            // Create a Pen object with the desired color and dash style
            Pen pen = new Pen(Color.Black, 2);
            pen.DashStyle = DashStyle.Dash;

            // Set the gap between the lines and the length of each line
            pen.DashPattern = new float[] { 10, 5 };

            // Calculate the center of the PictureBox control
            int centerX = pictureBox1.Width / 2;

            // Draw vertical dotted lines
            for (int x = centerX - 100; x <= centerX + 100; x += 60)
            {
                e.Graphics.DrawLine(pen, x, slidePosition % 60, x, pictureBox1.Height + slidePosition % 60);
            }

            // Get the current position of the car in front
            int carX = pictureBox1.Width / 2 - 20;


            // Draw the car in front
            e.Graphics.FillRectangle(Brushes.Red, carX, carY, 25, 50);


            // AUTOPILOT CAR

            e.Graphics.FillRectangle(Brushes.Blue, selfdriveX, selfdriveY, 25, 50);


            // other cars on the side lanes
            e.Graphics.FillRectangle(Brushes.Red, carX - 60, car1Y, 25, 50);
            e.Graphics.FillRectangle(Brushes.Red, carX - 60, car2Y, 25, 50);
            e.Graphics.FillRectangle(Brushes.Red, carX + 60, car3Y, 25, 50);
            e.Graphics.FillRectangle(Brushes.Red, carX + 60, car4Y, 25, 50);
            e.Graphics.FillRectangle(Brushes.Red, carX - 60, car5Y, 25, 50);
        }

        private const int LaserLength = 200;
        private const int LaserWidth = 2;
        private const int LaserOffsetX = 11;
        private const int LaserOffsetY = 50;

        private bool isObstacleDetected = false;
        private int obstacleDistance = -1;

        private bool leftBlocked = false;
        private bool rightBlocked = false;

        private Color laserColor = Color.Blue;
        private void DrawLaser(Graphics g)
        {
            int carX = selfdriveX;
            int startX = carX + LaserOffsetX;
            int startY = selfdriveY + LaserOffsetY;
            int endX = startX;
            int endY = startY - LaserLength;
            Pen laserPen = new Pen(laserColor, LaserWidth);
            g.DrawLine(laserPen, startX, startY, endX, endY);

            // Check if there is an obstacle in front
            Rectangle laserRect = new Rectangle(startX, endY, LaserWidth, LaserLength);

            Rectangle adjacentCheckLeft = new Rectangle(selfdriveX - 40, selfdriveY - 20, LaserWidth, LaserLength/2);
            Rectangle adjacentCheckRight = new Rectangle(selfdriveX + 65, selfdriveY - 20, LaserWidth, LaserLength / 2);
            g.DrawRectangle(laserPen, adjacentCheckLeft);
            g.DrawRectangle(laserPen, adjacentCheckRight);

            for (int i = 0; i < cars.GetLength(0); i++)
            {
                Rectangle obstacleRect = new Rectangle(cars[i, 0], cars[i, 1], 25, 50);
                if (adjacentCheckLeft.IntersectsWith(obstacleRect))
                {
                    leftBlocked = true;
                    label9.Text = "blocked lane";
                    break;
                }
                else
                {
                    leftBlocked = false;
                    label9.Text = "";
                }
            }
            for (int i = 0; i < cars.GetLength(0); i++)
            {
                Rectangle obstacleRect = new Rectangle(cars[i, 0], cars[i, 1], 25, 50);
                if (adjacentCheckRight.IntersectsWith(obstacleRect))
                {
                    rightBlocked = true;
                    label10.Text = "blocked lane";
                    break;
                }
                else
                {
                    rightBlocked = false;
                    label10.Text = "";
                }
            }
            

            isObstacleDetected = false;
            int index = -1;
            for(int i = 0; i < cars.GetLength(0); i++)
            {
                Rectangle obstacleRect = new Rectangle(cars[i, 0], cars[i, 1], 25, 200);
                if(laserRect.IntersectsWith(obstacleRect))
                {
                    // CASE 1 : The car is the only car that has been detected
                    isObstacleDetected = true;
                    if(index == -1)
                    {
                        index = i;
                        obstacleDistance = endY - (cars[index, 1] + 50) + 150;
                    }
                    
                    else
                    {
                        // CASE 2 : The car is the second car that is detected and it is closer than the first
                        if(endY - (cars[index, 1] + 50) + 150 < obstacleDistance)
                        {
                            obstacleDistance = endY - (cars[index, 1] + 50) + 150;
                            index = i;
                        }
                        // CASE 3 : The car is the second car that is detected and it is further than the first
                        else if(obstacleDistance < endY - (cars[index, 1] + 50) + 150)
                        {
                            break;
                        }

                    }
                    

                    
                }
            }


            Rectangle obstacleRect2 = new Rectangle(cars[0, 0], cars[0, 1], 25, 60);
            //System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.LightSeaGreen);
            //g.FillRectangle(myBrush, obstacleRect2);

            if (isObstacleDetected)
            {
                label4.Text = "Obstacle Detected: True";
                label5.Enabled = true;
                label5.Text = "Distance to Object: " + obstacleDistance;
                if(obstacleDistance < 0)
                {
                    richTextBox1.Text += "CRASHED!";
                }
            }
            else
            {
                obstacleDistance = -1;
                label4.Text = "Obstacle Detected: False";
                label5.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            slidePosition += 5;
            if (accelerating)
            {
                carSpeed = Math.Min(MaxSpeed, carSpeed + Acceleration);
            }
            else if (braking)
            {
                carSpeed = Math.Max(-MaxSpeed, carSpeed - Deceleration);
            }
            if (carSpeed > 0)
            {
                carY -= carSpeed;
                cars[0,1] = carY;
                carSpeed = Math.Max(0, carSpeed - Deceleration);
            }
            else if (carSpeed < 0)
            {
                carY -= carSpeed;
                cars[0, 1] = carY;
                carSpeed = Math.Min(0, carSpeed + Deceleration);
            }
            pictureBox1.Invalidate();

            // AUTOPILOT LOGIC / SELF DRIVE FUNCTION

            // if too close, start breaking
            if (obstacleDistance < followingDistance && isObstacleDetected)
            {
                selfdriveY += Deceleration;
            }
            // if no car within following distance, accelerate
            if (obstacleDistance > followingDistance)
            {
                selfdriveY -= Acceleration;
            }
        }
        // start and stop buttons
        private void startButton_Click(object sender, EventArgs e)
        {
            timer.Start();
            pictureBox1.Invalidate();
        }
        private void stopButton_Click_1(object sender, EventArgs e)
        {
            timer.Stop();
            pictureBox1.Invalidate();
        }
        // accelerate and brake buttons
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void accelerateButton_Click(object sender, EventArgs e)
        {
        }

        private void accelerateButton_MouseDown(object sender, MouseEventArgs e)
        {
            accelerating = true;
            richTextBox1.AppendText("\ntarget: accelerating");
            
        }

        private void accelerateButton_MouseUp(object sender, MouseEventArgs e)
        {
            accelerating = false;
        }

        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            braking = true;
            richTextBox1.AppendText("\ntarget: braking");
            
        }

        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            braking = false;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void followingDistanceSetter_Scroll(object sender, EventArgs e)
        {
            label7.Text = followingDistanceSetter.Value.ToString();
            followingDistance = followingDistanceSetter.Value * 10;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                laserColor = Color.Blue;
            }
            else
            {
                laserColor = Color.Transparent;
            }
        }

        private int currentLane = 2;

        private void button2_Click(object sender, EventArgs e) //left lane change
        {
            if(currentLane != 1)
            {
                if (!leftBlocked)
                {
                    currentLane--;
                    selfdriveX -= 60;
                }
                else
                {
                    richTextBox1.AppendText("\nleft lane blocked, cannot change");
                }
                
            }
        }

        private void button3_Click(object sender, EventArgs e) //right lane change
        {
            if(currentLane != 3)
            {
                if (!rightBlocked)
                {
                    currentLane++;
                    selfdriveX += 60;
                }
                else
                {
                    richTextBox1.AppendText("\nright lane blocked, cannot change");
                }
                
            }
        }
    }
}