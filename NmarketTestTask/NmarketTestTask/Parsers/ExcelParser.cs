using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using NmarketTestTask.Models;

namespace NmarketTestTask.Parsers
{
    // Парсер Excel-документов, реализующий интерфейс IParser
    public class ExcelParser : IParser
    {
        // Создание переменнойц с рабочим листом Excel-документа
        private IXLWorksheet worksheet;

        // Метод, загружающий файл и возвращающий список объектов House
        public IList<House> GetHouses(string filePath)
        {
            var result = new List<House>();

            try
            {
                // Загружаем файл Excel и берем первый рабочий лист
                worksheet = new XLWorkbook(filePath).Worksheets.First();
            }
            catch
            {
                // Ошибка при загрузке файла (например, файл не существует или поврежден)
                throw new Exception("Не удалось загрузить файл.");
            }

            // Поиск всех ячеек, содержащих слово "Дом"
            var houseMarkers = worksheet.Cells()
                .Where(cell => cell.GetString().Contains("Дом"))
                .ToList();

            if (!houseMarkers.Any())
                // Если ячеек с названием домов нет — выбрасываем исключение
                throw new Exception("Не найдены данные по домам.");

            // Формирование списка домов из найденных ячеек
            foreach (var pair in EnumeratePairs(houseMarkers))
            {
                var flats = ExtractFlats(pair.start, pair.end);

                result.Add(new House
                {
                    Name = pair.start.GetString(),
                    Flats = flats
                });
            }

            return result;
        }

        // Метод генерирует пары (текущий дом, следующий дом) для определения границ
        private IEnumerable<(IXLCell start, IXLCell end)> EnumeratePairs(List<IXLCell> markers)
        {
            for (int i = 0; i < markers.Count; i++)
            {
                var start = markers[i];
                var end = i + 1 < markers.Count ? markers[i + 1] : null;
                yield return (start, end);
            }
        }

        // Извлекает квартиры между текущим и следующим домом
        private List<Flat> ExtractFlats(IXLCell fromCell, IXLCell toCell)
        {
            var rowFrom = fromCell.WorksheetRow().RowNumber();
            var rowTo = toCell?.WorksheetRow().RowNumber() ?? int.MaxValue;

            // Фильтрация ячеек с номерами квартир между двумя домами
            return worksheet.Cells()
                .Where(cell =>
                    cell.GetString().Contains("№") &&
                    cell.WorksheetRow().RowNumber() > rowFrom &&
                    cell.WorksheetRow().RowNumber() < rowTo)
                .Select(cell => new Flat
                {
                    // Номер квартиры извлекается через регулярное выражение
                    Number = Regex.Match(cell.GetString(), @"\d+").Value,

                    // Цена берется из ячейки ниже текущей, в той же колонке
                    Price = worksheet.Cell(
                        cell.WorksheetRow().RowNumber() + 1,
                        cell.WorksheetColumn().ColumnNumber()
                    ).GetString()
                })
                .OrderBy(flat => int.TryParse(flat.Number, out var n) ? n : int.MaxValue)
                .ToList();
        }
    }
}