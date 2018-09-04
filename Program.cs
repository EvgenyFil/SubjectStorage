using System;
using System.Globalization;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace SubjectsStorage
{
    public interface IDataStorage
    {
        void PutSubject(Subject subject);
        void PutSubjects(IEnumerable<Subject> subjects);
        IEnumerable<Subject> GetAllSubjects();
    }

    // TODO: To think about using structure instead a class
    public class Subject
    {
        private static readonly int MaxNameLength = 25;
        // Note: First two numbers using for a region's code
        private static readonly long MinPassportNumber = 0100000001;
        private static readonly long MaxPassportNumber = 9999999999;

        public string Name { get; private set; }
        public string Surname { get; private set; }
        public string Patronymic { get; private set; }
        public long PassportNumber { get; private set; }
        public DateTime Birthday { get; private set; }

        // TODO: To think about right way to throw exceptions
        // TODO: To think about using only strings as arguments
        public static Subject CreateSubject(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday)
        {
            Subject newSubject = null;
            string errorText = null;
            if (CheckSubjectValues(name, surname, patronymic,
                passportNumber, birthday, out newSubject, out errorText))
            {
                return new Subject{
                    Name = name,
                    Surname = surname,
                    Patronymic = patronymic,
                    PassportNumber = passportNumber,
                    Birthday = birthday
                };
            }
            else
            {
                var exception = new ArgumentException();
                exception.Data.Add("Bad input", errorText);
                throw exception;
            }

        }

        private static bool CheckSubjectValues(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday,
            out string errorString)
        {
            errorString = null;
            if (!CheckSubjectName(name))
            {
                errorString = "Incorrect first name";
            }
            else if (!CheckSubjectName(surname))
            {
                errorString = "Incorrect surname";
            }
            else if ((patronymic != null) && (patronymic.Length > 0) && !CheckSubjectName(patronymic))
            {
                errorString = "Incorrect patronymic";
            }
            else if ((passportNumber < MinPassportNumber) || (passportNumber > MaxPassportNumber))
            {
                errorString = "Incorrect passport number";
            }
            /*
            else if (birthday == null)
            {
                errorString = "Birthday didn't set";
            }
            */
            if (errorString != null)
            {
                return false;
            }
            return true;
        }

        private Subject()
        {

        }

        private Subject(string name, string surname, string patronymic,
            string passportNumberStr, string birthdayStr) : this()
        {
            string errorString = null;
            long passportNumber;
            DateTime birthday;
            if (!long.TryParse(passportNumberStr, out passportNumber))
            {
                errorString = "Incorrect passport number"; 
            }
            if (!DateTime.TryParse(birthdayStr, out birthday))
            {
                errorString = "Incorrect birth day format";
            }
            if (errorString != null)
            {
                var exception = new ArgumentException();
                exception.Data.Add("Bad input", errorString);
                throw exception;
            }
            
            string errorText = null;
            if (CheckSubjectValues(name, surname, patronymic,
                passportNumber, birthday, out errorText))
            {
                Name = name;
                Surname = surname;
                Patronymic = patronymic;
                PassportNumber = passportNumber;
                Birthday = birthday;
            }
            else
            {
                var exception = new ArgumentException();
                exception.Data.Add("Bad input", errorText);
                throw exception;
            }
        }

        private static bool CheckSubjectName(string value)
        {
            if ((value.Length < 1) || (value.Length > MaxNameLength))
            {
                return false;
            }
            foreach (var ch in value)
            {
                if (((ch < (int)'А') || (ch > (int)'я'))
                    && ((ch != '-') && (ch != ' ')))
                {
                        return false;
                }
            }
            return true;
        }
        // TODO: deprecated, to delete
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

        // TODO: To make output more readable
        public override string ToString()
        {
            return String.Format("{0};{1};{2};{3};{4}",
                Name, Surname, Patronymic,
                PassportNumber.ToString(CultureInfo.InvariantCulture),
                Birthday.ToString(CultureInfo.InvariantCulture));
        }
    }
    // TODO: To change ToString() method to something else
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

        public void PutSubject(Subject subject)
        {
            _sw.WriteLine(subject);
            _sw.Flush();
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

        // TODO: Split Main() into a few methods (in switch clause at least)
        static void Main()
        {
            try
            {
                Console.WriteLine("1: {0}", Subject.CreateSubject("фыв", "йцу", "", 1, new DateTime()));
                Console.WriteLine("2: {0}", Subject.CreateSubject("фыв", "йцу", "й", 1234999999, new DateTime()));
                Console.WriteLine("3: {0}", Subject.CreateSubject("фыв", "йцу", "файацй)й", 1, new DateTime()));
                Console.WriteLine("4: {0}", Subject.CreateSubject("фыв", "йцу", null, 1, new DateTime()));
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Error: {0}", exc.Data["Bad input"]);
            }

            return;
            var path = @"/Users/estarodubtsev/Documents/Programming/Sharp_solutions/test2/db.txt";
            using (var dataProvider = new DataStorage(path))
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
                                Console.WriteLine(@"/show - show subjects from.. to..");
                                break;
                            case @"/new":
                                Console.WriteLine(@"Enter new subject's info separated by semicolon like this (or enter /exit):");
                                Console.WriteLine(@"name;surname;patronymic;passport_number;dd/mm/yyyy hh:mm:ss");
                                inputStr = Console.ReadLine();

                                string name, surname, patronymic, passportNumber, birthday;
                                Console.WriteLine(@"enter name: ");
                                name = Console.ReadLine();
                                Console.WriteLine(@"enter surname: ");
                                surname = Console.ReadLine();
                                Console.WriteLine(@"enter patronymic: ");
                                patronymic = Console.ReadLine();
                                Console.WriteLine(@"enter passport serial and number (without spaces): ");
                                passportNumber = Console.ReadLine();
                                Console.WriteLine(@"enter birth day (dd/mm/yyyy hh:mm:ss): ");
                                birthday = Console.ReadLine();
                                Subject nextSubj;
                                try
                                {

                                    dataProvider.PutSubject();
                                }
                                catch (System.Exception exc)
                                {
                                    Console.WriteLine("Error: {0}", exc.Data["Bad input"]);
                                }
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
                            case @"/show":
                                Console.WriteLine("Not implemented, please type another command");
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