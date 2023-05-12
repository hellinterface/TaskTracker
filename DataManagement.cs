using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Documents;
using System.Data.Common;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;

namespace TaskTracker
{

    public interface IPageWithUserSelect
    {
        void SetUsersFromUserSelectPage(UserSelectPage page);
        TextBox UserListTextBox { get; set; }
    }

    // Класс "Пользователь"
    public class OBJ_User
    {
        public string Username { get; init; }
        public string Password { get; set; }

        public OBJ_User()
        {
        }
        public OBJ_User(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
    // Класс "Доска"
    public class OBJ_Board
    {
        public string ID { get; init; } = Utilities.GetRandomString(6);
        public string Title { get; set; } = "Новая доска";
        public OBJ_User Owner { get; init; }
        public List<OBJ_User> UsersCanView { get; set; } = new List<OBJ_User>();
        public List<OBJ_User> UsersCanEdit { get; set; } = new List<OBJ_User>();

        public OBJ_Board()
        {
        }
    }
    // Класс "Столбец"/"Колонка"
    public class OBJ_Column
    {
        public string ID { get; init; } = Utilities.GetRandomString(6);
        public int Position { get; set; }
        public string Title { get; set; } = "Новый столбец";
        public List<OBJ_Card> Cards { get; set; } = new List<OBJ_Card>();
        public List<string> CardIDs
        {
            get // выдает id карточек, которые есть в столбце
            {
                List<string> result = new List<string>();
                foreach (var card in Cards) result.Add(card.ID);
                return result;
            }
        }

        public OBJ_Column()
        {
        }
    }
    // Класс "Карточка"
    public class OBJ_Card
    {
        public string ID { get; init; } = Utilities.GetRandomString(6);
        public int Position { get; set; }
        public OBJ_User Owner { get; init; }
        public List<OBJ_User> UsersCanEdit { get; set; } = new List<OBJ_User>();
        public string Title { get; set; } = "Новая карточка";
        public string Description { get; set; } = "";
        public System.Windows.Media.Color Color { get; set; } = System.Windows.Media.Color.FromArgb(255, 160, 160, 160);
        public List<OBJ_Image> Images { get; set; } = null;
        public List<string> ImageIDs
        {
            get
            {
                List<string> result = new List<string>();
                if (Images != null) foreach (OBJ_Image image in Images) result.Add(image.ID);
                return result;
            }
        }
        public List<OBJ_Task> Tasks { get; set; } = null;
        public List<string> TaskIDs { 
            get
            {
                List<string> result = new List<string>();
                if (Tasks != null) foreach (OBJ_Task task in Tasks) result.Add(task.ID);
                return result;
            }
        }

        public OBJ_Card()
        {
        }
    }
    // Класс "Задача" (в списке задач)
    public class OBJ_Task
    {
        public string ID { get; init; } = Utilities.GetRandomString(6);
        public string Text { get; set; } = "Новая задача";
        public bool Done { get; set; } = false;

        public OBJ_Task()
        {
        }
    }
    // Класс "Изображение"
    public class OBJ_Image
    {
        public string ID { get; init; } = Utilities.GetRandomString(6);
        public BitmapImage BitmapImage { get; protected set; }// само изображение
        private string base64;
        public string Base64 { // закодированное изображение
            get
            {
                return base64;
            }
            set
            {
                byte[] binaryData = Convert.FromBase64String(value);
                BitmapImage = new BitmapImage();
                BitmapImage.BeginInit();
                BitmapImage.StreamSource = new MemoryStream(binaryData);
                BitmapImage.EndInit();
                base64 = value;
            }
        }

        public OBJ_Image()
        {
        }
    }

    static class Utilities
    {
        // Сгенерировать случайную строку заданной длины.
        // нужно для идентификаторов столбцов
        public static string GetRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();
            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new String(stringChars);
        }
        public static bool ContainsBannedCharacters(string text)
        {
            if (text.Contains("|") ||
                text.Contains("¶") ||
                text.Contains("¿") ||
                text.Contains("->"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    static class SocketClient
    {
        static Socket sender;
        static byte[] bytes = new byte[(1024 * 1024 * 8)];

        public static void StartClient()
        {
            try
            {
                IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 8090);
                sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEP);
                    byte[] msg = Encoding.UTF8.GetBytes("ONLINE¿");
                    int bytesSent = sender.Send(msg);
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}", Encoding.UTF8.GetString(bytes, 0, bytesRec));
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public static void SocketClose()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
        public static string Send(string contentToSend)
        {
            if (contentToSend[contentToSend.Length - 1] != '¿') contentToSend += '¿';
            byte[] msg = Encoding.UTF8.GetBytes(contentToSend);
            int bytesSent = sender.Send(msg);
            bytes = new byte[(1024 * 1024 * 8)];
            int bytesRec = sender.Receive(bytes);
            string recivedString = Encoding.UTF8.GetString(bytes, 0, bytesRec).Replace("¿", "");
            return recivedString;
        }
    }

    // Класс "Таблица базы данных"
    class DatabaseTable
    {
        // Заголовок
        public List<string> Header = new List<string>();
        // Содержимое (строки)
        public List<List<string>> Table = new List<List<string>>();

        // Получить значение ячейки на указанной строке в столбце с указанным названием заголовка
        public string GetAt(int rowIndex, string columnTargetValue)
        {
            int columnIndex = Header.FindIndex(cell => cell.ToLower() == columnTargetValue.ToLower());
            if (columnIndex < 0) return null;
            return Table[rowIndex][columnIndex];
        }

        public DatabaseTable(string[] rows, int correctLength = -1)
        {
            if (rows.Length != correctLength && correctLength > 0)
            {
                throw new Exception();
            }
            Header = rows[0].Split('|').ToList();
            for (int i = 1; i < rows.Length; i++)
            {
                Table.Add(rows[i].Split('|').ToList());
            }
        }

        public DatabaseTable(string str, int correctLength = -1) : this(str.Split('\n'), correctLength)
        {

        }
    }

    // Связь с базой данных/сервером
    static class DatabaseCommunicator
    {

        private static string MakeQuery(IEnumerable<string> components)
        {
            return String.Join('¶', components);
        }

        // Получить пользователей с укзанным одним критерием
        public static OBJ_User[] GET_Users(string criteria)
        {
            string recieved = SocketClient.Send($"DB¶users¶GET¶{criteria}");
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_User[0];
            List<OBJ_User> users = new List<OBJ_User>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                OBJ_User currentUser = new OBJ_User()
                {
                    Username = table.GetAt(i, "username"),
                    Password = table.GetAt(i, "password")
                };
                users.Add(currentUser);
            }
            return users.ToArray();
        }
        // Получить пользователей с укзанными критериями
        public static OBJ_User[] GET_Users(string[] criteria)
        {
            return GET_Users(MakeQuery(criteria));
        }
        // Получить доски с укзанными критериями
        public static OBJ_Board[] GET_Boards(string criteria)
        {
            string recieved = SocketClient.Send(MakeQuery(new string[]{ "DB", "boards", "GET", criteria}));
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Board[0];
            var boards = new List<OBJ_Board>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                var usersCanViewList = new List<OBJ_User>();
                foreach (var username in table.GetAt(i, "users_can_view").Split(',').ToList().FindAll(e => e != ""))
                {
                    if (username == "*")
                    {
                        usersCanViewList.Add(Application.Current.Properties["AllUsers"] as OBJ_User);
                        continue;
                    }
                    OBJ_User tempUser = DatabaseCommunicator.GET_Users($"username|=|{username}")[0];
                    usersCanViewList.Add(tempUser);
                }
                var usersCanEditList = new List<OBJ_User>();
                foreach (var username in table.GetAt(i, "users_can_edit").Split(',').ToList().FindAll(e => e != ""))
                {
                    if (username == "*")
                    {
                        usersCanEditList.Add(Application.Current.Properties["AllUsers"] as OBJ_User);
                        continue;
                    }
                    OBJ_User tempUser = DatabaseCommunicator.GET_Users($"username|=|{username}")[0];
                    usersCanEditList.Add(tempUser);
                }
                OBJ_Board currentBoard = new OBJ_Board()
                {
                    ID = table.GetAt(i, "id"),
                    Title = table.GetAt(i, "title"),
                    Owner = DatabaseCommunicator.GET_Users($"username|=|{table.GetAt(i, "owner")}")[0],
                    UsersCanView = usersCanViewList,
                    UsersCanEdit = usersCanEditList
                };
                boards.Add(currentBoard);
            }
            return boards.ToArray();
        }
        // Получить столбцы с укзанными критериями
        public static OBJ_Column[] GET_Columns(string boardID, string criteria)
        {
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{boardID}_columns", "GET", criteria }));
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Column[0];
            var columns = new List<OBJ_Column>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                var cards = new List<OBJ_Card>();
                foreach (var cardID in table.GetAt(i, "card_ids").Split(',').ToList()) {
                    OBJ_Card[] recievedCards = GET_Cards(boardID, $"id|=|{cardID}");
                    if (recievedCards != null && recievedCards.Length == 1) cards.Add(recievedCards[0]);
                }
                cards = cards.OrderBy(card => card.Position).ToList();
                OBJ_Column currentColumn = new OBJ_Column()
                {
                    ID = table.GetAt(i, "id"),
                    Position = int.Parse(table.GetAt(i, "position")),
                    Title = table.GetAt(i, "title"),
                    Cards = cards
                };
                columns.Add(currentColumn);
            }
            return columns.ToArray();
        }
        // Получить карточки с укзанными критериями
        public static OBJ_Card[] GET_Cards(string boardID, string criteria)
        {
            // Запрос в БД
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{boardID}_cards", "GET", criteria }));
            // Конвертация в таблицу
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Card[0];
            // Цикл: конвертация строк таблицы в объекты карточек (OBJ_Card)
            var cards = new List<OBJ_Card>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                // Составляем список пользователей
                var usersCanEditList = new List<OBJ_User>();
                foreach (var username in table.GetAt(i, "users_can_edit").Split(',').ToList().FindAll(e => e != "")) // Фильтруем, чтобы пустых строк не было, а то всё полетит
                {
                    // Запрос с пользователем в БД и добавление в список card.UsersCanEdit
                    if (username == "*")
                    {
                        usersCanEditList.Add(Application.Current.Properties["AllUsers"] as OBJ_User);
                        continue;
                    }
                    OBJ_User tempUser = DatabaseCommunicator.GET_Users($"username|=|{username}")[0];
                    usersCanEditList.Add(tempUser);
                }
                // Создаем новый объект карточки
                OBJ_Card currentCard = new OBJ_Card()
                {
                    ID = table.GetAt(i, "id"),
                    Position = int.Parse(table.GetAt(i, "position")),
                    Owner = DatabaseCommunicator.GET_Users($"username|=|{table.GetAt(i, "owner")}")[0],
                    UsersCanEdit = usersCanEditList,
                    Title = table.GetAt(i, "title"),
                    Description = table.GetAt(i, "description"),
                    Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(table.GetAt(i, "color")),
                    Tasks = null // Не будем заранее подгружать то, что нам не нужно 
                    //= table.GetAt(i, "task_ids").Split(',').ToList().FindAll(e => e != "")
                };
                cards.Add(currentCard);
            }
            return cards.ToArray();
        }
        public static OBJ_Task[] GET_TasksFromCard(OBJ_Board board, OBJ_Card card)
        {
            // Запрос в БД
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "GET", $"id|=|{card.ID}" }));
            // Конвертация в таблицу
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Task[0];
            var resultList = new List<OBJ_Task>();
            foreach (var taskID in table.GetAt(0, "task_ids").Split(',').ToList().FindAll(e => e != ""))
            {
                OBJ_Task[] recievedTasks = GET_Tasks(board.ID, $"id|=|{taskID}");
                if (recievedTasks.Length != 0) resultList.Add(recievedTasks[0]);
            }
            return resultList.ToArray();
        }
        // Получить задачи с укзанными критериями
        public static OBJ_Task[] GET_Tasks(string boardID, string criteria)
        {
            // Запрос в БД
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{boardID}_tasks", "GET", criteria }));
            // Конвертация в таблицу
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Task[0];
            var resultList = new List<OBJ_Task>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                bool isDone = true;
                if (table.GetAt(i, "done") == "0") isDone = false;
                else isDone = true;
                // Создаем новый объект карточки
                OBJ_Task currentTask = new OBJ_Task()
                {
                    ID = table.GetAt(i, "id"),
                    Text = table.GetAt(i, "text"),
                    Done = isDone
                };
                resultList.Add(currentTask);
            }
            return resultList.ToArray();
        }
        public static OBJ_Image[] GET_ImagesFromCard(OBJ_Board board, OBJ_Card card)
        {
            // Запрос в БД
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "GET", $"id|=|{card.ID}" }));
            // Конвертация в таблицу
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Image[0];
            var resultList = new List<OBJ_Image>();
            foreach (var imageID in table.GetAt(0, "images").Split(',').ToList().FindAll(e => e != ""))
            {
                OBJ_Image[] recievedImages = GET_Images(board.ID, $"id|=|{imageID}");
                if (recievedImages.Length != 0) resultList.Add(recievedImages[0]);
            }
            return resultList.ToArray();
        }
        public static OBJ_Image[] GET_Images(string boardID, string criteria)
        {
            // Запрос в БД
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{boardID}_images", "GET", criteria }));
            // Конвертация в таблицу
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Image[0];
            var resultList = new List<OBJ_Image>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                // Создаем новый объект карточки
                OBJ_Image currentImage = new OBJ_Image()
                {
                    ID = table.GetAt(i, "id"),
                    Base64 = table.GetAt(i, "base64")
                };
                resultList.Add(currentImage);
            }
            return resultList.ToArray();
        }

        public static bool ADD_User(OBJ_User user)
        {
            if (user == null) { return false; }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                user.Username,
                user.Password,
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "users", "ADD", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        // Создать новый файл в базе данных с указанным названием и заголовком (названиями столбцов)
        public static bool ADD_Board(OBJ_Board board)
        {
            if (board == null) { return false; }
            // Создание списка имён пользователей с доступом на просмотр доски
            List<string> usersCanViewUsernames = new List<string>();
            foreach (var user in board.UsersCanView) { usersCanViewUsernames.Add(user.Username); }
            // Создание списка имён пользователей с доступом на редактирование доски
            List<string> usersCanEditUsernames = new List<string>();
            foreach (var user in board.UsersCanEdit) { usersCanEditUsernames.Add(user.Username); }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                board.ID,
                board.Title,
                board.Owner.Username,
                String.Join(',', usersCanViewUsernames),
                String.Join(',', usersCanEditUsernames)
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "boards", "ADD", tempString }));
            if (recieved == "<OK>") // Доска добавилась в таблицу досок
            {
                // Отправляем запросы на создание файлов с карточками, столбцами и задачами новой доски
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_CREATE", $"{board.ID}_cards", "id|position|owner|users_can_edit|title|description|color|images|task_ids" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_CREATE", $"{board.ID}_columns", "id|position|title|card_ids" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_CREATE", $"{board.ID}_tasks", "id|text|done" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_CREATE", $"{board.ID}_images", "id|base64" }));
                return true;
            }
            else return false;
        }

        public static bool ADD_Column(OBJ_Board board, OBJ_Column column)
        {
            if (column == null) { return false; }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                column.ID,
                column.Position.ToString(),
                column.Title,
                String.Join(',', column.CardIDs)
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_columns", "ADD", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        // Добавить карточку в базу указанной колонки
        public static bool ADD_Card(OBJ_Board board, OBJ_Column column, OBJ_Card card)
        {
            if (card == null) { return false; }
            // Создание списка имён пользователей с доступом на редактирование карточки
            List<string> usersCanEditUsernames = new List<string>();
            foreach (var user in card.UsersCanEdit) { usersCanEditUsernames.Add(user.Username); }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                card.ID,
                card.Position.ToString(),
                card.Owner.Username,
                String.Join(',', usersCanEditUsernames),
                card.Title,
                card.Description,
                new System.Windows.Media.ColorConverter().ConvertToString(card.Color),
                String.Join(',', card.Images),
                String.Join(',', card.TaskIDs)
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "ADD", tempString }));
            if (recieved == "<OK>") // Карточка добавилась в файл с карточками
            {
                // Добавляем ID карточки в столбец в файле со столбцами
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_columns", "SET", $"{column.ID}->card_ids->{String.Join(',', column.CardIDs)}" }));
                if (recieved == "<OK>") return true;
                else
                {
                    // Если вторая операция не вышла, то отменяем первую
                    recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "DEL", card.ID }));
                    return false;
                }
            }
            else return false;
        }

        // Добавить задачу к указанной карточке
        public static bool ADD_Task(OBJ_Board board, OBJ_Card card, OBJ_Task task)
        {
            // Делаем массив с компонентами запроса в БД
            int isDone;
            if (task.Done == true) isDone = 1;
            else isDone = 0;
            List<string> tempStringComponents = new List<string>
            {
                task.ID,
                task.Text,
                isDone.ToString()
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_tasks", "ADD", tempString }));
            if (recieved == "<OK>") // Карточка добавилась в файл с карточками
            {
                // Добавляем ID карточки в столбец в файле со столбцами
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "SET", $"{card.ID}->task_ids->{String.Join(',', card.TaskIDs)}" }));
                if (recieved == "<OK>") return true;
                else
                {
                    // Если вторая операция не вышла, то отменяем первую
                    recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_tasks", "DEL", task.ID }));
                    return false;
                }
            }
            else return false;
        }

        // Добавить изображение к указанной карточке
        public static bool ADD_Image(OBJ_Board board, OBJ_Card card, OBJ_Image image)
        {
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                image.ID,
                image.Base64
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_images", "ADD", tempString }));
            if (recieved == "<OK>") // Карточка добавилась в файл с карточками
            {
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "SET", $"{card.ID}->images->{String.Join(',', card.ImageIDs)}" }));
                if (recieved == "<OK>") return true;
                else
                {
                    // Если вторая операция не вышла, то отменяем первую
                    recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_images", "DEL", image.ID }));
                    return false;
                }
            }
            else return false;
        }

        // ADD_Task

        // Поставить указанные значения в указанное местопросмотр
        private static bool SET(string filename, string id, string column, string newValue)
        {
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", filename, "SET", $"{id}->{column}->{newValue}" }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        public static bool UPDATE_Board(OBJ_Board board)
        {
            // Создание списка имён пользователей с доступом на просмотр доски
            List<string> usersCanViewUsernames = new List<string>();
            foreach (var user in board.UsersCanView) { usersCanViewUsernames.Add(user.Username); }
            // Создание списка имён пользователей с доступом на редактирование доски
            List<string> usersCanEditUsernames = new List<string>();
            foreach (var user in board.UsersCanEdit) { usersCanEditUsernames.Add(user.Username); }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                $"{board.ID}->title->{board.Title}",
                $"{board.ID}->owner->{board.Owner.Username}",
                $"{board.ID}->users_can_view->{String.Join(',', usersCanViewUsernames)}",
                $"{board.ID}->users_can_edit->{String.Join(',', usersCanEditUsernames)}"
            };
            // Соединяем всё воедино
            string tempString = String.Join('¶', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "boards", "SET", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        public static bool UPDATE_Column(OBJ_Board board, OBJ_Column column)
        {
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                $"{column.ID}->position->{column.Position}",
                $"{column.ID}->title->{column.Title}",
                $"{column.ID}->card_ids->{String.Join(',', column.CardIDs)}"
            };
            // Соединяем всё воедино
            string tempString = String.Join('¶', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_columns", "SET", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        public static bool UPDATE_Card(OBJ_Board board, OBJ_Card card)
        {
            // Создание списка имён пользователей с доступом на редактирование карточки
            List<string> usersCanEditUsernames = new List<string>();
            foreach (var user in card.UsersCanEdit) { usersCanEditUsernames.Add(user.Username); }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                $"{card.ID}->position->{card.Position}",
                $"{card.ID}->owner->{card.Owner.Username}",
                $"{card.ID}->users_can_edit->{String.Join(',', usersCanEditUsernames)}",
                $"{card.ID}->title->{card.Title}",
                $"{card.ID}->description->{card.Description}",
                $"{card.ID}->color->{new System.Windows.Media.ColorConverter().ConvertToString(card.Color)}"
            };
            if (card.Images != null) tempStringComponents.Add($"{card.ID}->images->{String.Join(',', card.ImageIDs)}");
            if (card.Tasks != null) tempStringComponents.Add($"{card.ID}->task_ids->{String.Join(',', card.TaskIDs)}");
            // Соединяем всё воедино
            string tempString = String.Join('¶', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "SET", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        public static bool UPDATE_Task(OBJ_Board board, OBJ_Task task)
        {
            // Делаем массив с компонентами запроса в БД
            int isDone;
            if (task.Done == true) isDone = 1;
            else isDone = 0;
            List<string> tempStringComponents = new List<string>
            {
                $"{task.ID}->text->{task.Text}",
                $"{task.ID}->done->{isDone}"
            };
            // Соединяем всё воедино
            string tempString = String.Join('¶', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_tasks", "SET", tempString }));
            if (recieved == "<OK>") return true;
            else return false;
        }

        public static bool DEL_Board(OBJ_Board board)
        {
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "boards", "DEL", board.ID }));
            if (recieved == "<OK>")
            {
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_DELETE", $"{board.ID}_columns" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_DELETE", $"{board.ID}_cards" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_DELETE", $"{board.ID}_tasks" }));
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", "DB_DELETE", $"{board.ID}_images" }));
                return true;
            }
            else return false;
        }
        public static bool DEL_Column(OBJ_Board board, OBJ_Column column)
        {
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_columns", "DEL", column.ID }));
            if (recieved == "<OK>")
            {
                foreach (var card in column.Cards)
                {
                    column.Cards.Remove(card);
                    DEL_Card(board, column, card);
                }
                return true;
            }
            else return false;
        }
        public static bool DEL_Card(OBJ_Board board, OBJ_Column column, OBJ_Card card)
        {
            List<string> cardIDs = column.CardIDs.FindAll(id => id != card.ID);
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_columns", "SET", $"{column.ID}->card_ids->{String.Join(',', cardIDs)}" }));
            if (recieved == "<OK>")
            {
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "DEL", card.ID }));
                foreach (var task in card.Tasks)
                {
                    card.Tasks.Remove(task);
                    DEL_Task(board, card, task);
                }
                foreach (var image in card.Images)
                {
                    card.Images.Remove(image);
                    DEL_Image(board, card, image);
                }
                return true;
            }
            else return false;
        }
        public static bool DEL_Task(OBJ_Board board, OBJ_Card card, OBJ_Task task)
        {
            List<string> taskIDs = card.TaskIDs.FindAll(id => id != task.ID);
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "SET", $"{card.ID}->task_ids->{String.Join(',', taskIDs)}" }));
            if (recieved == "<OK>")
            {
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_tasks", "DEL", task.ID }));
                return true;
            }
            else return false;
        }
        public static bool DEL_Image(OBJ_Board board, OBJ_Card card, OBJ_Image image)
        {
            List<string> imageIDs = card.ImageIDs.FindAll(id => id != image.ID);
            string recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_cards", "SET", $"{card.ID}->task_ids->{String.Join(',', imageIDs)}" }));
            if (recieved == "<OK>")
            {
                recieved = SocketClient.Send(MakeQuery(new string[] { "DB", $"{board.ID}_images", "DEL", image.ID }));
                return true;
            }
            else return false;
        }

        private static string[] KEYS(string filename)
        {
            return SocketClient.Send($"DB¶{filename}¶KEYS").Split('\n');
        }
        public static string[] KEYS_Users()
        {
            return KEYS("users");
        }
        public static string[] KEYS_Boards()
        {
            return KEYS("boards");
        }
        public static string[] KEYS_Columns(OBJ_Board board)
        {
            return KEYS($"{board.ID}_columns");
        }
        public static string[] KEYS_Cards(OBJ_Board board)
        {
            return KEYS($"{board.ID}_cards");
        }
        public static string[] KEYS_Tasks(OBJ_Board board)
        {
            return KEYS($"{board.ID}_tasks");
        }
    }
}
