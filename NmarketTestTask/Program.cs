using System;                    //добавленно для использование Console
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NmarketTestTask.Models;
using NmarketTestTask.Parsers;

namespace NmarketTestTask
{
    class Program
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(List<House>));

        static void Main(string[] args)
        {
            Console.WriteLine("Старт парсинга...");

            Directory.CreateDirectory("Result");

            ParseAllFiles(new ExcelParser(), @"Files\Excel");
            ParseAllFiles(new Parsers.HtmlParser(), @"Files\Html");

            Console.WriteLine("Парсинг завершён. Смотрите результаты в папке Result.");
        }

        private static void ParseAllFiles(Models.IParser parser, string folder)
        {
            foreach (var file in Directory.GetFiles(folder))

            {
                var result = parser.GetHouses(file);
                var resultFile = $@"Result\{Path.GetFileName(file)}.xml";

                using (var sw = new StreamWriter(resultFile))
                    Serializer.Serialize(sw, result);

                Console.WriteLine($"Сохранено в {resultFile}");
            }
        }
    }
}