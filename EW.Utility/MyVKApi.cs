#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EW.Utility
{
    [SuppressMessage("ReSharper", "ImpureMethodCallOnReadonlyValueField")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public sealed class MyVkApi : IDisposable
    {
        private const string ListenUrl = "http://+:80/EngineersWars/";
        private const string ApiUrl = "https://api.vk.com/method/";
        private const string MessageSend = ApiUrl + "messages.send";
        private const string VkApiV = "5.71";

        static public readonly SortedList<int, MyCommand> ApiCommands = new SortedList<int, MyCommand>(MySave.Players.Count);

        //private static readonly DataContractJsonSerializer CallbackSer = new DataContractJsonSerializer(typeof(MyCallbackStruct<MyMessageStruct>));

        static private readonly byte[] OkArray = { 111, 107 };

        static private int _avalableCalls = 20;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HttpListener _httpListener = new HttpListener();
        static public MyVkApi LastApi { get; private set; }

        internal bool IsListening => _httpListener.IsListening;

        public MyVkApi()
        {
            _httpListener.Prefixes.Add(ListenUrl);
            // ReSharper disable once ObjectCreationAsStatement
            //new Timer(stateInfo => _avalableCalls = 20, null, 0, 1000);
            new Thread(Refresh).Start();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Engineers Wars Bot");
        }

        static private void Refresh()
        {
            while (LastApi != null)
            {
                Thread.Sleep(1000);
                _avalableCalls = 20;
            }
        }

        public void SetLastApi() => LastApi = this;

        public void StartListen()
        {
            _httpListener.Start();
            while (_httpListener.IsListening)
            {
                HttpListenerContext info = _httpListener.GetContext();
                new Task(() => UseContext(info)).Start();
            }
        }

        private void UseContext(HttpListenerContext context)
        {
            void Task1()
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.StatusDescription = HttpStatusCode.OK.ToString();
                byte[] text = MySave.BotSettings.ConfirmMode ? Encoding.UTF8.GetBytes(MySave.BotSettings.ConfirmString) : OkArray;
                context.Response.OutputStream.Write(text, 0, text.Length);
                try
                {
                    Console.WriteLine($"{DateTime.Now}: Отправлен ответ \"OK\" ВК");
                }
                catch (IOException)
                {
                }
            }

            void Task2()
            {
                DateTime t1 = DateTime.Now;
                MyCallbackStruct<MyMessageStruct> callback = (MyCallbackStruct<MyMessageStruct>)MyCallbackStruct<MyMessageStruct>.SerializerMessage.ReadObject(context.Request.InputStream);
                if (callback.secret != MySave.BotSettings.SecretCode)
                {
                    Console.WriteLine($"{DateTime.Now}: Неверный секретный код");
                    return;
                }

                if (!ApiCommands.ContainsKey(callback.@object.user_id))
                    ApiCommands.Add(callback.@object.user_id, new MyCommand(callback.@object.user_id));
                string mes = ApiCommands[callback.@object.user_id].ExecuteCommand(callback.@object.body, out string title);
                try
                {
                    SendMessage(callback.@object.user_id, mes, callback.@object.id, title);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e);
                    Console.ForegroundColor = ConsoleColor.Green;
                    throw;
                }

                DateTime t2 = DateTime.Now;
                try
                {
                    Console.WriteLine($"{DateTime.Now}: Затрачено мс: {(t2 - t1).TotalMilliseconds}");
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            Parallel.Invoke(Task1, Task2);
            try
            {
                context.Response.Close();
                context.Request.InputStream.Close();
            }
            catch (Exception)
            {
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{DateTime.Now}: Ошибка при отправке данных в ВК");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }
        }

        public void SendMessage(int vkId, string message, int messId, string title = "")
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (vkId == 0)
            {
                message = message.Replace("♔", "* ");
                message = message.Replace("🗹", "+");
                message = message.Replace("🗷", "—");
                message = message.Replace("　", "  ");
                lock (Console.Out)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    if (!string.IsNullOrWhiteSpace(title)) Console.WriteLine(title);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(message.TrimEnd());
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                return;
            }
            if (message.Length >= 7500)
                throw new ArgumentException("Слишком большая длина сообщения.", nameof(message));
            while (_avalableCalls <= 0)
            {
            }

            IReadOnlyDictionary<string, string> post = new Dictionary<string, string>(6)
                                                       {
                                                           {"v", VkApiV},
                                                           {
                                                               "access_token", MySave.BotSettings.VkToken
                                                           },
                                                           {
                                                               "peer_id", vkId.ToString(CultureInfo.InvariantCulture)
                                                           },
                                                           {"message", message},
                                                           {"title", title},
                                                           {
                                                               "random_id", vkId.ToString(CultureInfo.InvariantCulture) + messId.ToString(CultureInfo.InvariantCulture)
                                                           }
                                                       };
            HttpContent content = new FormUrlEncodedContent(post);
            --_avalableCalls;
            _httpClient.PostAsync(MessageSend, content);
        }

        public void SendMessage(int[] vkIDs, string message, int messId = 0, string title = "")
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            if (message.Length >= 7500)
                throw new ArgumentException("Слишком большая длина сообщения.", nameof(message));
            if (vkIDs.Length == 0) return;
            for (int i = 0; i < vkIDs.Length; i += 100)
            {
                while (_avalableCalls <= 0)
                {
                }

                IReadOnlyDictionary<string, string> post = new Dictionary<string, string>(6)
                                                           {
                                                               {"v", VkApiV},
                                                               {
                                                                   "access_token", MySave.BotSettings.VkToken
                                                               },
                                                               {
                                                                   "user_ids", string.Join(",", vkIDs.Skip(i).Take(100))
                                                               },
                                                               {"message", message},
                                                               {"title", title},
                                                               {
                                                                   "random_id", vkIDs[0].ToString(CultureInfo.InvariantCulture) + messId.ToString(CultureInfo.InvariantCulture)
                                                               }
                                                           };
                HttpContent content = new FormUrlEncodedContent(post);
                _avalableCalls--;
                _httpClient.PostAsync(MessageSend, content);
            }
        }

        public void Dispose()
        {
            (_httpListener as IDisposable).Dispose();
            _httpClient.Dispose();
        }

        [DataContract]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct MyCallbackStruct<T> : IEquatable<MyCallbackStruct<T>>
        {
            static internal readonly DataContractJsonSerializer SerializerMessage = new DataContractJsonSerializer(typeof(MyCallbackStruct<T>), MySave.SerializerSettings);

            [DataMember(Name = "type")] public readonly string type;
            [DataMember(Name = "object")] public readonly T @object;
            [DataMember(Name = "group_id")] public readonly int group_id;
            [DataMember(Name = "secret")] public readonly string secret;

            // ReSharper disable once UnusedMember.Local
            public MyCallbackStruct(string type, T @object, int groupId, string secret)
            {
                this.type = type ?? throw new ArgumentNullException(nameof(type));
                this.@object = @object;
                group_id = groupId;
                this.secret = secret ?? throw new ArgumentNullException(nameof(secret));
            }

            public override bool Equals(object obj) => obj is MyCallbackStruct<T> @struct && Equals(@struct);

            public bool Equals(MyCallbackStruct<T> other) => type == other.type && EqualityComparer<T>.Default.Equals(@object, other.@object) && group_id == other.group_id && secret == other.secret;

            public override int GetHashCode()
            {
                int hashCode = -983184484;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(type);
                hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(@object);
                // ReSharper disable once ImpureMethodCallOnReadonlyValueField
                hashCode = hashCode * -1521134295 + group_id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(secret);
                return hashCode;
            }
        }

        [DataContract]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct MyMessageStruct : IEquatable<MyMessageStruct>
        {
            static internal DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(MyMessageStruct), MySave.SerializerSettings);

            [DataMember(Name = "id")] public readonly int id;
            [DataMember(Name = "user_id")] public readonly int user_id;
            [DataMember(Name = "from_id")] public readonly int from_id;

            [IgnoreDataMember]
            public DateTime Time => date.CreateDateTimeFromUnixtime();

            [DataMember(Name = "date")] public readonly uint date;
            [DataMember(Name = "read_state")] public readonly byte read_state;
            [DataMember(Name = "out")] public readonly byte @out;
            [DataMember(Name = "title")] public readonly string title;
            [DataMember(Name = "body")] public readonly string body;


            public MyMessageStruct(int id, int userId, int fromId, uint date, byte readState, byte @out, string title, string body)
            {
                this.id = id;
                user_id = userId;
                from_id = fromId;
                this.date = date;
                read_state = readState;
                this.@out = @out;
                this.title = title ?? throw new ArgumentNullException(nameof(title));
                this.body = body ?? throw new ArgumentNullException(nameof(body));
            }

            public override bool Equals(object obj) => obj is MyMessageStruct @struct && Equals(@struct);

            public bool Equals(MyMessageStruct other) => id == other.id && user_id == other.user_id && from_id == other.from_id && date == other.date && read_state == other.read_state && @out == other.@out && title == other.title && body == other.body;

            public override int GetHashCode()
            {
                int hashCode = 2029763038;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + id.GetHashCode();
                hashCode = hashCode * -1521134295 + user_id.GetHashCode();
                hashCode = hashCode * -1521134295 + from_id.GetHashCode();
                hashCode = hashCode * -1521134295 + date.GetHashCode();
                hashCode = hashCode * -1521134295 + read_state.GetHashCode();
                hashCode = hashCode * -1521134295 + @out.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(title);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(body);
                return hashCode;
            }
        }
    }
}