using System.Net.Sockets;
using System.Text;

namespace FieldForTanks
{
    public partial class fieldForm : Form
    {

        private const string IP_SERVER_ADDR = "127.0.0.1";
        private const int PORT_SERVER_ADDR = 4000;

        private Socket socket;
        private int numberPlayer = 0;
        private userTank_1 tank_1; // Оголошуємо танк для гравця 1
        private userTank_2 tank_2; // Оголошуємо танк для гравця 2

        private bool handlingKeyDown = false; // Флаг для відстеження обробки події KeyDown

        public fieldForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += fieldForm_KeyDown; // Додаємо обробник події KeyDown до форми
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializePlayer();


            tank_1 = new userTank_1(); // Ініціалізуємо танк для гравця 1
            tank_2 = new userTank_2(); // Ініціалізуємо танк для гравця 2

            fieldPanel.Controls.Add(tank_1);  //  на фрму FieldForm на панель fieldPanel треба додати пользовательский єлемент Елемент userTank_1
            tank_1.Location = new System.Drawing.Point(0, 200);

            fieldPanel.Controls.Add(tank_2);
            tank_2.Location = new System.Drawing.Point(770, 200);
                        
            this.Focus();


        }

        private async Task InitializePlayer()
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(IP_SERVER_ADDR, PORT_SERVER_ADDR);

                // Отримання номера гравця
                string receivedPlayerNumber = await ReceiveDataAsync(socket);

                if (int.TryParse(receivedPlayerNumber, out int playerNumber))
                {
                    numberPlayer = playerNumber;
                    Text = (numberPlayer == 1) ? "Гравець 1" : "Гравець 2";
                    MessageBox.Show($"Гравець {numberPlayer}, ви підключились до гри.");

                }
                else
                {
                    MessageBox.Show($"Вибачте, до сервера вже підключено 2 гравці. Спробуйте пізніше.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка підключення до сервера: {ex.Message}");
            }
        }

        // метод отримання повідомлення
        private async Task<string> ReceiveDataAsync(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            return Encoding.Unicode.GetString(buffer, 0, bytesRead);
        }

        // метод переміщення танчика по панелі

        //  обробник подій для натискання клавіш на клавіатурі
        private async void fieldForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (handlingKeyDown) return; // Якщо подія вже обробляється, ігноруємо нову подію
            handlingKeyDown = true; // Встановлюємо флаг обробки події

            char direction = (char)e.KeyCode;
            //MessageBox.Show($"Key pressed: {direction}");

            if (numberPlayer == 1)
            {
                await MoveTank1Async(direction, tank_1);
            }
            else if (numberPlayer == 2)
            {
                await MoveTank2Async(direction, tank_2);
            }

            handlingKeyDown = false; // Знімаємо флаг обробки події
        }

        // метод переміщення танчика 1 по панелі
        private async Task MoveTank1Async(char direction, userTank_1 tank_1)
        {
            try
            {
                // Визначаємо різницю у координатах для переміщення танка
                int deltaX = 0, deltaY = 0;

                switch (direction)
                {
                    case 'W': deltaY = -10; break; // рух вгору
                    case 'S': deltaY = 10; break;  // рух вниз
                    case 'A': deltaX = -10; break; // рух вліво
                    case 'D': deltaX = 10; break;  // рух вправо
                    default: return;
                }

                // Отримуємо поточні координати танка
                Point currentLocation = tank_1.Location;

                // Обчислюємо нові координати танка
                Point newLocation = new Point(currentLocation.X + deltaX, currentLocation.Y + deltaY);

                // Оновлюємо локацію танка на формі
                tank_1.Location = newLocation;

                // Відправляємо нові координати на сервер
                string message = $"{direction}{newLocation.X},{newLocation.Y}";
                byte[] buffer = Encoding.Unicode.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                // Отримуємо рух танка противника
                await ReceiveOpponentTankMovement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при переміщенні танка: {ex.Message}");
            }
        }

        // метод переміщення танчика 2 по панелі
        private async Task MoveTank2Async(char direction, userTank_2 tank_2)
        {
            try
            {
                // Визначаємо різницю у координатах для переміщення танка
                int deltaX = 0, deltaY = 0;

                switch (direction)
                {
                    case 'I': deltaY = -10; break; // рух вгору
                    case 'K': deltaY = 10; break;  // рух вниз
                    case 'J': deltaX = -10; break; // рух вліво
                    case 'L': deltaX = 10; break;  // рух вправо
                    default: return;
                }

                // Отримуємо поточні координати танка
                Point currentLocation = tank_2.Location;

                // Обчислюємо нові координати танка
                Point newLocation = new Point(currentLocation.X + deltaX, currentLocation.Y + deltaY);

                // Оновлюємо локацію танка на формі
                tank_2.Location = newLocation;

                // Відправляємо нові координати на сервер (потрібно реалізувати)
                string message = $"{direction.ToString()},{newLocation.X},{newLocation.Y}";
                byte[] buffer = Encoding.Unicode.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                // Отримуємо рух танка противника
                await ReceiveOpponentTankMovement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при переміщенні танка: {ex.Message}");
            }
        }

        private async Task ReceiveOpponentTankMovement()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);

                // Отримання даних про рух танка противника з повідомлення
                

                string[] parts = message.Split(',');
                string direction = parts[0];
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);

                // Оновлення положення танка противника на формі
                if (numberPlayer == 1)
                {
                    tank_2.Location = new Point(x, y);
                }
                else if (numberPlayer == 2)
                {
                    tank_1.Location = new Point(x, y);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при отриманні руху танка противника: {ex.Message}");
            }
        }





    }
}
