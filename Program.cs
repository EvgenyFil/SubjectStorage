using System;
using System.Globalization;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace SubjectsStorage
{
    class Program
    {
        class Subject
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Patronymic { get; set; }
            public int PassportSerial { get; set; }
            public int PassportNumber { get; set; }
            public DateTime Birthday { get; set; }

            public static bool TryParse(string str, out Subject subject)
            {
                subject = null;
                var fields = str.Split(';');
                if (fields.Length != 6)
                {
                    return false;
                }
                try
                {
                    subject = new Subject
                    {
                        Name = fields[0],
                        Surname = fields[1],
                        Patronymic = fields[2],
                        PassportSerial = Int32.Parse(fields[3]),
                        PassportNumber = Int32.Parse(fields[4]),
                        Birthday = DateTime.Parse(fields[5], CultureInfo.InvariantCulture)
                    };
                    return true;
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Exception in GetData(): {0}", exc.Message);
                    return false;
                }
            }

            public override string ToString()
            {
                return String.Format("{0};{1};{2};{3};{4};{5}",
                    Name, Surname, Patronymic,
                    PassportSerial.ToString(CultureInfo.InvariantCulture),
                    PassportNumber.ToString(CultureInfo.InvariantCulture),
                    Birthday.ToString(CultureInfo.InvariantCulture));
            }
        }

        class DataProvider : IDisposable
        {
            public DataProvider(string filename)
            {
                try
                {
                    _file = new FileInfo(filename);
                    _stream = _file.Open(FileMode.OpenOrCreate);
                    _sr = new StreamReader(_stream);
                    _sw = new StreamWriter(_stream);
                }
                catch (Exception exc)
                {
                    // TODO: 
                    Console.WriteLine("Exception while creating DataProvider: {0}", exc.Message);
                }
            }

            public void PutSubject(Subject subject)
            {
                _sw.WriteLine(subject);
                _sw.Flush();
            }
            public IEnumerable<Subject> GetData()
            {
                var resultSet = new List<Subject>();
                string line = _sr.ReadLine();
                while (line != null)
                {
                    Subject subj;
                    if (Subject.TryParse(line, out subj))
                    {
                        resultSet.Add(subj);
                    }
                    line = _sr.ReadLine();
                }
                return resultSet;
            }

            public void Dispose()
            {
                // TODO: ??
                _stream.Flush();
                _stream.Close();
            }

            private StreamReader _sr;
            private StreamWriter _sw;
            private Stream _stream;
            private FileInfo _file;
        }

        static void Main()
        {
            var filename = @"/Users/estarodubtsev/Documents/Programming/Sharp_solutions/test2/db.txt";
            using (var dataProvider = new DataProvider(filename))
            {
                Console.WriteLine("db: ");
                foreach (var subj in dataProvider.GetData())
                {
                    Console.WriteLine(subj);
                }
                Console.WriteLine("------------");
                
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Enter new subject's info separated by semicolon like this (or enter /exit):");
                    Console.WriteLine(@"name;surname;patronymic;passport_serial;passport_number;dd/mm/yyyy hh:mm:ss");
                    var inputStr = Console.ReadLine();
                    if (inputStr == @"/exit")
                    {
                        Console.WriteLine("Exit");
                        break;
                    }
                    Subject nextSubj;
                    if (Subject.TryParse(inputStr, out nextSubj))
                    {
                        dataProvider.PutSubject(nextSubj);
                        Console.WriteLine("Subject added");
                    }
                    else
                    {
                        Console.WriteLine("There are some problems while parse string");
                    }
                }
                while (true);
            }
        }
    }
}