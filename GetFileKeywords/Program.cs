using System;
using System.IO;
using System.Collections.Generic;

namespace GetFileKeywords
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> basicKeywords = new List<string>();
            List<string> specificKeywords = new List<string>();
            string[] files = new string[1];
            string filePath = "";
            string final = "";

        Start: var start = Start();
            if (start == "file")
            {
                filePath = FileLocation();
                if (filePath == string.Empty)
                    goto Start;
                GetType: GetKeywordType(basicKeywords, specificKeywords);
                Choice: int decision = Choice(basicKeywords, specificKeywords);
                if (decision == 1)
                {
                Continue: var result = FileContinue(basicKeywords, specificKeywords, final, filePath);
                    if (result == 0)
                        goto Continue;
                    else
                        goto GetType;
                }
                else if (decision == 0)
                    goto Choice;
                else
                    goto Start;
            }
            else if (start == "folder")
            {
                files = FolderLocation(files);
                if (files == null)
                    goto Start;
                GetType: GetKeywordType(basicKeywords, specificKeywords);
                Choice: int decision = Choice(basicKeywords, specificKeywords);
                if (decision == 1)
                {
                Continue: var result = FolderContinue(basicKeywords, specificKeywords, final, files);
                    if (result == 0)
                        goto Continue;
                    else
                        goto GetType;
                }
                else if (decision == 0)
                    goto Choice;
                else
                    goto Start;
            }
            if (start == string.Empty)
                goto Start;
        }
        public static string Start()
        {
            Console.Write("Would you like to load a file or folder?  ");
            var result = Console.ReadLine().ToLower().ToLower();
            CheckRestart(result);
            CheckExit(result);
            if (result == "file")
                return "file";
            else if (result == "folder")
                return "folder";
            else
                return string.Empty;
        }

        #region FileEvents
        public static string FileLocation()
        {
            Console.Write("Enter the location of the file....  ");
            var filePath = Console.ReadLine().ToLower();
            CheckRestart(filePath);
            CheckExit(filePath);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File does not exist in this location please try again.");
                FileLocation();
            }
            return filePath;
        }
        private static int FileContinue(List<string> bKeywords, List<string> sKeywords, string final, string filePath)
        {
            if (bKeywords.Count == 0 && sKeywords.Count == 0)
            {
                Console.WriteLine("At least one word is required to continue.");
                return -1;
            }
            Console.Write("Enter save path....  ");
            var saveFilePath = Console.ReadLine().ToLower();
            if (File.Exists(saveFilePath))
            {
                Console.WriteLine("Save path is a file, please choose a folder.");
                FileContinue(bKeywords, sKeywords, final, filePath);
                return 0;
            }
            if (Directory.Exists(saveFilePath))
            {
                Console.Write("Enter file name....  ");
                var fileName = Console.ReadLine().ToLower();
                saveFilePath += $"\\{fileName}.txt";
                Console.Write("Saving file....  ");
                FileStream inFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                using (var sr = new StreamReader(inFile))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine().ToLower();
                        if (string.IsNullOrEmpty(line)) continue;
                        for (int i = 0; i < bKeywords.Count; i++)
                        {
                            if (line.IndexOf(bKeywords[i], StringComparison.CurrentCultureIgnoreCase) >= 0)
                                final += Environment.NewLine + line;
                        }
                        for (int i = 0; i < sKeywords.Count; i++)
                        {
                            var specificWord = sKeywords[i].Split(',');
                            if (line.IndexOf(specificWord[0], StringComparison.CurrentCultureIgnoreCase) >= 0)
                            {
                                var result = line.Substring(line.IndexOf(specificWord[0]), int.Parse(specificWord[1]));
                                final += Environment.NewLine + result;
                            }
                        }
                    }
                }
                File.WriteAllText(saveFilePath, final);
                Console.WriteLine("File saved.");
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
                Environment.Exit(0);
                return 1;
            }
            else
            {
                Console.WriteLine("The folder location is incorrect. Try again.");
                FileContinue(bKeywords, sKeywords, final, filePath);
                return 0;
            }
        }
        #endregion

        #region BothEvents
        public static void GetKeywordType(List<string> bKeywords, List<string> sKeywords)
        {
            Console.Write("Would you like to enter basic or specific keyword? \n (basic = grab entire line, specific = grab # of characters)  ");
            var pickWord = Console.ReadLine().ToLower().ToLower();
            CheckRestart(pickWord);
            CheckExit(pickWord);
            if (pickWord == "basic" || pickWord == "b")
                AddBasicKeyword(bKeywords);
            else if (pickWord == "specific" || pickWord == "s")
                AddSpecificKeyword(sKeywords);
            else
                GetKeywordType(bKeywords, sKeywords);
        }
        public static void AddBasicKeyword(List<string> bKeywords)
        {
            Console.Write("Enter a keyword to search for....  ");
            var addBasic = Console.ReadLine().ToLower();
            if (addBasic == "cancel")
                return;
            if (bKeywords.Count == 0)
            {
                bKeywords.Add(addBasic);
                Console.WriteLine($"{addBasic} added to keywords.");
            }
            else
            {
                if (bKeywords.IndexOf(addBasic) != -1)
                {
                    Console.WriteLine($"{addBasic} is already in the list.");
                    AddBasicKeyword(bKeywords);
                }
                bKeywords.Add(addBasic);
                Console.WriteLine($"{addBasic} added to keywords.");
            }
        }
        public static void AddSpecificKeyword(List<string> sKeywords)
        {
            Console.Write("Enter new specific search word....  ");
            var addSpecific = Console.ReadLine().ToLower();
            if (addSpecific == "cancel")
                return;
            Enter: Console.Write("Enter how many characters after word to grab....  ");
            var index = Console.ReadLine().ToLower();
            if (int.TryParse(index, out int result) == false)
            {
                Console.WriteLine("Entry must be a number");
                goto Enter;
            }
            if (sKeywords.IndexOf(addSpecific) != -1)
            {
                Console.WriteLine($"{addSpecific} is already in the list.");
                AddSpecificKeyword(sKeywords);
            }
            if (sKeywords.Count == 0)
            {
                sKeywords.Add($"{addSpecific},{index}");
                Console.WriteLine($"{addSpecific} added to keywords.");
            }
            else
            {
                if (sKeywords.IndexOf(addSpecific) != -1)
                {
                    Console.WriteLine($"{addSpecific} is already in the list.");
                    AddBasicKeyword(sKeywords);
                }
                sKeywords.Add(addSpecific);
                Console.WriteLine($"{addSpecific} added to keywords.");
            }
        }
        public static int Choice(List<string> bKeywords, List<string> sKeywords)
        {
            Console.Write("Would you like to continue, remove keywords, add specific keywords or add basic keywords?  ");
            var fileChoice = Console.ReadLine().ToLower().ToLower();
            CheckRestart(fileChoice);
            CheckExit(fileChoice);
            if (fileChoice == "continue" || fileChoice == "c")
                return 1;
            else if (fileChoice == "remove" || fileChoice == "remove keywords" || fileChoice == "r")
            {
                if (bKeywords.Count == 0 && sKeywords.Count == 0)
                {
                    Console.WriteLine("There are no words to remove, please add a word first.");
                    GetKeywordType(bKeywords, sKeywords);
                    return 0;
                }
                else
                {
                    KeywordsEdit(bKeywords, sKeywords);
                    return 0;
                }
            }
            else if (fileChoice == "add basic" || fileChoice == "add basic keywords" || fileChoice == "abk")
            {
                AddBasicKeyword(bKeywords);
                return 0;
            }
            else if (fileChoice == "add specific" || fileChoice == "add specific keywords" || fileChoice == "ask")
            {
                AddSpecificKeyword(sKeywords);
                return 0;
            }
            else
            {
                Choice(bKeywords, sKeywords);
                return 0;
            }
        }
        private static void KeywordsEdit(List<string> bKeywords, List<string> sKeywords)
        {
            Console.WriteLine("Basic Keywords:");
            bKeywords.ForEach(x => Console.WriteLine($"\t{x}"));
            Console.WriteLine();
            Console.WriteLine("Specific Keywords:");
            sKeywords.ForEach(x => Console.WriteLine($"\t{x}"));
            Console.WriteLine();
            Console.Write("Which word would you like to remove?  ");
            var remove = Console.ReadLine().ToLower();
            if (remove == "cancel")
                return;
            if (bKeywords.IndexOf(remove) != -1)
            {
                bKeywords.Remove(remove);
                Console.WriteLine($"{remove} was removed from basic keywords.");
                return;
            }
            foreach (var item in sKeywords)
            {
                var word = item.Split(',');
                if (word[0] == remove)
                {
                    sKeywords.Remove(item);
                    Console.WriteLine($"{remove} was removed from specific keywords.");
                    return;
                }
            }
            Console.WriteLine($"{remove} was not found in list of keywords, please try again");
            KeywordsEdit(bKeywords, sKeywords);
        }
        #endregion

        #region FolderEvents
        public static string[] FolderLocation(string[] files)
        {
            Console.Write("Enter the location of the folder....  ");
            var folderPath = Console.ReadLine().ToLower();
            CheckRestart(folderPath);
            CheckExit(folderPath);
            if (Directory.Exists(folderPath))
            {
                Console.WriteLine("Search recursively?");
                var result = Console.ReadLine().ToLower();
                if (result == "yes" || result == "y")
                    files = Directory.GetFiles(folderPath, "*",SearchOption.AllDirectories);
                else if (result == "no" || result == "n")
                    files = Directory.GetFiles(folderPath);
                Console.WriteLine("Files found: " + files.Length.ToString());
            }
            else
            {
                Console.WriteLine("Folder does not exist in this location please try again.");
                FolderLocation(files);
            }
            return files;
        }
        public static int FolderContinue(List<string> bKeywords, List<string> sKeywords, string final, string[] files)
        {
            if (bKeywords.Count == 0 && sKeywords.Count == 0)
            {
                Console.WriteLine("At least one word is required to continue.");
                return -1;
            }
            Console.Write("Enter save path....  ");
            var saveFilePath = Console.ReadLine().ToLower();
            if (File.Exists(saveFilePath))
            {
                Console.WriteLine("Save path is a file, please choose a folder.");
                FolderContinue(bKeywords, sKeywords, final, files);
                return 0;
            }
            if (Directory.Exists(saveFilePath))
            {
                Console.Write("Enter file name....  ");
                var fileName = Console.ReadLine().ToLower();
                saveFilePath += $"\\{fileName}.txt";
                Console.Write("Saving file....  ");
                foreach (var file in files)
                {
                    FileStream inFile = new FileStream(file, FileMode.Open, FileAccess.Read);
                    using (var sr = new StreamReader(inFile))
                    {
                        while (!sr.EndOfStream)
                        {
                            var line = sr.ReadLine().ToLower();
                            if (string.IsNullOrEmpty(line)) continue;
                            for (int i = 0; i < bKeywords.Count; i++)
                            {
                                if (line.IndexOf(bKeywords[i], StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    final += Environment.NewLine + line;
                            }
                            for (int i = 0; i < sKeywords.Count; i++)
                            {
                                var specificWord = sKeywords[i].Split(',');
                                if (line.IndexOf(specificWord[0], StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    if (line.IndexOf(specificWord[0], StringComparison.CurrentCultureIgnoreCase) >= 0)
                                    {
                                        var result = line.Substring(line.IndexOf(specificWord[0]), int.Parse(specificWord[1]));
                                        final += Environment.NewLine + result;
                                    }
                            }
                        }
                    }
                }
                File.WriteAllText(saveFilePath, final);
                Console.WriteLine("File saved.");
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
                Environment.Exit(0);
                return 1;
            }
            else
            {
                Console.WriteLine("The folder location is incorrect. Try again.");
                FolderContinue(bKeywords, sKeywords, final, files);
                return 0;
            }
        }
        #endregion

        #region Checks
        public static void CheckRestart(string answer)
        {
            if (answer == "restart")
                Start();
            else if (answer == "clear")
                Console.Clear();
        }
        public static void CheckExit(string answer)
        {
            if (answer == "exit" || answer == "close")
                Environment.Exit(0);
        }
        #endregion
    }
}
