
using System.Diagnostics;

//объявляем делегат для вывода сообщений
internal delegate void DelegateMessage(string text);

public class Clock
{
    //конструктор нашего класса принимающий аргументы из командной строки
    public Clock(string NameOfProcess, string LifeTime, string Frequency)
    {
        this.NameOf = NameOfProcess;
        //пытаемся преобразовать из строки в байт
        bool convertResult1 = Byte.TryParse(LifeTime, out this.LifeOf);
        bool convertResult2 = Byte.TryParse(Frequency, out this.FrequencyOf);
        //если успешно
        if (convertResult1 && convertResult2)
        {
            this.InitStream();
            writer.Invoke("Наблюдение запущено успешно.");
            this.Watch();
        }
        //если нет
        else
        {
            writer.Invoke("Введены некорректные данные.");
        }
    }

   //имя искомого процесса
    private string NameOf;
    //сам процесс
    private Process process;
    //массив всех процессов
    private Process[] processes;
    //время жизни процесса
    private byte LifeOf;
    //частота обновления 
    private byte FrequencyOf;
    //экземпляр нашего делегаиа
    private DelegateMessage writer;
    //экземпляр для записи
    private StreamWriter logStream;



    //запись в файл и лог
    private void WriteWithFile(string text)
    {
        Console.WriteLine("{0:t}\t{1}", DateTime.Now, text);
        this.logStream.WriteLine("{0}\t{1}", DateTime.Now, text);
    }

    //только в лог
    private void WriteWithoutFile(string text)
    {
        Console.WriteLine("{0:t}\t{1}", DateTime.Now, text);
    }

    //сам лог
    private void InitStream(string way = "log.txt")
    {
        try
        {
            this.logStream = new StreamWriter(way);
        }
        catch (IOException ex)
        {
            writer = this.WriteWithoutFile;
            writer.Invoke(String.Format("Не удалось создать логфайл: {0}", ex.Message));
        }
        finally
        {
            writer = this.WriteWithFile;
            writer.Invoke("Файл с логами создан.");
        }
    }

   
    private void Watch()
    {
        //ищем процесс
        try
        {
            processes = Process.GetProcessesByName(this.NameOf);
            if (processes.Length == 0)
                writer.Invoke("Процесс не найден.");
            //если нашли, то создаём счётчик наблюдения
            byte watchingMinutes = 0;

            while (processes.Length != 0)
            {
                //если время наблюдения больше временм жизни
                //убиваем процесс
                if (watchingMinutes >= this.LifeOf)
                {
                    this.Kill();
                    writer.Invoke("Процесс завершен принудительно.");
                    break;
                }
                //поток засыпает на время равное частоте
                //приводим к минутам
                Thread.Sleep(60000 * this.FrequencyOf);
                watchingMinutes++;
                writer.Invoke(String.Format("Процесс в работе уже {0} из {1} минут.",
                    watchingMinutes, this.LifeOf));
                processes = Process.GetProcessesByName(this.NameOf);
            }
        }
        //ловим исключение
        catch (Exception ex)
        {
            writer.Invoke(String.Format("Исключение при наблюдении: {0}", ex.Message));
        }
        //завершаем
        finally
        {
            writer.Invoke("Наблюдение завершено");
        }
    }

    //убиваем процесс
    private void Kill()
    {
        try
        {
            //перебираем процессы и находим наш
            foreach (Process item in Process.GetProcessesByName(this.NameOf))
            {
                item.Kill();
            }
        }
        catch(Exception ex)
        {
            writer.Invoke("Исключение при удалении");
        }
      
    }
}
