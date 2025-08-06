using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using NmarketTestTask.Models;

namespace NmarketTestTask.Parsers
{
    // Реализация интерфейса IParser, предназначена для извлечения данных о домах из HTML-документа
    public class HtmlParser : IParser
    {
        // Основной метод парсинга. Получает путь к HTML-файлу и возвращает список объектов House
        public IList<House> GetHouses(string path)
        {
            var houses = new List<House>(); // Результирующий список домов
            var doc = new HtmlDocument();   // Создание HTML-документа
            doc.Load(path);                 // Загрузка HTML из указанного пути

            // Поиск всех строк таблицы (предполагается, что первая строка — заголовки)
            var rows = doc.DocumentNode.SelectNodes("//table//tr");
            if (rows == null || rows.Count < 2)
                return houses; // Если таблица не найдена или в ней меньше 2 строк — возвращаем пустой список

            // Пропускаем заголовок и начинаем обработку данных со второй строки
            foreach (var row in rows.Skip(1))
            {
                var cells = row.SelectNodes("td");
                if (cells == null || cells.Count < 3)
                    continue; // Пропускаем строку, если в ней недостаточно ячеек

                // Извлекаем и обрабатываем данные из ячеек
                var houseName = cells[0].InnerText.Trim();            // Название дома
                var rawNumber = cells[1].InnerText;                   // Сырые данные о квартире (например, "Кв. 5")
                var flatNumber = Regex.Match(rawNumber, @"\d+").Value;// Извлечение номера квартиры с помощью регулярного выражения
                var price = cells[2].InnerText.Trim();                // Цена квартиры

                // Пропускаем строку, если название дома или номер квартиры отсутствуют
                if (string.IsNullOrWhiteSpace(houseName) || string.IsNullOrWhiteSpace(flatNumber))
                    continue;

                // Ищем, существует ли уже дом с таким названием
                var house = houses.FirstOrDefault(h => h.Name == houseName);
                if (house == null)
                {
                    // Если дом не найден — создаём новый объект House и добавляем в список
                    house = new House
                    {
                        Name = houseName,
                        Flats = new List<Flat>()
                    };
                    houses.Add(house);
                }

                // Добавляем информацию о квартире в список Flats соответствующего дома
                house.Flats.Add(new Flat
                {
                    Number = flatNumber,
                    Price = price
                });
            }

            // Возвращаем сформированный список домов с вложенными квартирами
            return houses;
        }
    }
}