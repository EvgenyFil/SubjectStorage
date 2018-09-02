using System;
using System.Globalization;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace SubjectsStorage
{
    public interface IDataStorage
    {
        void PutSubjects(IEnumerable<Subject> subjects);
        IEnumerable<Subject> GetAllSubjects();
    }
    public class Subject
    {
        public static int MaxNameLength = 25;

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Patronymic { get; set; }
        public int PassportNumber { get; set; }
        public DateTime Birthday { get; set; }

        public static Subject CreateSubject(string name, string surname,
            string patronymic, int passportNumber, DateTime birthday)
        {

            return null;
        }
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
                    PassportNumber = Int32.Parse(fields[3]),
                    Birthday = DateTime.Parse(fields[4], CultureInfo.InvariantCulture)
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
            return String.Format("{0};{1};{2};{3};{4}",
                Name, Surname, Patronymic,
                PassportNumber.ToString(CultureInfo.InvariantCulture),
                Birthday.ToString(CultureInfo.InvariantCulture));
        }

        private static bool CheckSubjectName(string value)
        {
            if (value.Length > MaxNameLength)
            {
                return false;
            }
            foreach (var ch in value)
            {
                if (((ch < (int)'А') || (ch > (int)'я'))
                    && ((ch != '-') || (ch != ' ')))
                {
                        return false;
                }
            }
            return true;
        }
    }
    // TODO: Change ToString() method to something else
    public static class SubjectsCsvWriter
    {
        public static void SaveToCsv(string path, IEnumerable<Subject> subjects)
        {
            using (var sw = new StreamWriter(path: path, append: false))
            {
                foreach (var subj in subjects)
                {
                    sw.WriteLine(subj);
                }                
            }
        }
    }
    class DataStorage : IDataStorage, IDisposable
    {
        public DataStorage(string filename)
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

        public void PutSubjects(IEnumerable<Subject> subjects)
        {
            foreach (var subj in subjects)
            {
                _sw.WriteLine(subj);                
            }
            _sw.Flush();
        }
        public IEnumerable<Subject> GetAllSubjects()
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

    class Program
    {

        static void Main()
        {
            var startChar = (int)'А';
            var endChar = (int)'я';
            for (int i = startChar; i <= endChar; i++)
            {
                Console.WriteLine((char)i);
            }
            return;
            var filename = @"/Users/estarodubtsev/Documents/Programming/Sharp_solutions/test2/db.txt";
            using (var dataProvider = new DataStorage(filename))
            {
                Console.WriteLine("db: ");
                foreach (var subj in dataProvider.GetAllSubjects())
                {
                    Console.WriteLine(subj);
                }
                Console.WriteLine("------------");
                
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Type '/help' for help or enter some command:");
                    Console.Write(">> ");
                    var inputStr = Console.ReadLine();

                    while (true)
                    {
                        switch (inputStr)
                        {
                            case @"/exit":
                                Console.WriteLine("Exit");
                                return;
                            case @"/help":
                                Console.WriteLine(@"/help - this manual");
                                Console.WriteLine(@"/exit - close program");
                                Console.WriteLine(@"/new  - add new subject");
                                Console.WriteLine(@"/show - show subjects from.. to");
                                break;
                            case @"/new":
                                Console.WriteLine(@"Enter new subject's info separated by semicolon like this (or enter /exit):");
                                Console.WriteLine(@"name;surname;patronymic;passport_serial;passport_number;dd/mm/yyyy hh:mm:ss");
                                inputStr = Console.ReadLine();
                                Subject nextSubj;
                                if (Subject.TryParse(inputStr, out nextSubj))
                                {
                                    dataProvider.PutSubjects(new [] { nextSubj } );
                                    Console.WriteLine("Subject added");
                                }
                                else
                                {
                                    Console.WriteLine("There are some problems while parse string");
                                }
                                break;
                            default:
                                break;
                        }
                        Console.Write(">> ");
                        inputStr = Console.ReadLine();
                    }

                }
                while (true);
            }
        }
    }
}