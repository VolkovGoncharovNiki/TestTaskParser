using System.Collections.Generic;            // Для работы с обобщёнными коллекциями (List<T>)
using HtmlAgilityPack;                       // Библиотека для парсинга HTML
using NmarketTestTask.Models;                // Модели House и Flat

namespace NmarketTestTask.Parsers
{
    // Парсер HTML-таблицы с данными о домах и квартирах.
    // Обрабатывает таблицы с тремя или четырьмя столбцами,
    // но не сохраняет информацию о площади квартир.
        public class HtmlParser : IParser
    {
        // Загружает HTML-файл и возвращает список домов с квартирами.
        // Информация о площади на выходе отсутствует.
                public IList<House> GetHouses(string path)
        {
            // Результирующий список домов
            var houses = new List<House>();

            // Загружаем HTML-документ
            var doc = new HtmlDocument();
            doc.Load(path);

            // Находим все строки таблицы (<tr>) внутри любых <table>
            var rows = doc.DocumentNode.SelectNodes("//table//tr");
            // Если таблица не найдена или не содержит строк данных — возвращаем пустой список
            if (rows == null || rows.Count < 2)
                return houses;

            // Пропускаем первую строку (заголовок) и обрабатываем остальные
            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];
                var cells = row.SelectNodes("td");

                // Ожидаем либо 3 ячейки (Дом, Номер, Стоимость),
                // либо 4 ячейки (Дом, Номер, Площадь, Стоимость)
                if (cells == null || (cells.Count != 3 && cells.Count != 4))
                    continue;

                // Извлекаем и очищаем содержимое ячеек
                var houseName = cells[0].InnerText.Trim();       // Название дома
                var rawFlat = cells[1].InnerText.Trim();      // Номер квартиры (например, "Кв. 12")

                // Если 4 столбца — цена во 4-м, иначе — в 3-м
                var price = cells.Count == 4
                    ? cells[3].InnerText.Trim()
                    : cells[2].InnerText.Trim();

                // Выделяем только цифры из номера квартиры
                string flatNumber = ExtractDigits(rawFlat);

                // Пропускаем, если нет имени дома или номера квартиры
                if (string.IsNullOrEmpty(houseName) || string.IsNullOrEmpty(flatNumber))
                    continue;

                // Пытаемся найти существующий объект House по имени
                House house = null;
                foreach (var h in houses)
                {
                    if (h.Name == houseName)
                    {
                        house = h;
                        break;
                    }
                }

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

                // Создаём объект Flat без поля Area
                var flat = new Flat
                {
                    Number = flatNumber,
                    Price = price
                };

                // Добавляем квартиру в соответствующий дом
                house.Flats.Add(flat);
            }

            // Возвращаем собранный список домов
            return houses;
        }

        
        // Вспомогательный метод для извлечения всех цифр из строки.
                private string ExtractDigits(string input)
        {
            var result = string.Empty;

            // Проходим по каждому символу и оставляем только цифры
            foreach (var c in input)
            {
                if (c >= '0' && c <= '9')
                    result += c;
            }

            return result;
        }
    }
}