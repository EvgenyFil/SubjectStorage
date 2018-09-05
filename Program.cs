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
        // TODO: To think about to deprecate this method
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
        // TODO: To think about to deprecate this
        public static Subject CreateSubject(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday)
        {
            throw new NotImplementedException();
            /*
            Subject newSubject = null;
            string errorText = null;
            if (CheckValues(name, surname, patronymic,
                passportNumber, birthday, out errorText))
            {
                return new Subject(name, surname, patronymic,
                    passportNumber, birthday);
            }
            else
            {
                var exception = new ArgumentException();
                exception.Data.Add("Bad input", errorText);
                throw exception;
            }
            */

        }
        public static Subject CreateSubject(string name, string surname,
            string patronymic, string passportNumber, string birthday)
        {
            return new Subject(name, surname, patronymic, passportNumber, birthday);
        }

        private static bool CheckValues(string name, string surname,
            string patronymic, long passportNumber, DateTime birthday,
            ref List<string> errorMessages)
        {
            if (errorMessages == null)
            {
                errorMessages = new List<string>(4);
            }
            if (!CheckStringValue(name))
            {
                errorMessages.Add("Incorrect first name");
            }
            else if (!CheckStringValue(surname))
            {
                errorMessages.Add("Incorrect surname");
            }
            else if ((patronymic != null) && (patronymic.Length > 0) && !CheckStringValue(patronymic))
            {
                errorMessages.Add("Incorrect patronymic");
            }
            else if ((passportNumber < MinPassportNumber) || (passportNumber > MaxPassportNumber))
            {
                errorMessages.Add("Incorrect passport number");
            }
            
            return errorMessages.Count == 0;
        }

        private Subject(string name, string surname, string patronymic,
            string passportNumberStr, string birthdayStr)
        {
            var errorMessages = new List<string>(6);
            long passportNumber = 0;
            DateTime birthday = new DateTime();
            try
            {
                passportNumber = long.Parse(passportNumberStr);              
            }
            catch (System.Exception)
            {                
                errorMessages.Add("Invalid passport number");
            }
            try
            {
                birthday = DateTime.Parse(birthdayStr, CultureInfo.InvariantCulture);                  
            }
            catch (System.Exception)
            {                
                errorMessages.Add("Invalid birth date");
            }
            if (CheckValues(name, surname, patronymic,
                passportNumber, birthday, ref errorMessages))
            {
                Name = name;
                Surname = surname;
                Patronymic = patronymic;
                PassportNumber = passportNumber;
                Birthday = birthday;
                return;
            }
            if (errorMessages.Count != 0)
            {
                var exception = new ArgumentException();
                foreach (var errorMessage in errorMessages)
                {
                    exception.Data.Add("Bad input", errorMessage);
                }
                throw exception;
            }
        }

        private static bool CheckStringValue(string value)
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
                subject = new Subject(fields[0], fields[1], fields[2],
                    fields[3], fields[4]);
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
    public static class SubjectsToCsvConverter
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
                var field = line.Split(';');
                if ((field != null) && (field.Length == 5))
                {
                    resultSet.Add(Subject.CreateSubject(field[0], field[1], field[2], field[3], field[4]));
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
            var path = @"/Users/estarodubtsev/Documents/Programming/Sharp_solutions/SubjectsStorage/db.txt";
            using (var dataStorage = new DataStorage(path))
            {
                do
                {
                    Console.WriteLine();
                    Console.WriteLine(@"Type '/help' for help or enter some command:");
                    Console.Write(">> ");
                    var inputStr = Console.ReadLine();

                    while (true)
                    {
                        // TODO: Extract method and dictionary with commands
                        switch (inputStr)
                        {
                            case @"/exit":
                                Console.WriteLine("Exit");
                                return;
                            case @"/help":
                                WriteHelp();
                                break;
                            case @"/new":
                                ReadAndSaveSubject(dataStorage);
                                break;
                            case @"/show":
                                foreach (var subj in dataStorage.GetAllSubjects())
                                {
                                    Console.WriteLine(subj);
                                }
                                //Console.WriteLine("Not implemented, please type another command");
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

        private static void ReadAndSaveSubject(DataStorage dataStorage)
        {
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
            try
            {
                Subject nextSubj = Subject.CreateSubject(name, surname, patronymic, passportNumber, birthday);
                dataStorage.PutSubject(nextSubj);
            }
            catch (System.Exception exc)
            {
                Console.WriteLine("Error: {0}", exc.Data["Bad input"]);
            }
        }

        private static void WriteHelp()
        {
            Console.WriteLine(@"/help - this manual");
            Console.WriteLine(@"/exit - close program");
            Console.WriteLine(@"/new  - add new subject");
            Console.WriteLine(@"/show - show subjects from.. to..");
        }
    }
}