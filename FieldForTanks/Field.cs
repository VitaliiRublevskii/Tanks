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
        private userTank_1 tank_1; // ��������� ���� ��� ������ 1
        private userTank_2 tank_2; // ��������� ���� ��� ������ 2

        private bool handlingKeyDown = false; // ���� ��� ���������� ������� ��䳿 KeyDown

        public fieldForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.KeyDown += fieldForm_KeyDown; // ������ �������� ��䳿 KeyDown �� �����
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializePlayer();


            tank_1 = new userTank_1(); // ���������� ���� ��� ������ 1
            tank_2 = new userTank_2(); // ���������� ���� ��� ������ 2

            fieldPanel.Controls.Add(tank_1);  //  �� ���� FieldForm �� ������ fieldPanel ����� ������ ���������������� ������� ������� userTank_1
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

                // ��������� ������ ������
                string receivedPlayerNumber = await ReceiveDataAsync(socket);

                if (int.TryParse(receivedPlayerNumber, out int playerNumber))
                {
                    numberPlayer = playerNumber;
                    Text = (numberPlayer == 1) ? "������� 1" : "������� 2";
                    MessageBox.Show($"������� {numberPlayer}, �� ����������� �� ���.");

                }
                else
                {
                    MessageBox.Show($"�������, �� ������� ��� ��������� 2 ������. ��������� �����.");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ���������� �� �������: {ex.Message}");
            }
        }

        // ����� ��������� �����������
        private async Task<string> ReceiveDataAsync(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            return Encoding.Unicode.GetString(buffer, 0, bytesRead);
        }

        // ����� ���������� ������� �� �����

        //  �������� ���� ��� ���������� ����� �� ��������
        private async void fieldForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (handlingKeyDown) return; // ���� ���� ��� ������������, �������� ���� ����
            handlingKeyDown = true; // ������������ ���� ������� ��䳿

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

            handlingKeyDown = false; // ������ ���� ������� ��䳿
        }

        // ����� ���������� ������� 1 �� �����
        private async Task MoveTank1Async(char direction, userTank_1 tank_1)
        {
            try
            {
                // ��������� ������ � ����������� ��� ���������� �����
                int deltaX = 0, deltaY = 0;

                switch (direction)
                {
                    case 'W': deltaY = -10; break; // ��� �����
                    case 'S': deltaY = 10; break;  // ��� ����
                    case 'A': deltaX = -10; break; // ��� ����
                    case 'D': deltaX = 10; break;  // ��� ������
                    default: return;
                }

                // �������� ������ ���������� �����
                Point currentLocation = tank_1.Location;

                // ���������� ��� ���������� �����
                Point newLocation = new Point(currentLocation.X + deltaX, currentLocation.Y + deltaY);

                // ��������� ������� ����� �� ����
                tank_1.Location = newLocation;

                // ³���������� ��� ���������� �� ������
                string message = $"{direction}{newLocation.X},{newLocation.Y}";
                byte[] buffer = Encoding.Unicode.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                // �������� ��� ����� ����������
                await ReceiveOpponentTankMovement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ��� ��������� �����: {ex.Message}");
            }
        }

        // ����� ���������� ������� 2 �� �����
        private async Task MoveTank2Async(char direction, userTank_2 tank_2)
        {
            try
            {
                // ��������� ������ � ����������� ��� ���������� �����
                int deltaX = 0, deltaY = 0;

                switch (direction)
                {
                    case 'I': deltaY = -10; break; // ��� �����
                    case 'K': deltaY = 10; break;  // ��� ����
                    case 'J': deltaX = -10; break; // ��� ����
                    case 'L': deltaX = 10; break;  // ��� ������
                    default: return;
                }

                // �������� ������ ���������� �����
                Point currentLocation = tank_2.Location;

                // ���������� ��� ���������� �����
                Point newLocation = new Point(currentLocation.X + deltaX, currentLocation.Y + deltaY);

                // ��������� ������� ����� �� ����
                tank_2.Location = newLocation;

                // ³���������� ��� ���������� �� ������ (������� ����������)
                string message = $"{direction.ToString()},{newLocation.X},{newLocation.Y}";
                byte[] buffer = Encoding.Unicode.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                // �������� ��� ����� ����������
                await ReceiveOpponentTankMovement();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������� ��� ��������� �����: {ex.Message}");
            }
        }

        private async Task ReceiveOpponentTankMovement()
        {
            try
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                string message = Encoding.Unicode.GetString(buffer, 0, bytesRead);

                // ��������� ����� ��� ��� ����� ���������� � �����������
                

                string[] parts = message.Split(',');
                string direction = parts[0];
                int x = int.Parse(parts[0]);
                int y = int.Parse(parts[1]);

                // ��������� ��������� ����� ���������� �� ����
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
                MessageBox.Show($"������� ��� �������� ���� ����� ����������: {ex.Message}");
            }
        }





    }
}
