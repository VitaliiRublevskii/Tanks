using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class TanksServer
    {
        private const int PORT_ADDR = 4000;
        private const int MAX_PLAYERS = 2;

        public List<Socket> connectedPlayers = new List<Socket>(); // Список підключених гравців

        private Socket serverSocket;

        int X = 0, Y = 0;
        string moove = "";

        public TanksServer()
        {
            //InitializeField();

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, PORT_ADDR);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen();
            Console.WriteLine("Server started\n");
        }

        // підключаємо гравців
        public async Task Start()
        {
            try
            {
                while (connectedPlayers.Count < MAX_PLAYERS)  //  поки не підключаться 2 гравці - чекаємо нових підключень
                {
                    Socket client = await serverSocket.AcceptAsync();

                    // Додатковий вивід для відстеження приєднання нових гравців
                    Console.WriteLine($"Player connected to the server. Connected players count: {(connectedPlayers.Count) + 1}");
                                        
                    string message = $"{(connectedPlayers.Count) + 1}";
                    byte[] bufferPl = Encoding.Unicode.GetBytes(message);
                    client.Send(bufferPl);
                    Console.WriteLine($"Player {(connectedPlayers.Count) + 1} connected to the server");
                    connectedPlayers.Add(client);


                    
                    
                    //_ = Task.Run(async () =>
                    //{
                    //    await newRoom.StartGame();
                    //    connectedPlayers.Clear();
                    //});

                    //  коли 
                }
                await StartGame();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // виходимо із циклу та починаємо гру
        }

        public async Task StartGame()
        {
            while (connectedPlayers.Count == 2)
            {
                // Початок гри для пари гравців
               

                // Слухаємо 1 гравця
                await ReceiveMovesAsync(connectedPlayers[0]);
                // ВІдправляємо зміни 2 гравцю
                await SendTankUpdateToOpponent(connectedPlayers[1]);

                // Слухаємо 2 гравця
                await ReceiveMovesAsync(connectedPlayers[1]);
                // ВІдправляємо зміни 1 гравцю
                await SendTankUpdateToOpponent(connectedPlayers[0]);

                // Перевіряємо, чи є переможець після ходу першого гравця
                //string winner = CheckWinner();
                //if (winner == "X" || winner == "O")
                //{
                //    Console.WriteLine($"Переможець: {winner}");
                //    await ReSendWinnerAsync(Users[0]);
                //    await ReSendWinnerAsync(Users[1]);
                //    return; // Завершуємо гру після виявлення переможця
                //}


                
            }
        }



        // прийом данних
        public async Task<string> ReceiveDataAsync(Socket socket)
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            return Encoding.Unicode.GetString(buffer, 0, bytesRead);
        }

        // прийом повідомлення від гравця та обробка його
        public async Task ReceiveMovesAsync(Socket socket)
        {
            string receivedData = await ReceiveDataAsync(socket);
            await Console.Out.WriteLineAsync(receivedData);
            await ProcessMove(receivedData);

        }
        // обробка повідомлення
        public async Task ProcessMove(string receivedData)
        {
            
            try
            {
                // Розділити повідомлення на напрямок та нові координати
                string[] parts = receivedData.Split(',');
                string direction = parts[0];
                int newX = int.Parse(parts[0]);
                int newY = int.Parse(parts[1]);
                   
                if (direction == "W" || direction == "S" || direction == "A" || direction == "D")
                {
                    if (direction == "W") Console.WriteLine("Гравець 1 зробил хід ВВЕРХ");
                    if (direction == "S") Console.WriteLine("Гравець 1 зробил хід ВНИЗ");
                    if (direction == "A") Console.WriteLine("Гравець 1 зробил хід НАЗАД");
                    if (direction == "D") Console.WriteLine("Гравець 1 зробил хід ВПЕРЕД");
                }
                else if (direction == "I" || direction == "K" || direction == "J" || direction == "L")
                {
                    if (direction == "I") Console.WriteLine("Гравець 2 зробил хід ВВЕРХ");
                    if (direction == "K") Console.WriteLine("Гравець 2 зробил хід ВНИЗ");
                    if (direction == "J") Console.WriteLine("Гравець 2 зробил хід ВПЕРЕД");
                    if (direction == "L") Console.WriteLine("Гравець 2 зробил хід НАЗАД");
                }
                X = newX; Y = newY; moove = direction;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка обробки повідомлення про рух танка: {ex.Message}");
            }
        }

        // відправка змін іншому гравцю
        public async Task<string> SendTankUpdateToOpponent(Socket socket)
        {
            try
            {
                string reSend = $"{moove},{X},{Y}";
                byte[] bytes = Encoding.Unicode.GetBytes(reSend);
                await socket.SendAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
                return reSend;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка під час відправки повідомлення: {ex.Message}");
                return null; // Або інший спеціальний код помилки, якщо потрібно
            }
        }













        // метод для прийняття повідомлення про хід гравця
        //public void HandleTankMovement(string message)
        //{
        //    try
        //    {
        //        // Розділити повідомлення на напрямок та нові координати
        //        string[] parts = message.Split(',');
        //        char direction = parts[0][0];
        //        int newX = int.Parse(parts[1]);
        //        int newY = int.Parse(parts[2]);

        //        // Знайти танк, який рухається
        //        Tank movingTank = null;
        //        if (direction == 'W' || direction == 'S' || direction == 'A' || direction == 'D')
        //        {
        //            // Гравець 1 рухає свій танк
        //            movingTank = player1Tank;
        //        }
        //        else if (direction == 'I' || direction == 'K' || direction == 'J' || direction == 'L')
        //        {
        //            // Гравець 2 рухає свій танк
        //            movingTank = player2Tank;
        //        }

        //        // Оновити координати танка
        //        if (movingTank != null)
        //        {
        //            movingTank.UpdatePosition(newX, newY);
        //        }

        //        // Відправити оновлену інформацію про танк іншому гравцю
        //        SendTankUpdateToOpponent(message);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Помилка обробки повідомлення про рух танка: {ex.Message}");
        //    }
        //}



    }
}
