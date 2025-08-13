using System.Collections.Generic;            // Для работы с обобщёнными коллекциями (List<T>)
using HtmlAgilityPack;                       // Библиотека для парсинга HTML-документов
using NmarketTestTask.Models;                // Модели данных: House и Flat

namespace NmarketTestTask.Parsers
{
    /// <summary>
    /// Парсер HTML-таблицы, использующий методологию БЭМ.
    /// Извлекает информацию о домах и квартирах из таблицы, где ячейки имеют классы:
    /// "house" — номер дома, "number" — номер квартиры, "price" — стоимость.
    /// Столбец "square" (площадь) игнорируется.
    /// </summary>
    public class HtmlParser : IParser
    {
        /// <summary>
        /// Основной метод парсинга HTML-файла.
        /// Загружает HTML-документ, извлекает строки таблицы, и формирует список объектов House.
        /// </summary>
        /// <param name="path">Путь к HTML-файлу</param>
        /// <returns>Список домов с вложенными квартирами</returns>
        public IList<House> GetHouses(string path)
        {
            // Список, в который будут добавляться дома
            var houses = new List<House>();

            // Загружаем HTML-документ с диска
            var doc = new HtmlDocument();
            doc.Load(path);

            // Находим все строки таблицы (<tr>) внутри любого <table>
            var rows = doc.DocumentNode.SelectNodes("//table//tr");

            // Если таблица не найдена или содержит только заголовок — возвращаем пустой список
            if (rows == null || rows.Count < 2)
                return houses;

            // Пропускаем первую строку (заголовок таблицы) и обрабатываем остальные
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];

                // Извлекаем ячейки по классам БЭМ: house, number, price
                var houseCell = row.SelectSingleNode(".//td[contains(@class, 'house')]");
                var numberCell = row.SelectSingleNode(".//td[contains(@class, 'number')]");
                var priceCell = row.SelectSingleNode(".//td[contains(@class, 'price')]");

                // Если одна из ячеек отсутствует — пропускаем строку
                if (houseCell == null || numberCell == null || priceCell == null)
                    continue;

                // Извлекаем текст из ячеек и очищаем от лишних пробелов
                var houseName = houseCell.InnerText.Trim();               // Название дома
                var rawFlatNumber = numberCell.InnerText.Trim();         // Номер квартиры (может содержать текст)
                var priceText = priceCell.InnerText.Trim();              // Стоимость квартиры

                // Извлекаем только цифры из номера квартиры (например, из "Кв. 12" получаем "12")
                var flatNumber = ExtractDigits(rawFlatNumber);

                // Если имя дома или номер квартиры пуст — пропускаем строку
                if (string.IsNullOrEmpty(houseName) || string.IsNullOrEmpty(flatNumber))
                    continue;

                // Ищем уже существующий дом с таким именем
                var house = houses.Find(h => h.Name == houseName);

                // Если дом не найден — создаём новый и добавляем в список
                if (house == null)
                {
                    house = new House
                    {
                        Name = houseName,
                        Flats = new List<Flat>()
                    };
                    houses.Add(house);
                }

                // Создаём объект Flat и заполняем его свойства
                var flat = new Flat
                {
                    Number = flatNumber,
                    Price = priceText
                };

                // Добавляем квартиру в список квартир соответствующего дома
                house.Flats.Add(flat);
            }

            // Возвращаем итоговый список домов с квартирами
            return houses;
        }

        /// <summary>
        /// Вспомогательный метод для извлечения всех цифр из строки.
        /// Используется для очистки номера квартиры от лишнего текста.
        /// </summary>
        /// <param name="input">Исходная строка (например, "Кв. 12")</param>
        /// <returns>Строка, содержащая только цифры (например, "12")</returns>
        private string ExtractDigits(string input)
        {
            var result = string.Empty;

            // Проходим по каждому символу и добавляем только цифры
            foreach (var c in input)
            {
                if (char.IsDigit(c))
                    result += c;
            }

            return result;
        }
    }
}