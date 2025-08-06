using System.Collections.Generic;            // Для работы с обобщёнными коллекциями (List<T>)
using HtmlAgilityPack;                       // Библиотека для парсинга HTML
using NmarketTestTask.Models;                // Модели House и Flat

namespace NmarketTestTask.Parsers
{
    
    // Парсер HTML-таблицы с данными о домах и квартирах.
    // Поддерживает таблицы с тремя столбцами (Дом, Номер, Стоимость)
    // и четырьмя столбцами (Дом, Номер, Площадь, Стоимость).
    
    public class HtmlParser : IParser
    {
        
        // Основной метод парсинга. 
        // Загружает HTML-файл по указанному пути и возвращает список домов с квартирами.
        
        public IList<House> GetHouses(string path)
        {
            // Результирующий список домов
            var houses = new List<House>();

            // Загружаем HTML-документ
            var doc = new HtmlDocument();
            doc.Load(path);

            // Находим все строки таблицы (<tr>) любой вложенности в <table>
            var rows = doc.DocumentNode.SelectNodes("//table//tr");
            // Если таблица не найдена или нет строк с данными — возвращаем пустой список
            if (rows == null || rows.Count < 2)
                return houses;

            // Пропускаем первую строку (обычно заголовок) и обрабатываем остальные
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
                var rawFlat = cells[1].InnerText.Trim();      // Номер квартиры в сыром виде (например, "Кв. 12")

                // Если 4 столбца — третий это площадь, иначе — null
                var area = cells.Count == 4
                    ? cells[2].InnerText.Trim()
                    : null;

                // Если 4 столбца — цена в четвёртом, иначе — в третьем
                var price = cells.Count == 4
                    ? cells[3].InnerText.Trim()
                    : cells[2].InnerText.Trim();

                // Выделяем только цифры из строки rawFlat
                string flatNumber = ExtractDigits(rawFlat);

                // Пропускаем, если нет имени дома или номера квартиры
                if (string.IsNullOrEmpty(houseName) || string.IsNullOrEmpty(flatNumber))
                    continue;

                // Ищем уже созданный объект House с таким именем
                House house = null;
                foreach (var h in houses)
                {
                    if (h.Name == houseName)
                    {
                        house = h;
                        break;
                    }
                }

                // Если дом не найден — создаём новый
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
                    Price = price
                };

                // Если в таблице была площадь — сохраняем её
                if (!string.IsNullOrEmpty(area))
                    flat.Area = area;

                // Добавляем квартиру в соответствующий дом
                house.Flats.Add(flat);
            }

            // Возвращаем собранный список домов
            return houses;
        }

        /// <summary>
        /// Вспомогательный метод для извлечения всех цифр из строки.
        /// Удаляет все символы, кроме '0'–'9'.
        /// </summary>
        /// <param name="input">Входная строка с любыми символами.</param>
        /// <returns>Строка, состоящая только из цифр.</returns>
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