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

namespace TaskTracker
{
    // Класс "Пользователь"
    public class OBJ_User
    {
        public string Username { get; set; }
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
        public string ID { get; set; } = Utilities.GetRandomString(6);
        public string Title { get; set; } = "Новая доска";
        public OBJ_User Owner { get; set; }
        public List<OBJ_User> UsersCanView { get; set; } = new List<OBJ_User>();
        public List<OBJ_User> UsersCanEdit { get; set; } = new List<OBJ_User>();
        public OBJ_Board()
        {
        }
    }
    // Класс "Столбец"/"Колонка"
    public class OBJ_Column
    {
        public string ID { get; set; }
        public int Position { get; set; }
        public string Title { get; set; }
        public List<string> CardIDs { get; set; } = null;
        public OBJ_Column()
        {
        }
    }
    // Класс "Карточка"
    public class OBJ_Card
    {
        public string ID { get; set; } = Utilities.GetRandomString(6);
        public int Position { get; set; }
        public OBJ_User Owner { get; set; }
        public List<OBJ_User> UsersCanEdit { get; set; } = new List<OBJ_User>();
        public string Title { get; set; } = "Новая карточка";
        public string Description { get; set; } = "";
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 160, 160, 160);
        public string[] Images { get; set; } = new string[0];
        public List<string> TaskListIDs { get; set; } = new List<string>();
        public List<OBJ_TaskListItem> TaskList { get; set; } = new List<OBJ_TaskListItem>();

        public OBJ_Card()
        {
        }
    }
    // Класс "Задача" (в списке задач)
    public class OBJ_TaskListItem
    {
        public string Title { get; set; }
        public bool Done { get; set; }
        public OBJ_TaskListItem()
        {
        }
    }

    static class Utilities
    {
        // Сгенерировать случайную строку заданной длины.
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
    }

    static class SocketClient
    {
        static Socket sender;
        static byte[] bytes = new byte[1024];

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
            bytes = new byte[1024];
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
        // Получить пользователей с укзанными критериями
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
        // Получить доски с укзанными критериями
        public static OBJ_Board[] GET_Boards(string criteria)
        {
            string recieved = SocketClient.Send($"DB¶boards¶GET¶{criteria}");
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Board[0];
            var boards = new List<OBJ_Board>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                var usersCanViewList = new List<OBJ_User>();
                foreach (var username in table.GetAt(i, "users_can_view").Split(',').ToList().FindAll(e => e != ""))
                {
                    OBJ_User tempUser = DatabaseCommunicator.GET_Users($"username|=|{username}")[0];
                    usersCanViewList.Add(tempUser);
                }
                var usersCanEditList = new List<OBJ_User>();
                foreach (var username in table.GetAt(i, "users_can_edit").Split(',').ToList().FindAll(e => e != ""))
                {
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
            string recieved = SocketClient.Send($"DB¶{boardID}_columns¶GET¶{criteria}");
            var table = new DatabaseTable(recieved);
            if (table.Table.Count == 0) return new OBJ_Column[0];
            var columns = new List<OBJ_Column>();
            for (int i = 0; i < table.Table.Count; i++)
            {
                OBJ_Column currentColumn = new OBJ_Column()
                {
                    ID = table.GetAt(i, "id"),
                    Position = int.Parse(table.GetAt(i, "position")),
                    Title = table.GetAt(i, "title"),
                    CardIDs = table.GetAt(i, "card_ids").Split(',').ToList()
                };
                columns.Add(currentColumn);
            }
            return columns.ToArray();
        }
        // Получить карточки с укзанными критериями
        public static OBJ_Card[] GET_Cards(string boardID, string criteria)
        {
            // Запрос в БД
            string recieved = SocketClient.Send($"DB¶{boardID}_cards¶GET¶{criteria}");
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
                    Color = System.Drawing.ColorTranslator.FromHtml(table.GetAt(i, "color")),
                    TaskListIDs = table.GetAt(i, "task_ids").Split(',').ToList().FindAll(e => e != "")
                };
                cards.Add(currentCard);
            }
            return cards.ToArray();
        }
        // Получить задачи с укзанными критериями
        public static OBJ_TaskListItem[] GET_Tasks(string criteria)
        {
            return new OBJ_TaskListItem[0];
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
            string recieved = SocketClient.Send($"DB¶boards¶ADD¶{tempString}");
            if (recieved == "<OK>") // Доска добавилась в таблицу досок
            {
                // Отправляем запросы на создание файлов с карточками, столбцами и задачами новой доски
                recieved = SocketClient.Send($"DB¶DB_CREATE¶{board.ID}_cards¶id|position|owner|users_can_edit|title|description|color|images|task_ids");
                recieved = SocketClient.Send($"DB¶DB_CREATE¶{board.ID}_columns¶id|position|title|card_ids");
                recieved = SocketClient.Send($"DB¶DB_CREATE¶{board.ID}_tasks¶id|text|done");
                return true;
            }
            else return false;
        }

        // ADD_Column

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
                ColorTranslator.ToHtml(card.Color),
                String.Join(',', card.Images),
                String.Join(',', card.TaskListIDs)
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send($"DB¶{board.ID}_cards¶ADD¶{tempString}");
            if (recieved == "<OK>") // Карточка добавилась в файл с карточками
            {
                // Добавляем ID карточки в столбец в файле со столбцами
                recieved = SocketClient.Send($"DB¶{board.ID}_columns¶SET¶{column.ID}->card_ids->{String.Join(',', column.CardIDs)}");
                if (recieved == "<OK>") return true;
                else
                {
                    // Если вторая операция не вышла, то отменяем первую
                    recieved = SocketClient.Send($"DB¶{board.ID}_cards¶DEL¶{card.ID}");
                    return false;
                }
            }
            else return false;
        }

        // ADD_Task

        // Добавить карточку в базу указанной колонки
        private static bool SET(string filename, string id, string column, string newValue)
        {
            string recieved = SocketClient.Send($"DB¶{filename}¶SET¶{id}->{column}->{newValue}");
            if (recieved == "<OK>") return true;
            else return false;
        }
        
        // UPDATE_Board
        // UPDATE_Column

        public static bool UPDATE_Card(OBJ_Board board, OBJ_Column column, OBJ_Card card)
        {
            // Создание списка имён пользователей с доступом на редактирование карточки
            List<string> usersCanEditUsernames = new List<string>();
            foreach (var user in card.UsersCanEdit) { usersCanEditUsernames.Add(user.Username); }
            // Делаем массив с компонентами запроса в БД
            List<string> tempStringComponents = new List<string>
            {
                $"{card.ID}->position->{card.Position.ToString()}",
                $"{card.ID}->owner->{card.Owner.Username}",
                $"{card.ID}->users_can_edit->{String.Join(',', usersCanEditUsernames)}",
                $"{card.ID}->title->{card.Title}",
                $"{card.ID}->description->{card.Description}",
                $"{card.ID}->color->{ColorTranslator.ToHtml(card.Color)}",
                $"{card.ID}->images->{String.Join(',', card.Images)}",
                $"{card.ID}->task_ids->{String.Join(',', card.TaskListIDs)}"
            };
            // Соединяем всё воедино
            string tempString = String.Join('|', tempStringComponents);
            // Отправляем
            string recieved = SocketClient.Send($"DB¶{board.ID}_cards¶SET¶{tempString}");
            if (recieved == "<OK>") return true;
            else return false;
        }

        // UPDATE_Task

        // DEL_Board
        // DEL_Column
        // DEL_Card
        // DEL_Task
    }
}
